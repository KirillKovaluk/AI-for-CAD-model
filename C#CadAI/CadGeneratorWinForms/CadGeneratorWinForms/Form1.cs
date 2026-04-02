using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using CadGenerator;
using System.Linq;

namespace CadGeneratorWinForms
{
    public partial class Form1 : Form
    {
        private ModelGenerator _generator;
        private TextParser _parser;
        private StlExporter _exporter;
        private CommandTranslator _commandTranslator;
        private string _modelsFolder;
        private bool _isGenerating = false;

        // История запросов
        private List<string> _queryHistory = new List<string>();
        private int _historyIndex = -1;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _modelsFolder = Path.Combine(Application.StartupPath, "GeneratedModels");
                if (!Directory.Exists(_modelsFolder))
                {
                    Directory.CreateDirectory(_modelsFolder);
                }

                _generator = new ModelGenerator();
                _parser = new TextParser();
                _exporter = new StlExporter();
                _commandTranslator = new CommandTranslator();

                // Подписываемся на события
                this.btnGenerate.Click += BtnGenerate_Click;
                this.btnOpenFolder.Click += BtnOpenFolder_Click;
                this.btnRefresh.Click += BtnRefresh_Click;
                this.btnOpenModel.Click += BtnOpenModel_Click;
                this.btnDelete.Click += BtnDelete_Click;
                this.btnClearAll.Click += BtnClearAll_Click;
                this.listBoxModels.SelectedIndexChanged += ListBoxModels_SelectedIndexChanged;
                this.listBoxModels.DoubleClick += ListBoxModels_DoubleClick;
                this.txtQuery.KeyPress += TxtQuery_KeyPress;
                this.txtQuery.KeyDown += TxtQuery_KeyDown;

                UpdateStatus("Готов к работе");
                UpdateModelsList();
                toolStripStatusLabel2.Text = $"Папка: {_modelsFolder}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void UpdateStatus(string status)
        {
            toolStripStatusLabel1.Text = $"Статус: {status}";
        }

        private void UpdateModelsList()
        {
            listBoxModels.Items.Clear();
            if (Directory.Exists(_modelsFolder))
            {
                // Показываем OBJ и STL файлы
                foreach (var file in Directory.GetFiles(_modelsFolder, "*.obj"))
                {
                    listBoxModels.Items.Add(Path.GetFileName(file));
                }
                foreach (var file in Directory.GetFiles(_modelsFolder, "*.stl"))
                {
                    listBoxModels.Items.Add(Path.GetFileName(file));
                }
            }
        }

        private async void BtnGenerate_Click(object sender, EventArgs e)
        {
            if (_isGenerating) return;
            await GenerateModelAsync();
        }

