using System;
using System.Collections.Generic;
using Core;
using Godot;

namespace UI
{
    /// <summary>
    /// Renders a traditional building structure with customizable properties.
    /// </summary>
    public partial class Karya3 : Node2D
    {
        // Color configuration
        [Export]
        private Color PrimaryColor { get; set; } = new Color("#C63651");

        [Export]
        private Color SecondaryColor { get; set; } = new Color("#F0718A");

        [Export]
        private Color OutlineColor { get; set; } = Colors.Black;

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
        private float OutlineThickness { get; set; } = 4;

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

        // Cached values for current drawing session
        private Vector2 _center;
        private float _scaleFactor;
        private BuildingDimensions _dimensions;

        // Instance of Primitif for line drawing
        private Primitif _primitif = new Primitif();

        /// <summary>
        /// Draws the building with all its components.
        /// </summary>
        public override void _Draw()
        {
            InitializeDrawingParameters();

            DrawMainHouseBody();
            DrawMainHouseBodyBackground();
            DrawCentralStructure();
            DrawRoof();
            DrawCentralStructure(isTop: true);
            DrawStairs();
            DrawLadder();
        }

        /// <summary>
        /// Initializes drawing parameters based on the current viewport size.
        /// </summary>
        private void InitializeDrawingParameters()
        {
            Vector2 windowSize = GetViewportRect().Size;
            _scaleFactor = Math.Min(windowSize.X / 800f, windowSize.Y / 600f);
            _center = windowSize / 2;

            // Calculate and cache building dimensions
            _dimensions = new BuildingDimensions(
                _center,
                BaseWidth * _scaleFactor,
                BaseHeight * _scaleFactor,
                WallHeight * _scaleFactor,
                RoofHeight * _scaleFactor
            );
        }

        /// <summary>
        /// Draws the ladder/stairs on the right side of the building
        /// </summary>
        private void DrawLadder()
        {
            // Position the ladder on the right side
            float ladderWidth = LadderWidth * _scaleFactor;
            float ladderLength = LadderLength * _scaleFactor;

            // Starting position for the ladder (right side of the building)
            float rightEdgeX =
                _dimensions.HousePosition.X + _dimensions.HouseWidth - 20 * _scaleFactor;
            float startY = _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;

            // Calculate end points of the ladder with steeper angle
            Vector2 ladderStart = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
            Vector2 ladderEnd = new Vector2(
                rightEdgeX + ladderLength,
                startY + ladderLength * 1.5f
            );

            // Calculate second rail slightly offset from the first
            Vector2 railOffset = new Vector2(0, 10 * _scaleFactor);

            // Draw fill polygon for the ladder (to match the red color in the image)
            List<Vector2> ladderPolygon = new List<Vector2>
            {
                ladderStart,
                ladderEnd,
                ladderEnd + railOffset,
                ladderStart + railOffset,
            };

            DrawPolygon(ladderPolygon.ToArray(), new Color[] { SecondaryColor });
            DrawOutline(ladderPolygon.ToArray(), OutlineThickness * _scaleFactor * 0.5f);
        }

        /// <summary>
        /// Draws the roof of the building with a curved top.
        /// </summary>
        private void DrawRoof()
        {
            float roofLeft = _dimensions.HousePosition.X + 10 * _scaleFactor;
            float roofRight =
                _dimensions.HousePosition.X + _dimensions.HouseWidth - 10 * _scaleFactor;
            float roofWidth = roofRight - roofLeft;
            float roofBaseY = _dimensions.RoofBaseY;
            float height = RoofHeight * _scaleFactor;

            List<Vector2> roofPoints = new List<Vector2>();

            // Generate curved roof points
            for (int i = 0; i <= RoofSegments; i++)
            {
                float t = (float)i / RoofSegments;
                float x = roofLeft + roofWidth * t;
                // Parabolic curve for the roof
                float y = roofBaseY - height - (4 * height * (float)Math.Pow(t - 0.5, 2));
                roofPoints.Add(new Vector2(x, y));
            }

            // Complete the polygon
            roofPoints.Add(new Vector2(roofRight, roofBaseY));
            roofPoints.Add(new Vector2(roofLeft, roofBaseY));

            DrawPolygon(roofPoints.ToArray(), new Color[] { PrimaryColor });
            DrawOutline(roofPoints.ToArray(), OutlineThickness * _scaleFactor);
        }

