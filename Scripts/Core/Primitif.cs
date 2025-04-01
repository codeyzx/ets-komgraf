using System;
using System.Collections.Generic;
using Godot;

namespace Core
{
    public partial class Primitif : RefCounted
    {
        public List<Vector2> LineBresenham(
            float x0,
            float y0,
            float x1,
            float y1,
            float dashLength = 5f,
            float gapLength = 5f
        )
        {
            List<Vector2> points = new List<Vector2>();

            float dx = Mathf.Abs(x1 - x0);
            float dy = Mathf.Abs(y1 - y0);
            float sx = x0 < x1 ? 1 : -1;
            float sy = y0 < y1 ? 1 : -1;
            float err = dx - dy;

            float totalLength = 0f;
            bool draw = true;

            if (dashLength == 5f && gapLength == 5f)
            {
                while (true)
                {
                    points.Add(new Vector2(x0, y0));
                    if (x0 == x1 && y0 == y1)
                        break;
                    float e2 = 2 * err;
                    if (e2 > -dy)
                    {
                        err -= dy;
                        x0 += sx;
                    }
                    if (e2 < dx)
                    {
                        err += dx;
                        y0 += sy;
                    }
                }
            }
            else
            {
                while (true)
                {
                    if (draw)
                    {
                        points.Add(new Vector2(x0, y0));
                    }

                    if (x0 == x1 && y0 == y1)
                    {
                        break;
                    }

                    float e2 = 2 * err;
                    if (e2 > -dy)
                    {
                        err -= dy;
                        x0 += sx;
                    }
                    if (e2 < dx)
                    {
                        err += dx;
                        y0 += sy;
                    }

                    totalLength += 1f;

                    if (draw && totalLength >= dashLength)
                    {
                        draw = false;
                        totalLength = 0f;
                    }
                    else if (!draw && totalLength >= gapLength)
                    {
                        draw = true;
                        totalLength = 0f;
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// Ultra-simplified method to draw a line using DrawPrimitive without freezing
        /// </summary>
        public void DrawBresenhamLine(
            CanvasItem canvas,
            Vector2 from,
            Vector2 to,
            Color color,
            float thickness = 1f
        )
        {
            // For zero or negative thickness, don't draw anything
            if (thickness <= 0)
                return;

            // For very thin lines, use the simple approach
            if (thickness <= 1f)
            {
                Vector2[] points = new Vector2[2];
                points[0] = from;
                points[1] = to;

                canvas.DrawPrimitive(
                    points,
                    new Color[] { color, color },
                    new Vector2[] { Vector2.Zero, Vector2.One }
                );
                return;
            }

            // For thicker lines, create a quad (rectangle) with the desired thickness
            Vector2 direction = (to - from).Normalized();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * (thickness / 2f);

            // Create four corners of the rectangle
            Vector2[] thickPoints = new Vector2[4];
            thickPoints[0] = from + perpendicular; // Top left
            thickPoints[1] = from - perpendicular; // Bottom left
            thickPoints[2] = to - perpendicular; // Bottom right
            thickPoints[3] = to + perpendicular; // Top right

            // Colors and UVs for the quad
            Color[] colors = new Color[] { color, color, color, color };
            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0), // Top left
                new Vector2(0, 1), // Bottom left
                new Vector2(1, 1), // Bottom right
                new Vector2(1, 0), // Top right
            };

            // Draw the thick line as a filled quad
            canvas.DrawPrimitive(thickPoints, colors, uvs);
        }

        /// <summary>
        /// Ultra-simplified method to draw polygon outlines without freezing
        /// </summary>
        public void DrawBresenhamLinePoints(
            CanvasItem canvas,
            Vector2[] points,
            Color color,
            float thickness = 1f,
            bool closed = true
        )
        {
            int count = points.Length;
            if (count < 2)
                return;

            int limit = closed ? count : count - 1;

            // Draw each segment with minimal operations
            for (int i = 0; i < limit; i++)
            {
                Vector2 from = points[i];
                Vector2 to = points[(i + 1) % count];

                // Use the simplified line method
                DrawBresenhamLine(canvas, from, to, color, thickness);
            }
        }
    }
}
