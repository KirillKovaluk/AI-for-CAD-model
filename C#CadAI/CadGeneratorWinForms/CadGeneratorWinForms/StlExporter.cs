using g3;
using System;
using System.Collections.Generic;
using System.IO;

namespace CadGeneratorWinForms
{
    public class StlExporter
    {
        public void ExportToStl(DMesh3 mesh, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Для STL файлов цвет не сохраняется (формат не поддерживает цвет)
                // Но мы можем сохранить информацию о цвете в отдельном файле
                SaveColorInfo(mesh, filePath + ".color.txt");

                var writer = new StandardMeshWriter();
                var meshes = new List<WriteMesh> { new WriteMesh(mesh) };

                var result = writer.Write(filePath, meshes, WriteOptions.Defaults);

                if (result.code == IOCode.Ok)
                {
                    Console.WriteLine($"✅ Модель сохранена: {filePath}");
                }
                else
                {
                    throw new Exception(result.message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                throw;
            }
        }

        private void SaveColorInfo(DMesh3 mesh, string colorFilePath)
        {
            try
            {
                if (mesh.HasVertexColors)
                {
                    using (StreamWriter writer = new StreamWriter(colorFilePath))
                    {
                        writer.WriteLine("# Информация о цветах модели");
                        writer.WriteLine($"# Вершин с цветом: {mesh.VertexCount}");

                        // Берем цвет первой вершины как основной
                        if (mesh.VertexCount > 0)
                        {
                            var color = mesh.GetVertexColor(0);
                            writer.WriteLine($"R: {color.x * 255:F0}");
                            writer.WriteLine($"G: {color.y * 255:F0}");
                            writer.WriteLine($"B: {color.z * 255:F0}");
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки сохранения цвета
            }
        }
    }
}