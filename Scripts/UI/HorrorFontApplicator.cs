using Godot;
using System;

namespace Scenes
{
    /// <summary>
    /// Utility class that applies horror fonts to all text elements in the game.
    /// This class should be attached to each scene that contains text elements.
    /// </summary>
    public partial class HorrorFontApplicator : Node
    {
        [Export]
        public string TitleFontPath { get; set; } = "res://Assets/Fonts/horror_title.ttf";
        
        [Export]
        public string SubtitleFontPath { get; set; } = "res://Assets/Fonts/horror_subtitle.ttf";
        
        [Export]
        public string TextFontPath { get; set; } = "res://Assets/Fonts/horror_text.ttf";
        
        [Export]
        public string ButtonFontPath { get; set; } = "res://Assets/Fonts/horror_button.ttf";
        
        [Export]
        public int TitleFontSize { get; set; } = 48;
        
        [Export]
        public int SubtitleFontSize { get; set; } = 32;
        
        [Export]
        public int TextFontSize { get; set; } = 18;
        
        [Export]
        public int ButtonFontSize { get; set; } = 24;
        
        [Export]
        public Color TitleFontColor { get; set; } = new Color(0.8f, 0.1f, 0.1f); // Blood red
        
        [Export]
        public Color SubtitleFontColor { get; set; } = new Color(0.7f, 0.0f, 0.0f); // Dark red
        
        [Export]
        public Color TextFontColor { get; set; } = new Color(0.8f, 0.8f, 0.8f); // Pale white
        
        [Export]
        public Color ButtonFontColor { get; set; } = new Color(0.6f, 0.1f, 0.1f); // Dark blood red
        
        [Export]
        public Color ButtonHoverFontColor { get; set; } = new Color(1.0f, 0.2f, 0.2f); // Bright blood red
        
        [Export]
        public Vector2 ShadowOffset { get; set; } = new Vector2(2, 2);
        
        [Export]
        public Color ShadowColor { get; set; } = new Color(0.0f, 0.0f, 0.0f, 0.7f);
        
        // Cache for loaded fonts
        private FontFile _titleFont;
        private FontFile _subtitleFont;
        private FontFile _textFont;
        private FontFile _buttonFont;
        
        public override void _Ready()
        {
            // Load fonts
            LoadFonts();
            
            // Apply fonts to all text elements in the scene
            ApplyFontsToScene(GetTree().Root);
            
            GD.Print("Horror fonts applied to scene");
        }
        
        private void LoadFonts()
        {
            try
            {
                // Try to load the title font
                if (ResourceLoader.Exists(TitleFontPath))
                {
                    _titleFont = GD.Load<FontFile>(TitleFontPath);
                }
                else
                {
                    GD.PrintErr($"Title font not found at path: {TitleFontPath}");
                }
                
                // Try to load the subtitle font
                if (ResourceLoader.Exists(SubtitleFontPath))
                {
                    _subtitleFont = GD.Load<FontFile>(SubtitleFontPath);
                }
                else
                {
                    GD.PrintErr($"Subtitle font not found at path: {SubtitleFontPath}");
                }
                
                // Try to load the text font
                if (ResourceLoader.Exists(TextFontPath))
                {
                    _textFont = GD.Load<FontFile>(TextFontPath);
                }
                else
                {
                    GD.PrintErr($"Text font not found at path: {TextFontPath}");
                }
                
                // Try to load the button font
                if (ResourceLoader.Exists(ButtonFontPath))
                {
                    _buttonFont = GD.Load<FontFile>(ButtonFontPath);
                }
                else
                {
                    GD.PrintErr($"Button font not found at path: {ButtonFontPath}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading fonts: {e.Message}");
            }
        }
        
        private void ApplyFontsToScene(Node node)
        {
            if (node == null) return;
            
            // Apply fonts based on node type
            if (node is Label label)
            {
                ApplyFontToLabel(label);
            }
            else if (node is RichTextLabel richTextLabel)
            {
                ApplyFontToRichTextLabel(richTextLabel);
            }
            else if (node is Button button)
            {
                ApplyFontToButton(button);
            }
            
            // Process all children recursively
            foreach (Node child in node.GetChildren())
            {
                ApplyFontsToScene(child);
            }
        }
        
        private void ApplyFontToLabel(Label label)
        {
            try
            {
                string nodeName = label.Name.ToString().ToLower();
                
                // Determine font type based on name or parent
                if (nodeName.Contains("title"))
                {
                    // Apply title font
                    if (_titleFont != null)
                    {
                        label.AddThemeFontOverride("font", _titleFont);
                        label.AddThemeFontSizeOverride("font_size", TitleFontSize);
                        label.AddThemeColorOverride("font_color", TitleFontColor);
                    }
                }
                else if (nodeName.Contains("subtitle") || 
                         nodeName.Contains("identity") ||
                         (label.GetParent() != null && label.GetParent().Name.ToString().ToLower().Contains("subtitle")))
                {
                    // Apply subtitle font
                    if (_subtitleFont != null)
                    {
                        label.AddThemeFontOverride("font", _subtitleFont);
                        label.AddThemeFontSizeOverride("font_size", SubtitleFontSize);
                        label.AddThemeColorOverride("font_color", SubtitleFontColor);
                    }
                }
                else
                {
                    // Apply regular text font
                    if (_textFont != null)
                    {
                        label.AddThemeFontOverride("font", _textFont);
                        label.AddThemeFontSizeOverride("font_size", TextFontSize);
                        label.AddThemeColorOverride("font_color", TextFontColor);
                    }
                }
                
                // Add shadow effect to all labels
                label.AddThemeConstantOverride("shadow_offset_x", (int)ShadowOffset.X);
                label.AddThemeConstantOverride("shadow_offset_y", (int)ShadowOffset.Y);
                label.AddThemeColorOverride("font_shadow_color", ShadowColor);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error applying font to label {label.Name}: {e.Message}");
            }
        }
        
        private void ApplyFontToRichTextLabel(RichTextLabel label)
        {
            try
            {
                // Apply text font to RichTextLabel
                if (_textFont != null)
                {
                    label.AddThemeFontOverride("normal_font", _textFont);
                    label.AddThemeFontSizeOverride("normal_font_size", TextFontSize);
                }
                
                // Use subtitle font for bold text
                if (_subtitleFont != null)
                {
                    label.AddThemeFontOverride("bold_font", _subtitleFont);
                    label.AddThemeFontSizeOverride("bold_font_size", TextFontSize);
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error applying font to rich text label {label.Name}: {e.Message}");
            }
        }
        
        private void ApplyFontToButton(Button button)
        {
            try
            {
                // Apply button font
                if (_buttonFont != null)
                {
                    button.AddThemeFontOverride("font", _buttonFont);
                    button.AddThemeFontSizeOverride("font_size", ButtonFontSize);
                    button.AddThemeColorOverride("font_color", ButtonFontColor);
                    button.AddThemeColorOverride("font_hover_color", ButtonHoverFontColor);
                }
                
                // Add shadow effect
                button.AddThemeConstantOverride("shadow_offset_x", (int)ShadowOffset.X);
                button.AddThemeConstantOverride("shadow_offset_y", (int)ShadowOffset.Y);
                button.AddThemeColorOverride("font_shadow_color", ShadowColor);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error applying font to button {button.Name}: {e.Message}");
            }
        }
    }
}
