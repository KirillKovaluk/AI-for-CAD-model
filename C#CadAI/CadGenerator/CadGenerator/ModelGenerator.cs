using g3;
using System;

namespace CadGenerator
{
    public class ModelGenerator
    {
        public DMesh3 GenerateModel(ShapeParameters parameters)
        {
            switch (parameters.ShapeType)
            {
                case "cube":
                    return GenerateCube(parameters.Size1, parameters.Size2, parameters.Size3);
                case "sphere":
                    return GenerateSphere(parameters.Size1);
                case "cylinder":
                    return GenerateCylinder(parameters.Size1, parameters.Size2);
                default:
                    return GenerateCube(10, 10, 10);
            }
        }

        private DMesh3 GenerateCube(double width, double height, double depth)
        {
            // Используем TrivialBox3Generator для создания куба
            var boxGen = new TrivialBox3Generator();

            // Создаем Box3d от двух углов
            boxGen.Box = new Box3d(
                new Vector3d(-width / 2, -height / 2, -depth / 2),  // Min угол
                new Vector3d(width / 2, height / 2, depth / 2)      // Max угол
            );

            boxGen.Generate();
            return boxGen.MakeDMesh();
        }

        private DMesh3 GenerateSphere(double radius)
        {
            // Используем Sphere3Generator_NormalizedCube для сферы
            var sphereGen = new Sphere3Generator_NormalizedCube();
            sphereGen.Radius = radius;
            sphereGen.Generate();

            return sphereGen.MakeDMesh();
        }

        private DMesh3 GenerateCylinder(double radius, double height)
        {
            // Создаем цилиндр вручную (это самый надежный способ)
            return CreateSimpleCylinder(radius, height);
        }

        // Ручное создание цилиндра
        private DMesh3 CreateSimpleCylinder(double radius, double height, int segments = 32)
        {
            var mesh = new DMesh3();

            // Массивы для хранения индексов вершин
            int[] bottomRing = new int[segments];
            int[] topRing = new int[segments];

            // Создаем вершины по кругу
            for (int i = 0; i < segments; i++)
            {
                double angle = (2 * Math.PI * i) / segments;
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);

                // Нижняя вершина
                bottomRing[i] = mesh.AppendVertex(new Vector3d(x, y, -height / 2));
                // Верхняя вершина
                topRing[i] = mesh.AppendVertex(new Vector3d(x, y, height / 2));
            }

            // Создаем треугольники для боковой поверхности
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;

                // Первый треугольник (нижний)
                mesh.AppendTriangle(bottomRing[i], bottomRing[next], topRing[i]);
                // Второй треугольник (верхний)
                mesh.AppendTriangle(bottomRing[next], topRing[next], topRing[i]);
            }

            return mesh;
        }
    }
}