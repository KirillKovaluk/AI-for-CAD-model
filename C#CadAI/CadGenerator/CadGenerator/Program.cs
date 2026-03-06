using System.Reflection;

namespace CadGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("\t ГЕНЕРАТОР CAD МОДЕЛЕЙ");
            Console.WriteLine("\t По текстовому описанию");
            Console.WriteLine("=========================================\n");

            CheckAvailableTypes();

            Console.WriteLine("Поддерживаемые команды:");
            Console.WriteLine("\t 1. куб 10x20x30");
            Console.WriteLine("\t 2. сфера радиусом 15");
            Console.WriteLine("\t 3. цилиндр 5 20 (радиус 5, высота 20)");
            Console.WriteLine("\t 4. cube 10 10 10");
            Console.WriteLine("\t 5. sphere radius 10");
            Console.WriteLine("\t 6. cylinder 5 20");
            Console.WriteLine("\t 7. Для выхода введите 'exit'\n");

            var parser = new TextParser();
            var generator = new ModelGenerator();
            var exporter = new StlExporter();

            while (true)
            {
                Console.Write("\n Введите описание модели: ");
                string input = Console.ReadLine();

                if (input?.ToLower() == "exit")
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                try
                {
                    Console.Write(" Анализирую описание... ");
                    var parameters = parser.Parse(input);

                    Console.WriteLine($"   Распознано: {parameters.ShapeType}");
                    Console.WriteLine($"   Параметры: {parameters.Size1} {parameters.Size2} {parameters.Size3}");

                    Console.Write(" Генерирую 3D модель... ");
                    var mesh = generator.GenerateModel(parameters);
                    Console.WriteLine($" (Вершин: {mesh.VertexCount}, Треугольников: {mesh.TriangleCount})");

                    string fileName = $"model_{DateTime.Now:yyyyMMdd_HHmmss}.stl";
                    Console.Write(" Сохраняю в файл... ");
                    exporter.ExportToStl(mesh, fileName);

                    Console.WriteLine(" Модель успешно создана!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Ошибка: {ex.Message}");
                }
            }

            Console.WriteLine("\n Программа завершена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void CheckAvailableTypes()
        {
            try
            {
                Console.WriteLine(" Проверка доступных классов geometry3Sharp...");

                Assembly assembly = Assembly.GetAssembly(typeof(g3.DMesh3));
                if (assembly != null)
                {
                    Console.WriteLine($" Библиотека geometry3Sharp загружена");

                    Type[] types = assembly.GetTypes();

                    bool hasTrivialBox = false;
                    bool hasSphereGen = false;

                    foreach (Type type in types)
                    {
                        if (type.Name == "TrivialBox3Generator")
                            hasTrivialBox = true;
                        if (type.Name == "Sphere3Generator_NormalizedCube")
                            hasSphereGen = true;
                    }

                    Console.WriteLine($"\t TrivialBox3Generator: {(hasTrivialBox ? "yes" : "no")}");
                    Console.WriteLine($"\t Sphere3Generator_NormalizedCube: {(hasSphereGen ? "yes" : "no")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Ошибка при проверке: {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}