using System;
using System.Collections.Generic;
using Core;
using Drawing.Configuration;
using Godot;

namespace Drawing.Renderers
{
    /// <summary>
    /// Renders a sketch of a traditional Bolon house using only line primitives.
    /// </summary>
    public class SketchRenderer
    {
        private readonly CanvasItem _canvas;
        private readonly Primitif _primitif;
        private readonly BuildingConfiguration _config;

        private Vector2 _center;
        private float _scaleFactor;
        private BuildingDimensions _dimensions;

        /// <summary>
        /// Initializes a new instance of the SketchRenderer class.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="config">The building configuration.</param>
        public SketchRenderer(CanvasItem canvas, BuildingConfiguration config)
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
            _scaleFactor = Math.Min(viewportSize.X / 800f, viewportSize.Y / 600f);
            _center = viewportSize / 2;

            // Calculate and cache building dimensions
            _dimensions = new BuildingDimensions(
                _center,
                _config.BaseWidth * _scaleFactor,
                _config.BaseHeight * _scaleFactor,
                _config.WallHeight * _scaleFactor,
                _config.RoofHeight * _scaleFactor
            );
        }

        /// <summary>
        /// Draws the building sketch with all its components.
        /// </summary>
        public void Draw()
        {
            DrawMainHouseOutline();
            DrawRoofOutline();
            DrawCentralStructureOutline(false);
            DrawCentralStructureOutline(true);
            DrawStairsOutline();
            DrawLadderOutline();
            DrawWindowsOutline();
        }

        /// <summary>
        /// Draws the main house body outline.
        /// </summary>
        private void DrawMainHouseOutline()
        {
            // Main house trapezoid shape
            float topWidth = _dimensions.HouseWidth - 44 * _scaleFactor;
            float bottomWidth = topWidth * 0.8f;
            float topLeftX = _dimensions.HousePosition.X + 22 * _scaleFactor;
            float bottomLeftX = topLeftX + (topWidth - bottomWidth) / 2;
            float extendedWallHeight = _dimensions.WallHeight + 11;

            Vector2[] mainWall =
            [
                new Vector2(topLeftX, _dimensions.RoofBaseY),
                new Vector2(topLeftX + topWidth, _dimensions.RoofBaseY),
                new Vector2(bottomLeftX + bottomWidth, _dimensions.RoofBaseY + extendedWallHeight),
                new Vector2(bottomLeftX, _dimensions.RoofBaseY + extendedWallHeight),
            ];

            // Draw the outline using primitives
            _primitif.DrawBresenhamLinePoints(_canvas, mainWall, Colors.Black, 2f * _scaleFactor);

            // Background wall (slightly smaller)
            float bgTopWidth = _dimensions.HouseWidth - 60 * _scaleFactor;
            float bgBottomWidth = bgTopWidth * 0.8f;
            float bgTopLeftX = _dimensions.HousePosition.X + 30 * _scaleFactor;
            float bgBottomLeftX = bgTopLeftX + (bgTopWidth - bgBottomWidth) / 2;
            float bgExtendedWallHeight = _dimensions.WallHeight + 3;

            Vector2[] bgWall =
            [
                new Vector2(bgTopLeftX, _dimensions.RoofBaseY),
                new Vector2(bgTopLeftX + bgTopWidth, _dimensions.RoofBaseY),
                new Vector2(
                    bgBottomLeftX + bgBottomWidth,
                    _dimensions.RoofBaseY + bgExtendedWallHeight
                ),
                new Vector2(bgBottomLeftX, _dimensions.RoofBaseY + bgExtendedWallHeight),
            ];

            // Draw the background outline
            _primitif.DrawBresenhamLinePoints(_canvas, bgWall, Colors.Black, 1.5f * _scaleFactor);
        }

        /// <summary>
        /// Draws the roof outline with curved top.
        /// </summary>
        private void DrawRoofOutline()
        {
            float roofLeft = _dimensions.HousePosition.X + 10 * _scaleFactor;
            float roofRight =
                _dimensions.HousePosition.X + _dimensions.HouseWidth - 10 * _scaleFactor;
            float roofWidth = roofRight - roofLeft;
            float roofBaseY = _dimensions.RoofBaseY;
            float height = _config.RoofHeight * _scaleFactor;

            List<Vector2> roofPoints = new List<Vector2>();

            // Generate curved roof points
            for (int i = 0; i <= _config.RoofSegments; i++)
            {
                float t = (float)i / _config.RoofSegments;
                float x = roofLeft + roofWidth * t;
                // Parabolic curve for the roof
                float y = roofBaseY - height - (4 * height * (float)Math.Pow(t - 0.5, 2));
                roofPoints.Add(new Vector2(x, y));
            }

            // Complete the polygon
            roofPoints.Add(new Vector2(roofRight, roofBaseY));
            roofPoints.Add(new Vector2(roofLeft, roofBaseY));

            // Draw the roof outline
            _primitif.DrawBresenhamLinePoints(
                _canvas,
                roofPoints.ToArray(),
                Colors.Black,
                2f * _scaleFactor
            );
        }

