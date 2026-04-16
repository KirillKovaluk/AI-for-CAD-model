using g3;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace CadGeneratorWinForms
{
    public class ModelGenerator
    {
        private Random _random = new Random();

        public DMesh3 GenerateModel(ShapeParameters parameters)
        {
            Console.WriteLine($"🎯 Генерирую {parameters.ShapeType}...");

            switch (parameters.ShapeType?.ToLower())
            {
                case "cube":
                    return GenerateSolidCube(parameters.Size1, parameters.Size2, parameters.Size3);
                case "sphere":
                    return GenerateSolidSphere(parameters.Size1);
                case "cylinder":
                    return GenerateSolidCylinder(parameters.Size1, parameters.Size2);
                case "rubik_cube":
                    return GenerateRubikCubeWithGaps();
                case "rubik_pyramid":
                    return GenerateSolidPyramid();
                case "house":
                    return GenerateSolidHouse();
                case "star":
                    return GenerateSolidStar3D();
                case "stairs":
                    return GenerateSolidStairs();
                case "column":
                    return GenerateDecoratedColumn();
                default:
                    return GenerateSolidCube(10, 10, 10);
            }
        }

        // ============ ОСНОВНЫЕ ФИГУРЫ ============

        private DMesh3 GenerateSolidCube(double w, double h, double d)
        {
            var mesh = new DMesh3();

            Vector3d[] vertices = new Vector3d[8];
            vertices[0] = new Vector3d(-w / 2, -h / 2, -d / 2);
            vertices[1] = new Vector3d(w / 2, -h / 2, -d / 2);
            vertices[2] = new Vector3d(w / 2, h / 2, -d / 2);
            vertices[3] = new Vector3d(-w / 2, h / 2, -d / 2);
            vertices[4] = new Vector3d(-w / 2, -h / 2, d / 2);
            vertices[5] = new Vector3d(w / 2, -h / 2, d / 2);
            vertices[6] = new Vector3d(w / 2, h / 2, d / 2);
            vertices[7] = new Vector3d(-w / 2, h / 2, d / 2);

            int[] v = new int[8];
            for (int i = 0; i < 8; i++)
                v[i] = mesh.AppendVertex(vertices[i]);

            mesh.AppendTriangle(v[0], v[1], v[2]);
            mesh.AppendTriangle(v[0], v[2], v[3]);
            mesh.AppendTriangle(v[4], v[6], v[5]);
            mesh.AppendTriangle(v[4], v[7], v[6]);
            mesh.AppendTriangle(v[0], v[3], v[7]);
            mesh.AppendTriangle(v[0], v[7], v[4]);
            mesh.AppendTriangle(v[1], v[5], v[6]);
            mesh.AppendTriangle(v[1], v[6], v[2]);
            mesh.AppendTriangle(v[0], v[4], v[5]);
            mesh.AppendTriangle(v[0], v[5], v[1]);
            mesh.AppendTriangle(v[3], v[2], v[6]);
            mesh.AppendTriangle(v[3], v[6], v[7]);

            return mesh;
        }

        private DMesh3 GenerateSolidSphere(double radius)
        {
            var mesh = new DMesh3();
            int stacks = 48;
            int slices = 96;

            for (int i = 0; i <= stacks; i++)
            {
                double theta = Math.PI * i / stacks;
                double sinTheta = Math.Sin(theta);
                double cosTheta = Math.Cos(theta);

                for (int j = 0; j <= slices; j++)
                {
                    double phi = 2 * Math.PI * j / slices;
                    double sinPhi = Math.Sin(phi);
                    double cosPhi = Math.Cos(phi);

                    double x = radius * sinTheta * cosPhi;
                    double y = radius * cosTheta;
                    double z = radius * sinTheta * sinPhi;

                    mesh.AppendVertex(new Vector3d(x, y, z));
                }
            }

            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int p1 = i * (slices + 1) + j;
                    int p2 = p1 + 1;
                    int p3 = (i + 1) * (slices + 1) + j;
                    int p4 = p3 + 1;

                    mesh.AppendTriangle(p1, p2, p3);
                    mesh.AppendTriangle(p2, p4, p3);
                }
            }

            return mesh;
        }

        private DMesh3 GenerateSolidCylinder(double radius, double height)
        {
            return CreateSolidCylinder(radius, height, 64);
        }

        // ============ КУБИК РУБИКА (зазор 10% от размера кубика) ============

        private DMesh3 GenerateRubikCubeWithGaps()
        {
            var combinedMesh = new DMesh3();

            double totalSize = 3.0;
            int cubesPerSide = 3;
            double cubeSize = totalSize / cubesPerSide;  // 1.0
            double gapPercent = 0.10;                    // 10% зазор
            double gap = cubeSize * gapPercent;           // зазор = 0.1
            double actualCubeSize = cubeSize - gap;       // реальный размер кубика = 0.9
            double offset = cubeSize;                     // смещение между центрами = 1.0

            double startPos = -totalSize / 2 + cubeSize / 2;

            Console.WriteLine($"   📐 Размер кубика: {cubeSize:F2}, зазор 10%: {gap:F2}, реальный размер: {actualCubeSize:F2}");

            Color[] faceColors = {
                Color.FromArgb(255, 255, 0, 0),     // красный
                Color.FromArgb(255, 255, 128, 0),   // оранжевый
                Color.FromArgb(255, 255, 255, 0),   // желтый
                Color.FromArgb(255, 255, 255, 255), // белый
                Color.FromArgb(255, 0, 255, 0),     // зеленый
                Color.FromArgb(255, 0, 0, 255)      // синий
            };

            for (int x = 0; x < cubesPerSide; x++)
            {
                for (int y = 0; y < cubesPerSide; y++)
                {
                    for (int z = 0; z < cubesPerSide; z++)
                    {
                        double posX = startPos + x * offset;
                        double posY = startPos + y * offset;
                        double posZ = startPos + z * offset;

                        int cubeX = x - 1;
                        int cubeY = y - 1;
                        int cubeZ = z - 1;

                        Color cubeColor;
                        if (cubeX == -1) cubeColor = faceColors[5];
                        else if (cubeX == 1) cubeColor = faceColors[4];
                        else if (cubeY == 1) cubeColor = faceColors[2];
                        else if (cubeY == -1) cubeColor = faceColors[3];
                        else if (cubeZ == 1) cubeColor = faceColors[0];
                        else if (cubeZ == -1) cubeColor = faceColors[1];
                        else cubeColor = Color.FromArgb(255, 80, 80, 80);

                        var cube = GenerateSolidCube(actualCubeSize, actualCubeSize, actualCubeSize);

                        foreach (int vid in cube.VertexIndices())
                        {
                            var v = cube.GetVertex(vid);
                            combinedMesh.AppendVertex(new Vector3d(v.x + posX, v.y + posY, v.z + posZ));
                        }

                        int startIdx = combinedMesh.VertexCount - cube.VertexCount;
                        foreach (int tid in cube.TriangleIndices())
                        {
                            var tri = cube.GetTriangle(tid);
                            combinedMesh.AppendTriangle(tri.a + startIdx, tri.b + startIdx, tri.c + startIdx);
                        }
                    }
                }
            }

            return combinedMesh;
        }

        // ============ ПИРАМИДА ============

        private DMesh3 GenerateSolidPyramid()
        {
            var mesh = new DMesh3();
            double size = 2.5;
            double height = 3.5;

            Vector3d[] vertices = {
                new Vector3d(0, height/2, 0),
                new Vector3d(-size/2, -height/2, -size/2),
                new Vector3d(size/2, -height/2, -size/2),
                new Vector3d(size/2, -height/2, size/2),
                new Vector3d(-size/2, -height/2, size/2)
            };

            int[] v = new int[5];
            for (int i = 0; i < 5; i++)
                v[i] = mesh.AppendVertex(vertices[i]);

            mesh.AppendTriangle(v[0], v[1], v[2]);
            mesh.AppendTriangle(v[0], v[2], v[3]);
            mesh.AppendTriangle(v[0], v[3], v[4]);
            mesh.AppendTriangle(v[0], v[4], v[1]);
            mesh.AppendTriangle(v[1], v[2], v[3]);
            mesh.AppendTriangle(v[1], v[3], v[4]);

            return mesh;
        }

        // ============ ДОМИК ============

        private DMesh3 GenerateSolidHouse()
        {
            var combinedMesh = new DMesh3();
            double width = 2.0;
            double wallHeight = 1.5;
            double roofHeight = 1.2;

            var walls = GenerateSolidCube(width, wallHeight, width);

            foreach (int vid in walls.VertexIndices())
            {
                var v = walls.GetVertex(vid);
                combinedMesh.AppendVertex(new Vector3d(v.x, v.y - wallHeight / 2, v.z));
            }
            int wallStart = combinedMesh.VertexCount - walls.VertexCount;
            foreach (int tid in walls.TriangleIndices())
            {
                var tri = walls.GetTriangle(tid);
                combinedMesh.AppendTriangle(tri.a + wallStart, tri.b + wallStart, tri.c + wallStart);
            }

            double roofBase = wallHeight / 2;
            double roofTop = roofBase + roofHeight;

            Vector3d[] roofVerts = {
                new Vector3d(-width/2, roofBase, -width/2),
                new Vector3d( width/2, roofBase, -width/2),
                new Vector3d( width/2, roofBase,  width/2),
                new Vector3d(-width/2, roofBase,  width/2),
                new Vector3d(0, roofTop, -width/2),
                new Vector3d(0, roofTop,  width/2)
            };

            int[] rv = new int[6];
            for (int i = 0; i < 6; i++)
                rv[i] = combinedMesh.AppendVertex(roofVerts[i]);

            combinedMesh.AppendTriangle(rv[0], rv[1], rv[4]);
            combinedMesh.AppendTriangle(rv[1], rv[4], rv[5]);
            combinedMesh.AppendTriangle(rv[2], rv[3], rv[5]);
            combinedMesh.AppendTriangle(rv[3], rv[5], rv[4]);
            combinedMesh.AppendTriangle(rv[0], rv[3], rv[4]);
            combinedMesh.AppendTriangle(rv[1], rv[2], rv[5]);
            combinedMesh.AppendTriangle(rv[0], rv[1], rv[2]);
            combinedMesh.AppendTriangle(rv[0], rv[2], rv[3]);

            return combinedMesh;
        }

        // ============ ЗВЕЗДА 3D (с объемом по оси Z) ============

        private DMesh3 GenerateSolidStar3D()
        {
            var mesh = new DMesh3();
            int points = 5;
            double outerRadius = 1.2;
            double innerRadius = 0.5;
            double thickness = 0.3;

            List<Vector3d> topPoints = new List<Vector3d>();
            List<Vector3d> bottomPoints = new List<Vector3d>();

            // Создаем точки верхней и нижней звезды
            for (int i = 0; i < points * 2; i++)
            {
                double angle = i * Math.PI / points - Math.PI / 2;
                double radius = (i % 2 == 0) ? outerRadius : innerRadius;
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);
                topPoints.Add(new Vector3d(x, y, thickness / 2));
                bottomPoints.Add(new Vector3d(x, y, -thickness / 2));
            }

            List<int> topVerts = new List<int>();
            List<int> bottomVerts = new List<int>();

            foreach (var p in topPoints)
                topVerts.Add(mesh.AppendVertex(p));
            foreach (var p in bottomPoints)
                bottomVerts.Add(mesh.AppendVertex(p));

            // Верхняя грань
            int centerTop = mesh.AppendVertex(new Vector3d(0, 0, thickness / 2));
            for (int i = 0; i < topPoints.Count; i++)
            {
                int next = (i + 1) % topPoints.Count;
                mesh.AppendTriangle(centerTop, topVerts[i], topVerts[next]);
            }

            // Нижняя грань
            int centerBottom = mesh.AppendVertex(new Vector3d(0, 0, -thickness / 2));
            for (int i = 0; i < bottomPoints.Count; i++)
            {
                int next = (i + 1) % bottomPoints.Count;
                mesh.AppendTriangle(centerBottom, bottomVerts[next], bottomVerts[i]);
            }

            // Боковые грани
            for (int i = 0; i < topPoints.Count; i++)
            {
                int next = (i + 1) % topPoints.Count;
                mesh.AppendTriangle(topVerts[i], bottomVerts[i], topVerts[next]);
                mesh.AppendTriangle(bottomVerts[i], bottomVerts[next], topVerts[next]);
            }

            return mesh;
        }

        // ============ ЛЕСТНИЦА (из отдельных блоков-параллелепипедов) ============

        private DMesh3 GenerateSolidStairs()
        {
            var combinedMesh = new DMesh3();
            int steps = 6;
            double stepWidth = 2.0;
            double stepHeight = 0.25;
            double stepDepth = 0.35;

            for (int i = 0; i < steps; i++)
            {
                double y = i * stepHeight;
                double z = i * stepDepth;

                var step = GenerateSolidCube(stepWidth, stepHeight, stepDepth);

                foreach (int vid in step.VertexIndices())
                {
                    var v = step.GetVertex(vid);
                    combinedMesh.AppendVertex(new Vector3d(v.x, v.y + y, v.z + z));
                }
                int startIdx = combinedMesh.VertexCount - step.VertexCount;
                foreach (int tid in step.TriangleIndices())
                {
                    var tri = step.GetTriangle(tid);
                    combinedMesh.AppendTriangle(tri.a + startIdx, tri.b + startIdx, tri.c + startIdx);
                }
            }

            Console.WriteLine($"   ✅ Лестница: {steps} ступеней");
            return combinedMesh;
        }

        // ============ КОЛОННА (основной цилиндр + выступающие меньшие цилиндры) ============

        private DMesh3 GenerateDecoratedColumn()
        {
            var combinedMesh = new DMesh3();
            double mainRadius = 0.6;
            double mainHeight = 3.0;
            double smallRadius = mainRadius / 5;  // в 5 раз меньше
            double smallHeight = 0.2;
            int segments = 48;

            // Основной цилиндр
            var mainColumn = CreateSolidCylinder(mainRadius, mainHeight, segments);

            foreach (int vid in mainColumn.VertexIndices())
            {
                var v = mainColumn.GetVertex(vid);
                combinedMesh.AppendVertex(v);
            }
            int colStart = combinedMesh.VertexCount - mainColumn.VertexCount;
            foreach (int tid in mainColumn.TriangleIndices())
            {
                var tri = mainColumn.GetTriangle(tid);
                combinedMesh.AppendTriangle(tri.a + colStart, tri.b + colStart, tri.c + colStart);
            }

            // Выступающие маленькие цилиндры по всей высоте
            int rings = 8;
            double ringSpacing = mainHeight / (rings + 1);

            for (int i = 1; i <= rings; i++)
            {
                double y = -mainHeight / 2 + i * ringSpacing;

                // Создаем кольцо из 8 маленьких цилиндров
                for (int j = 0; j < 8; j++)
                {
                    double angle = 2 * Math.PI * j / 8;
                    double x = mainRadius * Math.Cos(angle);
                    double z = mainRadius * Math.Sin(angle);

                    var smallCyl = CreateSolidCylinder(smallRadius, smallHeight, 16);

                    foreach (int vid in smallCyl.VertexIndices())
                    {
                        var v = smallCyl.GetVertex(vid);
                        combinedMesh.AppendVertex(new Vector3d(v.x + x, v.y + y, v.z + z));
                    }
                    int startIdx = combinedMesh.VertexCount - smallCyl.VertexCount;
                    foreach (int tid in smallCyl.TriangleIndices())
                    {
                        var tri = smallCyl.GetTriangle(tid);
                        combinedMesh.AppendTriangle(tri.a + startIdx, tri.b + startIdx, tri.c + startIdx);
                    }
                }
            }

            // Верхняя капитель
            double capRadius = mainRadius * 1.4;
            double capHeight = mainHeight * 0.1;
            var capital = CreateSolidCylinder(capRadius, capHeight, segments);

            foreach (int vid in capital.VertexIndices())
            {
                var v = capital.GetVertex(vid);
                combinedMesh.AppendVertex(new Vector3d(v.x, v.y + mainHeight / 2 - capHeight / 2, v.z));
            }
            int capStart = combinedMesh.VertexCount - capital.VertexCount;
            foreach (int tid in capital.TriangleIndices())
            {
                var tri = capital.GetTriangle(tid);
                combinedMesh.AppendTriangle(tri.a + capStart, tri.b + capStart, tri.c + capStart);
            }

            // Нижняя база
            var baseCap = CreateSolidCylinder(capRadius, capHeight, segments);

            foreach (int vid in baseCap.VertexIndices())
            {
                var v = baseCap.GetVertex(vid);
                combinedMesh.AppendVertex(new Vector3d(v.x, v.y - mainHeight / 2 + capHeight / 2, v.z));
            }
            int baseStart = combinedMesh.VertexCount - baseCap.VertexCount;
            foreach (int tid in baseCap.TriangleIndices())
            {
                var tri = baseCap.GetTriangle(tid);
                combinedMesh.AppendTriangle(tri.a + baseStart, tri.b + baseStart, tri.c + baseStart);
            }

            Console.WriteLine($"   ✅ Колонна: основной цилиндр + {rings * 8} выступающих");
            return combinedMesh;
        }

        // ============ ВСПОМОГАТЕЛЬНЫЙ МЕТОД ============

        private DMesh3 CreateSolidCylinder(double radius, double height, int segments)
        {
            var mesh = new DMesh3();

            int[] bottomRing = new int[segments];
            int[] topRing = new int[segments];

            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);

                bottomRing[i] = mesh.AppendVertex(new Vector3d(x, y, -height / 2));
                topRing[i] = mesh.AppendVertex(new Vector3d(x, y, height / 2));
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                mesh.AppendTriangle(bottomRing[i], bottomRing[next], topRing[i]);
                mesh.AppendTriangle(bottomRing[next], topRing[next], topRing[i]);
            }

            int bottomCenter = mesh.AppendVertex(new Vector3d(0, 0, -height / 2));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                mesh.AppendTriangle(bottomCenter, bottomRing[next], bottomRing[i]);
            }

            int topCenter = mesh.AppendVertex(new Vector3d(0, 0, height / 2));
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                mesh.AppendTriangle(topCenter, topRing[i], topRing[next]);
            }

            return mesh;
        }
    }
}