        /// <summary>
        /// Draws the main body of the house with trapezoidal shape.
        /// </summary>
        private void DrawMainHouseBody()
        {
            // Perbesar topWidth dengan mengurangi pengurangan dari 60 menjadi 40
            float topWidth = _dimensions.HouseWidth - 44 * _scaleFactor;

            // Buat perbandingan bottomWidth lebih kecil terhadap topWidth (0.8f)
            float bottomWidth = topWidth * 0.8f;

            float topLeftX = _dimensions.HousePosition.X + 22 * _scaleFactor;
            float bottomLeftX = topLeftX + (topWidth - bottomWidth) / 2;

            // Tambahkan tinggi ekstra untuk bagian bawah (misal 1.5x dari tinggi asli)
            float extendedWallHeight = _dimensions.WallHeight + 11;

            // Create the trapezoidal wall dengan extendedWallHeight
            Vector2[] mainWall = new Vector2[]
            {
                new Vector2(topLeftX, _dimensions.RoofBaseY),
                new Vector2(topLeftX + topWidth, _dimensions.RoofBaseY),
                new Vector2(
                    bottomLeftX + bottomWidth,
                    _dimensions.RoofBaseY + extendedWallHeight // Pakai extendedWallHeight
                ),
                new Vector2(bottomLeftX, _dimensions.RoofBaseY + extendedWallHeight), // Pakai extendedWallHeight
            };

            // Draw the main wall
            DrawPolygon(mainWall, new Color[] { PrimaryColor });

            // Draw the black outline
            DrawOutline(mainWall, OutlineThickness * _scaleFactor);
        }

        /// <summary>
        /// Draws the background main body of the house with trapezoidal shape.
        /// </summary>
        private void DrawMainHouseBodyBackground()
        {
            float topWidth = _dimensions.HouseWidth - 60 * _scaleFactor;
            float bottomWidth = topWidth * 0.8f;
            float topLeftX = _dimensions.HousePosition.X + 30 * _scaleFactor;
            float bottomLeftX = topLeftX + (topWidth - bottomWidth) / 2;

            // Tambahkan tinggi ekstra untuk bagian bawah (misal 1.5x dari tinggi asli)
            float extendedWallHeight = _dimensions.WallHeight + 3;

            // Create the trapezoidal wall
            Vector2[] mainWall = new Vector2[]
            {
                new Vector2(topLeftX, _dimensions.RoofBaseY),
                new Vector2(topLeftX + topWidth, _dimensions.RoofBaseY),
                new Vector2(bottomLeftX + bottomWidth, _dimensions.RoofBaseY + extendedWallHeight),
                new Vector2(bottomLeftX, _dimensions.RoofBaseY + extendedWallHeight),
            };

            // Draw the main wall
            DrawPolygon(mainWall, new Color[] { PrimaryColor });

            // Draw the black outline
            DrawOutline(mainWall, OutlineThickness * _scaleFactor);

            // Draw windows (using the same dimensions as the main wall)
            DrawWindows(topLeftX, bottomLeftX, topWidth, bottomWidth + 5);
        }

        /// <summary>
        /// Draws windows with vertical lines on the main house body.
        /// </summary>
        private void DrawWindows(
            float topLeftX,
            float bottomLeftX,
            float topWidth,
            float bottomWidth
        )
        {
            // Use the same dimensions as the main wall for full integration
            float windowTopY = _dimensions.RoofBaseY;
            float windowBottomY = _dimensions.RoofBaseY + _dimensions.WallHeight;

            // Calculate horizontal spacing for vertical lines
            float spacing = topWidth / (WindowLineCount + 1);

            // Draw straight vertical lines
            for (int i = 1; i <= WindowLineCount; i++)
            {
                float x = topLeftX + i * spacing;

                // Use the updated DrawBresenhamLine method that uses DrawPrimitive
                _primitif.DrawBresenhamLine(
                    this,
                    new Vector2(x, windowTopY),
                    new Vector2(x, windowBottomY),
                    OutlineColor,
                    OutlineThickness * _scaleFactor
                );
            }
        }

        /// <summary>
        /// Draws decorative panels on the walls.
        /// </summary>
        private void DrawWallPanels(
            float baseY,
            float topLeftX,
            float topWidth,
            float bottomLeftX = 0,
            float bottomWidth = 0
        )
        {
            float panelWidth = 10 * _scaleFactor;

            if (bottomLeftX == 0 && bottomWidth == 0)
            {
                bottomLeftX = topLeftX;
                bottomWidth = topWidth;
            }

            for (int i = 0; i < PanelCount; i++)
            {
                float t = (float)i / (PanelCount - 1);

                float topX = topLeftX + t * topWidth;
                float bottomX = bottomLeftX + t * bottomWidth;

                Vector2[] panel = new Vector2[]
                {
                    new Vector2(topX, baseY + 5 * _scaleFactor),
                    new Vector2(topX + panelWidth, baseY + 5 * _scaleFactor),
                    new Vector2(
                        bottomX + panelWidth * (bottomWidth / topWidth),
                        baseY + _dimensions.WallHeight - 5 * _scaleFactor
                    ),
                    new Vector2(bottomX, baseY + _dimensions.WallHeight - 5 * _scaleFactor),
                };

                DrawPolygon(panel, new Color[] { OutlineColor });
            }
        }

