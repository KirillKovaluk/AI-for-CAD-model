using System;
using System.Windows.Forms;

namespace CadGeneratorWinForms
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Включаем визуальные стили для современного вида
            Application.EnableVisualStyles();

            // Устанавливаем совместимость с текстовым рендерингом
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаем главную форму
            Application.Run(new Form1());
        }
    }
}