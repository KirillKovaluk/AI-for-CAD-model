using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CadGeneratorWinForms
{
    public partial class Form1 : Form
    {
        private ModelGenerator _generator;
        private TextParser _parser;
        private StlExporter _exporter;
        private string _modelsFolder;
        private bool _isGenerating = false;
        private List<string> _queryHistory = new List<string>();
        private int _historyIndex = -1;

        // Храним выбранное название цвета
        private string _selectedColorName = "Серый";

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

                this.btnGenerate.Click += BtnGenerate_Click;
                this.btnOpenFolder.Click += BtnOpenFolder_Click;
                this.btnRefresh.Click += BtnRefresh_Click;
                this.btnOpenModel.Click += BtnOpenModel_Click;
                this.btnDelete.Click += BtnDelete_Click;
                this.btnClearAll.Click += BtnClearAll_Click;
                this.listBoxModels.SelectedIndexChanged += ListBoxModels_SelectedIndexChanged;
                this.listBoxModels.DoubleClick += ListBoxModels_DoubleClick;
                this.txtQuery.KeyPress += TxtQuery_KeyPress;
                this.cmbColor.SelectedIndexChanged += CmbColor_SelectedIndexChanged;

                UpdateStatus("Готов к работе");
                UpdateModelsList();
                toolStripStatusLabel2.Text = $"Папка: {_modelsFolder}";

                // Устанавливаем цвет по умолчанию
                _selectedColorName = cmbColor.SelectedItem?.ToString() ?? "Серый";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void CmbColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedColorName = cmbColor.SelectedItem?.ToString() ?? "Серый";
            Console.WriteLine($"Выбран цвет: {_selectedColorName}");
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
                foreach (var file in Directory.GetFiles(_modelsFolder, "*.obj"))
                    listBoxModels.Items.Add(Path.GetFileName(file));
                foreach (var file in Directory.GetFiles(_modelsFolder, "*.stl"))
                    listBoxModels.Items.Add(Path.GetFileName(file));
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

        // ПРЯМОЕ ПОЛУЧЕНИЕ ЦВЕТА ПО НАЗВАНИЮ (без Color.xxx)
        private Color GetColorByName(string colorName)
        {
            switch (colorName)
            {
                case "Красный": return Color.FromArgb(255, 255, 0, 0);
                case "Синий": return Color.FromArgb(255, 0, 0, 255);
                case "Зеленый": return Color.FromArgb(255, 0, 255, 0);
                case "Желтый": return Color.FromArgb(255, 255, 255, 0);
                case "Голубой": return Color.FromArgb(255, 0, 255, 255);
                case "Белый": return Color.FromArgb(255, 255, 255, 255);
                case "Черный": return Color.FromArgb(255, 0, 0, 0);
                case "Серый": return Color.FromArgb(255, 128, 128, 128);
                default: return Color.FromArgb(255, 128, 128, 128);
            }
        }

        // ПОЛУЧЕНИЕ НАЗВАНИЯ ЦВЕТА
        private string GetColorName(Color color)
        {
            if (color.R == 255 && color.G == 0 && color.B == 0) return "красный";
            if (color.R == 0 && color.G == 0 && color.B == 255) return "синий";
            if (color.R == 0 && color.G == 255 && color.B == 0) return "зеленый";
            if (color.R == 255 && color.G == 255 && color.B == 0) return "желтый";
            if (color.R == 0 && color.G == 255 && color.B == 255) return "голубой";
            if (color.R == 255 && color.G == 255 && color.B == 255) return "белый";
            if (color.R == 0 && color.G == 0 && color.B == 0) return "черный";
            if (color.R == 128 && color.G == 128 && color.B == 128) return "серый";
            return "неизвестный";
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
                cmbColor.Enabled = false;
                progressBar1.Visible = true;

                UpdateStatus("Генерация...");
                lblResult.Text = "⏳ Генерация...";
                lblResult.ForeColor = Color.Orange;

                // ПОЛУЧАЕМ ЦВЕТ ПО НАЗВАНИЮ
                Color selectedColor = GetColorByName(_selectedColorName);
                Console.WriteLine($"🎨 Выбран цвет: {_selectedColorName} -> RGB({selectedColor.R},{selectedColor.G},{selectedColor.B})");

                var parameters = await Task.Run(() => _parser.Parse(query, selectedColor));
                var mesh = await Task.Run(() => _generator.GenerateModel(parameters));

                string fileName = $"model_{DateTime.Now:yyyyMMdd_HHmmss}.obj";
                string filePath = Path.Combine(_modelsFolder, fileName);
                await Task.Run(() => _exporter.ExportToObj(mesh, parameters.Color, filePath));

                UpdateModelsList();

                string sizeText = parameters.ShapeType == "cube" ? $"{parameters.Size1} x {parameters.Size2} x {parameters.Size3}" :
                                 parameters.ShapeType == "sphere" ? $"радиус {parameters.Size1}" :
                                 $"радиус {parameters.Size1}, высота {parameters.Size2}";

                string colorName = GetColorName(parameters.Color);
                string resultText = $"✅ Создан {colorName} {parameters.ShapeType}: {sizeText}";
                lblResult.Text = resultText;
                lblResult.ForeColor = Color.Green;

                UpdateStatus($"Сохранено: {fileName}");
                listBoxModels.SelectedItem = fileName;
                btnDelete.Enabled = true;
                btnOpenModel.Enabled = true;
                toolStripStatusLabel2.Text = $"Последняя модель: {colorName}";

                _queryHistory.Add(query);
                _historyIndex = _queryHistory.Count;
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
                cmbColor.Enabled = true;
                progressBar1.Visible = false;
            }
        }

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
            foreach (var file in Directory.GetFiles(_modelsFolder, "*.obj"))
                File.Delete(file);
            foreach (var file in Directory.GetFiles(_modelsFolder, "*.stl"))
                File.Delete(file);
            UpdateModelsList();
            lblResult.Text = "✅ Все файлы удалены";
            btnDelete.Enabled = false;
            btnOpenModel.Enabled = false;
        }
    }
}