        private void TxtQuery_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !_isGenerating)
            {
                GenerateModelAsync();
                e.Handled = true;
            }
        }

        private void TxtQuery_KeyDown(object sender, KeyEventArgs e)
        {
            // Навигация по истории (стрелки вверх/вниз)
            if (e.KeyCode == Keys.Up && _historyIndex > 0)
            {
                _historyIndex--;
                txtQuery.Text = _queryHistory[_historyIndex];
                txtQuery.SelectionStart = txtQuery.Text.Length;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down && _historyIndex < _queryHistory.Count - 1)
            {
                _historyIndex++;
                txtQuery.Text = _queryHistory[_historyIndex];
                txtQuery.SelectionStart = txtQuery.Text.Length;
                e.Handled = true;
            }
        }

        private Color GetSelectedColor()
        {
            string selectedColorName = cmbColor.SelectedItem?.ToString() ?? "Серый";

            switch (selectedColorName)
            {
                case "Красный": return Color.Red;
                case "Синий": return Color.Blue;
                case "Зеленый": return Color.Green;
                case "Желтый": return Color.Yellow;
                case "Оранжевый": return Color.Orange;
                case "Фиолетовый": return Color.Purple;
                case "Розовый": return Color.Pink;
                case "Черный": return Color.Black;
                case "Белый": return Color.White;
                case "Серый": return Color.Gray;
                case "Коричневый": return Color.Brown;
                case "Золотой": return Color.Gold;
                case "Серебряный": return Color.Silver;
                default: return Color.Gray;
            }
        }

        private async Task GenerateModelAsync()
        {
            string query = txtQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введите описание модели", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _isGenerating = true;
                btnGenerate.Enabled = false;
                txtQuery.Enabled = false;
                cmbColor.Enabled = false;
                progressBar1.Visible = true;

                UpdateStatus("Анализ команд...");
                lblResult.Text = "⏳ Анализ команд...";
                lblResult.ForeColor = Color.Orange;

                // Шаг 1: Переводим текст в последовательность команд
                Color selectedColor = GetSelectedColor();
                var commandSequence = await Task.Run(() => _commandTranslator.Translate(query));

                // Шаг 2: Показываем последовательность команд в консоли для отладки
                ShowCommandSequence(commandSequence);

                // Шаг 3: Выполняем команды
                UpdateStatus("Выполнение команд...");
                lblResult.Text = "⏳ Выполнение команд...";

                var mesh = await Task.Run(() => _commandTranslator.ExecuteCommands(commandSequence));

                if (mesh == null || mesh.TriangleCount == 0)
                {
                    throw new Exception("Не удалось создать модель из команд");
                }

                // Шаг 4: Сохраняем модель
                string fileName = $"model_{DateTime.Now:yyyyMMdd_HHmmss}.obj";
                string filePath = Path.Combine(_modelsFolder, fileName);
                await Task.Run(() => _exporter.ExportToObj(mesh, selectedColor, filePath));

                UpdateModelsList();

                // Формируем результат
                string resultText = $"✅ Выполнено {commandSequence.Commands.Count} команд\n";
                resultText += $"   {commandSequence}";

                lblResult.Text = resultText;
                lblResult.ForeColor = Color.Green;

                UpdateStatus($"Сохранено: {fileName}");

                listBoxModels.SelectedItem = fileName;
                btnDelete.Enabled = true;
                btnOpenModel.Enabled = true;

                toolStripStatusLabel2.Text = $"Последняя модель: {fileName}";

                // Добавляем в историю запросов
                _queryHistory.Add(query);
                _historyIndex = _queryHistory.Count;
            }
            catch (Exception ex)
            {
                lblResult.Text = $"❌ Ошибка: {ex.Message}";
                lblResult.ForeColor = Color.Red;
                UpdateStatus("Ошибка генерации");

                MessageBox.Show($"Ошибка: {ex.Message}\n\nПопробуйте использовать простые запросы:\n" +
                    "- куб 10x20x30\n" +
                    "- сфера радиусом 15\n" +
                    "- цилиндр 5 20",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _isGenerating = false;
                btnGenerate.Enabled = true;
                txtQuery.Enabled = true;
                cmbColor.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private void ShowCommandSequence(CommandSequence sequence)
        {
            System.Diagnostics.Debug.WriteLine($"\n📋 ПОСЛЕДОВАТЕЛЬНОСТЬ КОМАНД:");
            System.Diagnostics.Debug.WriteLine($"   Исходный текст: {sequence.OriginalText}");
            System.Diagnostics.Debug.WriteLine($"   Команды:");

            foreach (var cmd in sequence.Commands)
            {
                System.Diagnostics.Debug.WriteLine($"     - {cmd}");
            }

            // Также показываем в статусе
            UpdateStatus($"Распознано {sequence.Commands.Count} команд");
        }

        private string GetColorName(Color color)
        {
            if (color == Color.Red) return "красный";
            if (color == Color.Blue) return "синий";
            if (color == Color.Green) return "зеленый";
            if (color == Color.Yellow) return "желтый";
            if (color == Color.Orange) return "оранжевый";
            if (color == Color.Purple) return "фиолетовый";
            if (color == Color.Pink) return "розовый";
            if (color == Color.Black) return "черный";
            if (color == Color.White) return "белый";
            if (color == Color.Gray) return "серый";
            if (color == Color.Brown) return "коричневый";
            if (color == Color.Gold) return "золотой";
            if (color == Color.Silver) return "серебряный";
            return "неизвестный";
        }

        private void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", _modelsFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия папки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            UpdateModelsList();
            UpdateStatus("Список моделей обновлен");
        }

        private void BtnOpenModel_Click(object sender, EventArgs e)
        {
            OpenSelectedModel();
        }

        private void ListBoxModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnDelete.Enabled = listBoxModels.SelectedItem != null;
            btnOpenModel.Enabled = listBoxModels.SelectedItem != null;

            if (listBoxModels.SelectedItem != null)
            {
                string filePath = Path.Combine(_modelsFolder, listBoxModels.SelectedItem.ToString());
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    lblResult.Text = $"📁 {fileInfo.Name} ({fileInfo.Length / 1024} KB)";
                }
            }
        }

        private void ListBoxModels_DoubleClick(object sender, EventArgs e)
        {
            OpenSelectedModel();
        }

        private void OpenSelectedModel()
        {
            if (listBoxModels.SelectedItem != null)
            {
                string fileName = listBoxModels.SelectedItem.ToString();
                string filePath = Path.Combine(_modelsFolder, fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = filePath,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл: {ex.Message}\n\nФайл сохранен в: {filePath}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (listBoxModels.SelectedItem != null)
            {
                string fileName = listBoxModels.SelectedItem.ToString();
                string filePath = Path.Combine(_modelsFolder, fileName);

                var result = MessageBox.Show($"Удалить файл {fileName}?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        File.Delete(filePath);
                        UpdateModelsList();
                        lblResult.Text = "✅ Файл удален";
                        UpdateStatus("Файл удален");
                        btnDelete.Enabled = false;
                        btnOpenModel.Enabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            var files = Directory.GetFiles(_modelsFolder, "*.obj");
            var stlFiles = Directory.GetFiles(_modelsFolder, "*.stl");
            var allFiles = files.Concat(stlFiles).ToArray();

            if (allFiles.Length == 0)
            {
                MessageBox.Show("Нет файлов для удаления", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Удалить все {allFiles.Length} моделей?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    foreach (var file in allFiles)
                    {
                        File.Delete(file);
                    }
                    UpdateModelsList();
                    lblResult.Text = $"✅ Удалено {allFiles.Length} файлов";
                    UpdateStatus("Все файлы удалены");
                    btnDelete.Enabled = false;
                    btnOpenModel.Enabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}