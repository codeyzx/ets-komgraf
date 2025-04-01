using System;
using Core;
using Drawing;
using Godot;

namespace UI
{
    /// <summary>
    /// Represents the Karya3 scene, which displays a traditional building with horror elements.
    /// </summary>
    public partial class Karya3 : Node2D
    {
        // Building renderer
        private BuildingRenderer _buildingRenderer;

        // Building configuration
        private BuildingConfiguration _config = new BuildingConfiguration
        {
            PrimaryColor = new Color(0.3f, 0.3f, 0.35f), // Darker color for horror theme
            OutlineColor = new Color(0.1f, 0.1f, 0.1f),
            OutlineThickness = 2f,
            WindowLineCount = 3,
            RoofSegments = 10,
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

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();

            // Initialize building renderer
            _buildingRenderer = new BuildingRenderer(this, _config);

            // Initialize drawing parameters
            _buildingRenderer.InitializeDrawingParameters(GetViewportRect().Size);

            // Create UI controls
            CreateControls();

            // Create horror effects
            CreateHorrorEffects();
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
            _darkOverlay.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _darkOverlay.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _darkOverlay.Color = new Color(0, 0, 0, 0.3f);
            AddChild(_darkOverlay);

            // Create flicker timer
            _flickerTimer = new Timer();
            _flickerTimer.WaitTime = 0.1f;
            _flickerTimer.Autostart = false;
            _flickerTimer.OneShot = true;
            _flickerTimer.Timeout += OnFlickerTimeout;
            AddChild(_flickerTimer);
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
            vbox.Position = new Vector2(10, 10);
            AddChild(vbox);

            // Create title label
            Label titleLabel = new Label();
            titleLabel.Text = "Horror Animation Controls";
            titleLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
            vbox.AddChild(titleLabel);

            // Create animation controls label
            Label controlsLabel = new Label();
            controlsLabel.Text = "Press SPACE to start/restart animation";
            controlsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            vbox.AddChild(controlsLabel);

            // Create animation speed label
            _animationSpeedLabel = new Label();
            _animationSpeedLabel.Text = "Animation Speed (1-5): 1";
            _animationSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            vbox.AddChild(_animationSpeedLabel);

            // Create rotation speed label
            _rotationSpeedLabel = new Label();
            _rotationSpeedLabel.Text = "Rotation Speed (Q/W): 0";
            _rotationSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            vbox.AddChild(_rotationSpeedLabel);

            // Create scale speed label
            _scaleSpeedLabel = new Label();
            _scaleSpeedLabel.Text = "Scale Speed (A/S): 0";
            _scaleSpeedLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            vbox.AddChild(_scaleSpeedLabel);

            // Create horror effect label
            _horrorEffectLabel = new Label();
            _horrorEffectLabel.Text = "Horror Effect (Z/X): 1.0";
            _horrorEffectLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
            vbox.AddChild(_horrorEffectLabel);
        }

        /// <summary>
        /// Handles input events.
        /// </summary>
        /// <param name="event">The input event.</param>
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.Space:
                        // Start/restart animation
                        _buildingRenderer.StartAnimation();

                        // Start flicker effect
                        _flickerTimer.Start();
                        break;

                    case Key.Key1:
                        _buildingRenderer.SetAnimationSpeed(1.0f);
                        _animationSpeedLabel.Text = "Animation Speed (1-5): 1";
                        break;

                    case Key.Key2:
                        _buildingRenderer.SetAnimationSpeed(1.5f);
                        _animationSpeedLabel.Text = "Animation Speed (1-5): 2";
                        break;

                    case Key.Key3:
                        _buildingRenderer.SetAnimationSpeed(2.0f);
                        _animationSpeedLabel.Text = "Animation Speed (1-5): 3";
                        break;

                    case Key.Key4:
                        _buildingRenderer.SetAnimationSpeed(2.5f);
                        _animationSpeedLabel.Text = "Animation Speed (1-5): 4";
                        break;

                    case Key.Key5:
                        _buildingRenderer.SetAnimationSpeed(3.0f);
                        _animationSpeedLabel.Text = "Animation Speed (1-5): 5";
                        break;

                    case Key.Q:
                        // Decrease rotation speed by 0.5
                        float rotationSpeed = 0.5f;
                        _buildingRenderer.SetRotationSpeed(rotationSpeed);
                        _rotationSpeedLabel.Text = $"Rotation Speed (Q/W): {rotationSpeed}";
                        break;

                    case Key.W:
                        // Increase rotation speed by 0.5
                        rotationSpeed = 1.0f;
                        _buildingRenderer.SetRotationSpeed(rotationSpeed);
                        _rotationSpeedLabel.Text = $"Rotation Speed (Q/W): {rotationSpeed}";
                        break;

                    case Key.A:
                        // Decrease scale speed by 0.5
                        float scaleSpeed = 0.5f;
                        _buildingRenderer.SetScaleSpeed(scaleSpeed);
                        _scaleSpeedLabel.Text = $"Scale Speed (A/S): {scaleSpeed}";
                        break;

                    case Key.S:
                        // Increase scale speed by 0.5
                        scaleSpeed = 1.0f;
                        _buildingRenderer.SetScaleSpeed(scaleSpeed);
                        _scaleSpeedLabel.Text = $"Scale Speed (A/S): {scaleSpeed}";
                        break;

                    case Key.Z:
                        _horrorEffectIntensity = Math.Max(0, _horrorEffectIntensity - 0.1f);
                        _horrorEffectLabel.Text =
                            $"Horror Effect (Z/X): {_horrorEffectIntensity:F1}";
                        UpdateHorrorEffects();
                        break;

                    case Key.X:
                        _horrorEffectIntensity = Math.Min(2.0f, _horrorEffectIntensity + 0.1f);
                        _horrorEffectLabel.Text =
                            $"Horror Effect (Z/X): {_horrorEffectIntensity:F1}";
                        UpdateHorrorEffects();
                        break;
                }
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
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time elapsed since the last frame.</param>
        public override void _Process(double delta)
        {
            _buildingRenderer.UpdateAnimation((float)delta);
            QueueRedraw();
        }

        /// <summary>
        /// Called when the node needs to be redrawn.
        /// </summary>
        public override void _Draw()
        {
            _buildingRenderer.Draw();
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
