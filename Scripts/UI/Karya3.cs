using System;
using Core;
using Drawing;
using Godot;
using Karya3.Scripts.UI;

namespace UI
{
    /// <summary>
    /// Represents the Karya3 scene, which displays a traditional building with horror elements.
    /// </summary>
    public partial class Karya3 : Node2D
    {
        // Animation renderer for animations and components only (no house structure)
        private AnimationOnlyRenderer _animationRenderer;

        // Sketch renderer for line primitive drawing of the house
        private SketchRenderer _sketchRenderer;

        // Building configuration
        private BuildingConfiguration _config = new BuildingConfiguration
        {
            PrimaryColor = new Color(0.3f, 0.3f, 0.35f), // Darker color for horror theme
            SecondaryColor = new Color(0.2f, 0.2f, 0.25f), // Darker secondary color
            OutlineColor = new Color(0.1f, 0.1f, 0.1f),
            OutlineThickness = 4f, // Increased from 2f to 4f to match Karya2
            WindowLineCount = 7, // Changed from 3 to 7 to match Karya2
            RoofSegments = 30, // Changed from 10 to 30 to match Karya2
            PanelCount = 8, // Added to match Karya2
            ColumnCount = 9, // Added to match Karya2
            LadderLength = 80,
        };

        // UI controls
        private Label _animationSpeedLabel;
        private Label _rotationSpeedLabel;
        private Label _scaleSpeedLabel;
        private Label _horrorEffectLabel;
        private float _horrorEffectIntensity = 1.0f;

        // Horror effects
        private ColorRect _darkOverlay;
        private Timer _flickerTimer;
        private Random _random = new Random();

        // Show Begu Ganjang intro scene first
        private IntroScene _introScene;

        // Flag to control whether to draw the house structure
        private bool _drawHouseStructure = false;

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();

            // Create and show the intro scene
            _introScene = new IntroScene();
            AddChild(_introScene);

            // Connect to the intro scene's completion signal
            _introScene.TreeExiting += OnIntroCompleted;

            // Set input as handled to ensure we receive input events
            SetProcessInput(true);
        }

        private void OnIntroCompleted()
        {
            // Remove the intro scene
            _introScene.QueueFree();

            // Start the main game content
            InitializeMainGame();
        }

        private void InitializeMainGame()
        {
            // Initialize animation renderer for animation and components
            _animationRenderer = new AnimationOnlyRenderer(this, _config);

            // Initialize sketch renderer for line primitive drawing
            _sketchRenderer = new SketchRenderer(this, _config);

            // Initialize drawing parameters
            _animationRenderer.InitializeDrawingParameters(GetViewportRect().Size);
            _sketchRenderer.InitializeDrawingParameters(GetViewportRect().Size);

            // Create UI controls
            CreateControls();

            // Create horror effects
            CreateHorrorEffects();

            // Start the animation
            _animationRenderer.StartAnimation();
        }

        /// <summary>
        /// Creates the horror visual effects for the scene
        /// </summary>
        private void CreateHorrorEffects()
        {
            // Create a dark overlay
            _darkOverlay = new ColorRect();
            _darkOverlay.Color = new Color(0, 0, 0, 0.3f);
            _darkOverlay.ZIndex = 10; // Above the building but below UI
            AddChild(_darkOverlay);

            // Set the size after it's added to the scene tree
            SetOverlaySize();

            // Create a flicker timer
            _flickerTimer = new Timer();
            _flickerTimer.WaitTime = 0.1f; // 100ms between flickers
            _flickerTimer.OneShot = false;
            _flickerTimer.Timeout += OnFlickerTimeout;
            AddChild(_flickerTimer);

            // Start the timer after it's added to the scene tree
            CallDeferred(nameof(StartFlickerTimer));
        }

        /// <summary>
        /// Sets the size of the dark overlay after it has been added to the scene tree
        /// </summary>
        private void SetOverlaySize()
        {
            if (_darkOverlay.IsInsideTree())
            {
                _darkOverlay.Size = GetViewportRect().Size;
                _darkOverlay.Position = Vector2.Zero;
            }
        }

