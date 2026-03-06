using g3;

namespace CadGenerator
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

                var writer = new StandardMeshWriter();

                var meshes = new List<WriteMesh>();
                meshes.Add(new WriteMesh(mesh));

                var result = writer.Write(filePath, meshes, WriteOptions.Defaults);

                if (result.code == IOCode.Ok)
                {
                    Console.WriteLine($" Модель сохранена в файл: {filePath}");
                    Console.WriteLine($" Размер файла: {new FileInfo(filePath).Length} байт");
                }
                else
                {
                    Console.WriteLine($" Ошибка при сохранении: {result.message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка при сохранении файла: {ex.Message}");
            }
        }
    }
}