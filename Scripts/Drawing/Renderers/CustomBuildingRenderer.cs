using System;
using System.Collections.Generic;
using Drawing.Components.Characters;
using Drawing.Configuration;
using Godot;

namespace Drawing.Renderers
{
    /// <summary>
    /// Custom renderer that extends BuildingRenderer but skips drawing the house structure.
    /// Only handles animations and other components.
    /// </summary>
    public class CustomBuildingRenderer : BuildingRenderer
    {
        private bool _skipHouseStructure = true;

        /// <summary>
        /// Initializes a new instance of the CustomBuildingRenderer class.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="config">The building configuration.</param>
        public CustomBuildingRenderer(CanvasItem canvas, BuildingConfiguration config)
            : base(canvas, config) { }

        /// <summary>
        /// Sets whether to skip drawing the house structure.
        /// </summary>
        /// <param name="skip">Whether to skip drawing the house structure.</param>
        public void SetSkipHouseStructure(bool skip)
        {
            _skipHouseStructure = skip;
        }

        /// <summary>
        /// Overrides the Draw method to skip drawing the house structure.
        /// </summary>
        public new void Draw()
        {
            // If not skipping house structure, call the base Draw method
            if (!_skipHouseStructure)
            {
                base.Draw();
                return;
            }

            // Otherwise, only draw animations and components
            // This is a simplified version of the BuildingRenderer.Draw method
            // that skips drawing the house structure

            // Draw fog and horror effects
            DrawHorrorEffects();

            // Draw people and animations
            DrawPeopleAndAnimations();
        }

        /// <summary>
        /// Draws the fog and horror effects.
        /// </summary>
        private void DrawHorrorEffects()
        {
            // Apply flickering effect to the entire scene
            Random _random = new Random();
            float _flickerEffect = 0;

            if (_random.NextDouble() < 0.05)
            {
                _flickerEffect = (float)_random.NextDouble() * 0.2f;
            }
            else
            {
                _flickerEffect = Math.Max(0, _flickerEffect - 0.01f);
            }

            // Draw fog particles if they exist
            var canvas = GetCanvas();
            if (canvas != null)
            {
                // Get fog particles through reflection
                var fogParticlesField = GetType()
                    .BaseType.GetField(
                        "_fogParticles",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );

                var fogParticleTypesField = GetType()
                    .BaseType.GetField(
                        "_fogParticleTypes",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );

                var fogParticleRotationsField = GetType()
                    .BaseType.GetField(
                        "_fogParticleRotations",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );

                if (
                    fogParticlesField != null
                    && fogParticleTypesField != null
                    && fogParticleRotationsField != null
                )
                {
                    var fogParticles = fogParticlesField.GetValue(this) as List<Vector2>;
                    var fogParticleTypes = fogParticleTypesField.GetValue(this) as List<int>;
                    var fogParticleRotations =
                        fogParticleRotationsField.GetValue(this) as List<float>;

                    if (
                        fogParticles != null
                        && fogParticleTypes != null
                        && fogParticleRotations != null
                    )
                    {
                        for (int i = 0; i < fogParticles.Count; i++)
                        {
                            var particle = fogParticles[i];
                            int particleType = fogParticleTypes[i];
                            float rotation = fogParticleRotations[i];
                            float size = (float)(_random.NextDouble() * 8 + 5);
                            Color fogColor = new Color(0.9f, 0.9f, 0.95f, 0.15f);

                            // Apply transformation for rotation
                            canvas.DrawSetTransform(particle, rotation, Vector2.One);

                            // Draw different scary shapes based on particleType
                            switch (particleType % 6)
                            {
                                case 0: // Skull-like shape
                                    canvas.DrawCircle(Vector2.Zero, size, fogColor);
                                    canvas.DrawCircle(
                                        new Vector2(-size / 3, -size / 3),
                                        size / 3,
                                        fogColor
                                    );
                                    canvas.DrawCircle(
                                        new Vector2(size / 3, -size / 3),
                                        size / 3,
                                        fogColor
                                    );
                                    canvas.DrawRect(
                                        new Rect2(-size / 2, 0, size, size / 3),
                                        fogColor
                                    );
                                    break;

                                case 1: // Ghost-like shape
                                    Vector2[] ghostPoints =
                                    [
                                        new Vector2(0, -size),
                                        new Vector2(size, -size / 2),
                                        new Vector2(size, size / 2),
                                        new Vector2(size / 2, size),
                                        new Vector2(0, size / 2),
                                        new Vector2(-size / 2, size),
                                        new Vector2(-size, size / 2),
                                        new Vector2(-size, -size / 2),
                                    ];

                                    Color[] ghostColors = new Color[ghostPoints.Length];
                                    for (int j = 0; j < ghostColors.Length; j++)
                                    {
                                        ghostColors[j] = fogColor;
                                    }

                                    canvas.DrawPolygon(ghostPoints, ghostColors);
                                    break;

                                default: // Simple mist cloud
                                    canvas.DrawCircle(Vector2.Zero, size, fogColor);
                                    break;
                            }

                            // Reset transform
                            canvas.DrawSetTransform(Vector2.Zero, 0, Vector2.One);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the people and animations.
        /// </summary>
        private void DrawPeopleAndAnimations()
        {
            // Get people through reflection
            var peopleField = GetType()
                .BaseType.GetField(
                    "_people",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

            if (peopleField != null)
            {
                var people = peopleField.GetValue(this) as List<PersonComponent>;

                if (people != null)
                {
                    foreach (var person in people)
                    {
                        if (person != null)
                        {
                            // Call Draw method on each person
                            person.Draw();
                        }
                    }
                }
            }

            // Get rolling head through reflection
            var rollingHeadField = GetType()
                .BaseType.GetField(
                    "_rollingHead",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

            if (rollingHeadField != null)
            {
                var rollingHead = rollingHeadField.GetValue(this) as PersonComponent;

                if (rollingHead != null)
                {
                    // Call Draw method on rolling head
                    rollingHead.Draw();
                }
            }
        }

        /// <summary>
        /// Gets the canvas from the base class.
        /// </summary>
        private CanvasItem GetCanvas()
        {
            var canvasField = GetType()
                .BaseType.GetField(
                    "_canvas",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

            if (canvasField != null)
            {
                return canvasField.GetValue(this) as CanvasItem;
            }

            return null;
        }
    }
}
