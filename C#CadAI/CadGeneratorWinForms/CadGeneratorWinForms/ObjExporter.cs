using g3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CadGeneratorWinForms
{
    public class ObjExporter
    {
        public void ExportToOBJ(DMesh3 mesh, Color color, string filePath)
        {
            try
            {
                // Создаем директорию, если её нет
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Путь к MTL файлу (материалы)
                string mtlPath = Path.ChangeExtension(filePath, "mtl");
                string mtlFileName = Path.GetFileName(mtlPath);

                // Сохраняем MTL файл с цветом
                SaveMaterialFile(mtlPath, color);

                // Сохраняем OBJ файл
                SaveObjFile(mesh, filePath, mtlFileName);

                Console.WriteLine($"✅ OBJ модель с цветом сохранена: {filePath}");
                Console.WriteLine($"   MTL файл: {mtlPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка сохранения OBJ: {ex.Message}");
                throw;
            }
        }

        private void SaveMaterialFile(string mtlPath, Color color)
        {
            // Конвертируем RGB в значения от 0 до 1
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            string materialContent =
                $"# MTL файл для CAD генератора\n" +
                $"# Создан: {DateTime.Now}\n" +
                $"\n" +
                $"newmtl ModelMaterial\n" +
                $"  Ka 0.2 0.2 0.2\n" +           // Ambient reflection (темный)
                $"  Kd {r:F3} {g:F3} {b:F3}\n" +   // Diffuse reflection (основной цвет)
                $"  Ks 0.5 0.5 0.5\n" +            // Specular reflection (блики)
                $"  Ns 50.0\n" +                   // Shininess (блеск)
                $"  d 1.0\n" +                     // Opacity (непрозрачность)
                $"  illum 2\n";                    // Illumination model (2 = diffuse + specular)

            File.WriteAllText(mtlPath, materialContent);
            Console.WriteLine($"   📄 MTL сохранен: {Path.GetFileName(mtlPath)}");
        }

        private void SaveObjFile(DMesh3 mesh, string objPath, string mtlFileName)
        {
            using (StreamWriter writer = new StreamWriter(objPath))
            {
                // Заголовок OBJ файла
                writer.WriteLine($"# OBJ файл для CAD генератора");
                writer.WriteLine($"# Создан: {DateTime.Now}");
                writer.WriteLine($"# Вершин: {mesh.VertexCount}");
                writer.WriteLine($"# Треугольников: {mesh.TriangleCount}");
                writer.WriteLine($"");

                // Ссылка на MTL файл
                writer.WriteLine($"mtllib {mtlFileName}");
                writer.WriteLine($"");

                // Название объекта
                writer.WriteLine($"o CAD_Model");
                writer.WriteLine($"");

                // Используем материал
                writer.WriteLine($"usemtl ModelMaterial");
                writer.WriteLine($"");

                // Сохраняем все вершины (v x y z)
                foreach (int vid in mesh.VertexIndices())
                {
                    Vector3d v = mesh.GetVertex(vid);
                    writer.WriteLine($"v {v.x:F6} {v.y:F6} {v.z:F6}");
                }
                writer.WriteLine($"");

                // Сохраняем текстурные координаты (если есть)
                // Для простоты создаем дефолтные
                writer.WriteLine($"vt 0.0 0.0");
                writer.WriteLine($"vt 1.0 0.0");
                writer.WriteLine($"vt 0.0 1.0");
                writer.WriteLine($"vt 1.0 1.0");
                writer.WriteLine($"");

                // Сохраняем нормали (если есть)
                writer.WriteLine($"vn 0.0 0.0 1.0");
                writer.WriteLine($"vn 0.0 1.0 0.0");
                writer.WriteLine($"vn 1.0 0.0 0.0");
                writer.WriteLine($"");

                // Сохраняем грани (f v/vt/vn)
                // Используем формат: f vertex1/tex1/normal1 vertex2/tex2/normal2 vertex3/tex3/normal3
                int texIndex = 1;
                foreach (int tid in mesh.TriangleIndices())
                {
                    Index3i tri = mesh.GetTriangle(tid);
                    // OBJ индексы начинаются с 1, а не с 0
                    int v1 = tri.a + 1;
                    int v2 = tri.b + 1;
                    int v3 = tri.c + 1;

                    // Простой формат: f v1 v2 v3
                    writer.WriteLine($"f {v1} {v2} {v3}");
                }
            }

            Console.WriteLine($"   📄 OBJ сохранен: {Path.GetFileName(objPath)}");
        }

        // Метод для создания цветной модели с разными цветами для разных частей
        public void ExportToOBJWithColors(DMesh3 mesh, Dictionary<int, Color> vertexColors, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string mtlPath = Path.ChangeExtension(filePath, "mtl");
                string mtlFileName = Path.GetFileName(mtlPath);

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"# OBJ файл с цветами вершин");
                    writer.WriteLine($"# Создан: {DateTime.Now}");
                    writer.WriteLine($"mtllib {mtlFileName}");
                    writer.WriteLine($"o CAD_Model_Colored");

                    // Сохраняем вершины
                    foreach (int vid in mesh.VertexIndices())
                    {
                        Vector3d v = mesh.GetVertex(vid);
                        writer.WriteLine($"v {v.x:F6} {v.y:F6} {v.z:F6}");
                    }
                    writer.WriteLine($"");

                    // Сохраняем грани (простой формат)
                    foreach (int tid in mesh.TriangleIndices())
                    {
                        Index3i tri = mesh.GetTriangle(tid);
                        writer.WriteLine($"f {tri.a + 1} {tri.b + 1} {tri.c + 1}");
                    }
                }

                // Создаем MTL файл с основным цветом
                SaveMaterialFile(mtlPath, vertexColors.Count > 0 ?
                    vertexColors[0] : Color.Gray);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                throw;
            }
        }
    }
}