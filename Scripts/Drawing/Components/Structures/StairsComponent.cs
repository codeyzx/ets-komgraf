using Core;
using Drawing.Components.Building;
using Drawing.Configuration;
using Godot;

namespace Drawing.Components.Structures
{
    /// <summary>
    /// Component for drawing the stairs structure of the building.
    /// </summary>
    public class StairsComponent : BuildingComponent
    {
        private readonly Color _primaryColor;
        private readonly Color _outlineColor;
        private readonly float _outlineThickness;
        private readonly float _stairHeight;
        private readonly int _columnCount;

        /// <summary>
        /// Initializes a new instance of the StairsComponent class.
        /// </summary>
        public StairsComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color primaryColor,
            Color outlineColor,
            float outlineThickness,
            float stairHeight,
            int columnCount
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _primaryColor = primaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            _stairHeight = stairHeight;
            _columnCount = columnCount;
        }

        /// <summary>
        /// Draws the stair structure of the building.
        /// </summary>
        public override void Draw()
        {
            BuildingDimensions dimensions = new BuildingDimensions(
                Dimensions.Center,
                400 * ScaleFactor,
                Dimensions.HouseHeight,
                Dimensions.WallHeight,
                Dimensions.RoofHeight
            );

            float centerHeight = 20 * ScaleFactor;

            DrawStairColumns(dimensions, centerHeight);
            DrawStairBeams(dimensions);
        }

        /// <summary>
        /// Draws the vertical columns of the stair structure.
        /// </summary>
        private void DrawStairColumns(BuildingDimensions dimensions, float centerHeight)
        {
            float columnWidth = 10 * ScaleFactor;
            float margin = 40 * ScaleFactor;
            float columnSpacing = (dimensions.HouseWidth - 2 * margin) / (_columnCount - 1);

            for (int i = 0; i < _columnCount; i++)
            {
                float xPos = dimensions.HousePosition.X + margin + i * columnSpacing;

                Vector2[] column = new Vector2[]
                {
                    new Vector2(
                        xPos - columnWidth / 2,
                        centerHeight + dimensions.RoofBaseY + dimensions.WallHeight
                    ),
                    new Vector2(
                        xPos + columnWidth / 2,
                        centerHeight + dimensions.RoofBaseY + dimensions.WallHeight
                    ),
                    new Vector2(
                        xPos + columnWidth / 2,
                        centerHeight
                            + dimensions.RoofBaseY
                            + dimensions.WallHeight
                            + _stairHeight * ScaleFactor
                    ),
                    new Vector2(
                        xPos - columnWidth / 2,
                        centerHeight
                            + dimensions.RoofBaseY
                            + dimensions.WallHeight
                            + _stairHeight * ScaleFactor
                    ),
                };

                Canvas.DrawPolygon(column, new Color[] { _primaryColor });
                DrawOutline(column, ScaleFactor, _outlineColor);
            }
        }

        /// <summary>
        /// Draws the horizontal beams of the stair structure.
        /// </summary>
        private void DrawStairBeams(BuildingDimensions dimensions)
        {
            float beamHeight = 10 * ScaleFactor;
            Vector2 center = new Vector2(
                dimensions.HousePosition.X + dimensions.HouseWidth / 2,
                dimensions.HousePosition.Y
            );
            float stairWidth = dimensions.HouseWidth - 20 * ScaleFactor;
            float offsetY = 35;
            float margin = 30;

            // Upper beam
            Vector2[] upperBeam = CreateBeam(
                center,
                stairWidth,
                beamHeight,
                dimensions.HousePosition.Y + dimensions.HouseHeight - 10 - beamHeight - offsetY,
                margin
            );

            Canvas.DrawPolygon(upperBeam, new Color[] { _primaryColor });
            DrawOutline(upperBeam, ScaleFactor, _outlineColor);

            // Lower beam
            Vector2[] lowerBeam = CreateBeam(
                center,
                stairWidth,
                beamHeight,
                dimensions.HousePosition.Y + dimensions.HouseHeight - 10 - beamHeight,
                margin
            );

            Canvas.DrawPolygon(lowerBeam, new Color[] { _primaryColor });
            DrawOutline(lowerBeam, ScaleFactor, _outlineColor);
        }

        /// <summary>
        /// Creates a beam shape with the specified parameters.
        /// </summary>
        private Vector2[] CreateBeam(
            Vector2 center,
            float width,
            float height,
            float yPosition,
            float margin
        )
        {
            return new Vector2[]
            {
                new Vector2(center.X - width / 2 * ScaleFactor + margin, yPosition),
                new Vector2(center.X - width / 2 * ScaleFactor + width + 5, yPosition),
                new Vector2(center.X - width / 2 * ScaleFactor + width + 5, yPosition + height),
                new Vector2(center.X - width / 2 * ScaleFactor + margin, yPosition + height),
            };
        }
    }
}
