using System.Collections.Generic;
using Core;
using Drawing.Components;
using Godot;

namespace Drawing
{
    /// <summary>
    /// Coordinates the rendering of all building components.
    /// </summary>
    public class BuildingRenderer
    {
        private readonly CanvasItem _canvas;
        private readonly Primitif _primitif;
        private readonly BuildingConfiguration _config;
        private readonly List<BuildingComponent> _components = new List<BuildingComponent>();

        private Vector2 _center;
        private float _scaleFactor;
        private BuildingDimensions _dimensions;

        /// <summary>
        /// Initializes a new instance of the BuildingRenderer class.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="config">The building configuration.</param>
        public BuildingRenderer(CanvasItem canvas, BuildingConfiguration config)
        {
            _canvas = canvas;
            _config = config;
            _primitif = new Primitif();
        }

        /// <summary>
        /// Initializes the drawing parameters based on the current viewport size.
        /// </summary>
        /// <param name="viewportSize">The current viewport size.</param>
        public void InitializeDrawingParameters(Vector2 viewportSize)
        {
            _scaleFactor = System.Math.Min(viewportSize.X / 800f, viewportSize.Y / 600f);
            _center = viewportSize / 2;

            // Calculate and cache building dimensions
            _dimensions = new BuildingDimensions(
                _center,
                _config.BaseWidth * _scaleFactor,
                _config.BaseHeight * _scaleFactor,
                _config.WallHeight * _scaleFactor,
                _config.RoofHeight * _scaleFactor
            );

            // Initialize components
            InitializeComponents();
        }

        /// <summary>
        /// Initializes all building components.
        /// </summary>
        private void InitializeComponents()
        {
            _components.Clear();

            // Add main house component
            _components.Add(
                new MainHouseComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    _config.PrimaryColor,
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    _config.WindowLineCount
                )
            );

            // Add central structure components
            _components.Add(
                new CentralStructureComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    _config.SecondaryColor,
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    false
                )
            );

            // Add roof component
            _components.Add(
                new RoofComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    _config.PrimaryColor,
                    _config.OutlineColor,
                    _config.RoofHeight,
                    _config.OutlineThickness,
                    _config.RoofSegments
                )
            );

            // Add top central structure component
            _components.Add(
                new CentralStructureComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    _config.SecondaryColor,
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    true
                )
            );

            // Add stairs component
            _components.Add(
                new StairsComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    _config.PrimaryColor,
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    _config.StairHeight,
                    _config.ColumnCount
                )
            );

            // Add ladder component
            _components.Add(
                new LadderComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    _config.SecondaryColor,
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    _config.LadderWidth,
                    _config.LadderLength
                )
            );
        }

        /// <summary>
        /// Draws all building components.
        /// </summary>
        public void Draw()
        {
            foreach (var component in _components)
            {
                component.Draw();
            }
        }
    }
}
