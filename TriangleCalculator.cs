using System;
using System.Collections.Generic;

namespace TriangleLab3
{
    // Класс результата
    public class TriangleResult
    {
        public string Type { get; set; } = string.Empty;
        public List<(int X, int Y)> Coordinates { get; set; } = new List<(int X, int Y)>();
        public string ErrorMessage { get; set; } = string.Empty;

        public TriangleResult() { }

        public TriangleResult(string type, List<(int X, int Y)> coordinates, string errorMessage = "")
        {
            Type = type ?? string.Empty;
            Coordinates = coordinates ?? new List<(int X, int Y)>();
            ErrorMessage = errorMessage ?? string.Empty;
        }
    }

    // Класс a: статический метод из Лабораторной №1
    public static class TriangleCalculator
    {
        private const float Epsilon = 0.001f;

        public static TriangleResult CalculateTriangle(string? sideAStr, string? sideBStr, string? sideCStr)
        {
            // Проверка на null или нечисловые данные
            if (sideAStr == null || sideBStr == null || sideCStr == null ||
                !float.TryParse(sideAStr, out float sideA) ||
                !float.TryParse(sideBStr, out float sideB) ||
                !float.TryParse(sideCStr, out float sideC))
            {
                return new TriangleResult("", new List<(int X, int Y)> { (-2, -2), (-2, -2), (-2, -2) },
                    "Нечисловые входные данные или null");
            }

            // Проверка на положительные числа
            if (sideA <= 0 || sideB <= 0 || sideC <= 0)
            {
                return new TriangleResult("не треугольник", new List<(int X, int Y)> { (-1, -1), (-1, -1), (-1, -1) },
                    "Стороны должны быть положительными числами");
            }

            // Проверка на неравенство треугольника
            if (!IsValidTriangle(sideA, sideB, sideC))
            {
                return new TriangleResult("не треугольник", new List<(int X, int Y)> { (-1, -1), (-1, -1), (-1, -1) },
                    "Неравенство треугольника не выполняется");
            }

            string triangleType = DetermineTriangleType(sideA, sideB, sideC);
            var coordinates = CalculateCoordinates(sideA, sideB, sideC);

            return new TriangleResult(triangleType, coordinates, "");
        }

        private static bool IsValidTriangle(float a, float b, float c)
        {
            if (Math.Abs(a + b - c) < Epsilon ||
                Math.Abs(a + c - b) < Epsilon ||
                Math.Abs(b + c - a) < Epsilon)
                return false;

            return a + b > c && a + c > b && b + c > a;
        }

        private static string DetermineTriangleType(float a, float b, float c)
        {
            if (Math.Abs(a - b) <= Epsilon && Math.Abs(b - c) <= Epsilon)
                return "равносторонний";

            if (Math.Abs(a - b) <= Epsilon || Math.Abs(a - c) <= Epsilon || Math.Abs(b - c) <= Epsilon)
                return "равнобедренный";

            return "разносторонний";
        }

        private static List<(int X, int Y)> CalculateCoordinates(float a, float b, float c)
        {
            try
            {
                int ax = 10, ay = 90;
                int bx = ax + (int)Math.Round(c);
                int by = ay;

                float angleA = (float)Math.Acos((b * b + c * c - a * a) / (2 * b * c));
                float cx = ax + (int)Math.Round(b * Math.Cos(angleA));
                float cy = ay - (int)Math.Round(b * Math.Sin(angleA));

                float minX = Math.Min(ax, Math.Min(bx, cx));
                float maxX = Math.Max(ax, Math.Max(bx, cx));
                float minY = Math.Min(ay, Math.Min(by, cy));
                float maxY = Math.Max(ay, Math.Max(by, cy));

                float padding = 5;
                float scaleX = (100 - 2 * padding) / (maxX - minX);
                float scaleY = (100 - 2 * padding) / (maxY - minY);
                float scale = Math.Min(scaleX, scaleY);

                if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0)
                    scale = 1;

                int axScaled = (int)Math.Floor(padding + (ax - minX) * scale + 0.5);
                int ayScaled = (int)Math.Floor(padding + (ay - minY) * scale + 0.5);
                int bxScaled = (int)Math.Floor(padding + (bx - minX) * scale + 0.5);
                int byScaled = (int)Math.Floor(padding + (by - minY) * scale + 0.5);
                int cxScaled = (int)Math.Floor(padding + (cx - minX) * scale + 0.5);
                int cyScaled = (int)Math.Floor(padding + (cy - minY) * scale + 0.5);

                return new List<(int X, int Y)>
                {
                    (Math.Clamp(axScaled, 0, 100), Math.Clamp(ayScaled, 0, 100)),
                    (Math.Clamp(bxScaled, 0, 100), Math.Clamp(byScaled, 0, 100)),
                    (Math.Clamp(cxScaled, 0, 100), Math.Clamp(cyScaled, 0, 100))
                };
            }
            catch
            {
                return new List<(int X, int Y)> { (-1, -1), (-1, -1), (-1, -1) };
            }
        }
    }
}