using System;
using System.Collections.Generic;
using Core;
using Drawing.Configuration;
using Godot;

namespace Drawing.Components.Building
{
    /// <summary>
    /// Component for drawing the roof of a building.
    /// </summary>
    public class RoofComponent : BuildingComponent
    {
        private readonly Color _primaryColor;
        private readonly Color _outlineColor;
        private readonly float _roofHeight;
        private readonly float _outlineThickness;
        private readonly int _roofSegments;

        /// <summary>
        /// Initializes a new instance of the RoofComponent class.
        /// </summary>
        public RoofComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color primaryColor,
            Color outlineColor,
            float roofHeight,
            float outlineThickness,
            int roofSegments
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _primaryColor = primaryColor;
            _outlineColor = outlineColor;
            _roofHeight = roofHeight;
            _outlineThickness = outlineThickness;
            _roofSegments = roofSegments;
        }

        /// <summary>
        /// Draws the roof of the building with a curved top.
        /// </summary>
        public override void Draw()
        {
            float roofLeft = Dimensions.HousePosition.X + 10 * ScaleFactor;
            float roofRight = Dimensions.HousePosition.X + Dimensions.HouseWidth - 10 * ScaleFactor;
            float roofWidth = roofRight - roofLeft;
            float roofBaseY = Dimensions.RoofBaseY;
            float height = _roofHeight * ScaleFactor;

            List<Vector2> roofPoints = new List<Vector2>();

            // Generate curved roof points
            for (int i = 0; i <= _roofSegments; i++)
            {
                float t = (float)i / _roofSegments;
                float x = roofLeft + roofWidth * t;
                // Parabolic curve for the roof
                float y = roofBaseY - height - (4 * height * (float)Math.Pow(t - 0.5, 2));
                roofPoints.Add(new Vector2(x, y));
            }

            // Complete the polygon
            roofPoints.Add(new Vector2(roofRight, roofBaseY));
            roofPoints.Add(new Vector2(roofLeft, roofBaseY));

            Canvas.DrawPolygon(roofPoints.ToArray(), new Color[] { _primaryColor });
            DrawOutline(roofPoints.ToArray(), _outlineThickness * ScaleFactor, _outlineColor);
        }
    }
}
