using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using CadGenerator;

namespace CadGeneratorWinForms
{
    public partial class Form1 : Form
    {
        private ModelGenerator _generator;
        private TextParser _parser;
        private StlExporter _exporter;
        private string _modelsFolder;
        private bool _isGenerating = false;

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

        private async Task GenerateModelAsync()
        {
            string query = txtQuery.Text.Trim();
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show("Введите описание модели");
                return;
            }

            try
            {
                _isGenerating = true;
                btnGenerate.Enabled = false;
                txtQuery.Enabled = false;
                progressBar1.Visible = true;

                UpdateStatus("Генерация...");
                lblResult.Text = "⏳ Генерация...";
                lblResult.ForeColor = Color.Orange;

                var parameters = await Task.Run(() => _parser.Parse(query));
                var mesh = await Task.Run(() => _generator.GenerateModel(parameters));

                string fileName = $"model_{DateTime.Now:yyyyMMdd_HHmmss}.stl";
                string filePath = Path.Combine(_modelsFolder, fileName);

                await Task.Run(() => _exporter.ExportToStl(mesh, filePath));

                UpdateModelsList();

                // Формируем результат с учетом цвета
                string sizeText;
                if (parameters.ShapeType == "cube")
                    sizeText = $"{parameters.Size1} x {parameters.Size2} x {parameters.Size3}";
                else if (parameters.ShapeType == "sphere")
                    sizeText = $"радиус {parameters.Size1}";
                else // cylinder
                    sizeText = $"радиус {parameters.Size1}, высота {parameters.Size2}";

                string resultText = $"✅ Создан {parameters.ColorName} {parameters.ShapeType}: {sizeText}";
                lblResult.Text = resultText;

                // Устанавливаем цвет текста
                if (parameters.Color != Color.Gray && parameters.Color != Color.White && parameters.Color != Color.Black)
                {
                    lblResult.ForeColor = parameters.Color;
                }
                else
                {
                    lblResult.ForeColor = Color.Green;
                }

                UpdateStatus($"Сохранено: {fileName}");

                listBoxModels.SelectedItem = fileName;
                btnDelete.Enabled = true;
                btnOpenModel.Enabled = true;

                // Используем метод GetColorName здесь
                toolStripStatusLabel2.Text = $"Последняя модель: {GetColorName(parameters.Color)}";
            }
            catch (Exception ex)
            {
                lblResult.Text = $"❌ Ошибка: {ex.Message}";
                lblResult.ForeColor = Color.Red;
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
            finally
            {
                _isGenerating = false;
                btnGenerate.Enabled = true;
                txtQuery.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        // 👇 ВОТ СЮДА ДОБАВЬТЕ ЭТОТ МЕТОД 👇
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
        // 👆 КОНЕЦ МЕТОДА 👆

        private void BtnOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", _modelsFolder);
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            UpdateModelsList();
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
                var fileInfo = new FileInfo(filePath);
                lblResult.Text = $"📁 {fileInfo.Name} ({fileInfo.Length / 1024} KB)";
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
                string filePath = Path.Combine(_modelsFolder, listBoxModels.SelectedItem.ToString());
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (listBoxModels.SelectedItem != null)
            {
                string filePath = Path.Combine(_modelsFolder, listBoxModels.SelectedItem.ToString());
                File.Delete(filePath);
                UpdateModelsList();
                lblResult.Text = "✅ Файл удален";
                btnDelete.Enabled = false;
                btnOpenModel.Enabled = false;
            }
        }

        private void BtnClearAll_Click(object sender, EventArgs e)
        {
            foreach (var file in Directory.GetFiles(_modelsFolder, "*.stl"))
            {
                File.Delete(file);
            }
            UpdateModelsList();
            lblResult.Text = "✅ Все файлы удалены";
            btnDelete.Enabled = false;
            btnOpenModel.Enabled = false;
        }
    }
}