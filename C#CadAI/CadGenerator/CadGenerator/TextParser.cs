using System;
using System.Text.RegularExpressions;

namespace CadGenerator
{
    public class ShapeParameters
    {
        public string ShapeType { get; set; }
        public double Size1 { get; set; }
        public double Size2 { get; set; }
        public double Size3 { get; set; }
    }

    public class TextParser
    {
        public ShapeParameters Parse(string description)
        {
            var parameters = new ShapeParameters();
            description = description.ToLower();

            // Определяем тип фигуры
            if (description.Contains("куб") || description.Contains("cube"))
            {
                parameters.ShapeType = "cube";
                ExtractCubeDimensions(description, parameters);
            }
            else if (description.Contains("сфер") || description.Contains("sphere"))
            {
                parameters.ShapeType = "sphere";
                ExtractSphereDimensions(description, parameters);
            }
            else if (description.Contains("цилиндр") || description.Contains("cylinder"))
            {
                parameters.ShapeType = "cylinder";
                ExtractCylinderDimensions(description, parameters);
            }
            else
            {
                parameters.ShapeType = "cube"; // По умолчанию
                parameters.Size1 = 10;
                parameters.Size2 = 10;
                parameters.Size3 = 10;
            }

            return parameters;
        }

        private void ExtractCubeDimensions(string text, ShapeParameters parameters)
        {
            // Ищем размеры в формате: 10x10x10 или 10 10 10
            var match = Regex.Match(text, @"(\d+(?:\.\d+)?)\s*[xх]\s*(\d+(?:\.\d+)?)\s*[xх]\s*(\d+(?:\.\d+)?)");
            if (match.Success)
            {
                parameters.Size1 = double.Parse(match.Groups[1].Value);
                parameters.Size2 = double.Parse(match.Groups[2].Value);
                parameters.Size3 = double.Parse(match.Groups[3].Value);
            }
            else
            {
                // Ищем одно число
                match = Regex.Match(text, @"(\d+(?:\.\d+)?)");
                if (match.Success)
                {
                    double size = double.Parse(match.Groups[1].Value);
                    parameters.Size1 = size;
                    parameters.Size2 = size;
                    parameters.Size3 = size;
                }
                else
                {
                    parameters.Size1 = 10;
                    parameters.Size2 = 10;
                    parameters.Size3 = 10;
                }
            }
        }

        private void ExtractSphereDimensions(string text, ShapeParameters parameters)
        {
            var match = Regex.Match(text, @"(\d+(?:\.\d+)?)");
            if (match.Success)
            {
                parameters.Size1 = double.Parse(match.Groups[1].Value); // Радиус
            }
            else
            {
                parameters.Size1 = 10;
            }
        }

        private void ExtractCylinderDimensions(string text, ShapeParameters parameters)
        {
            var matches = Regex.Matches(text, @"(\d+(?:\.\d+)?)");
            if (matches.Count >= 2)
            {
                parameters.Size1 = double.Parse(matches[0].Value); // Радиус
                parameters.Size2 = double.Parse(matches[1].Value); // Высота
            }
            else
            {
                parameters.Size1 = 5; // Радиус
                parameters.Size2 = 10; // Высота
            }
        }
    }
}