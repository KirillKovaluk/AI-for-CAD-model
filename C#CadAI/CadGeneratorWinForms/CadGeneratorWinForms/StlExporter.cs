using g3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CadGeneratorWinForms
{
    public class StlExporter
    {
        public void ExportToObj(DMesh3 mesh, Color color, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Нормализуем RGB значения (просто используем значения, они уже в 0-255)
                int r = color.R;
                int g = color.G;
                int b = color.B;

                // Убеждаемся что значения в диапазоне 0-255
                if (r < 0) r = 0;
                if (r > 255) r = 255;
                if (g < 0) g = 0;
                if (g > 255) g = 255;
                if (b < 0) b = 0;
                if (b > 255) b = 255;

                float rf = r / 255f;
                float gf = g / 255f;
                float bf = b / 255f;

                Console.WriteLine($"🎨 Экспорт цвета: RGB({r},{g},{b}) -> ({rf:F3},{gf:F3},{bf:F3})");

                // 1. Сохраняем OBJ файл
                string objPath = filePath;
                using (StreamWriter writer = new StreamWriter(objPath))
                {
                    writer.WriteLine($"# OBJ файл");
                    writer.WriteLine($"# Цвет: RGB({r},{g},{b})");
                    writer.WriteLine($"mtllib {Path.GetFileName(Path.ChangeExtension(filePath, "mtl"))}");
                    writer.WriteLine($"o Model");
                    writer.WriteLine($"usemtl Material_{r}_{g}_{b}");
                    writer.WriteLine($"");

                    // Вершины
                    foreach (int vid in mesh.VertexIndices())
                    {
                        var v = mesh.GetVertex(vid);
                        writer.WriteLine($"v {v.x:F6} {v.y:F6} {v.z:F6}");
                    }
                    writer.WriteLine($"");

                    // Грани
                    foreach (int tid in mesh.TriangleIndices())
                    {
                        var tri = mesh.GetTriangle(tid);
                        writer.WriteLine($"f {tri.a + 1} {tri.b + 1} {tri.c + 1}");
                    }
                }

                // 2. Сохраняем MTL файл (правильный формат)
                string mtlPath = Path.ChangeExtension(filePath, "mtl");
                SaveMaterialFile(mtlPath, r, g, b, rf, gf, bf);

                // 3. Сохраняем отдельный файл с информацией о цвете
                string colorInfoPath = Path.ChangeExtension(filePath, "color.txt");
                File.WriteAllText(colorInfoPath,
                    $"Цвет модели:\n" +
                    $"  RGB: ({r}, {g}, {b})\n" +
                    $"  HEX: #{r:X2}{g:X2}{b:X2}\n" +
                    $"  Название: {GetColorName(color)}");

                Console.WriteLine($"✅ Сохранено: {Path.GetFileName(objPath)}");
                Console.WriteLine($"   Цвет: RGB({r},{g},{b})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                throw;
            }
        }

        private void SaveMaterialFile(string mtlPath, int r, int g, int b, float rf, float gf, float bf)
        {
            string colorName = GetColorName(r, g, b);

            string content =
                $"# MTL файл для 3D модели\n" +
                $"# Цвет: {colorName} RGB({r},{g},{b})\n" +
                $"\n" +
                $"newmtl Material_{r}_{g}_{b}\n" +
                $"  Ka {rf:F3} {gf:F3} {bf:F3}\n" +
                $"  Kd {rf:F3} {gf:F3} {bf:F3}\n" +
                $"  Ks 0.5 0.5 0.5\n" +
                $"  Ns 50.0\n" +
                $"  d 1.0\n" +
                $"  illum 2\n";

            File.WriteAllText(mtlPath, content);
            Console.WriteLine($"   MTL: Kd {rf:F3} {gf:F3} {bf:F3}");
        }

        private string GetColorName(int r, int g, int b)
        {
            if (r == 255 && g == 0 && b == 0) return "Red";
            if (r == 0 && g == 0 && b == 255) return "Blue";
            if (r == 0 && g == 255 && b == 0) return "Green";
            if (r == 255 && g == 255 && b == 0) return "Yellow";
            if (r == 0 && g == 255 && b == 255) return "Cyan";
            if (r == 255 && g == 255 && b == 255) return "White";
            if (r == 0 && g == 0 && b == 0) return "Black";
            if (r == 128 && g == 128 && b == 128) return "Gray";
            return "Custom";
        }

        private string GetColorName(Color color)
        {
            return GetColorName(color.R, color.G, color.B);
        }
    }
}