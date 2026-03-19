namespace CadGeneratorWinForms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.listBoxModels = new System.Windows.Forms.ListBox();
            this.lblResult = new System.Windows.Forms.Label();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.lblExamples = new System.Windows.Forms.Label();
            this.groupBoxModels = new System.Windows.Forms.GroupBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnOpenModel = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.groupBoxInput.SuspendLayout();
            this.groupBoxModels.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "CAD Генератор моделей по тексту";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // groupBoxInput
            this.groupBoxInput.Text = "Введите описание модели";
            this.groupBoxInput.Location = new System.Drawing.Point(12, 12);
            this.groupBoxInput.Size = new System.Drawing.Size(560, 120);
            this.groupBoxInput.TabIndex = 0;

            // txtQuery
            this.txtQuery.Location = new System.Drawing.Point(6, 25);
            this.txtQuery.Size = new System.Drawing.Size(440, 23);
            this.txtQuery.TabIndex = 0;
            this.txtQuery.Text = "куб 10x20x30";

            // btnGenerate
            this.btnGenerate.Location = new System.Drawing.Point(452, 24);
            this.btnGenerate.Size = new System.Drawing.Size(100, 30);
            this.btnGenerate.Text = "Сгенерировать";
            this.btnGenerate.UseVisualStyleBackColor = true;

            // lblExamples
            this.lblExamples.Location = new System.Drawing.Point(6, 55);
            this.lblExamples.Size = new System.Drawing.Size(440, 60);
            this.lblExamples.Text = "Примеры: куб 15x20x25 | сфера радиусом 30 | цилиндр 5 20 | шар диаметром 40";
            this.lblExamples.ForeColor = System.Drawing.Color.Gray;

            // groupBoxModels
            this.groupBoxModels.Text = "Сгенерированные модели";
            this.groupBoxModels.Location = new System.Drawing.Point(12, 140);
            this.groupBoxModels.Size = new System.Drawing.Size(560, 380);
            this.groupBoxModels.TabIndex = 1;

            // listBoxModels
            this.listBoxModels.Location = new System.Drawing.Point(6, 25);
            this.listBoxModels.Size = new System.Drawing.Size(440, 250);
            this.listBoxModels.TabIndex = 0;

            // btnOpenFolder
            this.btnOpenFolder.Location = new System.Drawing.Point(452, 25);
            this.btnOpenFolder.Size = new System.Drawing.Size(100, 30);
            this.btnOpenFolder.Text = "Открыть папку";
            this.btnOpenFolder.UseVisualStyleBackColor = true;

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(452, 61);
            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = true;

            // btnOpenModel
            this.btnOpenModel.Location = new System.Drawing.Point(452, 97);
            this.btnOpenModel.Size = new System.Drawing.Size(48, 30);
            this.btnOpenModel.Text = "📂";
            this.btnOpenModel.UseVisualStyleBackColor = true;
            this.btnOpenModel.Enabled = false;

            // btnDelete
            this.btnDelete.Location = new System.Drawing.Point(506, 97);
            this.btnDelete.Size = new System.Drawing.Size(48, 30);
            this.btnDelete.Text = "🗑️";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Enabled = false;

            // btnClearAll
            this.btnClearAll.Location = new System.Drawing.Point(452, 133);
            this.btnClearAll.Size = new System.Drawing.Size(100, 30);
            this.btnClearAll.Text = "Очистить все";
            this.btnClearAll.UseVisualStyleBackColor = true;

            // progressBar1
            this.progressBar1.Location = new System.Drawing.Point(452, 169);
            this.progressBar1.Size = new System.Drawing.Size(100, 20);
            this.progressBar1.Visible = false;

            // lblResult
            this.lblResult.Location = new System.Drawing.Point(6, 290);
            this.lblResult.Size = new System.Drawing.Size(440, 60);
            this.lblResult.Text = "Готов к работе";
            this.lblResult.ForeColor = System.Drawing.Color.Green;

            // statusStrip
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.toolStripStatusLabel1,
                this.toolStripStatusLabel2});
            this.statusStrip.Location = new System.Drawing.Point(0, 528);
            this.statusStrip.Size = new System.Drawing.Size(784, 22);
            this.statusStrip.TabIndex = 2;

            // toolStripStatusLabel1
            this.toolStripStatusLabel1.Text = "Статус: Готов";

            // toolStripStatusLabel2
            this.toolStripStatusLabel2.Text = "Папка: GeneratedModels";

            // pictureBoxPreview
            this.pictureBoxPreview.Location = new System.Drawing.Point(580, 12);
            this.pictureBoxPreview.Size = new System.Drawing.Size(200, 500);
            this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPreview.BackColor = System.Drawing.Color.White;
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreview.Visible = false;

            // Добавляем контролы в groupBoxInput
            this.groupBoxInput.Controls.Add(this.txtQuery);
            this.groupBoxInput.Controls.Add(this.btnGenerate);
            this.groupBoxInput.Controls.Add(this.lblExamples);

            // Добавляем контролы в groupBoxModels
            this.groupBoxModels.Controls.Add(this.listBoxModels);
            this.groupBoxModels.Controls.Add(this.btnOpenFolder);
            this.groupBoxModels.Controls.Add(this.btnRefresh);
            this.groupBoxModels.Controls.Add(this.btnOpenModel);
            this.groupBoxModels.Controls.Add(this.btnDelete);
            this.groupBoxModels.Controls.Add(this.btnClearAll);
            this.groupBoxModels.Controls.Add(this.progressBar1);
            this.groupBoxModels.Controls.Add(this.lblResult);

            // Добавляем все на форму
            this.Controls.Add(this.groupBoxInput);
            this.Controls.Add(this.groupBoxModels);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.pictureBoxPreview);

            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            this.groupBoxModels.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Объявление всех контролов
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ListBox listBoxModels;
        private System.Windows.Forms.Label lblResult;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.Label lblExamples;
        private System.Windows.Forms.GroupBox groupBoxModels;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnOpenModel;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
    }
}