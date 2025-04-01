using System;
using System.Collections.Generic;
using Core;
using Godot;
using Vector2 = Godot.Vector2;

namespace UI
{
    public partial class Karya1 : Node2D
    {
        private BentukDasar _bentukDasar = new BentukDasar();
        private const int MarginLeft = 50;
        private const int MarginTop = 50;

        public override void _Ready() { }

        public override void _Draw()
        {
            Vector2 windowSize = GetViewportRect().Size;
            int screenWidth = (int)windowSize[0];
            int screenHeight = (int)windowSize[1];
            int marginRight = screenWidth - MarginLeft;
            int marginBottom = screenHeight - MarginTop;

            MarginPixel(MarginLeft, MarginTop, marginRight, marginBottom);
            DrawCartesianLine(MarginLeft, MarginTop, marginRight, marginBottom);
            DrawVerticalCartesianLine(MarginLeft, MarginTop, marginRight, marginBottom);

            DrawOriginalShapes(MarginLeft, MarginTop);
        }

        private void DrawOriginalShapes(int marginLeft, int marginTop)
        {
            Vector2 windowSize = GetViewportRect().Size;
            float scaleFactor = Math.Min(windowSize.X / 200f, windowSize.Y / 200f);

            DrawTransformedTriangle(marginLeft, marginTop, 0, 0, 1, 0, new Godot.Color("#FF0000"));

            DrawTransformedTriangle(
                marginLeft,
                marginTop,
                -60,
                -20,
                1,
                0,
                new Godot.Color("#00FF00")
            );

            DrawTransformedTriangle(
                marginLeft,
                marginTop,
                0,
                -80,
                2f,
                0,
                new Godot.Color("#0000FF")
            );

            DrawTransformedTriangle(
                marginLeft,
                marginTop,
                50,
                30,
                1,
                45,
                new Godot.Color("#FFFF00")
            );

            DrawTransformedTriangle(
                marginLeft,
                marginTop,
                -60,
                -40,
                0.5f,
                0,
                new Godot.Color("#00FFFF")
            );
        }

        private void DrawTransformedTriangle(
            int marginLeft,
            int marginTop,
            float translateX,
            float translateY,
            float scaleFactor,
            float rotateDegrees,
            Godot.Color color
        )
        {
            try
            {
                rotateDegrees = rotateDegrees % 360;

                float[] baseVertices = new float[] { 0, 0, 10, 10, 5, 15 };

                float[] scaledVertices = new float[baseVertices.Length];
                float[] rotatedVertices = new float[baseVertices.Length];

                for (int i = 0; i < baseVertices.Length; i++)
                {
                    scaledVertices[i] = baseVertices[i] * scaleFactor;
                }

                if (rotateDegrees != 0)
                {
                    float radians = rotateDegrees * (float)Math.PI / 180f;
                    float cos = (float)Math.Cos(radians);
                    float sin = (float)Math.Sin(radians);

                    float centerX =
                        (scaledVertices[0] + scaledVertices[2] + scaledVertices[4]) / 3f;
                    float centerY =
                        (scaledVertices[1] + scaledVertices[3] + scaledVertices[5]) / 3f;

                    for (int i = 0; i < scaledVertices.Length; i += 2)
                    {
                        float x = scaledVertices[i] - centerX;
                        float y = scaledVertices[i + 1] - centerY;

                        rotatedVertices[i] = x * cos - y * sin + centerX;
                        rotatedVertices[i + 1] = x * sin + y * cos + centerY;
                    }
                }
                else
                {
                    Array.Copy(scaledVertices, rotatedVertices, scaledVertices.Length);
                }

                for (int i = 0; i < rotatedVertices.Length; i += 2)
                {
                    rotatedVertices[i] += translateX;
                    rotatedVertices[i + 1] += translateY;
                }

                Vector2[] screenPoints = new Vector2[3];
                for (int i = 0; i < rotatedVertices.Length; i += 2)
                {
                    screenPoints[i / 2] = ConvertWorldToPixel(
                        rotatedVertices[i],
                        rotatedVertices[i + 1],
                        marginLeft + 30,
                        marginTop + 100,
                        0
                    );
                }

                DrawLine(screenPoints[0], screenPoints[1], color);
                DrawLine(screenPoints[1], screenPoints[2], color);
                DrawLine(screenPoints[2], screenPoints[0], color);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"DrawTransformedTriangle error: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
            }
        }

        private void MarginPixel(int marginLeft, int marginTop, int marginRight, int marginBottom)
        {
            Godot.Color color = new Godot.Color("#32CD30");
            var margin = _bentukDasar.Margin(marginLeft, marginTop, marginRight, marginBottom);
            PutPixelAll(margin, color);
        }

