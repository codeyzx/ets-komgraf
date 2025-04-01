using System;
using System.Collections.Generic;
using Core;
using Godot;

namespace Drawing.Components
{
    /// <summary>
    /// Component for drawing an animated person.
    /// </summary>
    public class PersonComponent : BuildingComponent
    {
        private readonly Color _personColor;
        private readonly Color _outlineColor;
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

        /// <summary>
        /// Initializes a new instance of the PersonComponent class.
        /// </summary>
        public PersonComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color personColor,
            Color outlineColor,
            float outlineThickness,
            Vector2 initialPosition
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _personColor = personColor;
            _outlineColor = outlineColor;
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
        /// Sets the visibility of the person.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        /// <summary>
        /// Updates the animation with the specified delta time and speed.
        /// </summary>
        public void UpdateAnimation(float delta, float speed)
        {
            if (!_isVisible)
                return;

            // Update animation progress
            _animationProgress = Math.Min(_animationProgress + delta * speed, 1f);

            // Interpolate position, scale, and rotation
            _position = _position.Lerp(_targetPosition, delta * speed * 2f);
            _scale = Mathf.Lerp(_scale, _targetScale, delta * speed * 3f);
            _rotation = Mathf.Lerp(_rotation, _targetRotation, delta * speed * 4f);
        }

        /// <summary>
        /// Draws the person.
        /// </summary>
        public override void Draw()
        {
            if (!_isVisible || _scale <= 0.01f)
                return;

            // In Godot, we need to apply transformations to our drawing coordinates
            // rather than transforming the canvas itself
            DrawPerson(_position, _scale, _rotation);
        }

        /// <summary>
        /// Draws the person figure with the specified transformations.
        /// </summary>
        private void DrawPerson(Vector2 position, float scale, float rotation)
        {
            float personHeight = 30f * ScaleFactor * scale;
            float personWidth = 15f * ScaleFactor * scale;

            // Apply transformations to all points
            Vector2 TransformPoint(Vector2 point)
            {
                // Scale
                Vector2 scaled = point * scale;

                // Rotate
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);
                Vector2 rotated = new Vector2(
                    scaled.X * cos - scaled.Y * sin,
                    scaled.X * sin + scaled.Y * cos
                );

                // Translate
                return rotated + position;
            }

            // Draw head
            float headRadius = 5f * ScaleFactor * scale;
            Vector2 headCenter = TransformPoint(new Vector2(0, -personHeight + headRadius));

            // Draw body
            Vector2[] body = new Vector2[]
            {
                TransformPoint(new Vector2(0, -personHeight + headRadius * 2)), // Neck
                TransformPoint(new Vector2(0, -personHeight / 3)), // Bottom of torso
            };

            // Draw arms
            Vector2[] leftArm = new Vector2[]
            {
                TransformPoint(new Vector2(0, -personHeight + headRadius * 3)), // Shoulder
                TransformPoint(new Vector2(-personWidth / 2, -personHeight / 2)), // Hand
            };

            Vector2[] rightArm = new Vector2[]
            {
                TransformPoint(new Vector2(0, -personHeight + headRadius * 3)), // Shoulder
                TransformPoint(new Vector2(personWidth / 2, -personHeight / 2)), // Hand
            };

            // Draw legs
            Vector2[] leftLeg = new Vector2[]
            {
                TransformPoint(new Vector2(0, -personHeight / 3)), // Hip
                TransformPoint(new Vector2(-personWidth / 3, 0)), // Foot
            };

            Vector2[] rightLeg = new Vector2[]
            {
                TransformPoint(new Vector2(0, -personHeight / 3)), // Hip
                TransformPoint(new Vector2(personWidth / 3, 0)), // Foot
            };

            // Draw the person parts
            DrawCircle(headCenter, headRadius);
            Primitif.DrawBresenhamLine(
                Canvas,
                body[0],
                body[1],
                _personColor,
                _outlineThickness * ScaleFactor * 0.8f
            );
            Primitif.DrawBresenhamLine(
                Canvas,
                leftArm[0],
                leftArm[1],
                _personColor,
                _outlineThickness * ScaleFactor * 0.8f
            );
            Primitif.DrawBresenhamLine(
                Canvas,
                rightArm[0],
                rightArm[1],
                _personColor,
                _outlineThickness * ScaleFactor * 0.8f
            );
            Primitif.DrawBresenhamLine(
                Canvas,
                leftLeg[0],
                leftLeg[1],
                _personColor,
                _outlineThickness * ScaleFactor * 0.8f
            );
            Primitif.DrawBresenhamLine(
                Canvas,
                rightLeg[0],
                rightLeg[1],
                _personColor,
                _outlineThickness * ScaleFactor * 0.8f
            );
        }

        /// <summary>
        /// Draws a filled circle.
        /// </summary>
        private void DrawCircle(Vector2 center, float radius)
        {
            // Draw the filled circle
            Canvas.DrawCircle(center, radius, _personColor);

            // Draw the outline
            int segments = 16;
            Vector2[] points = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.Tau / segments;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            for (int i = 0; i < segments; i++)
            {
                int nextIndex = (i + 1) % segments;
                Primitif.DrawBresenhamLine(
                    Canvas,
                    points[i],
                    points[nextIndex],
                    _outlineColor,
                    _outlineThickness * ScaleFactor * 0.8f
                );
            }
        }
    }
}
