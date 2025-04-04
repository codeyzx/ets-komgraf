using Core;
using Drawing.Configuration;
using Godot;

namespace Drawing.Components.Building
{
    /// <summary>
    /// Base class for all building components.
    /// </summary>
    public abstract class BuildingComponent
    {
        protected readonly Primitif Primitif;
        protected readonly CanvasItem Canvas;
        protected readonly float ScaleFactor;
        protected readonly BuildingDimensions Dimensions;

        /// <summary>
        /// Initializes a new instance of the BuildingComponent class.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="primitif">The primitif drawing utility.</param>
        /// <param name="dimensions">The dimensions of the building.</param>
        /// <param name="scaleFactor">The scale factor for drawing.</param>
        protected BuildingComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor
        )
        {
            Canvas = canvas;
            Primitif = primitif;
            Dimensions = dimensions;
            ScaleFactor = scaleFactor;
        }

        /// <summary>
        /// Draws the component.
        /// </summary>
        public abstract void Draw();

        /// <summary>
        /// Draws an outline around a polygon.
        /// </summary>
        protected void DrawOutline(Vector2[] points, float thickness, Color? color = null)
        {
            Primitif.DrawBresenhamLinePoints(
                Canvas,
                points,
                color ?? Colors.Black,
                thickness,
                true
            );
        }
    }
}