        private List<float> ConvertToScreenSpace(
            float xa,
            float ya,
            float xb,
            float yb,
            int marginLeft,
            int marginTop
        )
        {
            Vector2 windowSize = GetViewportRect().Size;
            int axis = (int)(windowSize.X / 2f) + marginLeft;
            int ordinat = (int)(windowSize.Y / 2f) + marginTop;

            xa = axis + xa;
            xb = axis + xb;
            ya = ordinat - ya;
            yb = ordinat - yb;

            return new List<float> { xa, ya, xb, yb };
        }

        private void DrawCartesianLine(
            int marginLeft,
            int marginTop,
            int marginRight,
            int marginBottom
        )
        {
            float startX = -(marginRight - marginLeft) / 2f - 50;
            float endX = (marginRight - marginLeft) / 2f - 50;
            float startY = 50;
            float endY = 50;

            var convertedPoints = ConvertToScreenSpace(
                startX,
                startY,
                endX,
                endY,
                marginLeft,
                marginTop
            );

            List<Vector2> linePoints = _bentukDasar.Primitif.LineBresenham(
                convertedPoints[0],
                convertedPoints[1],
                convertedPoints[2],
                convertedPoints[3],
                5f,
                5f
            );

            Godot.Color lineColor = new Godot.Color("#00FF00");
            PutPixelAll(linePoints, lineColor);
        }

        private void DrawVerticalCartesianLine(
            int marginLeft,
            int marginTop,
            int marginRight,
            int marginBottom
        )
        {
            float startX = -50;
            float startY = -(marginBottom - marginTop) / 2f + 50;
            float endX = -50;
            float endY = (marginBottom - marginTop) / 2f + 50;

            var convertedPoints = ConvertToScreenSpace(
                startX,
                startY,
                endX,
                endY,
                marginLeft,
                marginTop
            );

            List<Vector2> linePoints = _bentukDasar.Primitif.LineBresenham(
                convertedPoints[0],
                convertedPoints[1],
                convertedPoints[2],
                convertedPoints[3],
                5f,
                5f
            );

            Godot.Color lineColor = new Godot.Color("#00FF00");
            PutPixelAll(linePoints, lineColor);
        }

        private List<float> ConvertToCartesian(float xa, float ya, float xb, float yb)
        {
            Vector2 windowSize = GetViewportRect().Size;

            int axis = (int)Math.Ceiling(windowSize.X / 2f);
            int ordinat = (int)Math.Ceiling(windowSize.Y / 2f);

            xa = axis + xa;
            xb = axis + xb;
            ya = ordinat - ya;
            yb = ordinat - yb;

            return new List<float> { xa, ya, xb, yb };
        }

        private void PutPixel(float x, float y, Godot.Color? color = null)
        {
            Godot.Color actualColor = color ?? Godot.Colors.White;
            Vector2[] points = new Vector2[] { new Vector2(Mathf.Round(x), Mathf.Round(y)) };
            Vector2[] uvs = new Vector2[]
            {
                Vector2.Zero,
                Vector2.Down,
                Vector2.One,
                Vector2.Right,
            };
            DrawPrimitive(points, new Godot.Color[] { actualColor }, uvs);
        }

        private void PutPixelAll(List<Vector2> dot, Godot.Color? color = null)
        {
            foreach (Vector2 point in dot)
            {
                PutPixel(point.X, point.Y, color);
            }
        }

        private Vector2 ConvertWorldToPixel(
            float xWorld,
            float yWorld,
            int marginLeft,
            int marginTop,
            float scaleFactor
        )
        {
            Vector2 windowSize = GetViewportRect().Size;

            float centerX = windowSize.X / 2f;
            float centerY = windowSize.Y / 2f;

            float pixelX = 0;
            float pixelY = 0;
            if (scaleFactor == 0)
            {
                pixelX = centerX + (xWorld * 4);
                pixelY = centerY - (yWorld * 4);
            }
            else
            {
                pixelX = centerX + (xWorld * scaleFactor);
                pixelY = centerY - (yWorld * scaleFactor);
            }

            return new Vector2(pixelX, pixelY);
        }

        public override void _ExitTree()
        {
            GD.Print($"_bentukDasar is null in _ExitTree(): {_bentukDasar == null}");
            _bentukDasar?.Dispose();
            _bentukDasar = null;
            GD.Print($"_bentukDasar is null in _ExitTree(): {_bentukDasar == null}");
            base._ExitTree();
        }

