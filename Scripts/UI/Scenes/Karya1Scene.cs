using Drawing.Configuration;
using Drawing.Renderers;
using Godot;

namespace Scenes
{
    /// <summary>
    /// Renders a sketch of a traditional Bolon house structure using only line primitives.
    /// </summary>
    public partial class Karya1Scene : Node2D
    {
        // Building renderer and configuration
        private SketchRenderer _sketchRenderer;
        private BuildingConfiguration _config = new BuildingConfiguration();

        // Structure configuration
        [Export]
        private float BaseWidth { get; set; } = 450;

        [Export]
        private float BaseHeight { get; set; } = 250;

        [Export]
        private float RoofHeight { get; set; } = 80;

        [Export]
        private float WallHeight { get; set; } = 100;

        [Export]
        private float StairHeight { get; set; } = 100;

        // Drawing configuration
        [Export]
        private float OutlineThickness { get; set; } = 2;

        [Export]
        private int RoofSegments { get; set; } = 30;

        [Export]
        private int PanelCount { get; set; } = 8;

        [Export]
        private int ColumnCount { get; set; } = 9;

        [Export]
        private int WindowLineCount { get; set; } = 7;

        // Ladder configuration
        [Export]
        private float LadderWidth { get; set; } = 15;

        [Export]
        private float LadderLength { get; set; } = 70;

        [Export]
        private int LadderStepCount { get; set; } = 5;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            // Update configuration from exported properties
            UpdateConfiguration();

            // Initialize the sketch renderer
            _sketchRenderer = new SketchRenderer(this, _config);

            // Force a redraw to render the sketch
            QueueRedraw();
        }

        /// <summary>
        /// Updates the configuration with the exported properties.
        /// </summary>
        private void UpdateConfiguration()
        {
            _config.BaseWidth = BaseWidth;
            _config.BaseHeight = BaseHeight;
            _config.RoofHeight = RoofHeight;
            _config.WallHeight = WallHeight;
            _config.StairHeight = StairHeight;
            _config.OutlineThickness = OutlineThickness;
            _config.RoofSegments = RoofSegments;
            _config.PanelCount = PanelCount;
            _config.ColumnCount = ColumnCount;
            _config.WindowLineCount = WindowLineCount;
            _config.LadderWidth = LadderWidth;
            _config.LadderLength = LadderLength;
            _config.LadderStepCount = LadderStepCount;
        }

        /// <summary>
        /// Called when the node needs to be redrawn.
        /// </summary>
        public override void _Draw()
        {
            // Initialize drawing parameters based on the current viewport size
            _sketchRenderer.InitializeDrawingParameters(GetViewportRect().Size);

            // Draw the building sketch with all its components
            _sketchRenderer.Draw();
        }

        /// <summary>
        /// Called when the viewport size changes.
        /// </summary>
        public override void _Notification(int what)
        {
            base._Notification(what);

            // Check if the notification is for a resize
            if (what == NotificationWMSizeChanged)
            {
                // Queue a redraw to update the sketch with the new viewport size
                QueueRedraw();
            }
        }
    }
}