        /// <summary>
        /// Draws the central structure outline.
        /// </summary>
        private void DrawCentralStructureOutline(bool isTop = false)
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
                _config.RoofHeight * _scaleFactor
            );

            float centerHeight = 10 * _scaleFactor;
            float margin = 25 * _scaleFactor;

            Vector2[] centerStructure =
            [
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
            ];

            // Draw the central structure outline
            _primitif.DrawBresenhamLinePoints(
                _canvas,
                centerStructure,
                Colors.Black,
                1.5f * _scaleFactor
            );
        }

        /// <summary>
        /// Draws the stairs outline.
        /// </summary>
        private void DrawStairsOutline()
        {
            BuildingDimensions dimensions = new BuildingDimensions(
                _center,
                400 * _scaleFactor,
                _dimensions.HouseHeight,
                _dimensions.WallHeight,
                _config.RoofHeight * _scaleFactor
            );

            float centerHeight = 20 * _scaleFactor;

            // Draw stair columns
            float columnWidth = 10 * _scaleFactor;
            float margin = 40 * _scaleFactor;
            float columnSpacing = (dimensions.HouseWidth - 2 * margin) / (_config.ColumnCount - 1);

            for (int i = 0; i < _config.ColumnCount; i++)
            {
                float xPos = dimensions.HousePosition.X + margin + i * columnSpacing;

                Vector2[] column =
                [
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
                            + _config.StairHeight * _scaleFactor
                    ),
                    new Vector2(
                        xPos - columnWidth / 2,
                        centerHeight
                            + dimensions.RoofBaseY
                            + dimensions.WallHeight
                            + _config.StairHeight * _scaleFactor
                    ),
                ];

                _primitif.DrawBresenhamLinePoints(_canvas, column, Colors.Black, _scaleFactor);
            }

            // Draw stair beams responsively
            // Beam config
            float beamHeight = 10 * _scaleFactor;
            float beamMargin = 30 * _scaleFactor;
            float stairWidth = dimensions.HouseWidth;
            Vector2 center = new Vector2(
                dimensions.HousePosition.X + dimensions.HouseWidth / 2,
                dimensions.HousePosition.Y
            );

            // Calculate the base position relative to the viewport size
            float baseY = dimensions.RoofBaseY + dimensions.WallHeight;

            // Calculate the beam positions based on the viewport size
            // This ensures the beams maintain proper positioning in fullscreen
            float stairHeight = _config.StairHeight * _scaleFactor;
            float upperBeamOffset = stairHeight * 0.45f; // 30% of stair height from the base
            float lowerBeamOffset = stairHeight * 0.75f; // 70% of stair height from the base

            // Posisi Y absolut responsif yang menyesuaikan dengan ukuran viewport
            float upperBeamY = baseY + upperBeamOffset;
            float lowerBeamY = baseY + lowerBeamOffset;

            // Gambar beam atas
            Vector2[] upperBeam = CreateBeam(
                center,
                stairWidth,
                beamHeight,
                upperBeamY,
                beamMargin
            );
            _primitif.DrawBresenhamLinePoints(_canvas, upperBeam, Colors.Black, _scaleFactor);

            // Gambar beam bawah
            Vector2[] lowerBeam = CreateBeam(
                center,
                stairWidth,
                beamHeight,
                lowerBeamY,
                beamMargin
            );
            _primitif.DrawBresenhamLinePoints(_canvas, lowerBeam, Colors.Black, _scaleFactor);
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
            float halfWidth = width / 2;
            float leftX = center.X - halfWidth + margin;
            float rightX = center.X + halfWidth - margin;

            return
            [
                new Vector2(leftX, yPosition),
                new Vector2(rightX, yPosition),
                new Vector2(rightX, yPosition + height),
                new Vector2(leftX, yPosition + height),
            ];
        }

        /// <summary>
        /// Draws the ladder outline.
        /// </summary>
        private void DrawLadderOutline()
        {
            // Position the ladder on the right side
            float ladderLength = _config.LadderLength * _scaleFactor;

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

            // Create ladder polygon
            List<Vector2> ladderPolygon = new List<Vector2>
            {
                ladderStart,
                ladderEnd,
                ladderEnd + railOffset,
                ladderStart + railOffset,
            };

            // Draw ladder outline
            _primitif.DrawBresenhamLinePoints(
                _canvas,
                ladderPolygon.ToArray(),
                Colors.Black,
                1.5f * _scaleFactor
            );

            for (int i = 1; i <= _config.LadderStepCount; i++)
            {
                float t = (float)i / (_config.LadderStepCount + 1);
                Vector2 stepStart = ladderStart + (ladderEnd - ladderStart) * t;
                Vector2 stepEnd = stepStart + railOffset;

                // Draw step
                _primitif.DrawBresenhamLine(
                    _canvas,
                    stepStart,
                    stepEnd,
                    Colors.Black,
                    _scaleFactor
                );
            }
        }

        /// <summary>
        /// Draws the windows outline.
        /// </summary>
        private void DrawWindowsOutline()
        {
            // Main wall dimensions for window placement
            float topWidth = _dimensions.HouseWidth - 60 * _scaleFactor;
            float topLeftX = _dimensions.HousePosition.X + 30 * _scaleFactor;

            // Window dimensions
            float windowTopY = _dimensions.RoofBaseY;
            float windowBottomY = _dimensions.RoofBaseY + _dimensions.WallHeight + 3 * _scaleFactor;

            // Calculate horizontal spacing for vertical lines
            float spacing = topWidth / (_config.WindowLineCount + 1);

            // Draw vertical window lines
            for (int i = 1; i <= _config.WindowLineCount; i++)
            {
                float x = topLeftX + i * spacing;

                // Draw vertical line
                _primitif.DrawBresenhamLine(
                    _canvas,
                    new Vector2(x, windowTopY),
                    new Vector2(x, windowBottomY),
                    Colors.Black,
                    1.5f * _scaleFactor
                );
            }
        }
    }
}
