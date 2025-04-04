using System;
using System.Linq;
using Core;
using Drawing.Components.Building;
using Drawing.Configuration;
using Godot;

namespace Drawing.Components.Characters
{
    /// <summary>
    /// Component for rendering a giant, terrifying humanoid figure.
    /// </summary>
    public class PersonComponent : BuildingComponent
    {
        #region Core Properties
        private readonly Color _primaryColor;
        private readonly Color _outlineColor;
        private readonly float _outlineThickness;

        private Vector2 _position;
        private Vector2 _targetPosition;
        private float _rotation = 0f;
        private float _targetRotation = 0f;
        private float _scale = 1f;
        private float _targetScale = 1f;
        private float _baseScale = 1f; // Base scale that affects overall size
        private bool _isVisible = false;
        private Random _random = new Random();
        private int _rotationBehaviorType = 0; // 0-Normal, 1-Reverse, 2-Oscillating, 3-Random, 4-Synchronized
        private float _rotationSpeedModifier = 1.0f; // Speed modifier for rotation
        #endregion

        #region Animation Parameters
        private float _moveSpeed = 100f;
        private float _rotationSpeed = 2f;
        private float _scaleSpeed = 1f;

        // Floating animation
        private float _floatTime = 0f;
        private float _floatAmplitude = 5f;
        private float _floatSpeed = 1f;

        // Creepy hand movement
        private float _fingerWiggleTime = 0f;
        private float _fingerWiggleSpeed = 3f;

        // Blood drip effect
        private float _bloodDripTime = 0f;
        private float _bloodDripSpeed = 0.7f;

        // Flickering effect
        private float _flickerTime = 0f;
        private float _flickerSpeed = 15f;
        private float _flickerIntensity = 0.4f;

        // 180-degree flip effect
        private bool _isVisible180Flip = false;
        private float _180FlipTimer = 0f;
        private float _180FlipInterval = 3.0f;

        // Distortion effect
        private float _distortionAmount = 0f;
        private float _distortionSpeed = 0.5f;
        private float _maxDistortion = 10f;
        private float _distortionDirection = 1f;

        // Head animation
        private float _headHoverDistance = 0f;
        private float _headTiltAmount = 0f;
        #endregion

        #region Body Proportions
        private float _bodyHeight;
        private float _bodyWidth;
        private float _headSize;
        private float _armLength;
        private float _legLength;
        #endregion

        #region Spawning Parameters
        private float _ghostSpreadFactor = 150f;
        private bool _useRandomSpawning = false;
        private float _verticalSpreadVariance = 100f;
        #endregion

        #region Rolling Head Animation
        private bool _isRollingHeadActive = false;
        private Vector2 _rollingHeadPosition;
        private float _rollingHeadRotation = 0f;
        private float _rollingHeadScale = 0.5f;
        private float _rollingHeadSpeed = 150f;
        private float _rollingHeadTargetX;
        private bool _isRollingHeadZooming = false;
        private float _rollingHeadZoomSpeed = 5f;
        #endregion

        /// <summary>
        /// Initializes a new instance of the PersonComponent class.
        /// </summary>
        public PersonComponent(
            CanvasItem canvas,
            Primitif primitif,
            BuildingDimensions dimensions,
            float scaleFactor,
            Color primaryColor,
            Color outlineColor,
            float outlineThickness,
            Vector2 startPosition
        )
            : base(canvas, primitif, dimensions, scaleFactor)
        {
            _primaryColor = primaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;

            // Randomize start position if spreading is enabled
            if (_useRandomSpawning)
            {
                float randomX =
                    startPosition.X + ((float)_random.NextDouble() * 2 - 1) * _ghostSpreadFactor;
                float randomY =
                    startPosition.Y - ((float)_random.NextDouble()) * _verticalSpreadVariance;
                _position = new Vector2(randomX, randomY);
                _targetPosition = _position;
            }
            else
            {
                _position = startPosition;
                _targetPosition = startPosition;
            }

            // Initialize body proportions - make it extremely tall
            _bodyHeight = 200f * scaleFactor;
            _bodyWidth = 40f * scaleFactor;
            _headSize = 30f * scaleFactor;
            _armLength = 120f * scaleFactor;
            _legLength = 100f * scaleFactor;

            // Randomize 180 flip timer
            _180FlipTimer = (float)_random.NextDouble() * _180FlipInterval;
        }