        /// <summary>
        /// Draws the central horizontal structure of the building.
        /// </summary>
        private void DrawCentralStructure(bool isTop = false)
        {
            float structureWidth = isTop ? 480 * _scaleFactor : 400 * _scaleFactor;
            Vector2 structureCenter = isTop
                ? _center - new Vector2(0, 100 * _scaleFactor)
                : _center + new Vector2(0, 10 * _scaleFactor);

            BuildingDimensions dimensions = new BuildingDimensions(
                structureCenter,
                structureWidth,
                _dimensions.HouseHeight,
                _dimensions.WallHeight,
                RoofHeight * _scaleFactor
            );

            float centerHeight = 10 * _scaleFactor;
            float margin = 25 * _scaleFactor;

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

            DrawPolygon(centerStructure, new Color[] { SecondaryColor });
            DrawOutline(centerStructure, OutlineThickness * _scaleFactor);
        }

        /// <summary>
        /// Draws the stair structure of the building.
        /// </summary>
        private void DrawStairs()
        {
            BuildingDimensions dimensions = new BuildingDimensions(
                _center,
                400 * _scaleFactor,
                _dimensions.HouseHeight,
                _dimensions.WallHeight,
                RoofHeight * _scaleFactor
            );

            float centerHeight = 20 * _scaleFactor;

            DrawStairColumns(dimensions, centerHeight);
            DrawStairBeams(dimensions);
        }

        /// <summary>
        /// Draws the vertical columns of the stair structure.
        /// </summary>
        private void DrawStairColumns(BuildingDimensions dimensions, float centerHeight)
        {
            float columnWidth = 10 * _scaleFactor;
            float margin = 40 * _scaleFactor;
            float columnSpacing = (dimensions.HouseWidth - 2 * margin) / (ColumnCount - 1);

            for (int i = 0; i < ColumnCount; i++)
            {
                float xPos = dimensions.HousePosition.X + margin + i * columnSpacing;

                Vector2[] column = new Vector2[]
                {
                    new Vector2(
                        xPos - columnWidth / 2,
                        centerHeight + dimensions.RoofBaseY + dimensions.WallHeight
                    ),
                    new Vector2(
                        xPos + columnWidth / 2,
                        centerHeight + dimensions.RoofBaseY + dimensions.WallHeight
                    ),
                    new Vector2(
                        xPos + columnWidth / 2,
                        centerHeight
                            + dimensions.RoofBaseY
                            + dimensions.WallHeight
                            + StairHeight * _scaleFactor
                    ),
                    new Vector2(
                        xPos - columnWidth / 2,
                        centerHeight
                            + dimensions.RoofBaseY
                            + dimensions.WallHeight
                            + StairHeight * _scaleFactor
                    ),
                };

                DrawPolygon(column, new Color[] { PrimaryColor });
                DrawOutline(column, _scaleFactor);
            }
        }

        /// <summary>
        /// Draws the horizontal beams of the stair structure.
        /// </summary>
        private void DrawStairBeams(BuildingDimensions dimensions)
        {
            float beamHeight = 10 * _scaleFactor;
            Vector2 center = new Vector2(
                dimensions.HousePosition.X + dimensions.HouseWidth / 2,
                dimensions.HousePosition.Y
            );
            float stairWidth = dimensions.HouseWidth - 20 * _scaleFactor;
            float offsetY = 35;
            float margin = 30;

            // Upper beam
            Vector2[] upperBeam = CreateBeam(
                center,
                stairWidth,
                beamHeight,
                dimensions.HousePosition.Y + dimensions.HouseHeight - 10 - beamHeight - offsetY,
                margin
            );

            DrawPolygon(upperBeam, new Color[] { PrimaryColor });
            DrawOutline(upperBeam, _scaleFactor);

            // Lower beam
            Vector2[] lowerBeam = CreateBeam(
                center,
                stairWidth,
                beamHeight,
                dimensions.HousePosition.Y + dimensions.HouseHeight - 10 - beamHeight,
                margin
            );

            DrawPolygon(lowerBeam, new Color[] { PrimaryColor });
            DrawOutline(lowerBeam, _scaleFactor);
        }

        /// <summary>
        /// Creates a beam shape with the specified parameters.
        /// </summary>
        private Vector2[] CreateBeam(
            Vector2 center,
            float width,
            float height,
            float yPosition,
            float margin
        )
        {
            return new Vector2[]
            {
                new Vector2(center.X - width / 2 * _scaleFactor + margin, yPosition),
                new Vector2(center.X - width / 2 * _scaleFactor + width + 5, yPosition),
                new Vector2(center.X - width / 2 * _scaleFactor + width + 5, yPosition + height),
                new Vector2(center.X - width / 2 * _scaleFactor + margin, yPosition + height),
            };
        }

        /// <summary>
        /// Draws an outline around a polygon.
        /// </summary>
        private void DrawOutline(Vector2[] points, float thickness, Color? color = null)
        {
            Color outlineColor = color ?? OutlineColor;

            // Use the updated DrawBresenhamLinePoints method that uses DrawPrimitive
            _primitif.DrawBresenhamLinePoints(this, points, outlineColor, thickness, true);
        }

        /// <summary>
        /// Stores the dimensions of a building component for easier reference.
        /// </summary>
        private struct BuildingDimensions
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
}