        private void DrawShapesInQuadrants(int marginLeft, int marginTop)
        {
            Vector2 windowSize = GetViewportRect().Size;
            float scaleFactor = Math.Min(Math.Min(windowSize.X / 200f, windowSize.Y / 200f), 5f);

            Godot.Color q1Color = new Godot.Color("#FF0000");
            Godot.Color q2Color = new Godot.Color("#00FF00");
            Godot.Color q3Color = new Godot.Color("#0000FF");
            Godot.Color q4Color = new Godot.Color("#FFFF00");

            DrawRightTriangle(20, 20, 40, 40, marginLeft, marginTop, 0, q1Color, Quadrant.Q1);

            DrawRightTrapezoid(
                40,
                20,
                20,
                20,
                30,
                40,
                marginLeft,
                marginTop,
                0,
                q2Color,
                Quadrant.Q2
            );

            DrawIsoscelesTriangle(30, 30, 40, marginLeft, marginTop, 0, q3Color, Quadrant.Q3);

            DrawIsoscelesTrapezoid(30, 30, 30, 50, marginLeft, marginTop, 0, q4Color, Quadrant.Q4);
        }

        private enum Quadrant
        {
            Q1,
            Q2,
            Q3,
            Q4,
        }

        private void DrawRightTriangle(
            float x1,
            float y1,
            float x2,
            float y2,
            int marginLeft,
            int marginTop,
            float scaleFactor,
            Godot.Color color,
            Quadrant quadrant
        )
        {
            (float fx1, float fy1, float fx2, float fy2) = AdjustCoordinates(
                x1,
                y1,
                x2,
                y2,
                quadrant
            );
            float fx3 = fx1;
            float fy3 = fy2;

            Vector2 p1 = ConvertWorldToPixel(fx1, fy1, marginLeft, marginTop, scaleFactor);
            Vector2 p2 = ConvertWorldToPixel(fx2, fy2, marginLeft, marginTop, scaleFactor);
            Vector2 p3 = ConvertWorldToPixel(fx3, fy3, marginLeft, marginTop, scaleFactor);

            DrawLine(p1, p2, color);
            DrawLine(p2, p3, color);
            DrawLine(p3, p1, color);
        }

        private void DrawRightTrapezoid(
            float x1,
            float y1,
            float x2,
            float y2,
            float x3,
            float y3,
            int marginLeft,
            int marginTop,
            float scaleFactor,
            Godot.Color color,
            Quadrant quadrant
        )
        {
            (
                float fx1,
                float fy1,
                float fx2,
                float fy2,
                float fx3,
                float fy3,
                float fx4,
                float fy4
            ) = AdjustCoordinates(x1, y1, x2, y2, x3, y3, x1, y3, quadrant);

            Vector2 p1 = ConvertWorldToPixel(fx1, fy1, marginLeft, marginTop, scaleFactor);
            Vector2 p2 = ConvertWorldToPixel(fx2, fy2, marginLeft, marginTop, scaleFactor);
            Vector2 p3 = ConvertWorldToPixel(fx3, fy3, marginLeft, marginTop, scaleFactor);
            Vector2 p4 = ConvertWorldToPixel(fx4, fy4, marginLeft, marginTop, scaleFactor);

            DrawLine(p1, p2, color);
            DrawLine(p2, p3, color);
            DrawLine(p3, p4, color);
            DrawLine(p4, p1, color);
        }

        private void DrawIsoscelesTriangle(
            float x1,
            float y1,
            float baseLength,
            int marginLeft,
            int marginTop,
            float scaleFactor,
            Godot.Color color,
            Quadrant quadrant
        )
        {
            float halfBase = baseLength / 2;

            float leftBaseX = x1 - halfBase;
            float rightBaseX = x1 + halfBase;
            float baseY = y1;

            float apexX = x1;
            float apexY = y1 - baseLength;

            (float fx1, float fy1, float fx2, float fy2, float fx3, float fy3) = AdjustCoordinates(
                leftBaseX,
                baseY,
                rightBaseX,
                baseY,
                apexX,
                apexY,
                quadrant
            );

            Vector2 p1 = ConvertWorldToPixel(fx1, fy1, marginLeft, marginTop, scaleFactor);
            Vector2 p2 = ConvertWorldToPixel(fx2, fy2, marginLeft, marginTop, scaleFactor);
            Vector2 p3 = ConvertWorldToPixel(fx3, fy3, marginLeft, marginTop, scaleFactor);

            DrawLine(p1, p2, color);
            DrawLine(p2, p3, color);
            DrawLine(p3, p1, color);
        }

