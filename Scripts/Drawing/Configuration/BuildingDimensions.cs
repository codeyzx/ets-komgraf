using Godot;

namespace Drawing.Configuration
{
    /// <summary>
    /// Stores the dimensions of a building component for easier reference.
    /// </summary>
    public struct BuildingDimensions
    {
        public Vector2 Center { get; }
        public Vector2 HousePosition { get; }
        public float HouseWidth { get; }
        public float HouseHeight { get; }
        public float WallHeight { get; }
        public float RoofHeight { get; }
        public float RoofBaseY { get; }

        public BuildingDimensions(
            Vector2 center,
            float width,
            float height,
            float wallHeight,
            float roofHeight
        )
        {
            Center = center;
            HouseWidth = width;
            HouseHeight = height;
            WallHeight = wallHeight;
            RoofHeight = roofHeight;
            HousePosition = center - new Vector2(width / 2, height / 2);
            RoofBaseY = HousePosition.Y + height - roofHeight - 120;
        }
    }
}
