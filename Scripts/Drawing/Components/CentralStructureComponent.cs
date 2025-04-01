using Core;
using Godot;

namespace Drawing.Components
{
    /// <summary>
    /// Component for drawing the central horizontal structure of the building.
    /// </summary>
    public class CentralStructureComponent : BuildingComponent
    {
        private readonly Color _secondaryColor;
        private readonly Color _outlineColor;
        private readonly float _outlineThickness;
        private readonly bool _isTop;

        /// <summary>
        /// Initializes a new instance of the CentralStructureComponent class.
        /// </summary>
        public CentralStructureComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color secondaryColor,
            Color outlineColor,
            float outlineThickness,
            bool isTop = false) : base(canvas, primitif, dimensions, scaleFactor)
        {
            _secondaryColor = secondaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            _isTop = isTop;
        }

        /// <summary>
        /// Draws the central horizontal structure of the building.
        /// </summary>
        public override void Draw()
        {
            float structureWidth = _isTop ? 480 * ScaleFactor : 400 * ScaleFactor;
            Vector2 structureCenter = _isTop
                ? Dimensions.Center - new Vector2(0, 100 * ScaleFactor)
                : Dimensions.Center + new Vector2(0, 10 * ScaleFactor);

            BuildingDimensions dimensions = new BuildingDimensions(
                structureCenter,
                structureWidth,
                Dimensions.HouseHeight,
                Dimensions.WallHeight,
                Dimensions.RoofHeight
            );

            float centerHeight = 10 * ScaleFactor;
            float margin = 25 * ScaleFactor;

            Vector2[] centerStructure = new Vector2[]
            {
                new Vector2(
                    dimensions.HousePosition.X + margin,
                    dimensions.RoofBaseY + dimensions.WallHeight
                ),
                new Vector2(
                    dimensions.HousePosition.X + structureWidth - margin,
                    dimensions.RoofBaseY + dimensions.WallHeight
                ),
                new Vector2(
                    dimensions.HousePosition.X + structureWidth - margin,
                    dimensions.RoofBaseY + dimensions.WallHeight + centerHeight
                ),
                new Vector2(
                    dimensions.HousePosition.X + margin,
                    dimensions.RoofBaseY + dimensions.WallHeight + centerHeight
                ),
            };

            Canvas.DrawPolygon(centerStructure, new Color[] { _secondaryColor });
            DrawOutline(centerStructure, _outlineThickness * ScaleFactor, _outlineColor);
        }
    }
}
