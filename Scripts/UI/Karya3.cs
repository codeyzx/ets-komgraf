using System;
using System.Collections.Generic;
using Core;
using Drawing;
using Godot;

namespace UI
{
    /// <summary>
    /// Renders a traditional building structure with customizable properties.
    /// Uses a modular architecture with separate components for different parts of the building.
    /// </summary>
    public partial class Karya3 : Node2D
    {
        // Building renderer and configuration
        private BuildingRenderer _buildingRenderer;
        private BuildingConfiguration _config = new BuildingConfiguration();

        // Animation state
        private bool _animationStarted = false;
        private float _animationSpeed = 1.0f;
        private float _rotationSpeed = 1.0f;
        private float _scaleSpeed = 1.0f;

        // UI elements for displaying controls
        private Label _controlsLabel;
        private Label _statusLabel;

        // Export properties to make them configurable in the Godot editor
        [Export]
        private Color PrimaryColor
        {
            get => _config.PrimaryColor;
            set => _config.PrimaryColor = value;
        }

        [Export]
        private Color SecondaryColor
        {
            get => _config.SecondaryColor;
            set => _config.SecondaryColor = value;
        }

        [Export]
        private Color OutlineColor
        {
            get => _config.OutlineColor;
            set => _config.OutlineColor = value;
        }

        [Export]
        private float BaseWidth
        {
            get => _config.BaseWidth;
            set => _config.BaseWidth = value;
        }

        [Export]
        private float BaseHeight
        {
            get => _config.BaseHeight;
            set => _config.BaseHeight = value;
        }

        [Export]
        private float RoofHeight
        {
            get => _config.RoofHeight;
            set => _config.RoofHeight = value;
        }

        [Export]
        private float WallHeight
        {
            get => _config.WallHeight;
            set => _config.WallHeight = value;
        }

        [Export]
        private float StairHeight
        {
            get => _config.StairHeight;
            set => _config.StairHeight = value;
        }

        [Export]
        private float OutlineThickness
        {
            get => _config.OutlineThickness;
            set => _config.OutlineThickness = value;
        }

        [Export]
        private int RoofSegments
        {
            get => _config.RoofSegments;
            set => _config.RoofSegments = value;
        }

        [Export]
        private int PanelCount
        {
            get => _config.PanelCount;
            set => _config.PanelCount = value;
        }

        [Export]
        private int ColumnCount
        {
            get => _config.ColumnCount;
            set => _config.ColumnCount = value;
        }

        [Export]
        private int WindowLineCount
        {
            get => _config.WindowLineCount;
            set => _config.WindowLineCount = value;
        }

        [Export]
        private float LadderWidth
        {
            get => _config.LadderWidth;
            set => _config.LadderWidth = value;
        }

        [Export]
        private float LadderLength
        {
            get => _config.LadderLength;
            set => _config.LadderLength = value;
        }

        [Export]
        private int LadderStepCount
        {
            get => _config.LadderStepCount;
            set => _config.LadderStepCount = value;
        }

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// </summary>
        public override void _Ready()
        {
            // Initialize the building renderer
            _buildingRenderer = new BuildingRenderer(this, _config);

            // Create UI elements for displaying controls
            CreateControlsUI();

            // Force a redraw to render the building
            QueueRedraw();
        }

        /// <summary>
        /// Creates UI elements for displaying controls.
        /// </summary>
        private void CreateControlsUI()
        {
            // Create a label for displaying controls
            _controlsLabel = new Label
            {
                Text =
                    "Controls:\n"
                    + "Space - Start/Restart Animation\n"
                    + "1-5 - Set Animation Speed (1=slowest, 5=fastest)\n"
                    + "Q/W - Decrease/Increase Rotation Speed\n"
                    + "A/S - Decrease/Increase Scale Speed",
                Position = new Vector2(20, 20),
                Theme = new Theme(),
            };

            // Create a label for displaying status
            _statusLabel = new Label
            {
                Text = "Animation: Not Started",
                Position = new Vector2(20, 120),
                Theme = new Theme(),
            };

            // Add the labels to the scene
            AddChild(_controlsLabel);
            AddChild(_statusLabel);
        }

        /// <summary>
        /// Updates the status label with the current animation parameters.
        /// </summary>
        private void UpdateStatusLabel()
        {
            _statusLabel.Text =
                $"Animation: {(_animationStarted ? "Running" : "Not Started")}\n"
                + $"Speed: {_animationSpeed:F1}x\n"
                + $"Rotation: {_rotationSpeed:F1}\n"
                + $"Scale: {_scaleSpeed:F1}";
        }

        /// <summary>
        /// Called when the node needs to be redrawn.
        /// </summary>
        public override void _Draw()
        {
            // Initialize drawing parameters based on the current viewport size
            _buildingRenderer.InitializeDrawingParameters(GetViewportRect().Size);

            // Draw the building with all its components
            _buildingRenderer.Draw();
        }

        /// <summary>
        /// Called every frame to update the scene.
        /// </summary>
        public override void _Process(double delta)
        {
            base._Process(delta);

            // Update animation if started
            if (_animationStarted)
            {
                _buildingRenderer.UpdateAnimation((float)delta);
                QueueRedraw();
            }
        }

        /// <summary>
        /// Handles input events.
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            // Check for keyboard input
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                HandleKeyInput(keyEvent.Keycode);
            }
        }

        /// <summary>
        /// Handles keyboard input.
        /// </summary>
        private void HandleKeyInput(Key keycode)
        {
            switch (keycode)
            {
                case Key.Space:
                    // Start/restart animation
                    _animationStarted = true;
                    _buildingRenderer.StartAnimation();
                    break;

                case Key.Key1:
                    // Set animation speed to 0.5x
                    _animationSpeed = 0.5f;
                    _buildingRenderer.SetAnimationSpeed(_animationSpeed);
                    break;

                case Key.Key2:
                    // Set animation speed to 1.0x
                    _animationSpeed = 1.0f;
                    _buildingRenderer.SetAnimationSpeed(_animationSpeed);
                    break;

                case Key.Key3:
                    // Set animation speed to 1.5x
                    _animationSpeed = 1.5f;
                    _buildingRenderer.SetAnimationSpeed(_animationSpeed);
                    break;

                case Key.Key4:
                    // Set animation speed to 2.0x
                    _animationSpeed = 2.0f;
                    _buildingRenderer.SetAnimationSpeed(_animationSpeed);
                    break;

                case Key.Key5:
                    // Set animation speed to 3.0x
                    _animationSpeed = 3.0f;
                    _buildingRenderer.SetAnimationSpeed(_animationSpeed);
                    break;

                case Key.Q:
                    // Decrease rotation speed
                    _rotationSpeed = Math.Max(0.1f, _rotationSpeed - 0.5f);
                    _buildingRenderer.SetRotationSpeed(_rotationSpeed);
                    break;

                case Key.W:
                    // Increase rotation speed
                    _rotationSpeed += 0.5f;
                    _buildingRenderer.SetRotationSpeed(_rotationSpeed);
                    break;

                case Key.A:
                    // Decrease scale speed
                    _scaleSpeed = Math.Max(0.1f, _scaleSpeed - 0.5f);
                    _buildingRenderer.SetScaleSpeed(_scaleSpeed);
                    break;

                case Key.S:
                    // Increase scale speed
                    _scaleSpeed += 0.5f;
                    _buildingRenderer.SetScaleSpeed(_scaleSpeed);
                    break;
            }

            // Update the status label
            UpdateStatusLabel();
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
                // Queue a redraw to update the building with the new viewport size
                QueueRedraw();
            }
        }
    }
}
