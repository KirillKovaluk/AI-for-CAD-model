using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using CadGenerator;
using g3;

namespace CadGeneratorWinForms
{
    // Типы инженерных команд
    public enum CommandType
    {
        CreateShape,    // Создать базовую фигуру
        BooleanUnion,   // Объединение
        BooleanSubtract, // Вычитание
        BooleanIntersect, // Пересечение
    }

    // Инженерная команда
    public class EngineeringCommand
    {
        public CommandType Type { get; set; }
        public string ShapeType { get; set; }
        public double Param1 { get; set; }
        public double Param2 { get; set; }
        public double Param3 { get; set; }
        public Color Color { get; set; }
        public string Description { get; set; }
        public EngineeringCommand SecondObject { get; set; } // Для бинарных операций
    }

    // Результат перевода - последовательность команд
    public class CommandSequence
    {
        public List<EngineeringCommand> Commands { get; set; } = new List<EngineeringCommand>();
        public string OriginalText { get; set; }
        public bool IsValid => Commands.Count > 0;

        public void Add(EngineeringCommand cmd) => Commands.Add(cmd);

        public override string ToString()
        {
            return string.Join("\n  ", Commands.Select(c => c.ToString()));
        }
    }

    // Транслятор текста в инженерные команды
    public class CommandTranslator
    {
        private TextParser _textParser = new TextParser();
        private ModelGenerator _generator = new ModelGenerator();

        // Основной метод перевода
        public CommandSequence Translate(string text)
        {
            var sequence = new CommandSequence();
            sequence.OriginalText = text;
            text = text.ToLower().Trim();

            // Проверяем на логические операции
            if (text.Contains("объедин") || text.Contains("union") ||
                text.Contains("скле") || text.Contains("соедин") ||
                text.Contains("внутри") || text.Contains("внутр") ||
                text.Contains("в котором") || text.Contains("содержит"))
            {
                TranslateCombinedShape(text, sequence);
            }
            else
            {
                // Простая фигура
                TranslateSimpleShape(text, sequence);
            }

            return sequence;
        }

        // Простая фигура
        private void TranslateSimpleShape(string text, CommandSequence sequence)
        {
            var parameters = _textParser.Parse(text);

            var cmd = new EngineeringCommand
            {
                Type = CommandType.CreateShape,
                ShapeType = parameters.ShapeType,
                Param1 = parameters.Size1,
                Param2 = parameters.Size2,
                Param3 = parameters.Size3,
                Color = parameters.Color,
                Description = $"Создать {parameters.ShapeType} размерами {parameters.Size1}x{parameters.Size2}x{parameters.Size3}"
            };
            sequence.Add(cmd);
        }

        // Объединенная фигура (куб с цилиндром внутри)
        private void TranslateCombinedShape(string text, CommandSequence sequence)
        {
            // Извлекаем размеры куба
            var cubeMatch = Regex.Match(text, @"куб[а-я]*\s*(\d+\.?\d*)(?:\s*[xх]\s*(\d+\.?\d*))?(?:\s*[xх]\s*(\d+\.?\d*))?");

            double cubeW = 10, cubeH = 10, cubeD = 10;
            if (cubeMatch.Success)
            {
                cubeW = double.Parse(cubeMatch.Groups[1].Value);
                cubeH = cubeMatch.Groups[2].Success ? double.Parse(cubeMatch.Groups[2].Value) : cubeW;
                cubeD = cubeMatch.Groups[3].Success ? double.Parse(cubeMatch.Groups[3].Value) : cubeW;
            }

            // Извлекаем параметры цилиндра
            var cylinderMatch = Regex.Match(text, @"цилиндр[а-я]*\s*(\d+\.?\d*)\s*(\d+\.?\d*)");
            double cylinderRadius = 5, cylinderHeight = 10;

            if (cylinderMatch.Success)
            {
                cylinderRadius = double.Parse(cylinderMatch.Groups[1].Value);
                cylinderHeight = double.Parse(cylinderMatch.Groups[2].Value);
            }
            else
            {
                // Альтернативный формат: "радиусом X высотой Y"
                var radiusMatch = Regex.Match(text, @"радиус[а-я]*\s*(\d+\.?\d*)");
                var heightMatch = Regex.Match(text, @"высот[а-я]*\s*(\d+\.?\d*)");
                if (radiusMatch.Success) cylinderRadius = double.Parse(radiusMatch.Groups[1].Value);
                if (heightMatch.Success) cylinderHeight = double.Parse(heightMatch.Groups[1].Value);
            }

            // Создаем команду для куба
            var cubeCmd = new EngineeringCommand
            {
                Type = CommandType.CreateShape,
                ShapeType = "cube",
                Param1 = cubeW,
                Param2 = cubeH,
                Param3 = cubeD,
                Description = $"Создать куб {cubeW}x{cubeH}x{cubeD}"
            };

            // Создаем команду для цилиндра
            var cylinderCmd = new EngineeringCommand
            {
                Type = CommandType.CreateShape,
                ShapeType = "cylinder",
                Param1 = cylinderRadius,
                Param2 = cylinderHeight,
                Description = $"Создать цилиндр (радиус {cylinderRadius}, высота {cylinderHeight})"
            };

            // Добавляем команду объединения
            var unionCmd = new EngineeringCommand
            {
                Type = CommandType.BooleanUnion,
                Description = $"Объединить куб с цилиндром (цилиндр внутри куба)",
                SecondObject = cylinderCmd
            };

            sequence.Add(cubeCmd);
            sequence.Add(unionCmd);
        }

