using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CadGenerator
{
    public class TextParser
    {
        // Словарь цветов на русском и английском
        private readonly Dictionary<string, Color> _colorMap = new Dictionary<string, Color>
        {
            // Русские названия
            {"красн", Color.Red},
            {"алый", Color.Red},
            {"синий", Color.Blue},
            {"голубой", Color.LightBlue},
            {"зелен", Color.Green},
            {"салатов", Color.LightGreen},
            {"желт", Color.Yellow},
            {"оранж", Color.Orange},
            {"фиолетов", Color.Purple},
            {"розов", Color.Pink},
            {"черн", Color.Black},
            {"бел", Color.White},
            {"сер", Color.Gray},
            {"коричнев", Color.Brown},
            {"золот", Color.Gold},
            {"серебр", Color.Silver},
            {"бронз", Color.FromArgb(205, 127, 50)},
            
            // Английские названия
            {"red", Color.Red},
            {"blue", Color.Blue},
            {"green", Color.Green},
            {"yellow", Color.Yellow},
            {"orange", Color.Orange},
            {"purple", Color.Purple},
            {"pink", Color.Pink},
            {"black", Color.Black},
            {"white", Color.White},
            {"gray", Color.Gray},
            {"grey", Color.Gray},
            {"brown", Color.Brown},
            {"gold", Color.Gold},
            {"silver", Color.Silver},
            {"bronze", Color.FromArgb(205, 127, 50)}
        };

        public ShapeParameters Parse(string description)
        {
            var parameters = new ShapeParameters();
            string originalText = description;
            description = description.ToLower().Trim();

            Console.WriteLine($"🔍 Парсинг: {description}");

            // 1. Ищем цвет в описании
            DetectColor(description, parameters);

            // 2. Определяем тип фигуры
            if (description.Contains("куб") ||
                description.Contains("cube") ||
                description.Contains("прямоугольник"))
            {
                parameters.ShapeType = "cube";
                ExtractCubeDimensions(description, parameters);
            }
            else if (description.Contains("сфер") ||
                     description.Contains("sphere") ||
                     description.Contains("шар") ||
                     description.Contains("ball"))
            {
                parameters.ShapeType = "sphere";
                ExtractSphereDimensions(description, parameters);
            }
            else if (description.Contains("цилиндр") ||
                     description.Contains("cylinder") ||
                     description.Contains("труб") ||
                     description.Contains("pipe"))
            {
                parameters.ShapeType = "cylinder";
                ExtractCylinderDimensions(description, parameters);
            }
            else
            {
                // Если не распознали - пытаемся угадать
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

            Console.WriteLine($"   🎨 Цвет: {parameters.ColorName}");
            return parameters;
        }

        private void DetectColor(string text, ShapeParameters parameters)
        {
            foreach (var colorPair in _colorMap)
            {
                if (text.Contains(colorPair.Key))
                {
                    parameters.Color = colorPair.Value;
                    parameters.ColorName = GetColorName(colorPair.Value);

                    // Удаляем цвет из текста, чтобы он не мешал парсингу размеров
                    text = text.Replace(colorPair.Key, "");
                    return;
                }
            }

            // Если цвет не найден, оставляем серый по умолчанию
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
                parameters.Size1 = numbers[0] / 2;
                parameters.Size2 = numbers[0];
                Console.WriteLine($"   📏 Только одно число: радиус={parameters.Size1}, высота={parameters.Size2}");
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