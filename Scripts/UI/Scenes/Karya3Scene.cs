using System;
using System.Reflection;
using Drawing.Components.Characters;
using Drawing.Configuration;
using Drawing.Renderers;
using Godot;

namespace Scenes
{
    /// <summary>
    /// Represents the Karya3 scene, which displays a traditional building with horror elements.
    /// </summary>
    public partial class Karya3Scene : Node2D
    {
        // Animation renderer for animations and components only (no house structure)
        private AnimationOnlyRenderer _animationRenderer;

        // Sketch renderer for line primitive drawing of the house
        private SketchRenderer _sketchRenderer;

        // Building configuration
        private readonly BuildingConfiguration _config = new BuildingConfiguration
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
        private Label _horrorEffectLabel;
        private float _horrorEffectIntensity = 1.0f;
        private float _ghostScale = 1.0f;
        private Label _ghostScaleLabel;
        private float _animationSpeed = 1.0f;

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
            CallDeferred(Node.MethodName.AddChild, _introScene);

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

            // Initialize animation speed and ghost scale
            _animationSpeed = 1.0f;
            _ghostScale = 1.0f;

            // Create UI controls
            CreateControls();

            // Create horror effects
            CreateHorrorEffects();

            // Start the animation
            _animationRenderer.StartAnimation();

            // Ensure the back button remains accessible by bringing it to the front
            EnsureBackButtonAccessibility();
        }

        /// <summary>
        /// Ensures the back button remains accessible by bringing it to the front
        /// </summary>
        private void EnsureBackButtonAccessibility()
        {
            // Find the Control node containing the back button
            var controlNode = GetNode<Control>("Control");
            if (controlNode != null)
            {
                // Remove the existing button if it exists
                var existingButton = controlNode.GetNode<Button>("Button");
                if (existingButton != null)
                {
                    existingButton.QueueFree();
                }

                // Create a completely new button
                Button newBackButton = new Button();
                newBackButton.Name = "NewBackButton";
                newBackButton.Text = "Back";
                newBackButton.Position = new Vector2(0, 0);
                newBackButton.Size = new Vector2(80, 30);
                newBackButton.ZIndex = 2000;

                // Style the button to make it stand out
                newBackButton.AddThemeColorOverride("font_color", new Color(1, 1, 1));
                newBackButton.AddThemeColorOverride("font_hover_color", new Color(1, 0.5f, 0.5f));
                newBackButton.AddThemeColorOverride("font_focus_color", new Color(1, 0.7f, 0.7f));
                newBackButton.AddThemeColorOverride("font_pressed_color", new Color(1, 0, 0));
                newBackButton.AddThemeColorOverride("bg_color", new Color(0.5f, 0.1f, 0.1f));
                newBackButton.AddThemeColorOverride("bg_hover_color", new Color(0.7f, 0.2f, 0.2f));

                // Connect the pressed signal directly to our navigation method
                newBackButton.Connect("pressed", Callable.From(() => NavigateToWelcomeScene()));

                // Add the new button to the control node
                controlNode.AddChild(newBackButton);
            }
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
            CallDeferred(Node.MethodName.AddChild, _darkOverlay);

            // Set the size after it's added to the scene tree
            SetOverlaySize();

            // Create a flicker timer
            _flickerTimer = new Timer();
            _flickerTimer.WaitTime = 0.1f; // 100ms between flickers
            _flickerTimer.OneShot = false;
            _flickerTimer.Timeout += OnFlickerTimeout;
            CallDeferred(Node.MethodName.AddChild, _flickerTimer);

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
            titleLabel.Text = "Horror Animation Controls";
            titleLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
            titleLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.CallDeferred(Node.MethodName.AddChild, titleLabel);

            // Create animation controls label
            Label controlsLabel = new Label();
            controlsLabel.Text = "Press SPACE to start/restart animation";
            controlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
            controlsLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.CallDeferred(Node.MethodName.AddChild, controlsLabel);

            // Create keyboard controls label
            Label keyboardControlsLabel = new Label();
            keyboardControlsLabel.Text = "Keyboard Controls";
            keyboardControlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            keyboardControlsLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.CallDeferred(Node.MethodName.AddChild, keyboardControlsLabel);

            // Create animation speed label
            _animationSpeedLabel = new Label();
            _animationSpeedLabel.Text = "Animation Speed (Q/W): 1.0";
            _animationSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _animationSpeedLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.CallDeferred(Node.MethodName.AddChild, _animationSpeedLabel);

            // Create ghost scale label
            _ghostScaleLabel = new Label();
            _ghostScaleLabel.Text = "Ghost Scale (D/F): 1.0";
            _ghostScaleLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _ghostScaleLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.CallDeferred(Node.MethodName.AddChild, _ghostScaleLabel);

            // Create darkness effect label
            _horrorEffectLabel = new Label();
            _horrorEffectLabel.Text = $"Darkness Effect (Z/X): {_horrorEffectIntensity:F1}";
            _horrorEffectLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _horrorEffectLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.CallDeferred(Node.MethodName.AddChild, _horrorEffectLabel);
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

            // Check for keyboard input to navigate back (Escape key)
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                GD.Print("Escape key pressed, navigating to Welcome scene");
                NavigateToWelcomeScene();
            }

