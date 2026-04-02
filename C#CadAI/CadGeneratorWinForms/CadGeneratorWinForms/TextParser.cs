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
    }

    public class TextParser
    {
        public ShapeParameters Parse(string description, Color selectedColor = default)
        {
            var parameters = new ShapeParameters();
            string originalText = description;
            description = description.ToLower().Trim();

            Console.WriteLine($"🔍 Парсинг: {description}");

            // 1. Определяем тип фигуры (с расширенными ключевыми словами)
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
                    Console.WriteLine($"   📌 Автоопределение: СФЕРА (1 число)");
                }
                else if (numbers.Count == 2)
                {
                    parameters.ShapeType = "cylinder";
                    parameters.Size1 = numbers[0];
                    parameters.Size2 = numbers[1];
                    Console.WriteLine($"   📌 Автоопределение: ЦИЛИНДР (2 числа)");
                }
                else
                {
                    parameters.ShapeType = "cube";
                    parameters.Size1 = 10;
                    parameters.Size2 = 10;
                    parameters.Size3 = 10;
                    Console.WriteLine($"   📌 Автоопределение: КУБ (по умолчанию)");
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

            // Ищем формат XxYxZ
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

                // Если указан диаметр, делим на 2
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
                // Первое число - радиус, второе - высота
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[1];

                // Если указан диаметр, корректируем
                if (text.Contains("диаметр") || text.Contains("диаметром") ||
                    text.Contains("diameter") || text.Contains("ø"))
                {
                    parameters.Size1 = numbers[0] / 2;
                    Console.WriteLine($"   📏 Диаметр {numbers[0]} -> радиус {parameters.Size1}");
                }

                // Если порядок перепутан (высота указана первой)
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
                // Если одно число - считаем что это радиус, высота = радиус * 2
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[0] * 2;
                Console.WriteLine($"   📏 Одно число: радиус={parameters.Size1}, высота={parameters.Size2}");
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
            var colorMap = new Dictionary<string, Color>
            {
                {"красн", Color.Red}, {"алый", Color.Red},
                {"син", Color.Blue}, {"голуб", Color.LightBlue},
                {"зелен", Color.Green}, {"салатов", Color.LightGreen},
                {"желт", Color.Yellow}, {"оранж", Color.Orange},
                {"фиолетов", Color.Purple}, {"розов", Color.Pink},
                {"черн", Color.Black}, {"бел", Color.White},
                {"сер", Color.Gray}, {"коричнев", Color.Brown},
                {"золот", Color.Gold}, {"серебр", Color.Silver},
                {"red", Color.Red}, {"blue", Color.Blue}, {"green", Color.Green},
                {"yellow", Color.Yellow}, {"orange", Color.Orange}, {"purple", Color.Purple},
                {"pink", Color.Pink}, {"black", Color.Black}, {"white", Color.White},
                {"gray", Color.Gray}, {"grey", Color.Gray}, {"brown", Color.Brown}
            };

            foreach (var colorPair in colorMap)
            {
                if (text.Contains(colorPair.Key))
                {
                    parameters.Color = colorPair.Value;
                    parameters.ColorName = GetColorName(colorPair.Value);
                    return;
                }
            }

            parameters.Color = Color.Gray;
            parameters.ColorName = "серый";
        }

        private string GetColorName(Color color)
        {
            if (color == Color.Red) return "красный";
            if (color == Color.Blue) return "синий";
            if (color == Color.Green) return "зеленый";
            if (color == Color.Yellow) return "желтый";
            if (color == Color.Orange) return "оранжевый";
            if (color == Color.Purple) return "фиолетовый";
            if (color == Color.Pink) return "розовый";
            if (color == Color.Black) return "черный";
            if (color == Color.White) return "белый";
            if (color == Color.Gray) return "серый";
            if (color == Color.Brown) return "коричневый";
            if (color == Color.Gold) return "золотой";
            if (color == Color.Silver) return "серебряный";
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