        // Выполнение последовательности команд (генерация модели)
        public DMesh3 ExecuteCommands(CommandSequence sequence)
        {
            if (!sequence.IsValid)
                return null;

            DMesh3 result = null;
            EngineeringCommand pendingUnion = null;

            foreach (var cmd in sequence.Commands)
            {
                switch (cmd.Type)
                {
                    case CommandType.CreateShape:
                        var shape = CreateShape(cmd);
                        if (result == null)
                        {
                            result = shape;
                        }
                        else if (pendingUnion != null)
                        {
                            // Если есть ожидающая операция объединения
                            result = CombineShapes(result, shape, pendingUnion);
                            pendingUnion = null;
                        }
                        break;

                    case CommandType.BooleanUnion:
                        // Запоминаем, что следующий объект нужно объединить
                        pendingUnion = cmd;
                        break;
                }
            }

            return result;
        }

        private DMesh3 CreateShape(EngineeringCommand cmd)
        {
            var parameters = new ShapeParameters
            {
                ShapeType = cmd.ShapeType,
                Size1 = cmd.Param1,
                Size2 = cmd.Param2,
                Size3 = cmd.Param3,
                Color = cmd.Color
            };
            return _generator.GenerateModel(parameters);
        }

        private DMesh3 CombineShapes(DMesh3 baseShape, DMesh3 addShape, EngineeringCommand operation)
        {
            Console.WriteLine($"🔧 Объединение фигур...");

            try
            {
                // Создаем объединенную сетку
                var combinedMesh = new DMesh3();

                // Копируем все вершины из первой фигуры
                foreach (int vid in baseShape.VertexIndices())
                {
                    combinedMesh.AppendVertex(baseShape.GetVertex(vid));
                }

                int baseVertexCount = baseShape.VertexCount;

                // Копируем все вершины из второй фигуры со смещением (цилиндр по центру куба)
                foreach (int vid in addShape.VertexIndices())
                {
                    var v = addShape.GetVertex(vid);
                    combinedMesh.AppendVertex(v);
                }

                // Копируем треугольники из первой фигуры
                foreach (int tid in baseShape.TriangleIndices())
                {
                    var tri = baseShape.GetTriangle(tid);
                    combinedMesh.AppendTriangle(tri.a, tri.b, tri.c);
                }

                // Копируем треугольники из второй фигуры со смещением индексов
                foreach (int tid in addShape.TriangleIndices())
                {
                    var tri = addShape.GetTriangle(tid);
                    combinedMesh.AppendTriangle(tri.a + baseVertexCount,
                                                tri.b + baseVertexCount,
                                                tri.c + baseVertexCount);
                }

                Console.WriteLine($"   ✅ Объединение выполнено. Вершин: {combinedMesh.VertexCount}, Треугольников: {combinedMesh.TriangleCount}");
                return combinedMesh;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Ошибка объединения: {ex.Message}");
                return baseShape;
            }
        }
    }
}