            // Update animation if animation renderer is initialized
            if (_animationRenderer != null)
            {
                // Update the animation but don't draw the house structure
                _animationRenderer.UpdateAnimation((float)delta);

                // Apply ghost scale periodically to ensure it's applied
                if (Time.GetTicksMsec() % 1000 < 50) // Apply roughly every second
                {
                    ApplyGhostScale();
                }

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
            // Skip if not ready or if event is null
            if (@event == null || !IsInsideTree())
            {
                return;
            }

            base._Input(@event);

            // Check for mouse clicks in the back button area (top-left corner)
            if (
                @event is InputEventMouseButton mouseEvent
                && mouseEvent.ButtonIndex == MouseButton.Left
                && mouseEvent.Pressed
            )
            {
                // Define the back button area (top-left corner)
                Rect2 backButtonArea = new Rect2(0, 0, 100, 50);

                // Check if the click is within the back button area
                if (backButtonArea.HasPoint(mouseEvent.Position))
                {
                    GD.Print("Back button area clicked at " + mouseEvent.Position);
                    NavigateToWelcomeScene();

                    // Mark the event as handled - with null check
                    var viewport = GetViewport();
                    if (viewport != null)
                    {
                        viewport.SetInputAsHandled();
                    }
                }
            }

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

                    case Key.Q:
                        // Decrease animation speed
                        if (_animationRenderer != null)
                        {
                            _animationSpeed = Math.Max(_animationSpeed - 0.1f, 0.1f);
                            _animationRenderer.SetAnimationSpeed(_animationSpeed);
                            _animationSpeedLabel.Text =
                                $"Animation Speed (Q/W): {_animationSpeed:F1}";
                        }
                        break;

                    case Key.W:
                        // Increase animation speed
                        if (_animationRenderer != null)
                        {
                            _animationSpeed = Math.Min(_animationSpeed + 0.1f, 5.0f);
                            _animationRenderer.SetAnimationSpeed(_animationSpeed);
                            _animationSpeedLabel.Text =
                                $"Animation Speed (Q/W): {_animationSpeed:F1}";
                        }
                        break;

                    case Key.Z:
                        // Decrease darkness effect intensity
                        _horrorEffectIntensity = Math.Max(_horrorEffectIntensity - 0.1f, 0f);
                        _horrorEffectLabel.Text =
                            $"Darkness Effect (Z/X): {_horrorEffectIntensity:F1}";
                        UpdateHorrorEffects();
                        break;

                    case Key.X:
                        // Increase darkness effect intensity
                        _horrorEffectIntensity = Math.Min(_horrorEffectIntensity + 0.1f, 2f);
                        _horrorEffectLabel.Text =
                            $"Darkness Effect (Z/X): {_horrorEffectIntensity:F1}";
                        UpdateHorrorEffects();
                        break;

                    case Key.D:
                        // Decrease ghost scale
                        _ghostScale = Math.Max(_ghostScale - 0.1f, 0.5f);
                        _ghostScaleLabel.Text = $"Ghost Scale (D/F): {_ghostScale:F1}";

                        // Apply ghost scale to people using reflection
                        ApplyGhostScale();
                        break;

                    case Key.F:
                        // Increase ghost scale
                        _ghostScale = Math.Min(_ghostScale + 0.1f, 2.0f);
                        _ghostScaleLabel.Text = $"Ghost Scale (D/F): {_ghostScale:F1}";

                        // Apply ghost scale to people using reflection
                        ApplyGhostScale();
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
        /// Applies the current ghost scale to all people in the scene
        /// </summary>
        private void ApplyGhostScale()
        {
            if (_animationRenderer == null)
                return;

            // Get the people field using reflection
            var peopleField = typeof(BuildingRenderer).GetField(
                "_people",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            if (peopleField != null)
            {
                var people =
                    peopleField.GetValue(_animationRenderer)
                    as System.Collections.Generic.List<PersonComponent>;
                if (people != null && people.Count > 0)
                {
                    foreach (var person in people)
                    {
                        if (person != null)
                        {
                            person.SetBaseScale(_ghostScale);
                        }
                    }
                }
            }
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