        /// <summary>
        /// Creates the UI controls for adjusting animation parameters.
        /// </summary>
        private void CreateControls()
        {
            // Create a VBoxContainer for the labels
            VBoxContainer vbox = new VBoxContainer();
            vbox.AnchorTop = 0;
            vbox.AnchorBottom = 0;
            vbox.AnchorLeft = 0;
            vbox.AnchorRight = 0;
            vbox.Position = new Vector2(GetViewportRect().Size.X - 300, 10);

            // Add the container first, then set its size
            CallDeferred(Node.MethodName.AddChild, vbox);
            CallDeferred("SetVBoxSize", vbox);

            // Create title label
            Label titleLabel = new Label();
            titleLabel.Text = "Karya 3: Line Primitives & Animation";
            titleLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
            titleLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(titleLabel);

            // Create animation controls label
            Label controlsLabel = new Label();
            controlsLabel.Text = "Press SPACE to start/restart animation";
            controlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            controlsLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(controlsLabel);

            // Create animation speed label
            _animationSpeedLabel = new Label();
            _animationSpeedLabel.Text = "Animation Speed (Z/X): 1.0";
            _animationSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _animationSpeedLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_animationSpeedLabel);

            // Create rotation speed label
            _rotationSpeedLabel = new Label();
            _rotationSpeedLabel.Text = "Rotation Speed (Q/W): 1.0";
            _rotationSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _rotationSpeedLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_rotationSpeedLabel);

            // Create scale speed label
            _scaleSpeedLabel = new Label();
            _scaleSpeedLabel.Text = "Scale Speed (A/S): 1.0";
            _scaleSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _scaleSpeedLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_scaleSpeedLabel);

            // Create horror effect label
            _horrorEffectLabel = new Label();
            _horrorEffectLabel.Text = "Horror Effect (E/D): 1.0";
            _horrorEffectLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _horrorEffectLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_horrorEffectLabel);

            // Create a back button to return to the welcome scene
            var backButton = new Button
            {
                Text = "Back to Welcome",
                Position = new Vector2(10, 10),
                Size = new Vector2(150, 40)
            };
            backButton.Pressed += NavigateToWelcomeScene;
            AddChild(backButton);
        }

        /// <summary>
        /// Sets the size of the VBox container.
        /// </summary>
        private void SetVBoxSize(VBoxContainer vbox)
        {
            if (vbox != null && vbox.IsInsideTree())
            {
                vbox.Size = new Vector2(280, GetViewportRect().Size.Y - 20);
            }
        }

        /// <summary>
        /// Handles the flicker effect timeout
        /// </summary>
        private void OnFlickerTimeout()
        {
            // Only apply flicker if horror effect is enabled
            if (_horrorEffectIntensity > 0)
            {
                // Random chance to flicker based on intensity
                if (_random.NextDouble() < 0.3f * _horrorEffectIntensity)
                {
                    // Toggle the overlay alpha between normal and more transparent
                    if (_darkOverlay.Color.A > 0.15f)
                    {
                        _darkOverlay.Color = new Color(0, 0, 0, 0.1f);
                    }
                    else
                    {
                        _darkOverlay.Color = new Color(0, 0, 0, 0.3f * _horrorEffectIntensity);
                    }

                    // Force a redraw to update the scene
                    QueueRedraw();
                }
            }
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time elapsed since the last frame.</param>
        public override void _Process(double delta)
        {
            base._Process(delta);

            // Update animation if animation renderer is initialized
            if (_animationRenderer != null)
            {
                // Update the animation but don't draw the house structure
                _animationRenderer.UpdateAnimation((float)delta);

                // Force a redraw to update the scene
                QueueRedraw();
            }

            // Ensure the overlay covers the entire viewport if it's resized
            if (_darkOverlay != null && _darkOverlay.IsInsideTree())
            {
                SetOverlaySize();
            }
        }

        /// <summary>
        /// Called when the node needs to be redrawn.
        /// </summary>
        public override void _Draw()
        {
            // First draw the building using sketch renderer (line primitives only)
            if (_sketchRenderer != null)
            {
                _sketchRenderer.Draw();
            }

            // Then draw animations and components using the animation renderer
            // This renderer will skip drawing the house structure
            if (_animationRenderer != null)
            {
                _animationRenderer.Draw();
            }
        }

        /// <summary>
        /// Handles input events directly.
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            // Handle key presses for animation control
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Escape:
                        // Navigate back to the welcome scene
                        NavigateToWelcomeScene();
                        break;

                    case Key.Space:
                        // Start the animation
                        if (_animationRenderer != null)
                        {
                            _animationRenderer.StartAnimation();
                        }
                        break;

                    case Key.Z:
                        // Decrease animation speed
                        if (_animationRenderer != null)
                        {
                            float speed = 0.5f;
                            _animationRenderer.SetAnimationSpeed(speed);
                            _animationSpeedLabel.Text = $"Animation Speed (Z/X): {speed:F1}";
                        }

                        // Decrease horror effect intensity
                        _horrorEffectIntensity = Math.Max(_horrorEffectIntensity - 0.1f, 0f);
                        _horrorEffectLabel.Text =
                            $"Horror Effect (E/D): {_horrorEffectIntensity:F1}";
                        UpdateHorrorEffects();
                        break;

                    case Key.X:
                        // Increase animation speed
                        if (_animationRenderer != null)
                        {
                            float speed = 1.5f;
                            _animationRenderer.SetAnimationSpeed(speed);
                            _animationSpeedLabel.Text = $"Animation Speed (Z/X): {speed:F1}";
                        }

                        // Increase horror effect intensity
                        _horrorEffectIntensity = Math.Min(_horrorEffectIntensity + 0.1f, 2f);
                        _horrorEffectLabel.Text =
                            $"Horror Effect (E/D): {_horrorEffectIntensity:F1}";
                        UpdateHorrorEffects();
                        break;

                    case Key.Q:
                        // Decrease rotation speed
                        float rotSpeed = 0f;
                        if (rotSpeed > 0)
                            rotSpeed -= 0.5f;
                        _animationRenderer.SetRotationSpeed(rotSpeed);
                        _rotationSpeedLabel.Text = $"Rotation Speed (Q/W): {rotSpeed}";
                        break;

                    case Key.W:
                        // Increase rotation speed
                        rotSpeed = 1.0f;
                        _animationRenderer.SetRotationSpeed(rotSpeed);
                        _rotationSpeedLabel.Text = $"Rotation Speed (Q/W): {rotSpeed}";
                        break;

                    case Key.A:
                        // Decrease scale speed
                        float scaleSpeed = 0f;
                        if (scaleSpeed > 0)
                            scaleSpeed -= 0.5f;
                        _animationRenderer.SetScaleSpeed(scaleSpeed);
                        _scaleSpeedLabel.Text = $"Scale Speed (A/S): {scaleSpeed}";
                        break;

                    case Key.S:
                        // Increase scale speed
                        scaleSpeed = 1.0f;
                        _animationRenderer.SetScaleSpeed(scaleSpeed);
                        _scaleSpeedLabel.Text = $"Scale Speed (A/S): {scaleSpeed}";
                        break;
                }
            }
        }

        /// <summary>
        /// Navigates back to the Welcome scene
        /// </summary>
        private void NavigateToWelcomeScene()
        {
            string scenePath = "res://Scenes/UI/Welcome.tscn";
            if (GetTree().ChangeSceneToFile(scenePath) != Error.Ok)
            {
                GD.PrintErr($"Failed to load scene: {scenePath}");
            }
        }

        /// <summary>
        /// Starts the flicker timer after it has been added to the scene tree
        /// </summary>
        private void StartFlickerTimer()
        {
            if (_flickerTimer.IsInsideTree())
            {
                _flickerTimer.Start();
            }
        }

        /// <summary>
        /// Updates the horror effects based on the current intensity
        /// </summary>
        private void UpdateHorrorEffects()
        {
            // Update overlay darkness
            _darkOverlay.Color = new Color(0, 0, 0, 0.3f * _horrorEffectIntensity);
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
                // Update drawing parameters with the new viewport size
                if (_animationRenderer != null)
                {
                    _animationRenderer.InitializeDrawingParameters(GetViewportRect().Size);
                }

                if (_sketchRenderer != null)
                {
                    _sketchRenderer.InitializeDrawingParameters(GetViewportRect().Size);
                }
            }
        }
    }
}