        private void DrawIsoscelesTrapezoid(
            float x1,
            float y1,
            float baseLength,
            float topLength,
            int marginLeft,
            int marginTop,
            float scaleFactor,
            Godot.Color color,
            Quadrant quadrant
        )
        {
            float halfBase = baseLength / 2;
            float halfTop = topLength / 2;
            float height = baseLength / 2;

            float leftBaseX = x1 - halfBase;
            float rightBaseX = x1 + halfBase;
            float baseY = y1;

            float leftTopX = x1 - halfTop;
            float rightTopX = x1 + halfTop;
            float topY = y1 + height;

            (
                float fx1,
                float fy1,
                float fx2,
                float fy2,
                float fx3,
                float fy3,
                float fx4,
                float fy4
            ) = AdjustCoordinates(
                leftBaseX,
                baseY,
                rightBaseX,
                baseY,
                rightTopX,
                topY,
                leftTopX,
                topY,
                quadrant
            );

            Vector2 p1 = ConvertWorldToPixel(fx1, fy1, marginLeft, marginTop, scaleFactor);
            Vector2 p2 = ConvertWorldToPixel(fx2, fy2, marginLeft, marginTop, scaleFactor);
            Vector2 p3 = ConvertWorldToPixel(fx3, fy3, marginLeft, marginTop, scaleFactor);
            Vector2 p4 = ConvertWorldToPixel(fx4, fy4, marginLeft, marginTop, scaleFactor);

            DrawLine(p1, p2, color);
            DrawLine(p2, p3, color);
            DrawLine(p3, p4, color);
            DrawLine(p4, p1, color);
        }

        private (float, float, float, float) AdjustCoordinates(
            float x1,
            float y1,
            float x2,
            float y2,
            Quadrant quadrant
        )
        {
            switch (quadrant)
            {
                case Quadrant.Q1:
                    return (x1, y1, x2, y2);
                case Quadrant.Q2:
                    return (-Math.Abs(x1), y1, -Math.Abs(x2), y2);
                case Quadrant.Q3:
                    return (-Math.Abs(x1), -Math.Abs(y1), -Math.Abs(x2), -Math.Abs(y2));
                case Quadrant.Q4:
                    return (x1, -Math.Abs(y1), x2, -Math.Abs(y2));
                default:
                    return (x1, y1, x2, y2);
            }
        }

        private (float, float, float, float, float, float) AdjustCoordinates(
            float x1,
            float y1,
            float x2,
            float y2,
            float x3,
            float y3,
            Quadrant quadrant
        )
        {
            switch (quadrant)
            {
                case Quadrant.Q1:
                    return (x1, y1, x2, y2, x3, y3);
                case Quadrant.Q2:
                    return (-Math.Abs(x1), y1, -Math.Abs(x2), y2, -Math.Abs(x3), y3);
                case Quadrant.Q3:
                    return (
                        -Math.Abs(x1),
                        -Math.Abs(y1),
                        -Math.Abs(x2),
                        -Math.Abs(y2),
                        -Math.Abs(x3),
                        -Math.Abs(y3)
                    );
                case Quadrant.Q4:
                    return (x1, -Math.Abs(y1), x2, -Math.Abs(y2), x3, -Math.Abs(y3));
                default:
                    return (x1, y1, x2, y2, x3, y3);
            }
        }

        private (float, float, float, float, float, float, float, float) AdjustCoordinates(
            float x1,
            float y1,
            float x2,
            float y2,
            float x3,
            float y3,
            float x4,
            float y4,
            Quadrant quadrant
        )
        {
            switch (quadrant)
            {
                case Quadrant.Q1:
                    return (x1, y1, x2, y2, x3, y3, x4, y4);
                case Quadrant.Q2:
                    return (
                        -Math.Abs(x1),
                        y1,
                        -Math.Abs(x2),
                        y2,
                        -Math.Abs(x3),
                        y3,
                        -Math.Abs(x4),
                        y4
                    );
                case Quadrant.Q3:
                    return (
                        -Math.Abs(x1),
                        -Math.Abs(y1),
                        -Math.Abs(x2),
                        -Math.Abs(y2),
                        -Math.Abs(x3),
                        -Math.Abs(y3),
                        -Math.Abs(x4),
                        -Math.Abs(y4)
                    );
                case Quadrant.Q4:
                    return (
                        x1,
                        -Math.Abs(y1),
                        x2,
                        -Math.Abs(y2),
                        x3,
                        -Math.Abs(y3),
                        x4,
                        -Math.Abs(y4)
                    );
                default:
                    return (x1, y1, x2, y2, x3, y3, x4, y4);
            }
        }

