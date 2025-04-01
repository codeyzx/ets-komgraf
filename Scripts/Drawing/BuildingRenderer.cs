using System;
using System.Collections.Generic;
using Core;
using Drawing.Components;
using Godot;

namespace Drawing
{
    /// <summary>
    /// Renderer for the building structure with all its components.
    /// </summary>
    public class BuildingRenderer
    {
        private readonly CanvasItem _canvas;
        private readonly Primitif _primitif;
        private readonly BuildingConfiguration _config;

        private Vector2 _center;
        private float _scaleFactor;
        private BuildingDimensions _dimensions;

        // Components
        private RoofComponent _roofComponent;
        private MainHouseComponent _mainHouseComponent;
        private CentralStructureComponent _centralStructureComponent;
        private StairsComponent _stairsComponent;
        private LadderComponent _ladderComponent;

        // Animation properties
        private bool _showLadder = false;
        private float _animationTime = 0f;
        private float _animationSpeed = 1f;
        private float _rotationSpeed = 0f;
        private float _scaleSpeed = 0f;
        private bool _animationEnabled = false;
        private int _animationStage = 0;

        // People animation
        private List<PersonComponent> _people = new List<PersonComponent>();
        private const int PEOPLE_COUNT = 5;

        /// <summary>
        /// Initializes a new instance of the BuildingRenderer class.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="config">The building configuration.</param>
        public BuildingRenderer(CanvasItem canvas, BuildingConfiguration config)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _config = config ?? throw new ArgumentNullException(nameof(config));
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

            // Initialize components if they haven't been created yet
            if (_roofComponent == null)
            {
                InitializeComponents();
                InitializePeople();
            }
        }

        /// <summary>
        /// Initializes all building components.
        /// </summary>
        private void InitializeComponents()
        {
            _roofComponent = new RoofComponent(
                _canvas,
                _primitif,
                _dimensions,
                _scaleFactor,
                _config.PrimaryColor,
                _config.OutlineColor,
                _config.RoofHeight,
                _config.OutlineThickness,
                _config.RoofSegments
            );

            _mainHouseComponent = new MainHouseComponent(
                _canvas,
                _primitif,
                _dimensions,
                _scaleFactor,
                _config.PrimaryColor,
                _config.OutlineColor,
                _config.OutlineThickness,
                _config.WindowLineCount
            );

            _centralStructureComponent = new CentralStructureComponent(
                _canvas,
                _primitif,
                _dimensions,
                _scaleFactor,
                _config.SecondaryColor,
                _config.OutlineColor,
                _config.OutlineThickness
            );

            _stairsComponent = new StairsComponent(
                _canvas,
                _primitif,
                _dimensions,
                _scaleFactor,
                _config.SecondaryColor,
                _config.OutlineColor,
                _config.OutlineThickness,
                _config.StairHeight,
                _config.ColumnCount
            );

            _ladderComponent = new LadderComponent(
                _canvas,
                _primitif,
                _dimensions,
                _scaleFactor,
                _config.SecondaryColor,
                _config.OutlineColor,
                _config.OutlineThickness,
                _config.LadderWidth,
                _config.LadderLength
            );
        }

