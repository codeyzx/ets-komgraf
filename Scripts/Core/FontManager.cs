using System.Collections.Generic;
using Godot;

namespace Core
{
    /// <summary>
    /// Manages horror fonts throughout the application
    /// </summary>
    public static class FontManager
    {
        // Font paths - update these paths when you add the actual font files
        public const string HorrorTitle = "res://Assets/Fonts/horror_title.ttf";
        public const string HorrorSubtitle = "res://Assets/Fonts/horror_subtitle.ttf";
        public const string HorrorText = "res://Assets/Fonts/horror_text.ttf";
        public const string HorrorButton = "res://Assets/Fonts/horror_button.ttf";

        // Font sizes
        public const int TitleSize = 48;
        public const int SubtitleSize = 32;
        public const int TextSize = 18;
        public const int ButtonSize = 24;

        // Font colors
        public static readonly Color TitleColor = new Color(0.8f, 0.1f, 0.1f); // Blood red
        public static readonly Color SubtitleColor = new Color(0.7f, 0.0f, 0.0f); // Dark red
        public static readonly Color TextColor = new Color(0.8f, 0.8f, 0.8f); // Pale white
        public static readonly Color ButtonColor = new Color(0.6f, 0.1f, 0.1f); // Dark blood red
        public static readonly Color ButtonHoverColor = new Color(1.0f, 0.2f, 0.2f); // Bright blood red

        // Shadow properties
        public static readonly Vector2 TextShadowOffset = new Vector2(2, 2);
        public static readonly Color TextShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.7f);

        // Cache for loaded fonts
        private static readonly Dictionary<string, FontFile> _fontCache =
            new Dictionary<string, FontFile>();

        /// <summary>
        /// Applies title font to a Label
        /// </summary>
        public static void ApplyTitleFont(Label label)
        {
            if (label == null)
                return;

            FontFile font = GetFont(HorrorTitle);
            if (font != null)
            {
                label.AddThemeFontOverride("font", font);
                label.AddThemeFontSizeOverride("font_size", TitleSize);
                label.AddThemeColorOverride("font_color", TitleColor);

                // Add shadow effect
                label.AddThemeConstantOverride("shadow_offset_x", (int)TextShadowOffset.X);
                label.AddThemeConstantOverride("shadow_offset_y", (int)TextShadowOffset.Y);
                label.AddThemeColorOverride("font_shadow_color", TextShadowColor);
            }
        }

        /// <summary>
        /// Applies subtitle font to a Label
        /// </summary>
        public static void ApplySubtitleFont(Label label)
        {
            if (label == null)
                return;

            FontFile font = GetFont(HorrorSubtitle);
            if (font != null)
            {
                label.AddThemeFontOverride("font", font);
                label.AddThemeFontSizeOverride("font_size", SubtitleSize);
                label.AddThemeColorOverride("font_color", SubtitleColor);

                // Add shadow effect
                label.AddThemeConstantOverride("shadow_offset_x", (int)TextShadowOffset.X);
                label.AddThemeConstantOverride("shadow_offset_y", (int)TextShadowOffset.Y);
                label.AddThemeColorOverride("font_shadow_color", TextShadowColor);
            }
        }

        /// <summary>
        /// Applies text font to a Label
        /// </summary>
        public static void ApplyTextFont(Label label)
        {
            if (label == null)
                return;

            FontFile font = GetFont(HorrorText);
            if (font != null)
            {
                label.AddThemeFontOverride("font", font);
                label.AddThemeFontSizeOverride("font_size", TextSize);
                label.AddThemeColorOverride("font_color", TextColor);

                // Add shadow effect
                label.AddThemeConstantOverride("shadow_offset_x", (int)TextShadowOffset.X);
                label.AddThemeConstantOverride("shadow_offset_y", (int)TextShadowOffset.Y);
                label.AddThemeColorOverride("font_shadow_color", TextShadowColor);
            }
        }

        /// <summary>
        /// Applies text font to a RichTextLabel
        /// </summary>
        public static void ApplyTextFont(RichTextLabel label)
        {
            if (label == null)
                return;

            FontFile font = GetFont(HorrorText);
            if (font != null)
            {
                label.AddThemeFontOverride("normal_font", font);
                label.AddThemeFontSizeOverride("normal_font_size", TextSize);

                // Bold font can use the subtitle font
                FontFile boldFont = GetFont(HorrorSubtitle);
                if (boldFont != null)
                {
                    label.AddThemeFontOverride("bold_font", boldFont);
                    label.AddThemeFontSizeOverride("bold_font_size", TextSize);
                }
            }
        }

        /// <summary>
        /// Applies button font to a Button
        /// </summary>
        public static void ApplyButtonFont(Button button)
        {
            if (button == null)
                return;

            FontFile font = GetFont(HorrorButton);
            if (font != null)
            {
                button.AddThemeFontOverride("font", font);
                button.AddThemeFontSizeOverride("font_size", ButtonSize);
                button.AddThemeColorOverride("font_color", ButtonColor);
                button.AddThemeColorOverride("font_hover_color", ButtonHoverColor);

                // Add shadow effect
                button.AddThemeConstantOverride("shadow_offset_x", (int)TextShadowOffset.X);
                button.AddThemeConstantOverride("shadow_offset_y", (int)TextShadowOffset.Y);
                button.AddThemeColorOverride("font_shadow_color", TextShadowColor);
            }
        }

        /// <summary>
        /// Gets a font from cache or loads it if not cached
        /// </summary>
        private static FontFile GetFont(string path)
        {
            if (_fontCache.TryGetValue(path, out FontFile cachedFont))
            {
                return cachedFont;
            }

            // Try to load the font
            if (ResourceLoader.Exists(path))
            {
                FontFile font = GD.Load<FontFile>(path);
                if (font != null)
                {
                    _fontCache[path] = font;
                    return font;
                }

                GD.PrintErr($"Failed to load font: {path}");
                return null;
            }

            GD.PrintErr($"Font file not found: {path}");
            return null;
        }

        /// <summary>
        /// Applies horror fonts to all text elements in a scene
        /// </summary>
        public static void ApplyHorrorFontsToScene(Node scene)
        {
            if (scene == null)
                return;

            // Process all children recursively
            foreach (Node child in scene.GetChildren())
            {
                // Apply fonts based on node type
                if (child is Label label)
                {
                    // Determine font type based on name or parent
                    if (label.Name.ToString().ToLower().Contains("title"))
                    {
                        ApplyTitleFont(label);
                    }
                    else if (
                        label.Name.ToString().ToLower().Contains("subtitle")
                        || label.Name.ToString().ToLower().Contains("identity")
                        || label.GetParent().Name.ToString().ToLower().Contains("subtitle")
                    )
                    {
                        ApplySubtitleFont(label);
                    }
                    else
                    {
                        ApplyTextFont(label);
                    }
                }
                else if (child is RichTextLabel richTextLabel)
                {
                    ApplyTextFont(richTextLabel);
                }
                else if (child is Button button)
                {
                    ApplyButtonFont(button);
                }

                // Recursively process children
                ApplyHorrorFontsToScene(child);
            }
        }
    }
}
