using System;
using System.Collections.Generic;
using Core;
using Godot;

namespace Drawing.Components
{
    /// <summary>
    /// Component for drawing an animated Begu Ganjang ghost.
    /// </summary>
    public class PersonComponent : BuildingComponent
    {
        private readonly Color _ghostColor;
        private readonly Color _outlineColor;
        private readonly Color _tongueColor;
        private readonly float _outlineThickness;

        // Animation properties
        private Vector2 _position;
        private float _scale;
        private float _rotation;

        // Target properties for animation
        private Vector2 _targetPosition;
        private float _targetScale;
        private float _targetRotation;

        // Animation state
        private bool _isVisible = false;
        private float _animationProgress = 0f;
        private float _tongueAnimation = 0f;
        private float _floatingAnimation = 0f;

        private float _floatTime = 0f;
        private float _floatAmplitude = 5f;
        private float _floatSpeed = 1f;

        private float _tongueExtension = 0f;
        private float _tongueDirection = 1f;
        private float _tongueSpeed = 2f;

        private float _moveSpeed = 100f;
        private float _rotationSpeed = 2f;
        private float _scaleSpeed = 1f;

        /// <summary>
        /// Initializes a new instance of the PersonComponent class.
        /// </summary>
        public PersonComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color ghostColor,
            Color outlineColor,
            float outlineThickness,
            Vector2 initialPosition
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _ghostColor = ghostColor;
            _outlineColor = outlineColor;
            _tongueColor = new Color(0.9f, 0.2f, 0.2f); // Red color for tongue
            _outlineThickness = outlineThickness;

            // Initialize animation properties
            _position = initialPosition;
            _targetPosition = initialPosition;
            _scale = 0f;
            _targetScale = 1f;
            _rotation = 0f;
            _targetRotation = 0f;
        }

        /// <summary>
        /// Gets the current position of the person.
        /// </summary>
        /// <returns>The current position.</returns>
        public Vector2 GetPosition()
        {
            return _position;
        }

        /// <summary>
        /// Sets the target position for animation.
        /// </summary>
        public void SetTargetPosition(Vector2 position)
        {
            _targetPosition = position;
        }

        /// <summary>
        /// Sets the target scale for animation.
        /// </summary>
        public void SetTargetScale(float scale)
        {
            _targetScale = scale;
        }

        /// <summary>
        /// Sets the target rotation for animation.
        /// </summary>
        public void SetTargetRotation(float rotation)
        {
            _targetRotation = rotation;
        }

        /// <summary>
        /// Sets the visibility of the ghost.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        /// <summary>
        /// Updates the animation with the specified delta time and speed.
        /// </summary>
        public void UpdateAnimation(float delta, float speedMultiplier = 1.0f)
        {
            // Update position
            Vector2 direction = _targetPosition - _position;
            if (direction.Length() > 1f)
            {
                _position += direction.Normalized() * _moveSpeed * delta * speedMultiplier;
            }

            // Update rotation
            float rotationDiff = _targetRotation - _rotation;
            if (Math.Abs(rotationDiff) > 0.01f)
            {
                _rotation += rotationDiff * _rotationSpeed * delta * speedMultiplier;
            }

            // Update scale
            float scaleDiff = _targetScale - _scale;
            if (Math.Abs(scaleDiff) > 0.01f)
            {
                _scale += scaleDiff * _scaleSpeed * delta * speedMultiplier;
            }

            // Update tongue animation
            _tongueExtension += _tongueDirection * _tongueSpeed * delta * speedMultiplier;
            if (_tongueExtension > 0.5f)
            {
                _tongueExtension = 0.5f;
                _tongueDirection = -1f;
            }
            else if (_tongueExtension < 0f)
            {
                _tongueExtension = 0f;
                _tongueDirection = 1f;
            }

            // Update floating animation
            _floatTime += _floatSpeed * delta * speedMultiplier;
        }

