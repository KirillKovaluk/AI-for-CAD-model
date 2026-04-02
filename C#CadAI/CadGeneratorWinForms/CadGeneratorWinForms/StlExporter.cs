using g3;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace CadGeneratorWinForms
{
    public class StlExporter
    {
        private ObjExporter _objExporter = new ObjExporter();

        // STL экспорт (без цвета)
        public void ExportToStl(DMesh3 mesh, string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var writer = new StandardMeshWriter();
                var meshes = new List<WriteMesh> { new WriteMesh(mesh) };

                var result = writer.Write(filePath, meshes, WriteOptions.Defaults);

                if (result.code == IOCode.Ok)
                {
                    Console.WriteLine($"✅ STL модель сохранена: {filePath}");
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

        // OBJ экспорт с цветом
        public void ExportToObj(DMesh3 mesh, Color color, string filePath)
        {
            _objExporter.ExportToOBJ(mesh, color, filePath);
        }

        // Автоматический выбор формата по расширению
        public void Export(DMesh3 mesh, Color color, string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            if (extension == ".obj")
            {
                ExportToObj(mesh, color, filePath);
            }
            else if (extension == ".stl")
            {
                ExportToStl(mesh, filePath);
            }
            else
            {
                // По умолчанию OBJ с цветом
                ExportToObj(mesh, color, filePath + ".obj");
            }
        }
    }
}