using g3;
using System;
using System.Collections.Generic;

namespace CadGeneratorWinForms
{
    public class ModelGenerator
    {
        public DMesh3 GenerateModel(ShapeParameters parameters)
        {
            Console.WriteLine($"🎯 Генерирую {parameters.ShapeType}...");

            switch (parameters.ShapeType?.ToLower())
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

        // ============ КУБ ============
        private DMesh3 GenerateCube(double width, double height, double depth)
        {
            var mesh = new DMesh3();

            // 8 вершин куба (векторно)
            Vector3d[] vertices = new Vector3d[8];
            vertices[0] = new Vector3d(-width / 2, -height / 2, -depth / 2);
            vertices[1] = new Vector3d(width / 2, -height / 2, -depth / 2);
            vertices[2] = new Vector3d(width / 2, height / 2, -depth / 2);
            vertices[3] = new Vector3d(-width / 2, height / 2, -depth / 2);
            vertices[4] = new Vector3d(-width / 2, -height / 2, depth / 2);
            vertices[5] = new Vector3d(width / 2, -height / 2, depth / 2);
            vertices[6] = new Vector3d(width / 2, height / 2, depth / 2);
            vertices[7] = new Vector3d(-width / 2, height / 2, depth / 2);

            // Добавляем вершины в mesh
            int[] v = new int[8];
            for (int i = 0; i < 8; i++)
                v[i] = mesh.AppendVertex(vertices[i]);

            // 12 треугольников (по 2 на каждую грань)
            // Передняя грань (Z = -depth/2)
            mesh.AppendTriangle(v[0], v[1], v[2]);
            mesh.AppendTriangle(v[0], v[2], v[3]);

            // Задняя грань (Z = depth/2)
            mesh.AppendTriangle(v[4], v[6], v[5]);
            mesh.AppendTriangle(v[4], v[7], v[6]);

            // Левая грань (X = -width/2)
            mesh.AppendTriangle(v[0], v[3], v[7]);
            mesh.AppendTriangle(v[0], v[7], v[4]);

            // Правая грань (X = width/2)
            mesh.AppendTriangle(v[1], v[5], v[6]);
            mesh.AppendTriangle(v[1], v[6], v[2]);

            // Нижняя грань (Y = -height/2)
            mesh.AppendTriangle(v[0], v[4], v[5]);
            mesh.AppendTriangle(v[0], v[5], v[1]);

            // Верхняя грань (Y = height/2)
            mesh.AppendTriangle(v[3], v[2], v[6]);
            mesh.AppendTriangle(v[3], v[6], v[7]);

            Console.WriteLine($"   ✅ Куб: {width}x{height}x{depth}");
            return mesh;
        }

        // ============ СФЕРА (векторная формула) ============
        private DMesh3 GenerateSphere(double radius, int latitudeSegments = 32, int longitudeSegments = 64)
        {
            var mesh = new DMesh3();

            // Создаем вершины с использованием сферических координат
            List<int>[,] vertexIndices = new List<int>[latitudeSegments + 1, longitudeSegments];

            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                double theta = Math.PI * lat / latitudeSegments;  // угол от полюса до полюса (0 до PI)
                double sinTheta = Math.Sin(theta);
                double cosTheta = Math.Cos(theta);

                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    double phi = 2 * Math.PI * lon / longitudeSegments;  // угол вокруг оси (0 до 2PI)
                    double sinPhi = Math.Sin(phi);
                    double cosPhi = Math.Cos(phi);

                    // Векторная формула сферы: x = r * sin(theta) * cos(phi)
                    //                           y = r * cos(theta)
                    //                           z = r * sin(theta) * sin(phi)
                    double x = radius * sinTheta * cosPhi;
                    double y = radius * cosTheta;
                    double z = radius * sinTheta * sinPhi;

                    int vertexIndex = mesh.AppendVertex(new Vector3d(x, y, z));

                    if (vertexIndices[lat, lon] == null)
                        vertexIndices[lat, lon] = new List<int>();
                    vertexIndices[lat, lon].Add(vertexIndex);
                }
            }

            // Создаем треугольники
            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    int nextLon = (lon + 1) % longitudeSegments;

                    int v1 = vertexIndices[lat, lon][0];
                    int v2 = vertexIndices[lat, nextLon][0];
                    int v3 = vertexIndices[lat + 1, nextLon][0];
                    int v4 = vertexIndices[lat + 1, lon][0];

                    // Верхний треугольник
                    mesh.AppendTriangle(v1, v2, v3);
                    // Нижний треугольник
                    mesh.AppendTriangle(v1, v3, v4);
                }
            }

            Console.WriteLine($"   ✅ Сфера: радиус={radius}, сегментов={latitudeSegments}x{longitudeSegments}");
            return mesh;
        }

        // ============ ЦИЛИНДР (векторная формула) ============
        private DMesh3 GenerateCylinder(double radius, double height, int radialSegments = 64, int heightSegments = 32)
        {
            var mesh = new DMesh3();

            // Создаем вершины
            List<int>[,] vertices = new List<int>[heightSegments + 1, radialSegments];

            for (int h = 0; h <= heightSegments; h++)
            {
                double y = -height / 2 + (height * h / heightSegments);

                for (int r = 0; r < radialSegments; r++)
                {
                    double angle = 2 * Math.PI * r / radialSegments;
                    double x = radius * Math.Cos(angle);
                    double z = radius * Math.Sin(angle);

                    int vertexIndex = mesh.AppendVertex(new Vector3d(x, y, z));

                    if (vertices[h, r] == null)
                        vertices[h, r] = new List<int>();
                    vertices[h, r].Add(vertexIndex);
                }
            }

            // Боковая поверхность
            for (int h = 0; h < heightSegments; h++)
            {
                for (int r = 0; r < radialSegments; r++)
                {
                    int nextR = (r + 1) % radialSegments;

                    int v1 = vertices[h, r][0];
                    int v2 = vertices[h, nextR][0];
                    int v3 = vertices[h + 1, nextR][0];
                    int v4 = vertices[h + 1, r][0];

                    mesh.AppendTriangle(v1, v2, v3);
                    mesh.AppendTriangle(v1, v3, v4);
                }
            }

            // Нижнее основание (центр + веер)
            int bottomCenter = mesh.AppendVertex(new Vector3d(0, -height / 2, 0));
            for (int r = 0; r < radialSegments; r++)
            {
                int nextR = (r + 1) % radialSegments;
                int v1 = vertices[0, r][0];
                int v2 = vertices[0, nextR][0];
                mesh.AppendTriangle(bottomCenter, v1, v2);
            }

            // Верхнее основание (центр + веер)
            int topCenter = mesh.AppendVertex(new Vector3d(0, height / 2, 0));
            for (int r = 0; r < radialSegments; r++)
            {
                int nextR = (r + 1) % radialSegments;
                int v1 = vertices[heightSegments, r][0];
                int v2 = vertices[heightSegments, nextR][0];
                mesh.AppendTriangle(topCenter, v2, v1);
            }

            Console.WriteLine($"   ✅ Цилиндр: радиус={radius}, высота={height}, сегментов={radialSegments}x{heightSegments}");
            return mesh;
        }
    }
}