using Godot;
using System;
using System.Linq;
using Core;

namespace Drawing.Components
{
    /// <summary>
    /// Component for rendering a giant, terrifying humanoid figure.
    /// </summary>
    public class PersonComponent : BuildingComponent
    {
        private readonly Color _primaryColor;
        private readonly Color _outlineColor;
        private readonly float _outlineThickness;
        
        private Vector2 _position;
        private Vector2 _targetPosition;
        private float _rotation = 0f;
        private float _targetRotation = 0f;
        private float _scale = 1f;
        private float _targetScale = 1f;
        private bool _isVisible = false;
        
        // Animation parameters
        private float _moveSpeed = 100f;
        private float _rotationSpeed = 2f;
        private float _scaleSpeed = 1f;
        
        // Body proportions
        private float _bodyHeight;
        private float _bodyWidth;
        private float _headSize;
        private float _armLength;
        private float _legLength;
        
        // Horror animation effects
        private float _floatTime = 0f;
        private float _floatAmplitude = 5f;
        private float _floatSpeed = 1f;
        
        // Creepy hand movement
        private float _fingerWiggleTime = 0f;
        private float _fingerWiggleSpeed = 3f;
        
        // Creepy head tilt
        private float _headTiltAmount = 0f;
        private float _headTiltDirection = 1f;
        private float _headTiltSpeed = 0.5f;
        
        // Detached head effect
        private float _headDetachAmount = 0f;
        private float _headDetachDirection = 1f;
        private float _headDetachSpeed = 0.3f;
        private float _headRotationSpeed = 1.2f;
        
        // Blood drip effect
        private float _bloodDripTime = 0f;
        private float _bloodDripSpeed = 0.7f;
        private Random _random = new Random();
        
