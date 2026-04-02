using CadGenerator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CadGeneratorWinForms
{
    public class TextParser
    {
        // Существующий метод (без цвета)
        public ShapeParameters Parse(string description)
        {
            var parameters = new ShapeParameters();
            description = description.ToLower().Trim();

            Console.WriteLine($"🔍 Парсинг: {description}");

            // Определяем тип фигуры
            if (description.Contains("куб") || description.Contains("cube") || description.Contains("прямоугольник"))
            {
                parameters.ShapeType = "cube";
                ExtractCubeDimensions(description, parameters);
            }
            else if (description.Contains("сфер") || description.Contains("sphere") ||
                     description.Contains("шар") || description.Contains("ball"))
            {
                parameters.ShapeType = "sphere";
                ExtractSphereDimensions(description, parameters);
            }
            else if (description.Contains("цилиндр") || description.Contains("cylinder") ||
                     description.Contains("труб") || description.Contains("pipe"))
            {
                parameters.ShapeType = "cylinder";
                ExtractCylinderDimensions(description, parameters);
            }
            else
            {
                var numbers = ExtractAllNumbers(description);
                if (numbers.Count == 1)
                {
                    parameters.ShapeType = "sphere";
                    parameters.Size1 = numbers[0];
                }
                else if (numbers.Count >= 3)
                {
                    parameters.ShapeType = "cube";
                    parameters.Size1 = numbers[0];
                    parameters.Size2 = numbers[1];
                    parameters.Size3 = numbers[2];
                }
                else
                {
                    parameters.ShapeType = "cube";
                    parameters.Size1 = 10;
                    parameters.Size2 = 10;
                    parameters.Size3 = 10;
                }
            }

            // Пробуем найти цвет в тексте
            DetectColor(description, parameters);

            return parameters;
        }

        // НОВЫЙ МЕТОД с поддержкой цвета из ComboBox
        public ShapeParameters Parse(string description, Color selectedColor)
        {
            // Сначала парсим как обычно
            var parameters = Parse(description);

            // Если передан конкретный цвет, используем его (приоритет выше, чем цвет из текста)
            if (selectedColor != Color.Empty)
            {
                parameters.Color = selectedColor;
                parameters.ColorName = GetColorName(selectedColor);
            }

            return parameters;
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

        // ... остальные методы (ExtractCubeDimensions, ExtractSphereDimensions, 
        // ExtractCylinderDimensions, ExtractAllNumbers) остаются без изменений ...

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
                }
            }
            else
            {
                parameters.Size1 = 10;
            }
        }

        private void ExtractCylinderDimensions(string text, ShapeParameters parameters)
        {
            var numbers = ExtractAllNumbers(text);

            if (numbers.Count >= 2)
            {
                parameters.Size1 = numbers[0];
                parameters.Size2 = numbers[1];

                if (text.Contains("диаметр") || text.Contains("diameter") || text.Contains("ø"))
                {
                    parameters.Size1 = numbers[0] / 2;
                }

                if ((text.Contains("высот") && numbers[0] > numbers[1]) ||
                    (text.IndexOf("высот") < text.IndexOf("радиус") && numbers[0] > numbers[1]))
                {
                    double temp = parameters.Size1;
                    parameters.Size1 = parameters.Size2;
                    parameters.Size2 = temp;
                }
            }
            else if (numbers.Count == 1)
            {
                parameters.Size1 = numbers[0] / 2;
                parameters.Size2 = numbers[0];
            }
            else
            {
                parameters.Size1 = 5;
                parameters.Size2 = 10;
            }
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