using Drawing.Configuration;
using Drawing.Renderers;
using Godot;

namespace Scenes
{
    /// <summary>
    /// Renders a traditional building structure with customizable properties.
    /// Uses a modular architecture with separate components for different parts of the building.
    /// </summary>
    public partial class Karya2Scene : Node2D
    {
        // Building renderer and configuration
        private BuildingRenderer _buildingRenderer;
        private BuildingConfiguration _config = new BuildingConfiguration();

        // Export properties to make them configurable in the Godot editor
        [Export]
        private Color PrimaryColor
        {
            get => _config.PrimaryColor;
            set => _config.PrimaryColor = value;
        }

        [Export]
        private Color SecondaryColor
        {
            get => _config.SecondaryColor;
            set => _config.SecondaryColor = value;
        }

        [Export]
        private Color OutlineColor
        {
            get => _config.OutlineColor;
            set => _config.OutlineColor = value;
        }

        [Export]
        private float BaseWidth
        {
            get => _config.BaseWidth;
            set => _config.BaseWidth = value;
        }

        [Export]
        private float BaseHeight
        {
            get => _config.BaseHeight;
            set => _config.BaseHeight = value;
        }

        [Export]
        private float RoofHeight
        {
            get => _config.RoofHeight;
            set => _config.RoofHeight = value;
        }

        [Export]
        private float WallHeight
        {
            get => _config.WallHeight;
            set => _config.WallHeight = value;
        }

        [Export]
        private float StairHeight
        {
            get => _config.StairHeight;
            set => _config.StairHeight = value;
        }

        [Export]
        private float OutlineThickness
        {
            get => _config.OutlineThickness;
            set => _config.OutlineThickness = value;
        }

        [Export]
        private int RoofSegments
        {
            get => _config.RoofSegments;
            set => _config.RoofSegments = value;
        }

        [Export]
        private int PanelCount
        {
            get => _config.PanelCount;
            set => _config.PanelCount = value;
        }

        [Export]
        private int ColumnCount
        {
            get => _config.ColumnCount;
            set => _config.ColumnCount = value;
        }

        [Export]
        private int WindowLineCount
        {
            get => _config.WindowLineCount;
            set => _config.WindowLineCount = value;
        }

        [Export]
        private float LadderWidth
        {
            get => _config.LadderWidth;
            set => _config.LadderWidth = value;
        }

        [Export]
        private float LadderLength
        {
            get => _config.LadderLength;
            set => _config.LadderLength = value;
        }

        [Export]
        private int LadderStepCount
        {
            get => _config.LadderStepCount;
            set => _config.LadderStepCount = value;
        }

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            // Initialize the building renderer
            _buildingRenderer = new BuildingRenderer(this, _config);

            // Enable the ladder on the right side of the building
            _buildingRenderer.ShowLadder(true);

            // Force a redraw to render the building
            QueueRedraw();
        }

        /// <summary>
        /// Called when the node needs to be redrawn.
        /// </summary>
        public override void _Draw()
        {
            // Initialize drawing parameters based on the current viewport size
            _buildingRenderer.InitializeDrawingParameters(GetViewportRect().Size);

            // Draw the building with all its components
            _buildingRenderer.Draw();
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
                // Queue a redraw to update the building with the new viewport size
                QueueRedraw();
            }
        }
    }
}
