using Godot;

namespace Drawing
{
    /// <summary>
    /// Configuration class for building parameters.
    /// </summary>
    public class BuildingConfiguration
    {
        // Color configuration
        public Color PrimaryColor { get; set; } = new Color("#C63651");
        public Color SecondaryColor { get; set; } = new Color("#F0718A");
        public Color OutlineColor { get; set; } = Colors.Black;

        // Structure configuration
        public float BaseWidth { get; set; } = 450;
        public float BaseHeight { get; set; } = 250;
        public float RoofHeight { get; set; } = 80;
        public float WallHeight { get; set; } = 100;
        public float StairHeight { get; set; } = 100;

        // Drawing configuration
        public float OutlineThickness { get; set; } = 4;
        public int RoofSegments { get; set; } = 30;
        public int PanelCount { get; set; } = 8;
        public int ColumnCount { get; set; } = 9;
        public int WindowLineCount { get; set; } = 7;

        // Ladder configuration
        public float LadderWidth { get; set; } = 15;
        public float LadderLength { get; set; } = 70;
        public int LadderStepCount { get; set; } = 5;
    }
}
