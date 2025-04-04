using System;
using System.Collections.Generic;
using Drawing.Components.Characters;
using Drawing.Configuration;
using Godot;

namespace Drawing.Renderers
{
    /// <summary>
    /// A specialized renderer that extends BuildingRenderer but only draws animations and components,
    /// not the house structure itself.
    /// </summary>
    public class AnimationOnlyRenderer : BuildingRenderer
    {
        /// <summary>
        /// Initializes a new instance of the AnimationOnlyRenderer class.
        /// </summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="config">The building configuration.</param>
        public AnimationOnlyRenderer(CanvasItem canvas, BuildingConfiguration config)
            : base(canvas, config) { }

        /// <summary>
        /// Overrides the Draw method to only draw animations and components,
        /// not the house structure.
        /// </summary>
        public new void Draw()
        {
            // Call the base UpdateAnimation method to ensure animations are updated
            // This is normally called in _Process, but we call it here to be safe
            UpdateAnimation(0.016f); // Assume 60 FPS

            // Access the private fields using reflection
            var fogParticlesField = typeof(BuildingRenderer).GetField(
                "_fogParticles",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var fogParticleTypesField = typeof(BuildingRenderer).GetField(
                "_fogParticleTypes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var fogParticleRotationsField = typeof(BuildingRenderer).GetField(
                "_fogParticleRotations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var canvasField = typeof(BuildingRenderer).GetField(
                "_canvas",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var randomField = typeof(BuildingRenderer).GetField(
                "_random",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var peopleField = typeof(BuildingRenderer).GetField(
                "_people",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var rollingHeadField = typeof(BuildingRenderer).GetField(
                "_rollingHead",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            var animationEnabledField = typeof(BuildingRenderer).GetField(
                "_animationEnabled",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            if (
                fogParticlesField != null
                && fogParticleTypesField != null
                && fogParticleRotationsField != null
                && canvasField != null
                && randomField != null
                && peopleField != null
                && rollingHeadField != null
                && animationEnabledField != null
            )
            {
                var fogParticles = fogParticlesField.GetValue(this) as List<Vector2>;
                var fogParticleTypes = fogParticleTypesField.GetValue(this) as List<int>;
                var fogParticleRotations = fogParticleRotationsField.GetValue(this) as List<float>;
                var canvas = canvasField.GetValue(this) as CanvasItem;
                var random = randomField.GetValue(this) as Random;
                var people = peopleField.GetValue(this) as List<PersonComponent>;
                var rollingHead = rollingHeadField.GetValue(this) as PersonComponent;
                var animationEnabled = (bool)animationEnabledField.GetValue(this);

                if (
                    fogParticles != null
                    && fogParticleTypes != null
                    && fogParticleRotations != null
                    && canvas != null
                    && random != null
                    && people != null
                )
                {
                    // Draw fog particles
                    if (animationEnabled)
                    {
                        foreach (var particle in fogParticles)
                        {
                            int index = fogParticles.IndexOf(particle);
                            int particleType = fogParticleTypes[index];
                            float rotation = fogParticleRotations[index];
                            float size = (float)(random.NextDouble() * 8 + 5);
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
                                    Vector2[] ghostPoints = new Vector2[]
                                    {
                                        new Vector2(0, -size),
                                        new Vector2(size, -size / 2),
                                        new Vector2(size, size / 2),
                                        new Vector2(size / 2, size),
                                        new Vector2(0, size / 2),
                                        new Vector2(-size / 2, size),
                                        new Vector2(-size, size / 2),
                                        new Vector2(-size, -size / 2),
                                    };

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

                    // Draw people
                    foreach (var person in people)
                    {
                        if (person != null)
                        {
                            person.Draw();
                        }
                    }

                    // Draw rolling head
                    if (rollingHead != null)
                    {
                        rollingHead.Draw();
                    }
                }
            }
        }
    }
}
