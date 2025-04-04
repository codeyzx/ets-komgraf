using System.Collections.Generic;
using Core;
using Drawing.Components.Building;
using Drawing.Configuration;
using Godot;

namespace Drawing.Components.Structures
{
    /// <summary>
    /// Component for drawing the ladder on the right side of the building.
    /// </summary>
    public class LadderComponent : BuildingComponent
    {
        private readonly Color _secondaryColor;
        private readonly Color _outlineColor;
        private readonly float _outlineThickness;
        private readonly float _ladderWidth;
        private readonly float _ladderLength;

        /// <summary>
        /// Initializes a new instance of the LadderComponent class.
        /// </summary>
        public LadderComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color secondaryColor,
            Color outlineColor,
            float outlineThickness,
            float ladderWidth,
            float ladderLength
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _secondaryColor = secondaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            _ladderWidth = ladderWidth;
            _ladderLength = ladderLength;
        }

        /// <summary>
        /// Draws the ladder/stairs on the right side of the building.
        /// </summary>
        public override void Draw()
        {
            // Position the ladder on the right side
            float ladderWidth = _ladderWidth * ScaleFactor;
            float ladderLength = _ladderLength * ScaleFactor;

            // Starting position for the ladder (right side of the building)
            float rightEdgeX =
                Dimensions.HousePosition.X + Dimensions.HouseWidth - 20 * ScaleFactor;
            float startY = Dimensions.RoofBaseY + Dimensions.WallHeight * ScaleFactor;

            // Calculate end points of the ladder with steeper angle
            Vector2 ladderStart = new Vector2(rightEdgeX - 30 * ScaleFactor, startY);
            Vector2 ladderEnd = new Vector2(
                rightEdgeX + ladderLength,
                startY + ladderLength * 1.5f
            );

            // Calculate second rail slightly offset from the first
            Vector2 railOffset = new Vector2(0, 10 * ScaleFactor);

            // Draw fill polygon for the ladder (to match the red color in the image)
            List<Vector2> ladderPolygon = new List<Vector2>
            {
                ladderStart,
                ladderEnd,
                ladderEnd + railOffset,
                ladderStart + railOffset,
            };

            Canvas.DrawPolygon(ladderPolygon.ToArray(), [_secondaryColor]);
            DrawOutline(
                ladderPolygon.ToArray(),
                _outlineThickness * ScaleFactor * 0.5f,
                _outlineColor
            );
        }
    }
}
