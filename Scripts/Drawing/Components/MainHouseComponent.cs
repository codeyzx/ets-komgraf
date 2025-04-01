using System.Collections.Generic;
using Core;
using Godot;

namespace Drawing.Components
{
    /// <summary>
    /// Component for drawing the main house body.
    /// </summary>
    public class MainHouseComponent : BuildingComponent
    {
        private readonly Color _primaryColor;
        private readonly Color _outlineColor;
        private readonly float _outlineThickness;
        private readonly int _windowLineCount;

        /// <summary>
        /// Initializes a new instance of the MainHouseComponent class.
        /// </summary>
        public MainHouseComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color primaryColor,
            Color outlineColor,
            float outlineThickness,
            int windowLineCount) : base(canvas, primitif, dimensions, scaleFactor)
        {
            _primaryColor = primaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            _windowLineCount = windowLineCount;
        }

        /// <summary>
        /// Draws the main body of the house with trapezoidal shape.
        /// </summary>
        public override void Draw()
        {
            DrawMainHouseBody();
            DrawMainHouseBodyBackground();
        }

        /// <summary>
        /// Draws the main body of the house with trapezoidal shape.
        /// </summary>
        private void DrawMainHouseBody()
        {
            // Perbesar topWidth dengan mengurangi pengurangan dari 60 menjadi 40
            float topWidth = Dimensions.HouseWidth - 44 * ScaleFactor;

            // Buat perbandingan bottomWidth lebih kecil terhadap topWidth (0.8f)
            float bottomWidth = topWidth * 0.8f;

            float topLeftX = Dimensions.HousePosition.X + 22 * ScaleFactor;
            float bottomLeftX = topLeftX + (topWidth - bottomWidth) / 2;

            // Tambahkan tinggi ekstra untuk bagian bawah
            float extendedWallHeight = Dimensions.WallHeight + 11;

            // Create the trapezoidal wall dengan extendedWallHeight
            Vector2[] mainWall = new Vector2[]
            {
                new Vector2(topLeftX, Dimensions.RoofBaseY),
                new Vector2(topLeftX + topWidth, Dimensions.RoofBaseY),
                new Vector2(
                    bottomLeftX + bottomWidth,
                    Dimensions.RoofBaseY + extendedWallHeight
                ),
                new Vector2(bottomLeftX, Dimensions.RoofBaseY + extendedWallHeight),
            };

            // Draw the main wall
            Canvas.DrawPolygon(mainWall, new Color[] { _primaryColor });

            // Draw the black outline
            DrawOutline(mainWall, _outlineThickness * ScaleFactor, _outlineColor);
        }

        /// <summary>
        /// Draws the background main body of the house with trapezoidal shape.
        /// </summary>
        private void DrawMainHouseBodyBackground()
        {
            float topWidth = Dimensions.HouseWidth - 60 * ScaleFactor;
            float bottomWidth = topWidth * 0.8f;
            float topLeftX = Dimensions.HousePosition.X + 30 * ScaleFactor;
            float bottomLeftX = topLeftX + (topWidth - bottomWidth) / 2;

            // Tambahkan tinggi ekstra untuk bagian bawah
            float extendedWallHeight = Dimensions.WallHeight + 3;

            // Create the trapezoidal wall
            Vector2[] mainWall = new Vector2[]
            {
                new Vector2(topLeftX, Dimensions.RoofBaseY),
                new Vector2(topLeftX + topWidth, Dimensions.RoofBaseY),
                new Vector2(bottomLeftX + bottomWidth, Dimensions.RoofBaseY + extendedWallHeight),
                new Vector2(bottomLeftX, Dimensions.RoofBaseY + extendedWallHeight),
            };

            // Draw the main wall
            Canvas.DrawPolygon(mainWall, new Color[] { _primaryColor });

            // Draw the black outline
            DrawOutline(mainWall, _outlineThickness * ScaleFactor, _outlineColor);

            // Draw windows (using the same dimensions as the main wall)
            DrawWindows(topLeftX, bottomLeftX, topWidth, bottomWidth + 5);
        }

        /// <summary>
        /// Draws windows with vertical lines on the main house body.
        /// </summary>
        private void DrawWindows(
            float topLeftX,
            float bottomLeftX,
            float topWidth,
            float bottomWidth
        )
        {
            // Use the same dimensions as the main wall for full integration
            float windowTopY = Dimensions.RoofBaseY;
            float windowBottomY = Dimensions.RoofBaseY + Dimensions.WallHeight;

            // Calculate horizontal spacing for vertical lines
            float spacing = topWidth / (_windowLineCount + 1);

            // Draw straight vertical lines
            for (int i = 1; i <= _windowLineCount; i++)
            {
                float x = topLeftX + i * spacing;

                // Use the updated DrawBresenhamLine method that uses DrawPrimitive
                Primitif.DrawBresenhamLine(
                    Canvas,
                    new Vector2(x, windowTopY),
                    new Vector2(x, windowBottomY),
                    _outlineColor,
                    _outlineThickness * ScaleFactor
                );
            }
        }
    }
}
