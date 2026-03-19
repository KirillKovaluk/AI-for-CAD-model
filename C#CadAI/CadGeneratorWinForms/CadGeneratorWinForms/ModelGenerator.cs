using CadGenerator;
using g3;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CadGeneratorWinForms
{
    public class ModelGenerator
    {
        public DMesh3 GenerateModel(ShapeParameters parameters)
        {
            Console.WriteLine($"🎯 Генерирую {parameters.ColorName} {parameters.ShapeType}...");

            DMesh3 mesh = null;

            switch (parameters.ShapeType?.ToLower())
            {
                case "cube":
                    mesh = GenerateCube(parameters.Size1, parameters.Size2, parameters.Size3, parameters.Color);
                    break;

                case "sphere":
                    mesh = GenerateSphere(parameters.Size1, parameters.Color);
                    break;

                case "cylinder":
                    mesh = GenerateCylinder(parameters.Size1, parameters.Size2, parameters.Color);
                    break;

                default:
                    mesh = GenerateCube(10, 10, 10, Color.Gray);
                    break;
            }

            return mesh;
        }

        private DMesh3 GenerateCube(double width, double height, double depth, Color color)
        {
            try
            {
                var boxGen = new TrivialBox3Generator();
                boxGen.Box = new Box3d(
                    new Vector3d(-width / 2, -height / 2, -depth / 2),
                    new Vector3d(width / 2, height / 2, depth / 2)
                );
                boxGen.Generate();
                var mesh = boxGen.MakeDMesh();

                // Добавляем цвет к вершинам
                AddColorToMesh(mesh, color);

                Console.WriteLine($"   ✅ Цветной куб создан: {width} x {height} x {depth}");
                return mesh;
            }
            catch
            {
                return CreateSimpleCube(width, height, depth, color);
            }
        }

        private DMesh3 GenerateSphere(double radius, Color color)
        {
            try
            {
                var sphereGen = new Sphere3Generator_NormalizedCube();
                sphereGen.Radius = radius;
                sphereGen.Generate();
                var mesh = sphereGen.MakeDMesh();

                // Добавляем цвет к вершинам
                AddColorToMesh(mesh, color);

                Console.WriteLine($"   ✅ Цветная сфера создана: радиус {radius}");
                return mesh;
            }
            catch
            {
                return CreateSimpleSphere(radius, color);
            }
        }

        private DMesh3 GenerateCylinder(double radius, double height, Color color)
        {
            try
            {
                var mesh = CreateSimpleCylinder(radius, height, color);
                return mesh;
            }
            catch
            {
                return new DMesh3();
            }
        }

        // Добавляем цвет к каждой вершине mesh
        private void AddColorToMesh(DMesh3 mesh, Color color)
        {
            // Конвертируем System.Drawing.Color в Vector3f для g3
            Vector3f colorVec = new Vector3f(color.R / 255f, color.G / 255f, color.B / 255f);

            // Добавляем цвет к каждой вершине
            foreach (int vid in mesh.VertexIndices())
            {
                mesh.SetVertexColor(vid, colorVec);
            }
        }

        private DMesh3 CreateSimpleCube(double width, double height, double depth, Color color)
        {
            var mesh = new DMesh3();

            // Создаем вершины
            int v0 = mesh.AppendVertex(new Vector3d(-width / 2, -height / 2, -depth / 2));
            int v1 = mesh.AppendVertex(new Vector3d(width / 2, -height / 2, -depth / 2));
            int v2 = mesh.AppendVertex(new Vector3d(width / 2, height / 2, -depth / 2));
            int v3 = mesh.AppendVertex(new Vector3d(-width / 2, height / 2, -depth / 2));
            int v4 = mesh.AppendVertex(new Vector3d(-width / 2, -height / 2, depth / 2));
            int v5 = mesh.AppendVertex(new Vector3d(width / 2, -height / 2, depth / 2));
            int v6 = mesh.AppendVertex(new Vector3d(width / 2, height / 2, depth / 2));
            int v7 = mesh.AppendVertex(new Vector3d(-width / 2, height / 2, depth / 2));

            // Добавляем цвет ко всем вершинам
            Vector3f colorVec = new Vector3f(color.R / 255f, color.G / 255f, color.B / 255f);
            foreach (int vid in mesh.VertexIndices())
            {
                mesh.SetVertexColor(vid, colorVec);
            }

            // Создаем треугольники
            mesh.AppendTriangle(v0, v1, v2);
            mesh.AppendTriangle(v0, v2, v3);
            mesh.AppendTriangle(v4, v6, v5);
            mesh.AppendTriangle(v4, v7, v6);
            mesh.AppendTriangle(v0, v3, v7);
            mesh.AppendTriangle(v0, v7, v4);
            mesh.AppendTriangle(v1, v5, v6);
            mesh.AppendTriangle(v1, v6, v2);
            mesh.AppendTriangle(v0, v4, v5);
            mesh.AppendTriangle(v0, v5, v1);
            mesh.AppendTriangle(v3, v2, v6);
            mesh.AppendTriangle(v3, v6, v7);

            return mesh;
        }

        private DMesh3 CreateSimpleSphere(double radius, Color color, int subdivisions = 2)
        {
            try
            {
                var mesh = CreateOctahedron(radius);

                for (int i = 0; i < subdivisions; i++)
                {
                    mesh = SubdivideSphere(mesh, radius);
                }

                // Добавляем цвет
                Vector3f colorVec = new Vector3f(color.R / 255f, color.G / 255f, color.B / 255f);
                foreach (int vid in mesh.VertexIndices())
                {
                    mesh.SetVertexColor(vid, colorVec);
                }

                return mesh;
            }
            catch
            {
                return new DMesh3();
            }
        }

        private DMesh3 CreateSimpleCylinder(double radius, double height, Color color, int segments = 32)
        {
            var mesh = new DMesh3();

            int[] bottomRing = new int[segments];
            int[] topRing = new int[segments];

            // Создаем вершины
            for (int i = 0; i < segments; i++)
            {
                double angle = (2 * Math.PI * i) / segments;
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);

                bottomRing[i] = mesh.AppendVertex(new Vector3d(x, y, -height / 2));
                topRing[i] = mesh.AppendVertex(new Vector3d(x, y, height / 2));
            }

            // Добавляем цвет ко всем вершинам
            Vector3f colorVec = new Vector3f(color.R / 255f, color.G / 255f, color.B / 255f);
            foreach (int vid in mesh.VertexIndices())
            {
                mesh.SetVertexColor(vid, colorVec);
            }

            // Боковые грани
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                mesh.AppendTriangle(bottomRing[i], bottomRing[next], topRing[i]);
                mesh.AppendTriangle(bottomRing[next], topRing[next], topRing[i]);
            }

            // Нижнее основание
            int bottomCenter = mesh.AppendVertex(new Vector3d(0, 0, -height / 2));
            mesh.SetVertexColor(bottomCenter, colorVec);
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                mesh.AppendTriangle(bottomCenter, bottomRing[next], bottomRing[i]);
            }

            // Верхнее основание
            int topCenter = mesh.AppendVertex(new Vector3d(0, 0, height / 2));
            mesh.SetVertexColor(topCenter, colorVec);
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                mesh.AppendTriangle(topCenter, topRing[i], topRing[next]);
            }

            return mesh;
        }

        private DMesh3 CreateOctahedron(double radius)
        {
            var mesh = new DMesh3();

            int top = mesh.AppendVertex(new Vector3d(0, radius, 0));
            int bottom = mesh.AppendVertex(new Vector3d(0, -radius, 0));
            int front = mesh.AppendVertex(new Vector3d(0, 0, radius));
            int back = mesh.AppendVertex(new Vector3d(0, 0, -radius));
            int left = mesh.AppendVertex(new Vector3d(-radius, 0, 0));
            int right = mesh.AppendVertex(new Vector3d(radius, 0, 0));

            mesh.AppendTriangle(top, front, right);
            mesh.AppendTriangle(top, right, back);
            mesh.AppendTriangle(top, back, left);
            mesh.AppendTriangle(top, left, front);
            mesh.AppendTriangle(bottom, right, front);
            mesh.AppendTriangle(bottom, back, right);
            mesh.AppendTriangle(bottom, left, back);
            mesh.AppendTriangle(bottom, front, left);

            return mesh;
        }

        private DMesh3 SubdivideSphere(DMesh3 mesh, double radius)
        {
            var newMesh = new DMesh3();
            var midPoints = new Dictionary<(int, int), int>();

            int GetMidPoint(int i1, int i2)
            {
                var key = i1 < i2 ? (i1, i2) : (i2, i1);
                if (midPoints.ContainsKey(key))
                    return midPoints[key];

                var p1 = mesh.GetVertex(i1);
                var p2 = mesh.GetVertex(i2);
                var mid = (p1 + p2) * 0.5;
                mid.Normalize();
                mid *= radius;

                int newIndex = newMesh.AppendVertex(mid);
                midPoints[key] = newIndex;
                return newIndex;
            }

            foreach (var tid in mesh.TriangleIndices())
            {
                var tri = mesh.GetTriangle(tid);
                int v1 = tri.a;
                int v2 = tri.b;
                int v3 = tri.c;

                int a = GetMidPoint(v1, v2);
                int b = GetMidPoint(v2, v3);
                int c = GetMidPoint(v3, v1);

                int nv1 = newMesh.AppendVertex(mesh.GetVertex(v1));
                int nv2 = newMesh.AppendVertex(mesh.GetVertex(v2));
                int nv3 = newMesh.AppendVertex(mesh.GetVertex(v3));

                newMesh.AppendTriangle(nv1, a, c);
                newMesh.AppendTriangle(nv2, b, a);
                newMesh.AppendTriangle(nv3, c, b);
                newMesh.AppendTriangle(a, b, c);
            }

            return newMesh;
        }
    }
}