        private void DrawLine(
            Vector2 start,
            Vector2 end,
            Godot.Color color,
            float dashLength = 7f,
            float gapLength = 7f
        )
        {
            try
            {
                float dx = Math.Abs(end.X - start.X);
                float dy = Math.Abs(end.Y - start.Y);

                if (dx > 1000 || dy > 1000)
                {
                    DrawPrimitive(
                        new Vector2[] { start, end },
                        new Godot.Color[] { color },
                        new Vector2[] { Vector2.Zero, Vector2.One }
                    );
                    return;
                }

                int steps = (int)Math.Max(dx, dy);

                for (int i = 0; i <= steps; i++)
                {
                    float t = steps > 0 ? (float)i / steps : 0;

                    float x = start.X + t * (end.X - start.X);
                    float y = start.Y + t * (end.Y - start.Y);

                    if (dashLength > 0 && gapLength > 0)
                    {
                        int currentStep = i % (int)(dashLength + gapLength);
                        if (currentStep >= dashLength)
                            continue;
                    }

                    PutPixel(x, y, color);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"DrawLine error: {ex.Message}");
            }
        }

        private void DrawCircle(int marginLeft, int marginTop)
        {
            Vector2 windowSize = GetViewportRect().Size;
            float scaleFactor = Math.Min(windowSize.X / 200f, windowSize.Y / 200f);

            float circleCenterX = 35;
            float circleCenterY = 30;
            float radius = 25;

            List<Vector2> circlePoints = new List<Vector2>();

            int numPoints = 360;
            for (int i = 0; i < numPoints; i++)
            {
                float angle = (float)(i * 2 * Math.PI / numPoints);
                float x = circleCenterX + radius * (float)Math.Cos(angle);
                float y = circleCenterY + radius * (float)Math.Sin(angle);
                circlePoints.Add(new Vector2(x, y));
            }

            List<Vector2> screenPoints = new List<Vector2>();
            foreach (Vector2 point in circlePoints)
            {
                Vector2 converted = ConvertWorldToPixel(
                    point.X,
                    point.Y,
                    marginLeft,
                    marginTop,
                    scaleFactor
                );
                screenPoints.Add(converted);
            }

            Godot.Color circleColor = new Godot.Color("#FF00FF");

            float dashLength = 10f;
            float gapLength = 10f;
            bool draw = true;
            float distance = 0f;

            for (int i = 0; i < screenPoints.Count; i++)
            {
                if (draw)
                {
                    PutPixel(screenPoints[i].X, screenPoints[i].Y, circleColor);
                }

                if (i < screenPoints.Count - 1)
                {
                    distance += screenPoints[i].DistanceTo(screenPoints[i + 1]);
                }

                if (distance >= dashLength && draw)
                {
                    draw = false;
                    distance = 0f;
                }
                else if (distance >= gapLength && !draw)
                {
                    draw = true;
                    distance = 0f;
                }
            }
        }

        private void DrawEllipse(int marginLeft, int marginTop)
        {
            Vector2 windowSize = GetViewportRect().Size;
            float scaleFactor = Math.Min(windowSize.X / 200f, windowSize.Y / 200f);

            float ellipseCenterX = -50;
            float ellipseCenterY = -40;
            float rx = 35;
            float ry = 20;

            List<Vector2> ellipsePoints = new List<Vector2>();

            int numPoints = 360;
            for (int i = 0; i < numPoints; i++)
            {
                float angle = (float)(i * 2 * Math.PI / numPoints);
                float x = ellipseCenterX + rx * (float)Math.Cos(angle);
                float y = ellipseCenterY + ry * (float)Math.Sin(angle);
                ellipsePoints.Add(new Vector2(x, y));
            }

            List<Vector2> screenPoints = new List<Vector2>();
            foreach (Vector2 point in ellipsePoints)
            {
                Vector2 converted = ConvertWorldToPixel(
                    point.X,
                    point.Y,
                    marginLeft,
                    marginTop,
                    scaleFactor
                );
                screenPoints.Add(converted);
            }

            Godot.Color ellipseColor = new Godot.Color("#00FFFF");

            float dashLength = 10f;
            float gapLength = 10f;
            bool draw = true;
            float distance = 0f;

            for (int i = 0; i < screenPoints.Count; i++)
            {
                if (draw)
                {
                    PutPixel(screenPoints[i].X, screenPoints[i].Y, ellipseColor);
                }

                if (i < screenPoints.Count - 1)
                {
                    distance += screenPoints[i].DistanceTo(screenPoints[i + 1]);
                }

                if (distance >= dashLength && draw)
                {
                    draw = false;
                    distance = 0f;
                }
                else if (distance >= gapLength && !draw)
                {
                    draw = true;
                    distance = 0f;
                }
            }
        }
    }
}
