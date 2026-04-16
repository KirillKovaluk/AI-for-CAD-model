using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CadGeneratorWinForms
{
    public class ShapeParameters
    {
        public string ShapeType { get; set; }
        public double Size1 { get; set; }
        public double Size2 { get; set; }
        public double Size3 { get; set; }
        public Color Color { get; set; } = Color.Gray;
        public string ColorName { get; set; } = "серый";
        public List<Color> Colors { get; set; } // Для разноцветных объектов
    }

    public class TextParser
    {
        public ShapeParameters Parse(string description, Color selectedColor = default)
        {
            var parameters = new ShapeParameters();
            string originalText = description;
            description = description.ToLower().Trim();

            Console.WriteLine($"🔍 Парсинг: {description}");

            // ============ СЛОЖНЫЕ ОБЪЕКТЫ (приоритет выше) ============

            // Кубик Рубика
            if (description.Contains("кубик рубик") || description.Contains("rubik") ||
                description.Contains("куб рубик") || description.Contains("рубик"))
            {
                parameters.ShapeType = "rubik_cube";
                parameters.Size1 = 3; // 3x3
                Console.WriteLine($"   📌 Распознано: КУБИК РУБИКА");
                return parameters;
            }

            // Пирамидка Рубика
            if ((description.Contains("пирамид") && description.Contains("рубик")) ||
                description.Contains("пирамидка") || description.Contains("pyramid"))
            {
                parameters.ShapeType = "rubik_pyramid";
                Console.WriteLine($"   📌 Распознано: ПИРАМИДКА РУБИКА");
                return parameters;
            }

            // Домик
            if (description.Contains("домик") || description.Contains("house") || description.Contains("дом"))
            {
                parameters.ShapeType = "house";
                Console.WriteLine($"   📌 Распознано: ДОМИК");
                return parameters;
            }

            // Звезда
            if (description.Contains("звезд") || description.Contains("star"))
            {
                parameters.ShapeType = "star";
                Console.WriteLine($"   📌 Распознано: ЗВЕЗДА");
                return parameters;
            }

            // Лестница
            if (description.Contains("лестниц") || description.Contains("stairs") || description.Contains("ступен"))
            {
                parameters.ShapeType = "stairs";
                Console.WriteLine($"   📌 Распознано: ЛЕСТНИЦА");
                return parameters;
            }

            // Колонна
            if (description.Contains("колонн") || description.Contains("column") || description.Contains("столб"))
            {
                parameters.ShapeType = "column";
                Console.WriteLine($"   📌 Распознано: КОЛОННА");
                return parameters;
            }

            // ============ ПРОСТЫЕ ФИГУРЫ ============

            // Определяем тип фигуры
            if (description.Contains("куб") || description.Contains("cube") ||
                description.Contains("прямоугольник") || description.Contains("box"))
            {
                parameters.ShapeType = "cube";
                ExtractCubeDimensions(description, parameters);
                Console.WriteLine($"   📌 Распознано: КУБ");
            }
            else if (description.Contains("цилиндр") || description.Contains("cylinder") ||
                     description.Contains("труб") || description.Contains("pipe") ||
                     description.Contains("колонн") || description.Contains("столб"))
            {
                parameters.ShapeType = "cylinder";
                ExtractCylinderDimensions(description, parameters);
                Console.WriteLine($"   📌 Распознано: ЦИЛИНДР");
            }
            else if (description.Contains("сфер") || description.Contains("sphere") ||
                     description.Contains("шар") || description.Contains("ball") ||
                     description.Contains("глобус"))
            {
                parameters.ShapeType = "sphere";
                ExtractSphereDimensions(description, parameters);
                Console.WriteLine($"   📌 Распознано: СФЕРА");
            }
            else
            {
                // Автоопределение по количеству чисел
                var numbers = ExtractAllNumbers(description);
                if (numbers.Count == 1)
                {
                    parameters.ShapeType = "sphere";
                    parameters.Size1 = numbers[0];
                    Console.WriteLine($"   📌 Автоопределение: СФЕРА");
                }
                else if (numbers.Count == 2)
                {
                    parameters.ShapeType = "cylinder";
                    parameters.Size1 = numbers[0];
                    parameters.Size2 = numbers[1];
                    Console.WriteLine($"   📌 Автоопределение: ЦИЛИНДР");
                }
                else
                {
                    parameters.ShapeType = "cube";
                    parameters.Size1 = 10;
                    parameters.Size2 = 10;
                    parameters.Size3 = 10;
                    Console.WriteLine($"   📌 Автоопределение: КУБ");
                }
            }

            // 2. Определяем цвет
            if (selectedColor != Color.Empty && selectedColor != Color.Gray)
            {
                parameters.Color = selectedColor;
                parameters.ColorName = GetColorName(selectedColor);
            }
            else
            {
                DetectColor(description, parameters);
            }

            Console.WriteLine($"   🎨 Цвет: {parameters.ColorName}");
            Console.WriteLine($"   📐 Размеры: {parameters.Size1}, {parameters.Size2}, {parameters.Size3}");

            return parameters;
        }

        private void ExtractCubeDimensions(string text, ShapeParameters parameters)
        {
            var numbers = ExtractAllNumbers(text);

            var match = Regex.Match(text, @"(\d+(?:\.\d+)?)\s*[xх]\s*(\d+(?:\.\d+)?)\s*[xх]\s*(\d+(?:\.\d+)?)");
            if (match.Success)
            {
                parameters.Size1 = double.Parse(match.Groups[1].Value.Replace('.', ','));
                parameters.Size2 = double.Parse(match.Groups[2].Value.Replace('.', ','));
                parameters.Size3 = double.Parse(match.Groups[3].Value.Replace('.', ','));
            }
            else if (numbers.Count >= 3)
            {
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[1];
                parameters.Size3 = numbers[2];
            }
            else if (numbers.Count == 2)
            {
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[1];
                parameters.Size3 = numbers[0];
            }
            else if (numbers.Count == 1)
            {
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[0];
                parameters.Size3 = numbers[0];
            }
            else
            {
                parameters.Size1 = 10;
                parameters.Size2 = 10;
                parameters.Size3 = 10;
            }
        }

        private void ExtractSphereDimensions(string text, ShapeParameters parameters)
        {
            var numbers = ExtractAllNumbers(text);

            if (numbers.Count > 0)
            {
                parameters.Size1 = numbers[0];

                if (text.Contains("диаметр") || text.Contains("диаметром") ||
                    text.Contains("diameter") || text.Contains("ø"))
                {
                    parameters.Size1 = numbers[0] / 2;
                    Console.WriteLine($"   📏 Диаметр {numbers[0]} -> радиус {parameters.Size1}");
                }
            }
            else
            {
                parameters.Size1 = 10;
            }

            parameters.Size2 = 0;
            parameters.Size3 = 0;
        }

        private void ExtractCylinderDimensions(string text, ShapeParameters parameters)
        {
            var numbers = ExtractAllNumbers(text);

            if (numbers.Count >= 2)
            {
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[1];

                if (text.Contains("диаметр") || text.Contains("диаметром") ||
                    text.Contains("diameter") || text.Contains("ø"))
                {
                    parameters.Size1 = numbers[0] / 2;
                    Console.WriteLine($"   📏 Диаметр {numbers[0]} -> радиус {parameters.Size1}");
                }

                if ((text.Contains("высот") && numbers[0] > numbers[1]) ||
                    (text.IndexOf("высот") < text.IndexOf("радиус") && numbers[0] > numbers[1]))
                {
                    double temp = parameters.Size1;
                    parameters.Size1 = parameters.Size2;
                    parameters.Size2 = temp;
                    Console.WriteLine($"   🔄 Перестановка: радиус={parameters.Size1}, высота={parameters.Size2}");
                }
            }
            else if (numbers.Count == 1)
            {
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[0] * 2;
            }
            else
            {
                parameters.Size1 = 5;
                parameters.Size2 = 10;
            }

            parameters.Size3 = 0;
        }

        private void DetectColor(string text, ShapeParameters parameters)
        {
            text = text.ToLower();

            // Основные цвета
            if (text.Contains("красн") || text.Contains("red"))
            { parameters.Color = Color.FromArgb(255, 255, 0, 0); parameters.ColorName = "красный"; return; }

            if (text.Contains("син") && !text.Contains("голуб"))
            { parameters.Color = Color.FromArgb(255, 0, 0, 255); parameters.ColorName = "синий"; return; }

            if (text.Contains("зелен") || text.Contains("green"))
            { parameters.Color = Color.FromArgb(255, 0, 255, 0); parameters.ColorName = "зеленый"; return; }

            if (text.Contains("желт") || text.Contains("yellow"))
            { parameters.Color = Color.FromArgb(255, 255, 255, 0); parameters.ColorName = "желтый"; return; }

            if (text.Contains("голуб") || text.Contains("cyan"))
            { parameters.Color = Color.FromArgb(255, 0, 255, 255); parameters.ColorName = "голубой"; return; }

            if (text.Contains("бел") && !text.Contains("голуб"))
            { parameters.Color = Color.FromArgb(255, 255, 255, 255); parameters.ColorName = "белый"; return; }

            if (text.Contains("черн") || text.Contains("black"))
            { parameters.Color = Color.FromArgb(255, 0, 0, 0); parameters.ColorName = "черный"; return; }

            if (text.Contains("сер") || text.Contains("gray") || text.Contains("grey"))
            { parameters.Color = Color.FromArgb(255, 128, 128, 128); parameters.ColorName = "серый"; return; }

            parameters.Color = Color.FromArgb(255, 128, 128, 128);
            parameters.ColorName = "серый";
        }

        private string GetColorName(Color color)
        {
            if (color.R == 255 && color.G == 0 && color.B == 0) return "красный";
            if (color.R == 0 && color.G == 0 && color.B == 255) return "синий";
            if (color.R == 0 && color.G == 255 && color.B == 0) return "зеленый";
            if (color.R == 255 && color.G == 255 && color.B == 0) return "желтый";
            if (color.R == 0 && color.G == 255 && color.B == 255) return "голубой";
            if (color.R == 255 && color.G == 255 && color.B == 255) return "белый";
            if (color.R == 0 && color.G == 0 && color.B == 0) return "черный";
            if (color.R == 128 && color.G == 128 && color.B == 128) return "серый";
            return "неизвестный";
        }

        private List<double> ExtractAllNumbers(string text)
        {
            var numbers = new List<double>();
            var matches = Regex.Matches(text, @"\d+[\.,]?\d*");

            foreach (Match match in matches)
            {
                string value = match.Value.Replace('.', ',');
                if (double.TryParse(value, out double result))
                {
                    numbers.Add(result);
                }
            }
            return numbers;
        }
    }
}