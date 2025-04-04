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

        // Horror effects
        private float _flickerEffect = 0f;
        private float _fogEffect = 0f;
        private Random _random = new Random();
        private List<Vector2> _fogParticles = new List<Vector2>();
        private const int FOG_PARTICLE_COUNT = 50;

        // Ladder animation
        private bool _isLadderAnimating = false;
        private float _ladderOpenAmount = 0f;
        private float _ladderOpenSpeed = 1.0f;

        // Rolling head animation
        private bool _rollingHeadStarted = false;
        private PersonComponent _rollingHead;

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

            // Initialize fog particles
            _fogParticles.Clear();
            for (int i = 0; i < FOG_PARTICLE_COUNT; i++)
            {
                _fogParticles.Add(
                    new Vector2(
                        _random.Next(
                            (int)(_dimensions.HousePosition.X - 100),
                            (int)(_dimensions.HousePosition.X + _dimensions.HouseWidth + 200)
                        ),
                        _random.Next(
                            (int)(_dimensions.RoofBaseY),
                            (int)(_dimensions.RoofBaseY + _dimensions.WallHeight * 2)
                        )
                    )
                );
            }

            // Calculate ladder position for people to start from
            float rightEdgeX =
                _dimensions.HousePosition.X + _dimensions.HouseWidth - 20 * _scaleFactor;
            float startY = _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
            Vector2 ladderStart = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
            Vector2 ladderEnd = new Vector2(
                rightEdgeX + _config.LadderLength * _scaleFactor,
                startY + _config.LadderLength * _scaleFactor * 1.5f
            );

            // Create a rolling head component
            _rollingHead = new PersonComponent(
                _canvas,
                _primitif,
                _dimensions,
                _scaleFactor,
                new Color(0.8f, 0.7f, 0.7f, 1.0f), // Pale flesh color
                _config.OutlineColor,
                _config.OutlineThickness,
                ladderStart // Initial position (will be updated when animation starts)
            );

            // Create giant, terrifying humanoid figures around the house
            for (int i = 0; i < PEOPLE_COUNT; i++)
            {
                // Start positions around the house perimeter for more threatening appearance
                // Spread ghosts more widely around the house with varied distances
                float angle = i * (Mathf.Tau / PEOPLE_COUNT) + (float)_random.NextDouble() * 0.5f;
                float distance = 300 * _scaleFactor + _random.Next(100, 300) * _scaleFactor;

                Vector2 startPosition = new Vector2(
                    _dimensions.Center.X + (float)Math.Cos(angle) * distance,
                    _dimensions.Center.Y + (float)Math.Sin(angle) * distance
                );

                // Create giant humanoid component with blood-red or dark color variations
                Color primaryColor;

                // More varied colors for ghosts
                switch (i % 4)
                {
                    case 0:
                        // White
                        primaryColor = new Color(1f, 1f, 1f, 0.9f);
                        break;
                    case 1:
                        // Dark shadowy
                        primaryColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
                        break;
                    case 2:
                        // Pale ghostly
                        primaryColor = new Color(0.5f, 0.5f, 0.6f, 0.8f);
                        break;
                    default:
                        // Dark reddish-brown
                        primaryColor = new Color(0.3f, 0.1f, 0.05f, 0.9f);
                        break;
                }

                PersonComponent giant = new PersonComponent(
                    _canvas,
                    _primitif,
                    _dimensions,
                    _scaleFactor,
                    primaryColor,
                    _config.OutlineColor,
                    _config.OutlineThickness,
                    startPosition
                );

                // Set initial scale based on position - further ones appear larger for depth effect
                float initialScale = 0.8f + (float)_random.NextDouble() * 1.0f;
                giant.SetTargetScale(initialScale);

                // Set initial rotation for variety
                giant.SetTargetRotation((float)_random.NextDouble() * 0.4f - 0.2f);

                _people.Add(giant);
            }
        }

        /// <summary>
        /// Draws the building with all its components.
        /// </summary>
        public void Draw()
        {
            // Apply flickering effect to the entire scene
            if (_animationEnabled && _random.NextDouble() < 0.05)
            {
                _flickerEffect = (float)_random.NextDouble() * 0.2f;
            }
            else
            {
                _flickerEffect = Math.Max(0, _flickerEffect - 0.01f);
            }

            // Create a dark overlay for horror effect
            if (_animationEnabled)
            {
                // Draw fog particles
                foreach (var particle in _fogParticles)
                {
                    float size = (float)(_random.NextDouble() * 5 + 3) * _scaleFactor;
                    _canvas.DrawCircle(particle, size, new Color(0.9f, 0.9f, 0.95f, 0.1f));
                }

                // Draw a semi-transparent dark overlay
                Vector2[] overlay = new Vector2[]
                {
                    new Vector2(0, 0),
                    new Vector2(_canvas.GetViewportRect().Size.X, 0),
                    new Vector2(_canvas.GetViewportRect().Size.X, _canvas.GetViewportRect().Size.Y),
                    new Vector2(0, _canvas.GetViewportRect().Size.Y),
                };

                Color overlayColor = new Color(0.05f, 0.05f, 0.1f, 0.3f + _flickerEffect);
                _canvas.DrawPolygon(
                    overlay,
                    new Color[] { overlayColor, overlayColor, overlayColor, overlayColor }
                );
            }

            // Draw main components
            _roofComponent.Draw();
            _mainHouseComponent.Draw();
            _centralStructureComponent.Draw();
            _stairsComponent.Draw();

            // Draw ladder with animation if it's visible
            if (_showLadder)
            {
                // Apply transformation for ladder animation
                if (_isLadderAnimating)
                {
                    float rightEdgeX =
                        _dimensions.HousePosition.X + _dimensions.HouseWidth - 20 * _scaleFactor;
                    float startY = _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;

                    // Calculate ladder pivot point (top of ladder)
                    Vector2 ladderPivot = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);

                    // Apply rotation transformation for ladder opening animation
                    _canvas.DrawSetTransform(
                        ladderPivot,
                        _ladderOpenAmount * Mathf.Pi / 2, // Rotate from 0 to 90 degrees
                        Vector2.One
                    );

                    // Draw the ladder at the origin (pivot point)
                    _ladderComponent.Draw();

                    // Reset transformation
                    _canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
                }
                else
                {
                    _ladderComponent.Draw();
                }
            }

            // Draw rolling head if animation is active
            if (_rollingHeadStarted)
            {
                _rollingHead.DrawRollingHead();
            }

            // Draw giant humanoid figures
            foreach (var giant in _people)
            {
                giant.Draw();
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

            // Update fog particles
            for (int i = 0; i < _fogParticles.Count; i++)
            {
                Vector2 particle = _fogParticles[i];

                // Move particles slowly
                particle.X += (float)(_random.NextDouble() * 2 - 1) * delta * 10;
                particle.Y -= (float)_random.NextDouble() * delta * 5;

                // Reset particles that move too far
                if (
                    particle.Y < _dimensions.RoofBaseY - 100
                    || particle.X < _dimensions.HousePosition.X - 200
                    || particle.X > _dimensions.HousePosition.X + _dimensions.HouseWidth + 300
                )
                {
                    particle = new Vector2(
                        _random.Next(
                            (int)(_dimensions.HousePosition.X - 100),
                            (int)(_dimensions.HousePosition.X + _dimensions.HouseWidth + 200)
                        ),
                        _dimensions.RoofBaseY + _dimensions.WallHeight * 2
                    );
                }

                _fogParticles[i] = particle;
            }

            // Update all giants' animations
            foreach (var person in _people)
            {
                person.UpdateAnimation(delta, _animationSpeed);
            }

            // Update rolling head animation if active
            if (_rollingHeadStarted)
            {
                _rollingHead.UpdateAnimation(delta, _animationSpeed);
            }

            // Update ladder animation if active
            if (_isLadderAnimating)
            {
                _ladderOpenAmount += _ladderOpenSpeed * delta;
                if (_ladderOpenAmount >= 1.0f)
                {
                    _ladderOpenAmount = 1.0f;
                    _isLadderAnimating = false;

                    // Start rolling head animation when ladder is fully open
                    if (!_rollingHeadStarted)
                    {
                        _rollingHeadStarted = true;

                        // Calculate start and target positions for rolling head
                        float rightEdgeX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            - 20 * _scaleFactor;
                        float startY =
                            _dimensions.RoofBaseY + _dimensions.WallHeight * _scaleFactor;
                        Vector2 startPosition = new Vector2(rightEdgeX - 30 * _scaleFactor, startY);
                        float targetX =
                            _dimensions.HousePosition.X
                            + _dimensions.HouseWidth
                            + 100 * _scaleFactor;

                        // Start the rolling head animation
                        _rollingHead.StartRollingHeadAnimation(startPosition, targetX);
                    }
                }
            }

            // Update animation based on current stage
            switch (_animationStage)
            {
                case 0: // Eerie silence, then ladder appears
                    if (_animationTime >= 1.0f)
                    {
                        // Show the ladder
                        _showLadder = true;

                        // Start ladder opening animation
                        _isLadderAnimating = true;
                        _ladderOpenAmount = 0f;

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 1;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 1: // Wait for rolling head animation to complete, then start showing giants
                    // Check if rolling head animation has started but is no longer active (completed)
                    if (_rollingHeadStarted && !_rollingHead.IsRollingHeadActive())
                    {
                        // Make all giants visible immediately after jump scare
                        foreach (var giant in _people)
                        {
                            giant.SetVisible(true);
                        }

                        // Position giants around the house
                        for (int i = 0; i < _people.Count; i++)
                        {
                            float angle = i * (Mathf.Tau / _people.Count);
                            float distance = 200 * _scaleFactor;

                            Vector2 targetPos = new Vector2(
                                _dimensions.Center.X + (float)Math.Cos(angle) * distance,
                                _dimensions.Center.Y + (float)Math.Sin(angle) * distance * 0.5f
                            );

                            _people[i].SetTargetPosition(targetPos);
                        }

                        _animationStage = 6; // Skip directly to the stage where giants close in
                        _animationTime = 0f;
                    }
                    // If rolling head animation hasn't started yet, wait a bit longer
                    else if (_animationTime >= 5.0f)
                    {
                        // Fallback in case rolling head animation doesn't complete
                        _animationStage = 2;
                        _animationTime = 0f;
                    }
                    break;

                case 2: // More giants appear and begin to move toward the house (fallback path)
                    if (_people.Count > 1 && _animationTime >= 1.0f)
                    {
                        // Make second giant visible
                        _people[1].SetVisible(true);

                        // Move first giant closer to the house
                        if (_people.Count > 0)
                        {
                            Vector2 targetPos = new Vector2(
                                _dimensions.Center.X - 100 * _scaleFactor,
                                _dimensions.RoofBaseY + _dimensions.WallHeight * 1.5f
                            );
                            _people[0].SetTargetPosition(targetPos);
                        }

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 3;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 3: // Third giant appears, others continue moving
                    if (_people.Count > 2 && _animationTime >= 1.0f)
                    {
                        // Make third giant visible
                        _people[2].SetVisible(true);

                        // Move second giant closer to the house from another direction
                        if (_people.Count > 1)
                        {
                            Vector2 targetPos = new Vector2(
                                _dimensions.Center.X + 120 * _scaleFactor,
                                _dimensions.RoofBaseY + _dimensions.WallHeight * 1.2f
                            );
                            _people[1].SetTargetPosition(targetPos);
                        }

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 4;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 4: // Fourth giant appears, others continue surrounding the house
                    if (_people.Count > 3 && _animationTime >= 1.0f)
                    {
                        // Make fourth giant visible
                        _people[3].SetVisible(true);

                        // Move third giant to position
                        if (_people.Count > 2)
                        {
                            Vector2 targetPos = new Vector2(
                                _dimensions.Center.X - 50 * _scaleFactor,
                                _dimensions.RoofBaseY - 100 * _scaleFactor
                            );
                            _people[2].SetTargetPosition(targetPos);
                        }

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 5;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 5: // Final giant appears, all giants now surround the house
                    if (_people.Count > 4 && _animationTime >= 1.0f)
                    {
                        // Make final giant visible
                        _people[4].SetVisible(true);

                        // Move fourth giant to position
                        if (_people.Count > 3)
                        {
                            Vector2 targetPos = new Vector2(
                                _dimensions.Center.X + 80 * _scaleFactor,
                                _dimensions.RoofBaseY - 50 * _scaleFactor
                            );
                            _people[3].SetTargetPosition(targetPos);
                        }

                        if (_animationTime >= 2.0f)
                        {
                            _animationStage = 6;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 6: // All giants now begin to close in on the house
                    {
                        // Move all giants closer to the house in a threatening manner
                        for (int i = 0; i < _people.Count; i++)
                        {
                            // Calculate position closer to the house
                            float angle = i * (Mathf.Tau / _people.Count);
                            float distance = 150 * _scaleFactor * (1.0f - _animationTime * 0.1f);
                            distance = Math.Max(distance, 100 * _scaleFactor); // Don't get too close

                            Vector2 targetPos = new Vector2(
                                _dimensions.Center.X + (float)Math.Cos(angle) * distance,
                                _dimensions.Center.Y + (float)Math.Sin(angle) * distance * 0.5f
                            );

                            _people[i].SetTargetPosition(targetPos);
                        }

                        if (_animationTime >= 3.0f)
                        {
                            _animationStage = 7;
                            _animationTime = 0f;
                        }
                    }
                    break;

                case 7: // Final horror sequence - giants reach toward the house
                    {
                        // Make giants perform threatening movements
                        for (int i = 0; i < _people.Count; i++)
                        {
                            // Alternate scaling up and down for breathing effect
                            float scaleOffset =
                                (float)Math.Sin(_animationTime * 2 + i) * 0.1f + 1.0f;
                            _people[i].SetTargetScale(scaleOffset);

                            // Slight rotation for swaying effect
                            float rotationOffset =
                                (float)Math.Sin(_animationTime * 1.5f + i * 0.5f) * 0.15f;
                            _people[i].SetTargetRotation(rotationOffset);
                        }
                    }
                    break;
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
            _isLadderAnimating = false;
            _ladderOpenAmount = 0f;
            _rollingHeadStarted = false;

            // Reset giants
            foreach (var giant in _people)
            {
                giant.SetVisible(false);
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