        #region Position and Animation Control
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
            // Add random offset for spreading effect
            if (_useRandomSpawning)
            {
                float randomX =
                    position.X + ((float)_random.NextDouble() * 2 - 1) * _ghostSpreadFactor;
                float randomY =
                    position.Y - ((float)_random.NextDouble()) * _verticalSpreadVariance;
                _targetPosition = new Vector2(randomX, randomY);
            }
            else
            {
                _targetPosition = position;
            }
        }

        /// <summary>
        /// Sets the visibility of the person.
        /// </summary>
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
        }

        /// <summary>
        /// Sets the target rotation for animation.
        /// </summary>
        public void SetTargetRotation(float rotation)
        {
            _targetRotation = rotation;
        }

        /// <summary>
        /// Sets the target scale for animation.
        /// </summary>
        public void SetTargetScale(float scale)
        {
            _targetScale = scale;
        }

        /// <summary>
        /// Sets the base scale for the character, affecting overall size.
        /// </summary>
        /// <param name="scale">The base scale to set (0.5 to 2.0 recommended)</param>
        public void SetBaseScale(float scale)
        {
            _baseScale = Math.Clamp(scale, 0.5f, 2.0f);
        }

        /// <summary>
        /// Sets the rotation behavior for this ghost character.
        /// </summary>
        /// <param name="behaviorType">The type of rotation behavior (0-Normal, 1-Reverse, 2-Oscillating, 3-Random, 4-Synchronized)</param>
        /// <param name="speedModifier">Speed modifier for the rotation (1.0f is default)</param>
        public void SetRotationBehavior(int behaviorType, float speedModifier)
        {
            _rotationBehaviorType = Math.Clamp(behaviorType, 0, 4);
            _rotationSpeedModifier = Math.Clamp(speedModifier, 0.5f, 3.0f);
        }
        #endregion

        #region Animation Updates
        /// <summary>
        /// Updates the animation with the specified delta time and speed.
        /// </summary>
        public void UpdateAnimation(float delta, float speedMultiplier = 1.0f)
        {
            UpdatePosition(delta, speedMultiplier);
            UpdateRotation(delta, speedMultiplier);
            UpdateScale(delta, speedMultiplier);
            UpdateEffects(delta, speedMultiplier);
            UpdateRollingHead(delta, speedMultiplier);
        }

        /// <summary>
        /// Updates the position animation.
        /// </summary>
        private void UpdatePosition(float delta, float speedMultiplier)
        {
            // Calculate direction to target position
            Vector2 direction = _targetPosition - _position;
            if (direction.Length() > 1f)
            {
                _position += direction.Normalized() * _moveSpeed * delta * speedMultiplier;
            }
        }

        /// <summary>
        /// Updates the rotation animation.
        /// </summary>
        private void UpdateRotation(float delta, float speedMultiplier)
        {
            // Apply different rotation behaviors based on the behavior type
            switch (_rotationBehaviorType)
            {
                case 0: // Normal
                    _targetRotation = (float)Math.Sin(_floatTime * _rotationSpeedModifier) * 0.15f;
                    break;
                case 1: // Reverse
                    _targetRotation = (float)Math.Sin(-_floatTime * _rotationSpeedModifier) * 0.15f;
                    break;
                case 2: // Oscillating
                    _targetRotation =
                        (float)Math.Sin(_floatTime * 3f * _rotationSpeedModifier)
                        * (float)Math.Cos(_floatTime * _rotationSpeedModifier)
                        * 0.25f;
                    break;
                case 3: // Random
                    if (_random.NextDouble() < 0.01f * _rotationSpeedModifier)
                    {
                        _targetRotation = (float)(_random.NextDouble() * 0.4f - 0.2f);
                    }
                    break;
                case 4: // Synchronized
                    _targetRotation = (float)Math.Sin(_floatTime * 2f) * 0.15f * _rotationSpeedModifier;
                    break;
            }

            // Smoothly interpolate current rotation towards target rotation
            float diff = _targetRotation - _rotation;
            if (Math.Abs(diff) > 0.001f)
            {
                _rotation += diff * speedMultiplier * delta;
            }
        }

        /// <summary>
        /// Updates the scale animation.
        /// </summary>
        private void UpdateScale(float delta, float speedMultiplier)
        {
            float scaleDiff = _targetScale - _scale;
            if (Math.Abs(scaleDiff) > 0.01f)
            {
                _scale += scaleDiff * _scaleSpeed * delta * speedMultiplier;
            }
        }

        /// <summary>
        /// Updates various horror effects.
        /// </summary>
        private void UpdateEffects(float delta, float speedMultiplier)
        {
            // Update floating animation
            _floatTime += _floatSpeed * delta * speedMultiplier;

            // Update finger wiggle animation
            _fingerWiggleTime += _fingerWiggleSpeed * delta * speedMultiplier;

            // Update distortion effect
            _distortionAmount += _distortionDirection * _distortionSpeed * delta * speedMultiplier;
            if (Math.Abs(_distortionAmount) > _maxDistortion)
            {
                _distortionDirection *= -1;
            }

            // Update blood drip animation
            _bloodDripTime += _bloodDripSpeed * delta * speedMultiplier;

            // Update flickering effect
            _flickerTime += _flickerSpeed * delta * speedMultiplier;

            // Update 180-degree flip timer (sudden appearance behind you effect)
            _180FlipTimer += delta;
            if (_180FlipTimer > _180FlipInterval)
            {
                _180FlipTimer = 0f;
                _isVisible180Flip = !_isVisible180Flip;

                if (_isVisible180Flip)
                {
                    // Reverse direction when flipping
                    _rotation += Mathf.Pi;
                }
            }
        }

        /// <summary>
        /// Updates the rolling head animation if active.
        /// </summary>
        private void UpdateRollingHead(float delta, float speedMultiplier)
        {
            if (!_isRollingHeadActive)
                return;

            if (!_isRollingHeadZooming)
            {
                // Calculate the slope of the ladder (approximately 45 degrees or π/4 radians)
                float ladderAngle = Mathf.Pi / 4; // 45 degrees in radians

                // Roll the head along the ladder's slope
                float moveDistance = _rollingHeadSpeed * delta * speedMultiplier;

                // Update position along the ladder's slope
                _rollingHeadPosition.X += moveDistance * Mathf.Cos(ladderAngle);
                _rollingHeadPosition.Y += moveDistance * Mathf.Sin(ladderAngle);

                // Rotate the head as it rolls
                _rollingHeadRotation += 5f * delta * speedMultiplier;

                // Check if head reached target position (using X coordinate as threshold)
                if (_rollingHeadPosition.X >= _rollingHeadTargetX)
                {
                    _isRollingHeadZooming = true;
                    // Use center of the screen instead of dimensions
                    Vector2 viewportSize = Canvas.GetViewportRect().Size;
                    _rollingHeadPosition = viewportSize / 2;
                }
            }
            else
            {
                // Zoom the head
                _rollingHeadScale += _rollingHeadZoomSpeed * delta * speedMultiplier;

                // If head is too big, stop the animation
                if (_rollingHeadScale > 10f)
                {
                    _isRollingHeadActive = false;
                }
            }
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Draws the person component.
        /// </summary>
        public override void Draw()
        {
            if (!_isVisible)
                return;

            // Apply flickering effect for horror
            float flickerVisibility = (float)Math.Sin(_flickerTime) * _flickerIntensity;
            if (flickerVisibility < -0.3f)
            {
                // Occasionally skip rendering for flickering effect
                return;
            }
            float floatY = (float)Math.Sin(_floatTime) * _floatAmplitude;

            // Apply distortion for more horror
            float distortedScale = _scale * (1f + (float)Math.Sin(_distortionAmount) * 0.1f);

            // Apply position, rotation, and scale transformations with base scale
            Canvas.DrawSetTransform(
                _position + new Vector2(0, floatY),
                _rotation,
                new Vector2(distortedScale * _baseScale, distortedScale * _baseScale)
            );

            // Draw the giant humanoid figure
            DrawGiantHumanoid();

            // Reset transformation
            Canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);

            // Draw rolling head separately if active
            if (_isRollingHeadActive)
            {
                DrawRollingHead();
            }
        }

        /// <summary>
        /// Draws a giant, terrifying humanoid figure.
        /// </summary>
        private void DrawGiantHumanoid()
        {
            // Draw proper human-like head
            Vector2 headPosition = new Vector2(0, -_bodyHeight * 0.9f);
            DrawHead(headPosition, _headSize);

            // Draw torso (elongated for more menacing appearance)
            DrawTorso();

            // Draw arms (extra long and menacing)
            DrawArm(true); // Left arm
            DrawArm(false); // Right arm

            // Draw legs
            DrawLeg(true); // Left leg
            DrawLeg(false); // Right leg

            // Draw blood drips for horror effect
            DrawBloodDrips();
        }

        /// <summary>
        /// Draws a proper human-like head with improved horror effects.
        /// </summary>
        private void DrawHead(Vector2 position, float size)
        {
            // Draw the head as a proper circle (human-like)
            Canvas.DrawCircle(position, size, _primaryColor);

            // Draw head outline for better definition
            Canvas.DrawArc(position, size, 0, Mathf.Tau, 32, _outlineColor, _outlineThickness);

            // Draw creepy eyes (large, hollow)
            float eyeSize = size * 0.25f;
            float eyeSpacing = size * 0.4f;

            // Add distortion to eyes for more horror
            float eyeDistortion = (float)Math.Sin(_headHoverDistance * 2) * size * 0.1f;

            // Left eye - hollow black with blood
            Canvas.DrawCircle(
                position + new Vector2(-eyeSpacing, -size * 0.1f + eyeDistortion),
                eyeSize,
                Colors.Black
            );
            Canvas.DrawCircle(
                position + new Vector2(-eyeSpacing, -size * 0.1f + eyeDistortion),
                eyeSize * 0.2f,
                new Color(0.8f, 0, 0)
            );

            // Right eye - hollow black with blood
            Canvas.DrawCircle(
                position + new Vector2(eyeSpacing, -size * 0.1f - eyeDistortion),
                eyeSize,
                Colors.Black
            );
            Canvas.DrawCircle(
                position + new Vector2(eyeSpacing, -size * 0.1f - eyeDistortion),
                eyeSize * 0.2f,
                new Color(0.8f, 0, 0)
            );

            // Draw creepy smile (jagged)
            DrawCreepyMouth(position + new Vector2(0, size * 0.3f), size * 0.7f);
        }

        /// <summary>
        /// Draws a creepy, jagged mouth.
        /// </summary>
        private void DrawCreepyMouth(Vector2 position, float width)
        {
            float height = width * 0.3f;
            int segments = 8;

            Vector2[] mouthPoints = new Vector2[segments * 2];

            // Create jagged mouth shape
            for (int i = 0; i < segments; i++)
            {
                float xPos = -width / 2 + (width * i / (segments - 1));
                float yOffset = (i % 2 == 0) ? height * 0.5f : 0;

                mouthPoints[i] = position + new Vector2(xPos, yOffset);
                mouthPoints[segments * 2 - i - 1] =
                    position + new Vector2(xPos, height + yOffset * 0.5f);
            }

            // Draw mouth filled with dark red
            Canvas.DrawPolygon(
                mouthPoints,
                Enumerable.Repeat(new Color(0.5f, 0, 0), mouthPoints.Length).ToArray()
            );

            // Draw teeth (sharp and menacing)
            for (int i = 0; i < segments - 1; i++)
            {
                if (i % 2 == 0)
                {
                    Vector2 toothBase = mouthPoints[i];
                    Vector2 toothTip = toothBase + new Vector2(0, height * 0.7f);

                    Primitif.DrawBresenhamLine(
                        Canvas,
                        toothBase,
                        toothTip,
                        Colors.White,
                        _outlineThickness * 0.8f
                    );
                }
            }
        }

        /// <summary>
        /// Draws an arm with creepy, large hands.
        /// </summary>
        private void DrawArm(bool isLeft)
        {
            float sideMultiplier = isLeft ? -1 : 1;
            float shoulderWidth = _bodyWidth * 0.6f;
            float handSize = _bodyWidth * 0.7f;

            // Shoulder position
            Vector2 shoulderPos = new Vector2(
                sideMultiplier * shoulderWidth / 2,
                -_bodyHeight * 0.7f
            );

            // Elbow position (bent outward for more threatening pose)
            Vector2 elbowPos =
                shoulderPos + new Vector2(sideMultiplier * _armLength * 0.5f, _armLength * 0.3f);

            // Hand position with finger wiggle animation for creepy effect
            float fingerWiggle = (float)Math.Sin(_fingerWiggleTime) * 10f;
            Vector2 handPos =
                elbowPos
                + new Vector2(
                    sideMultiplier * _armLength * 0.5f + fingerWiggle * sideMultiplier,
                    _armLength * 0.7f
                );

            // Draw upper arm
            Primitif.DrawBresenhamLine(
                Canvas,
                shoulderPos,
                elbowPos,
                _primaryColor,
                _bodyWidth * 0.25f
            );

            // Draw lower arm
            Primitif.DrawBresenhamLine(Canvas, elbowPos, handPos, _primaryColor, _bodyWidth * 0.2f);

            // Draw creepy hand with long fingers
            DrawCreepyHand(handPos, handSize, isLeft);
        }

        /// <summary>
        /// Draws a creepy hand with long, sharp fingers.
        /// </summary>
        private void DrawCreepyHand(Vector2 position, float size, bool isLeft)
        {
            float sideMultiplier = isLeft ? -1 : 1;
            int fingerCount = 5;

            // Draw palm
            Canvas.DrawCircle(position, size * 0.3f, _primaryColor);

            // Draw long, sharp fingers
            for (int i = 0; i < fingerCount; i++)
            {
                float angle = (i - fingerCount / 2) * 0.25f;

                // Add wiggle to each finger for creepy effect
                float wiggleOffset = (float)Math.Sin(_fingerWiggleTime + i * 0.5f) * 5f;

                // Calculate finger positions
                Vector2 fingerBase = position;
                Vector2 fingerTip =
                    fingerBase
                    + new Vector2(
                        (float)Math.Cos(angle) * size * sideMultiplier
                            + wiggleOffset * sideMultiplier,
                        (float)Math.Sin(angle) * size - wiggleOffset
                    );

                // Draw finger
                Primitif.DrawBresenhamLine(
                    Canvas,
                    fingerBase,
                    fingerTip,
                    _primaryColor,
                    _outlineThickness * 1.5f
                );

                // Draw sharp fingernail/claw
                Vector2 clawTip =
                    fingerTip + new Vector2(sideMultiplier * size * 0.2f, size * 0.1f);

                Primitif.DrawBresenhamLine(
                    Canvas,
                    fingerTip,
                    clawTip,
                    _outlineColor,
                    _outlineThickness
                );
            }
        }

        /// <summary>
        /// Draws a leg.
        /// </summary>
        private void DrawLeg(bool isLeft)
        {
            float sideMultiplier = isLeft ? -1 : 1;
            float hipWidth = _bodyWidth * 0.4f;
            float footSize = _bodyWidth * 0.5f;

            // Hip position
            Vector2 hipPos = new Vector2(sideMultiplier * hipWidth, 0);

            // Knee position
            Vector2 kneePos =
                hipPos + new Vector2(sideMultiplier * _legLength * 0.2f, _legLength * 0.5f);

            // Foot position
            Vector2 footPos =
                kneePos + new Vector2(sideMultiplier * _legLength * 0.1f, _legLength * 0.5f);

            // Draw upper leg
            Primitif.DrawBresenhamLine(Canvas, hipPos, kneePos, _primaryColor, _bodyWidth * 0.3f);

            // Draw lower leg
            Primitif.DrawBresenhamLine(Canvas, kneePos, footPos, _primaryColor, _bodyWidth * 0.25f);

            // Draw foot
            Vector2 toePos = footPos + new Vector2(sideMultiplier * footSize, 0);
            Primitif.DrawBresenhamLine(Canvas, footPos, toePos, _primaryColor, _bodyWidth * 0.2f);
        }

        /// <summary>
        /// Draws blood drips for horror effect.
        /// </summary>
        private void DrawBloodDrips()
        {
            // Blood color
            Color bloodColor = new Color(0.7f, 0, 0);

            // Draw blood dripping from mouth
            int dripCount = 3;
            for (int i = 0; i < dripCount; i++)
            {
                float xOffset = (i - dripCount / 2) * _headSize * 0.3f;
                float yOffset = (float)Math.Sin(_bloodDripTime + i) * _headSize;

                // Drip starting position (mouth)
                Vector2 dripStart = new Vector2(xOffset, -_bodyHeight * 0.9f + _headSize * 0.5f);

                // Drip end position
                Vector2 dripEnd = dripStart + new Vector2(0, _headSize * 0.5f + yOffset);

                // Draw blood drip
                Primitif.DrawBresenhamLine(
                    Canvas,
                    dripStart,
                    dripEnd,
                    bloodColor,
                    _outlineThickness
                );

                // Draw blood drop at the end
                Canvas.DrawCircle(dripEnd, _outlineThickness * 1.5f, bloodColor);
            }

            // Draw blood on hands
            // DrawBloodOnHands();
        }

        /// <summary>
        /// Activates the rolling head animation.
        /// </summary>
        public void StartRollingHeadAnimation(Vector2 startPosition, float targetX)
        {
            _isRollingHeadActive = true;
            _rollingHeadPosition = startPosition;
            _rollingHeadTargetX = targetX;
            _rollingHeadRotation = 0f;
            _rollingHeadScale = 0.5f;
            _isRollingHeadZooming = false;
        }

        /// <summary>
        /// Checks if the rolling head animation is currently active.
        /// </summary>
        /// <returns>True if the rolling head animation is active, false otherwise.</returns>
        public bool IsRollingHeadActive()
        {
            return _isRollingHeadActive;
        }

        /// <summary>
        /// Draws a rolling head if the animation is active.
        /// </summary>
        public void DrawRollingHead()
        {
            if (!_isRollingHeadActive)
                return;

            // Apply transformation for the rolling head
            Canvas.DrawSetTransform(
                _rollingHeadPosition,
                _rollingHeadRotation,
                new Vector2(_rollingHeadScale, _rollingHeadScale)
            );

            // Draw the severed head
            float headSize = 30f;

            // Draw the head
            Canvas.DrawCircle(Vector2.Zero, headSize, _primaryColor);

            // Draw neck stump with blood
            Canvas.DrawCircle(
                new Vector2(0, headSize * 0.6f),
                headSize * 0.4f,
                new Color(0.7f, 0.1f, 0.1f)
            );

            // Draw jagged flesh around neck
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.Tau / 8;
                float radius = headSize * 0.4f;
                Vector2 basePoint = new Vector2(0, headSize * 0.6f);
                Vector2 fleshPoint =
                    basePoint
                    + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);

                Vector2 outerPoint =
                    basePoint
                    + new Vector2(
                        (float)Math.Cos(angle) * radius * 1.3f,
                        (float)Math.Sin(angle) * radius * 1.3f
                    );

                Primitif.DrawBresenhamLine(
                    Canvas,
                    fleshPoint,
                    outerPoint,
                    new Color(0.7f, 0.1f, 0.1f),
                    _outlineThickness
                );
            }

            // Draw eyes
            float eyeSize = headSize * 0.25f;
            float eyeSpacing = headSize * 0.4f;

            // Eyes rolled back in horror
            Canvas.DrawCircle(new Vector2(-eyeSpacing, -headSize * 0.1f), eyeSize, Colors.White);
            Canvas.DrawCircle(new Vector2(eyeSpacing, -headSize * 0.1f), eyeSize, Colors.White);

            // Pupils looking up (rolled back)
            Canvas.DrawCircle(
                new Vector2(-eyeSpacing, -headSize * 0.25f),
                eyeSize * 0.5f,
                Colors.Black
            );
            Canvas.DrawCircle(
                new Vector2(eyeSpacing, -headSize * 0.25f),
                eyeSize * 0.5f,
                Colors.Black
            );

            // Draw mouth in a horrified expression
            Vector2[] mouthPoints = new Vector2[6];
            mouthPoints[0] = new Vector2(-headSize * 0.4f, headSize * 0.2f);
            mouthPoints[1] = new Vector2(-headSize * 0.2f, headSize * 0.3f);
            mouthPoints[2] = new Vector2(0, headSize * 0.35f);
            mouthPoints[3] = new Vector2(headSize * 0.2f, headSize * 0.3f);
            mouthPoints[4] = new Vector2(headSize * 0.4f, headSize * 0.2f);

            for (int i = 0; i < mouthPoints.Length - 1; i++)
            {
                Primitif.DrawBresenhamLine(
                    Canvas,
                    mouthPoints[i],
                    mouthPoints[i + 1],
                    Colors.Black,
                    _outlineThickness
                );
            }

            // Draw blood dripping from mouth
            for (int i = 0; i < 3; i++)
            {
                float xOffset = (i - 1) * headSize * 0.2f;
                float yOffset = (float)Math.Sin(_bloodDripTime + i) * headSize * 0.2f;

                Vector2 bloodStart = new Vector2(xOffset, headSize * 0.3f);
                Vector2 bloodEnd = bloodStart + new Vector2(0, headSize * 0.2f + yOffset);

                Primitif.DrawBresenhamLine(
                    Canvas,
                    bloodStart,
                    bloodEnd,
                    new Color(0.7f, 0, 0),
                    _outlineThickness
                );
            }

            // Reset transformation
            Canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
        }

        /// <summary>
        /// Draws the torso of the humanoid figure.
        /// </summary>
        private void DrawTorso()
        {
            // Draw torso (elongated for more menacing appearance)
            Vector2[] torsoPoints =
            [
                new Vector2(-_bodyWidth / 2, -_bodyHeight * 0.8f),
                new Vector2(_bodyWidth / 2, -_bodyHeight * 0.8f),
                new Vector2(_bodyWidth / 2 * 0.8f, 0),
                new Vector2(-_bodyWidth / 2 * 0.8f, 0),
            ];

            Canvas.DrawPolygon(
                torsoPoints,
                [_primaryColor, _primaryColor, _primaryColor, _primaryColor]
            );

            // Draw torso outline
            for (int i = 0; i < torsoPoints.Length; i++)
            {
                int nextIndex = (i + 1) % torsoPoints.Length;
                Primitif.DrawBresenhamLine(
                    Canvas,
                    torsoPoints[i],
                    torsoPoints[nextIndex],
                    _outlineColor,
                    _outlineThickness
                );
            }
        }
        #endregion
    }
}
