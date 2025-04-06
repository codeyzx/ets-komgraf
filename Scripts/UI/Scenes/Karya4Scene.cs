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
    public partial class Karya4Scene : Node2D
    {
        // Building renderer
        private BuildingRenderer _buildingRenderer;

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
            IsLadderAnimating = true,
        };

        // UI controls
        private Label _animationSpeedLabel;
        private Label _rotationSpeedLabel;
        private Label _scaleSpeedLabel;
        private Label _horrorEffectLabel;
        private Label _ghostScaleLabel;
        private float _horrorEffectIntensity = 1.0f;
        private float _ghostScale = 1.0f;

        // Horror effects
        private ColorRect _darkOverlay;
        private Timer _flickerTimer;
        private Random _random = new Random();

        // Show Begu Ganjang intro scene first
        private IntroScene _introScene;

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
            // Initialize building renderer
            _buildingRenderer = new BuildingRenderer(this, _config);

            // Initialize drawing parameters
            _buildingRenderer.InitializeDrawingParameters(GetViewportRect().Size);

            // Create UI controls
            CreateControls();

            // Create horror effects
            CreateHorrorEffects();

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
            // Create dark overlay for horror atmosphere
            _darkOverlay = new ColorRect();
            _darkOverlay.AnchorRight = 1;
            _darkOverlay.AnchorBottom = 1;
            _darkOverlay.AnchorLeft = 0;
            _darkOverlay.AnchorTop = 0;

            // Set the size after the node is added to the scene tree
            CallDeferred(Node.MethodName.AddChild, _darkOverlay);
            CallDeferred("SetOverlaySize");

            _darkOverlay.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _darkOverlay.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _darkOverlay.Color = new Color(0, 0, 0, 0.3f);

            // Create flicker timer
            _flickerTimer = new Timer();
            _flickerTimer.WaitTime = 0.1f;
            _flickerTimer.Autostart = false;
            _flickerTimer.OneShot = true;
            _flickerTimer.Timeout += OnFlickerTimeout;
            CallDeferred(Node.MethodName.AddChild, _flickerTimer);
        }

        /// <summary>
        /// Sets the size of the dark overlay after it has been added to the scene tree
        /// </summary>
        private void SetOverlaySize()
        {
            if (_darkOverlay != null && _darkOverlay.IsInsideTree())
            {
                _darkOverlay.SetDeferred("size", GetViewportRect().Size);
            }
        }

        /// <summary>
        /// Creates UI controls for the animation.
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
            vbox.AddChild(titleLabel);

            // Create animation controls label
            Label controlsLabel = new Label();
            controlsLabel.Text = "Press SPACE to start/restart animation";
            controlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
            controlsLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(controlsLabel);

            // Create keyboard controls label
            Label keyboardControlsLabel = new Label();
            keyboardControlsLabel.Text = "Keyboard Controls";
            keyboardControlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            keyboardControlsLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(keyboardControlsLabel);

            // Create animation speed label
            _animationSpeedLabel = new Label();
            _animationSpeedLabel.Text = "Animation Speed (Q/W): 1.0";
            _animationSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _animationSpeedLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_animationSpeedLabel);

            // Create ghost scale label
            _ghostScaleLabel = new Label();
            _ghostScaleLabel.Text = "Ghost Scale (D/F): 1.0";
            _ghostScaleLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _ghostScaleLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_ghostScaleLabel);

            // Create darkness effect label
            _horrorEffectLabel = new Label();
            _horrorEffectLabel.Text = $"Darkness Effect (Z/X): {_horrorEffectIntensity:F1}";
            _horrorEffectLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            _horrorEffectLabel.HorizontalAlignment = HorizontalAlignment.Right;
            vbox.AddChild(_horrorEffectLabel);
        }

        /// <summary>
        /// Sets the size of the VBoxContainer after it has been added to the scene tree
        /// </summary>
        private void SetVBoxSize(VBoxContainer vbox)
        {
            if (vbox != null && vbox.IsInsideTree())
            {
                vbox.SetDeferred("size", new Vector2(300, GetViewportRect().Size.Y - 20));
            }
        }

        /// <summary>
        /// Handles the flicker effect timeout
        /// </summary>
        private void OnFlickerTimeout()
        {
            if (_random.NextDouble() < 0.3f * _horrorEffectIntensity)
            {
                // Create a brief flicker
                _darkOverlay.Color = new Color(
                    0,
                    0,
                    0,
                    0.3f + (float)_random.NextDouble() * 0.3f * _horrorEffectIntensity
                );
                _flickerTimer.WaitTime = 0.05f + (float)_random.NextDouble() * 0.1f;
            }
            else
            {
                // Reset flicker
                _darkOverlay.Color = new Color(0, 0, 0, 0.3f);
                _flickerTimer.WaitTime = 0.5f + (float)_random.NextDouble() * 2.0f;
            }

            _flickerTimer.Start();
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

            // Update the building renderer animation if it exists
            if (_buildingRenderer != null)
            {
                _buildingRenderer.UpdateAnimation((float)delta);

                // Apply ghost scale
                ApplyAnimationSettings();

                // Trigger a redraw
                QueueRedraw();
            }

            // Update flicker timer if it exists
            if (_flickerTimer != null && _flickerTimer.IsInsideTree() && !_flickerTimer.IsStopped())
            {
                // Ensure the timer is started
                if (!_flickerTimer.IsStopped())
                {
                    _flickerTimer.Paused = false;
                }
            }
        }

        /// <summary>
        /// Applies animation settings from UI controls to the animation
        /// </summary>
        private void ApplyAnimationSettings()
        {
            // Apply ghost scale to all people in the scene
            if (_buildingRenderer != null)
            {
                // We'll use reflection to access the private _people field in BuildingRenderer
                var peopleField = _buildingRenderer
                    .GetType()
                    .GetField(
                        "_people",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );

                if (peopleField != null)
                {
                    var people =
                        peopleField.GetValue(_buildingRenderer)
                        as System.Collections.Generic.List<Drawing.Components.Characters.PersonComponent>;
                    if (people != null)
                    {
                        // Apply different rotation settings for each ghost
                        for (int i = 0; i < people.Count; i++)
                        {
                            people[i].SetBaseScale(_ghostScale);

                            // Set default rotation behavior for each ghost
                            people[i].SetRotationBehavior(0, 1.0f + (i * 0.2f));
                        }
                    }
                }
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

            // Handle animation control keys
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Skip key handling if building renderer is not initialized
                if (_buildingRenderer == null)
                    return;

                switch (keyEvent.Keycode)
                {
                    case Key.Space:
                        // Start/restart animation
                        _buildingRenderer.StartAnimation();

                        // Start flicker effect if timer is in the scene tree
                        if (_flickerTimer.IsInsideTree())
                        {
                            _flickerTimer.Start();
                        }
                        else
                        {
                            // If timer is not yet in the scene tree, defer the start
                            CallDeferred("StartFlickerTimer");
                        }
                        break;

                    case Key.Q:
                        // Decrease animation speed
                        float animSpeed = Math.Max(
                            (
                                _buildingRenderer
                                    .GetType()
                                    .GetField(
                                        "_animationSpeed",
                                        BindingFlags.NonPublic | BindingFlags.Instance
                                    )
                                    .GetValue(_buildingRenderer) as float?
                                ?? 1.0f
                            ) - 0.1f,
                            0.1f
                        );
                        _buildingRenderer.SetAnimationSpeed(animSpeed);
                        _animationSpeedLabel.Text = $"Animation Speed (Q/W): {animSpeed:F1}";
                        break;

                    case Key.W:
                        // Increase animation speed
                        animSpeed = Math.Min(
                            (
                                _buildingRenderer
                                    .GetType()
                                    .GetField(
                                        "_animationSpeed",
                                        BindingFlags.NonPublic | BindingFlags.Instance
                                    )
                                    .GetValue(_buildingRenderer) as float?
                                ?? 1.0f
                            ) + 0.1f,
                            5.0f
                        );
                        _buildingRenderer.SetAnimationSpeed(animSpeed);
                        _animationSpeedLabel.Text = $"Animation Speed (Q/W): {animSpeed:F1}";
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
                        break;

                    case Key.F:
                        // Increase ghost scale
                        _ghostScale = Math.Min(_ghostScale + 0.1f, 2.0f);
                        _ghostScaleLabel.Text = $"Ghost Scale (D/F): {_ghostScale:F1}";
                        break;
                }
            }
        }

        /// <summary>
        /// Called when the node needs to be redrawn.
        /// </summary>
        public override void _Draw()
        {
            // Draw the building if the renderer has been initialized
            if (_buildingRenderer != null)
            {
                _buildingRenderer.Draw();
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
            if (what == NotificationWMSizeChanged && _buildingRenderer != null)
            {
                // Update drawing parameters with the new viewport size
                _buildingRenderer.InitializeDrawingParameters(GetViewportRect().Size);
            }
        }
    }
}