        /// <summary>
        /// Draws the ghost.
        /// </summary>
        public override void Draw()
        {
            if (!_isVisible)
                return;

            // Calculate floating effect
            float floatY = (float)Math.Sin(_floatTime) * _floatAmplitude;

            // Apply scale and rotation transformations
            Canvas.DrawSetTransform(
                _position + new Vector2(0, floatY),
                _rotation,
                new Vector2(_scale, _scale)
            );

            // Draw the ghost body (tall and thin for Begu Ganjang)
            float bodyHeight = 80 * ScaleFactor;
            float bodyWidth = 15 * ScaleFactor;

            // Head
            float headRadius = 10 * ScaleFactor;
            Canvas.DrawCircle(new Vector2(0, -bodyHeight * 0.45f), headRadius, _ghostColor);

            // Outline for head - using DrawCircle with outline color instead of DrawCircleOutline
            Canvas.DrawArc(
                new Vector2(0, -bodyHeight * 0.45f),
                headRadius + _outlineThickness / 2,
                0,
                Mathf.Tau,
                32,
                _outlineColor
            );

            // Eyes (empty and hollow for creepy effect)
            float eyeRadius = 3 * ScaleFactor;
            Canvas.DrawCircle(
                new Vector2(-4 * ScaleFactor, -bodyHeight * 0.45f - 2 * ScaleFactor),
                eyeRadius,
                Colors.Black
            );
            Canvas.DrawCircle(
                new Vector2(4 * ScaleFactor, -bodyHeight * 0.45f - 2 * ScaleFactor),
                eyeRadius,
                Colors.Black
            );

            // Body (elongated for Begu Ganjang)
            Vector2[] bodyPoints = new Vector2[]
            {
                new Vector2(-bodyWidth / 2, -bodyHeight * 0.4f),
                new Vector2(bodyWidth / 2, -bodyHeight * 0.4f),
                new Vector2(bodyWidth / 2, bodyHeight * 0.5f),
                new Vector2(-bodyWidth / 2, bodyHeight * 0.5f),
            };
            Canvas.DrawPolygon(
                bodyPoints,
                new Color[] { _ghostColor, _ghostColor, _ghostColor, _ghostColor }
            );

            // Outline for body - using DrawLines instead of DrawPolygonOutline
            for (int i = 0; i < bodyPoints.Length; i++)
            {
                int nextIndex = (i + 1) % bodyPoints.Length;
                Primitif.DrawBresenhamLine(
                    Canvas,
                    bodyPoints[i],
                    bodyPoints[nextIndex],
                    _outlineColor,
                    _outlineThickness
                );
            }

            // Draw tongue (key feature of Begu Ganjang)
            float tongueLength = 30 * ScaleFactor * (1 + _tongueExtension);
            float tongueWidth = 5 * ScaleFactor;

            // Tongue base point is at the bottom of the head
            Vector2 tongueStart = new Vector2(0, -bodyHeight * 0.4f + 5 * ScaleFactor);

            // Create a wavy tongue effect
            Vector2[] tonguePoints = new Vector2[6];
            tonguePoints[0] = tongueStart;
            tonguePoints[1] = tongueStart + new Vector2(-tongueWidth / 2, tongueLength * 0.2f);
            tonguePoints[2] = tongueStart + new Vector2(-tongueWidth, tongueLength * 0.5f);
            tonguePoints[3] = tongueStart + new Vector2(0, tongueLength);
            tonguePoints[4] = tongueStart + new Vector2(tongueWidth, tongueLength * 0.5f);
            tonguePoints[5] = tongueStart + new Vector2(tongueWidth / 2, tongueLength * 0.2f);

            // Draw the tongue with a reddish color
            Color tongueColor = new Color(0.8f, 0.1f, 0.1f);
            Canvas.DrawPolygon(
                tonguePoints,
                new Color[]
                {
                    tongueColor,
                    tongueColor,
                    tongueColor,
                    tongueColor,
                    tongueColor,
                    tongueColor,
                }
            );

            // Outline for tongue - using DrawLines instead of DrawPolygonOutline
            for (int i = 0; i < tonguePoints.Length; i++)
            {
                int nextIndex = (i + 1) % tonguePoints.Length;
                Primitif.DrawBresenhamLine(
                    Canvas,
                    tonguePoints[i],
                    tonguePoints[nextIndex],
                    _outlineColor,
                    _outlineThickness * 0.5f
                );
            }

            // Reset transformation
            Canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
        }
    }
}
