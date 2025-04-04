using System;
using Godot;

namespace Core
{
    /// <summary>
    /// Provides primitive drawing operations with optimized performance.
    /// </summary>
    public partial class Primitif : RefCounted
    {
        /// <summary>
        /// Draws a line using Bresenham's algorithm with DrawPrimitive for optimal performance.
        /// </summary>
        /// <param name="canvas">The canvas item to draw on.</param>
        /// <param name="from">Starting point of the line.</param>
        /// <param name="to">Ending point of the line.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thickness of the line in pixels.</param>
        public void DrawBresenhamLine(
            CanvasItem canvas,
            Vector2 from,
            Vector2 to,
            Color color,
            float thickness = 1f
        )
        {
            ArgumentNullException.ThrowIfNull(canvas);

            // For zero or negative thickness, don't draw anything
            if (thickness <= 0)
                return;

            // For very thin lines, use the simple approach
            if (thickness <= 1f)
            {
                DrawThinLine(canvas, from, to, color);
                return;
            }

            // For thicker lines, create a quad (rectangle) with the desired thickness
            DrawThickLine(canvas, from, to, color, thickness);
        }

        /// <summary>
        /// Draws a thin line (thickness <= 1) using a simple two-point primitive.
        /// </summary>
        private static void DrawThinLine(CanvasItem canvas, Vector2 from, Vector2 to, Color color)
        {
            Vector2[] points = [from, to];
            canvas.DrawPrimitive(points, [color, color], [Vector2.Zero, Vector2.One]);
        }

        /// <summary>
        /// Draws a thick line as a filled quad with the specified thickness.
        /// </summary>
        private static void DrawThickLine(
            CanvasItem canvas,
            Vector2 from,
            Vector2 to,
            Color color,
            float thickness
        )
        {
            Vector2 direction = (to - from).Normalized();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * (thickness / 2f);

            // Create four corners of the rectangle
            Vector2[] thickPoints =
            [
                from + perpendicular, // Top left
                from - perpendicular, // Bottom left
                to - perpendicular, // Bottom right
                to + perpendicular, // Top right
            ];

            // Colors and UVs for the quad
            Color[] colors = [color, color, color, color];
            Vector2[] uvs =
            [
                new Vector2(0, 0), // Top left
                new Vector2(0, 1), // Bottom left
                new Vector2(1, 1), // Bottom right
                new Vector2(1, 0), // Top right
            ];

            // Draw the thick line as a filled quad
            canvas.DrawPrimitive(thickPoints, colors, uvs);
        }

        /// <summary>
        /// Draws polygon outlines using Bresenham's algorithm for each edge.
        /// </summary>
        /// <param name="canvas">The canvas item to draw on.</param>
        /// <param name="points">Array of points defining the polygon vertices.</param>
        /// <param name="color">Color of the outline.</param>
        /// <param name="thickness">Thickness of the outline in pixels.</param>
        /// <param name="closed">Whether to close the polygon by connecting the last point to the first.</param>
        public void DrawBresenhamLinePoints(
            CanvasItem canvas,
            Vector2[] points,
            Color color,
            float thickness = 1f,
            bool closed = true
        )
        {
            ArgumentNullException.ThrowIfNull(canvas);
            ArgumentNullException.ThrowIfNull(points);

            if (points.Length < 2)
                return;

            int count = points.Length;
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