        // Rolling head animation
        private bool _isRollingHeadActive = false;
        private Vector2 _rollingHeadPosition;
        private float _rollingHeadRotation = 0f;
        private float _rollingHeadScale = 0.5f;
        private float _rollingHeadSpeed = 150f;
        private float _rollingHeadTargetX;
        private bool _isRollingHeadZooming = false;
        private float _rollingHeadZoomSpeed = 5f;

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
            Vector2 startPosition) : base(canvas, primitif, dimensions, scaleFactor)
        {
            _primaryColor = primaryColor;
            _outlineColor = outlineColor;
            _outlineThickness = outlineThickness;
            _position = startPosition;
            _targetPosition = startPosition;
            
            // Initialize body proportions - make it extremely tall
            _bodyHeight = 200f * scaleFactor;
            _bodyWidth = 40f * scaleFactor;
            _headSize = 30f * scaleFactor;
            _armLength = 120f * scaleFactor;
            _legLength = 100f * scaleFactor;
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
            
            // Update floating animation
            _floatTime += _floatSpeed * delta * speedMultiplier;
            
            // Update finger wiggle animation
            _fingerWiggleTime += _fingerWiggleSpeed * delta * speedMultiplier;
            
            // Update head tilt animation (creepy head movement)
            _headTiltAmount += _headTiltDirection * _headTiltSpeed * delta * speedMultiplier;
            if (Math.Abs(_headTiltAmount) > 0.3f)
            {
                _headTiltDirection *= -1;
            }
            
            // Update head detach animation
            _headDetachAmount += _headDetachDirection * _headDetachSpeed * delta * speedMultiplier;
            if (_headDetachAmount > 10f)
            {
                _headDetachAmount = 10f;
                _headDetachDirection = -1f;
            }
            else if (_headDetachAmount < 0f)
            {
                _headDetachAmount = 0f;
                _headDetachDirection = 1f;
            }
            
            // Update blood drip animation
            _bloodDripTime += _bloodDripSpeed * delta * speedMultiplier;
            
            // Update rolling head animation if active
            if (_isRollingHeadActive)
            {
                if (!_isRollingHeadZooming)
                {
                    // Roll the head
                    _rollingHeadPosition.X += _rollingHeadSpeed * delta * speedMultiplier;
                    _rollingHeadRotation += 5f * delta * speedMultiplier;
                    
                    // Check if head reached target position
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
        }

        /// <summary>
        /// Draws the person component.
        /// </summary>
        public override void Draw()
        {
            if (!_isVisible)
                return;

            // Calculate floating effect for creepy movement
            float floatY = (float)Math.Sin(_floatTime) * _floatAmplitude;
            
            // Apply scale and rotation transformations
            Canvas.DrawSetTransform(
                _position + new Vector2(0, floatY), 
                _rotation, 
                new Vector2(_scale, _scale)
            );

            // Draw the giant humanoid figure
            DrawGiantHumanoid();

            // Reset transformation
            Canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
        }
        
        /// <summary>
        /// Draws a giant, terrifying humanoid figure.
        /// </summary>
        private void DrawGiantHumanoid()
        {
            // Draw head with creepy tilt
            DrawHead(new Vector2(0, -_bodyHeight * 0.9f), _headSize, _headTiltAmount);
            
            // Draw torso (elongated for more menacing appearance)
            Vector2[] torsoPoints = new Vector2[]
            {
                new Vector2(-_bodyWidth/2, -_bodyHeight * 0.8f),
                new Vector2(_bodyWidth/2, -_bodyHeight * 0.8f),
                new Vector2(_bodyWidth/2 * 0.8f, 0),
                new Vector2(-_bodyWidth/2 * 0.8f, 0)
            };
            
            Canvas.DrawPolygon(
                torsoPoints,
                new Color[] { _primaryColor, _primaryColor, _primaryColor, _primaryColor }
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
            
            // Draw arms (extra long and menacing)
            DrawArm(true);  // Left arm
            DrawArm(false); // Right arm
            
            // Draw legs
            DrawLeg(true);  // Left leg
            DrawLeg(false); // Right leg
            
            // Draw blood drips for horror effect
            DrawBloodDrips();
        }
        
        /// <summary>
        /// Draws the head of the giant humanoid.
        /// </summary>
        private void DrawHead(Vector2 position, float size, float tilt)
        {
            // Apply head tilt for creepy effect
            Canvas.DrawSetTransform(
                position + new Vector2(0, _headDetachAmount), // Detached head effect
                tilt,
                Vector2.One
            );
            
            // Draw the head (slightly oval for creepier look)
            Canvas.DrawCircle(Vector2.Zero, size, _primaryColor);
            
            // Draw a crack across the head for horror effect
            float crackWidth = size * 1.5f;
            float crackOffset = size * 0.2f;
            
            // Jagged crack line
            Vector2[] crackPoints = new Vector2[6];
            crackPoints[0] = new Vector2(-crackWidth/2, -crackOffset);
            crackPoints[1] = new Vector2(-crackWidth/3, -crackOffset + size * 0.1f);
            crackPoints[2] = new Vector2(-crackWidth/6, -crackOffset - size * 0.15f);
            crackPoints[3] = new Vector2(crackWidth/6, -crackOffset + size * 0.05f);
            crackPoints[4] = new Vector2(crackWidth/3, -crackOffset - size * 0.1f);
            crackPoints[5] = new Vector2(crackWidth/2, -crackOffset);
            
            // Draw the crack
            for (int i = 0; i < crackPoints.Length - 1; i++)
            {
                Primitif.DrawBresenhamLine(
                    Canvas,
                    crackPoints[i],
                    crackPoints[i+1],
                    Colors.Black,
                    _outlineThickness * 0.8f
                );
            }
            
            // Draw exposed brain/flesh in the crack
            for (int i = 0; i < crackPoints.Length - 1; i++)
            {
                Vector2 midPoint = (crackPoints[i] + crackPoints[i+1]) / 2;
                Vector2 fleshPoint = midPoint + new Vector2(0, -size * 0.05f);
                Canvas.DrawCircle(fleshPoint, size * 0.05f, new Color(0.8f, 0.2f, 0.2f));
            }
            
            Canvas.DrawArc(
                Vector2.Zero,
                size + _outlineThickness/2,
                0,
                Mathf.Tau,
                32,
                _outlineColor
            );
            
            // Draw creepy eyes (large, hollow)
            float eyeSize = size * 0.25f;
            float eyeSpacing = size * 0.4f;
            
            // Left eye - hollow black with blood
            Canvas.DrawCircle(new Vector2(-eyeSpacing, -size * 0.1f), eyeSize, Colors.Black);
            Canvas.DrawCircle(new Vector2(-eyeSpacing, -size * 0.1f), eyeSize * 0.2f, new Color(0.8f, 0, 0));
            
            // Right eye - hollow black with blood
            Canvas.DrawCircle(new Vector2(eyeSpacing, -size * 0.1f), eyeSize, Colors.Black);
            Canvas.DrawCircle(new Vector2(eyeSpacing, -size * 0.1f), eyeSize * 0.2f, new Color(0.8f, 0, 0));
            
            // Draw creepy smile (jagged)
            DrawCreepyMouth(new Vector2(0, size * 0.3f), size * 0.7f);
            
            // Reset transform after drawing head
            Canvas.DrawSetTransform(
                _position,
                _rotation,
                new Vector2(_scale, _scale)
            );
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
                float xPos = -width/2 + (width * i / (segments - 1));
                float yOffset = (i % 2 == 0) ? height * 0.5f : 0;
                
                mouthPoints[i] = position + new Vector2(xPos, yOffset);
                mouthPoints[segments * 2 - i - 1] = position + new Vector2(xPos, height + yOffset * 0.5f);
            }
            
            // Draw mouth filled with dark red
            Canvas.DrawPolygon(mouthPoints, Enumerable.Repeat(new Color(0.5f, 0, 0), mouthPoints.Length).ToArray());
            
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
            Vector2 shoulderPos = new Vector2(sideMultiplier * shoulderWidth/2, -_bodyHeight * 0.7f);
            
            // Elbow position (bent outward for more threatening pose)
            Vector2 elbowPos = shoulderPos + new Vector2(
                sideMultiplier * _armLength * 0.5f,
                _armLength * 0.3f
            );
            
            // Hand position with finger wiggle animation for creepy effect
            float fingerWiggle = (float)Math.Sin(_fingerWiggleTime) * 10f;
            Vector2 handPos = elbowPos + new Vector2(
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
            Primitif.DrawBresenhamLine(
                Canvas,
                elbowPos,
                handPos,
                _primaryColor,
                _bodyWidth * 0.2f
            );
            
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
                float angle = (i - fingerCount/2) * 0.25f;
                
                // Add wiggle to each finger for creepy effect
                float wiggleOffset = (float)Math.Sin(_fingerWiggleTime + i) * 5f;
                
                // Calculate finger positions
                Vector2 fingerBase = position;
                Vector2 fingerTip = fingerBase + new Vector2(
                    (float)Math.Cos(angle) * size * sideMultiplier + wiggleOffset * sideMultiplier,
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
                Vector2 clawTip = fingerTip + new Vector2(
                    sideMultiplier * size * 0.2f,
                    size * 0.1f
                );
                
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
            Vector2 kneePos = hipPos + new Vector2(
                sideMultiplier * _legLength * 0.2f,
                _legLength * 0.5f
            );
            
            // Foot position
            Vector2 footPos = kneePos + new Vector2(
                sideMultiplier * _legLength * 0.1f,
                _legLength * 0.5f
            );
            
            // Draw upper leg
            Primitif.DrawBresenhamLine(
                Canvas,
                hipPos,
                kneePos,
                _primaryColor,
                _bodyWidth * 0.3f
            );
            
            // Draw lower leg
            Primitif.DrawBresenhamLine(
                Canvas,
                kneePos,
                footPos,
                _primaryColor,
                _bodyWidth * 0.25f
            );
            
            // Draw foot
            Vector2 toePos = footPos + new Vector2(sideMultiplier * footSize, 0);
            Primitif.DrawBresenhamLine(
                Canvas,
                footPos,
                toePos,
                _primaryColor,
                _bodyWidth * 0.2f
            );
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
                float xOffset = (i - dripCount/2) * _headSize * 0.3f;
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
            DrawBloodOnHands();
        }
        
        /// <summary>
        /// Draws blood stains on hands for horror effect.
        /// </summary>
        private void DrawBloodOnHands()
        {
            // Blood color
            Color bloodColor = new Color(0.7f, 0, 0);
            
            // Left hand position
            Vector2 leftHandPos = new Vector2(
                -_bodyWidth/2 - _armLength,
                -_bodyHeight * 0.7f + _armLength
            );
            
            // Right hand position
            Vector2 rightHandPos = new Vector2(
                _bodyWidth/2 + _armLength,
                -_bodyHeight * 0.7f + _armLength
            );
            
            // Draw blood splatters on hands
            DrawBloodSplatter(leftHandPos, _bodyWidth * 0.8f, bloodColor);
            DrawBloodSplatter(rightHandPos, _bodyWidth * 0.8f, bloodColor);
        }
        
        /// <summary>
        /// Draws a blood splatter effect.
        /// </summary>
        private void DrawBloodSplatter(Vector2 center, float radius, Color bloodColor)
        {
            int splatterCount = 5;
            
            for (int i = 0; i < splatterCount; i++)
            {
                float angle = i * Mathf.Tau / splatterCount;
                float distance = radius * (0.5f + (float)_random.NextDouble() * 0.5f);
                
                Vector2 splatterPos = center + new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );
                
                float size = radius * 0.2f * (0.5f + (float)_random.NextDouble() * 0.5f);
                Canvas.DrawCircle(splatterPos, size, bloodColor);
            }
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
            Canvas.DrawCircle(new Vector2(0, headSize * 0.6f), headSize * 0.4f, new Color(0.7f, 0.1f, 0.1f));
            
            // Draw jagged flesh around neck
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.Tau / 8;
                float radius = headSize * 0.4f;
                Vector2 basePoint = new Vector2(0, headSize * 0.6f);
                Vector2 fleshPoint = basePoint + new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
                
                Vector2 outerPoint = basePoint + new Vector2(
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
            Canvas.DrawCircle(new Vector2(-eyeSpacing, -headSize * 0.25f), eyeSize * 0.5f, Colors.Black);
            Canvas.DrawCircle(new Vector2(eyeSpacing, -headSize * 0.25f), eyeSize * 0.5f, Colors.Black);
            
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
                    mouthPoints[i+1],
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
    }
}
