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
        private readonly float _ladderLength;
        private readonly bool _isLadderAnimating;

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
            float ladderLength,
            bool isLadderAnimating
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _secondaryColor = secondaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            _ladderLength = ladderLength;
            _isLadderAnimating = isLadderAnimating;
        }

        /// <summary>
        /// Draws the ladder/stairs on the right side of the building.
        /// </summary>
        public override void Draw()
        {
            // Position the ladder on the right side
            float ladderLength = _ladderLength * ScaleFactor;

            // Use percentages of building dimensions rather than fixed offsets
            float rightEdgeX =
                Dimensions.HousePosition.X + Dimensions.HouseWidth - (20 * ScaleFactor);

            // Start from the base of the roof plus an additional offset to move it lower
            float startY =
                Dimensions.RoofBaseY
                + ladderLength
                + (
                    _isLadderAnimating
                        ? DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen
                            ? 40 * ScaleFactor
                            : 30 * ScaleFactor
                        : 40 * ScaleFactor
                ); // Added 50 units offset

            // Calculate ladder start and end points using relative positioning
            Vector2 ladderStart = new Vector2(rightEdgeX - (30 * ScaleFactor), startY);

            // Create consistent end point that scales properly with the viewport
            Vector2 ladderEnd = new Vector2(
                rightEdgeX + ladderLength,
                startY + (ladderLength * 1.5f) // Increased multiplier to 1.5f for steeper angle
            );

            Vector2 railOffset = new Vector2(0, 10 * ScaleFactor);

            // Draw fill polygon for the ladder
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
