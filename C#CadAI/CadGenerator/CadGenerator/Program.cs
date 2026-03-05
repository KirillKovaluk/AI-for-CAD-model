using System;
using System.Reflection;

namespace CadGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("   ГЕНЕРАТОР CAD МОДЕЛЕЙ");
            Console.WriteLine("   По текстовому описанию");
            Console.WriteLine("=========================================\n");

            // Проверка доступных типов
            CheckAvailableTypes();

            Console.WriteLine("📝 Поддерживаемые команды:");
            Console.WriteLine("  • куб 10x20x30");
            Console.WriteLine("  • сфера радиусом 15");
            Console.WriteLine("  • цилиндр 5 20 (радиус 5, высота 20)");
            Console.WriteLine("  • cube 10 10 10");
            Console.WriteLine("  • sphere radius 10");
            Console.WriteLine("  • cylinder 5 20");
            Console.WriteLine("\n❌ Для выхода введите 'exit'\n");

            var parser = new TextParser();
            var generator = new ModelGenerator();
            var exporter = new StlExporter();

            while (true)
            {
                Console.Write("\n🔤 Введите описание модели: ");
                string input = Console.ReadLine();

                if (input?.ToLower() == "exit")
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                try
                {
                    // Шаг 1: Парсинг текста
                    Console.Write("🔍 Анализирую описание... ");
                    var parameters = parser.Parse(input);
                    Console.WriteLine("✅");

                    Console.WriteLine($"   📌 Распознано: {parameters.ShapeType}");
                    Console.WriteLine($"   📐 Параметры: {parameters.Size1} {parameters.Size2} {parameters.Size3}");

                    // Шаг 2: Генерация модели
                    Console.Write("⚙️ Генерирую 3D модель... ");
                    var mesh = generator.GenerateModel(parameters);
                    Console.WriteLine($"✅ (Вершин: {mesh.VertexCount}, Треугольников: {mesh.TriangleCount})");

                    // Шаг 3: Сохранение в файл
                    string fileName = $"model_{DateTime.Now:yyyyMMdd_HHmmss}.stl";
                    Console.Write("💾 Сохраняю в файл... ");
                    exporter.ExportToStl(mesh, fileName);

                    Console.WriteLine("🎉 Модель успешно создана!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Ошибка: {ex.Message}");
                }
            }

            Console.WriteLine("\n👋 Программа завершена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void CheckAvailableTypes()
        {
            try
            {
                Console.WriteLine("🔍 Проверка доступных классов geometry3Sharp...");

                Assembly assembly = Assembly.GetAssembly(typeof(g3.DMesh3));
                if (assembly != null)
                {
                    Console.WriteLine($"✅ Библиотека geometry3Sharp загружена");

                    // Проверяем наличие нужных классов
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

                    Console.WriteLine($"   TrivialBox3Generator: {(hasTrivialBox ? "✅" : "❌")}");
                    Console.WriteLine($"   Sphere3Generator_NormalizedCube: {(hasSphereGen ? "✅" : "❌")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при проверке: {ex.Message}");
            }

            Console.WriteLine();
        }
    }
}