        /// <summary>
        /// Initializes the people components for animation.
        /// </summary>
        private void InitializePeople()
        {
            _people.Clear();

            // Calculate ladder position for people to start from
            float rightEdgeX =
                _dimensions.HousePosition.X + _dimensions.HouseWidth - 20 * _scaleFactor;
            float startY = _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
            Vector2 ladderStart = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
            Vector2 ladderEnd = new Vector2(
                rightEdgeX + _config.LadderLength * _scaleFactor,
                startY + _config.LadderLength * _scaleFactor * 1.5f
            );

            // Create people starting from inside the house
            for (int i = 0; i < PEOPLE_COUNT; i++)
            {
                // Start position inside the house
                Vector2 startPosition = new Vector2(
                    _dimensions.HousePosition.X
                        + _dimensions.HouseWidth / 2
                        - i * 20 * _scaleFactor,
                    _dimensions.RoofBaseY + _dimensions.WallHeight / 2
                );

                // Create person component
                PersonComponent person = new PersonComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    new Color(0.8f, 0.2f, 0.2f), // Person color (reddish)
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    startPosition
                );

                _people.Add(person);
            }
        }

        /// <summary>
        /// Draws the building with all its components.
        /// </summary>
        public void Draw()
        {
            // Draw main components
            _roofComponent.Draw();
            _mainHouseComponent.Draw();
            _centralStructureComponent.Draw();
            _stairsComponent.Draw();

            // Draw ladder only if it's visible in the animation
            if (_showLadder)
            {
                _ladderComponent.Draw();
            }

            // Draw people
            foreach (var person in _people)
            {
                person.Draw();
            }
        }

        /// <summary>
        /// Updates the animation state.
        /// </summary>
        /// <param name="delta">Time elapsed since the last update.</param>
        public void UpdateAnimation(float delta)
        {
            if (!_animationEnabled)
                return;

            // Update animation time
            _animationTime += delta * _animationSpeed;

            // Update animation based on current stage
            switch (_animationStage)
            {
                case 0: // Show ladder
                    if (_animationTime >= 1.0f)
                    {
                        _showLadder = true;
                        _animationStage = 1;
                        _animationTime = 0f;
                    }
                    break;

                case 1: // First person appears and moves to ladder
                    if (_animationTime >= 0.5f && _people.Count > 0)
                    {
                        _people[0].SetVisible(true);

                        // Move to ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderTop = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
                        _people[0].SetTargetPosition(ladderTop);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 2;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 2: // First person climbs down ladder
                    if (_animationTime >= 0.5f && _people.Count > 0)
                    {
                        // Move down the ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderBottom = new Vector2(
                            rightEdgeX + _config.LadderLength * _scaleFactor,
                            startY + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[0].SetTargetPosition(ladderBottom);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 3;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 3: // First person moves to position, second person appears
                    if (_animationTime >= 0.5f && _people.Count > 1)
                    {
                        // Move first person to final position
                        Vector2 finalPosition = new Vector2(
                            _dimensions.HousePosition.X
                                + _dimensions.HouseWidth
                                + 50 * _scaleFactor,
                            _dimensions.RoofBaseY
                                + _dimensions.WallHeight * _scaleFactor
                                + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[0].SetTargetPosition(finalPosition);

                        // Show second person
                        _people[1].SetVisible(true);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 4;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 4: // Second person moves to ladder
                    if (_animationTime >= 0.5f && _people.Count > 1)
                    {
                        // Move to ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderTop = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
                        _people[1].SetTargetPosition(ladderTop);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 5;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 5: // Second person climbs down ladder
                    if (_animationTime >= 0.5f && _people.Count > 1)
                    {
                        // Move down the ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderBottom = new Vector2(
                            rightEdgeX + _config.LadderLength * _scaleFactor,
                            startY + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[1].SetTargetPosition(ladderBottom);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 6;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 6: // Second person moves to position, third person appears
                    if (_animationTime >= 0.5f && _people.Count > 2)
                    {
                        // Move second person to final position
                        Vector2 finalPosition = new Vector2(
                            _dimensions.HousePosition.X
                                + _dimensions.HouseWidth
                                + 80 * _scaleFactor,
                            _dimensions.RoofBaseY
                                + _dimensions.WallHeight * _scaleFactor
                                + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[1].SetTargetPosition(finalPosition);

                        // Show third person
                        _people[2].SetVisible(true);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 7;
                            _animationTime = 0f;
                        }
                    }
                    break;

                // Continue with similar patterns for remaining people
                case 7: // Repeat for third person
                    if (_animationTime >= 0.5f && _people.Count > 2)
                    {
                        // Move to ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderTop = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
                        _people[2].SetTargetPosition(ladderTop);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 8;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 8: // Third person climbs down
                    if (_animationTime >= 0.5f && _people.Count > 2)
                    {
                        // Move down the ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderBottom = new Vector2(
                            rightEdgeX + _config.LadderLength * _scaleFactor,
                            startY + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[2].SetTargetPosition(ladderBottom);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 9;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 9: // Third person moves to position, fourth person appears
                    if (_animationTime >= 0.5f && _people.Count > 3)
                    {
                        // Move third person to final position
                        Vector2 finalPosition = new Vector2(
                            _dimensions.HousePosition.X
                                + _dimensions.HouseWidth
                                + 110 * _scaleFactor,
                            _dimensions.RoofBaseY
                                + _dimensions.WallHeight * _scaleFactor
                                + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[2].SetTargetPosition(finalPosition);

                        // Show fourth person
                        _people[3].SetVisible(true);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 10;
                            _animationTime = 0f;
                        }
                    }
                    break;

                // Continue with similar patterns for remaining people
                case 10: // Repeat for fourth person
                    if (_animationTime >= 0.5f && _people.Count > 3)
                    {
                        // Move to ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderTop = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
                        _people[3].SetTargetPosition(ladderTop);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 11;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 11: // Fourth person climbs down
                    if (_animationTime >= 0.5f && _people.Count > 3)
                    {
                        // Move down the ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderBottom = new Vector2(
                            rightEdgeX + _config.LadderLength * _scaleFactor,
                            startY + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[3].SetTargetPosition(ladderBottom);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 12;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 12: // Fourth person moves to position, fifth person appears
                    if (_animationTime >= 0.5f && _people.Count > 4)
                    {
                        // Move fourth person to final position
                        Vector2 finalPosition = new Vector2(
                            _dimensions.HousePosition.X
                                + _dimensions.HouseWidth
                                + 140 * _scaleFactor,
                            _dimensions.RoofBaseY
                                + _dimensions.WallHeight * _scaleFactor
                                + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[3].SetTargetPosition(finalPosition);

                        // Show fifth person
                        _people[4].SetVisible(true);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 13;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 13: // Fifth person moves to ladder
                    if (_animationTime >= 0.5f && _people.Count > 4)
                    {
                        // Move to ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderTop = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
                        _people[4].SetTargetPosition(ladderTop);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 14;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 14: // Fifth person climbs down ladder
                    if (_animationTime >= 0.5f && _people.Count > 4)
                    {
                        // Move down the ladder
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 ladderBottom = new Vector2(
                            rightEdgeX + _config.LadderLength * _scaleFactor,
                            startY + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[4].SetTargetPosition(ladderBottom);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 15;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 15: // Fifth person moves to position, all people in line
                    if (_animationTime >= 0.5f && _people.Count > 4)
                    {
                        // Move fifth person to final position
                        Vector2 finalPosition = new Vector2(
                            _dimensions.HousePosition.X
                                + _dimensions.HouseWidth
                                + 170 * _scaleFactor,
                            _dimensions.RoofBaseY
                                + _dimensions.WallHeight * _scaleFactor
                                + _config.LadderLength * _scaleFactor * 1.5f
                        );
                        _people[4].SetTargetPosition(finalPosition);

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 16;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 16: // Final formation - rotate and scale
                    for (int i = 0; i < _people.Count; i++)
                    {
                        // Apply rotation based on rotation speed
                        _people[i]
                            .SetTargetRotation(
                                (float)Math.Sin(_animationTime * _rotationSpeed) * 0.2f
                            );

                        // Apply scale based on scale speed
                        float scale = 1.0f + (float)Math.Sin(_animationTime * _scaleSpeed) * 0.2f;
                        _people[i].SetTargetScale(scale);
                    }
                    break;
            }

            // Update each person's animation
            foreach (var person in _people)
            {
                person.UpdateAnimation(delta, _animationSpeed);
            }
        }

        /// <summary>
        /// Starts the animation sequence.
        /// </summary>
        public void StartAnimation()
        {
            _animationEnabled = true;
            _animationStage = 0;
            _animationTime = 0f;
            _showLadder = false;

            // Reset people
            foreach (var person in _people)
            {
                person.SetVisible(false);
            }
        }

        /// <summary>
        /// Sets the animation speed.
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            _animationSpeed = Math.Max(0.1f, Math.Min(5.0f, speed));
        }

        /// <summary>
        /// Sets the rotation speed for the final animation.
        /// </summary>
        public void SetRotationSpeed(float speed)
        {
            _rotationSpeed = speed;
        }

        /// <summary>
        /// Sets the scale speed for the final animation.
        /// </summary>
        public void SetScaleSpeed(float speed)
        {
            _scaleSpeed = speed;
        }
    }
}
