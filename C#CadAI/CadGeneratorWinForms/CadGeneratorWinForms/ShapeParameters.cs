using System.Drawing;

namespace CadGenerator
{
    public class ShapeParameters
    {
        public string ShapeType { get; set; }
        public double Size1 { get; set; }
        public double Size2 { get; set; }
        public double Size3 { get; set; }
        public Color Color { get; set; } = Color.Gray; // Цвет по умолчанию
        public string ColorName { get; set; } = "серый";
    }
}