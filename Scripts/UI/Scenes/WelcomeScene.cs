using System;
using System.Collections.Generic;
using Core;
using Godot;

namespace Scenes
{
    public partial class WelcomeScene : Control
    {
        // Nodes for effects
        private TextureRect _background;
        private TextureRect _pattern;
        private Control _mouseEffectContainer;
        private AudioStreamPlayer _backgroundMusic;
        private Timer _flickerTimer;
        private ShaderMaterial _backgroundMaterial;
        private ShaderMaterial _patternMaterial;
        private Label _titleLabel;
        private VBoxContainer _buttonsContainer;

        // Effect parameters
        private float _distortionIntensity = 0.0f;
        private float _noiseIntensity = 0.0f;
        private bool _isFlickering = false;
        private RandomNumberGenerator _rng = new RandomNumberGenerator();

        // Audio players
        private AudioStreamPlayer _heartbeatSound;
        private AudioStreamPlayer _sirenSound;
        private AudioStreamPlayer _hoverSound;

        // Timer for effects
        private Timer _effectTimer;
        private Timer _glitchTimer;
        private Timer _heartbeatTimer;

        // Random generator
        private List<Timer> _randomEffectTimers = new List<Timer>();

        // State tracking
        private bool _isIntenseModeActive = false;
        private float _intensityLevel = 0.0f;
        private float _mouseTrackingIntensity = 0.3f;
        private Vector2 _lastMousePosition = new Vector2(0.5f, 0.5f);

        // Add a field to track the last hovered button
        private Button _lastHoveredButton = null;

        // Track if the hover sound was recently played to prevent rapid firing
        private float _lastHoverSoundTime = 0f;

        public override void _Ready()
        {
            GD.Print("Welcome Scene Loaded");

            try
            {
                // Initialize nodes
                _background = GetNode<TextureRect>("MarginContainer/Background");
                _pattern = GetNode<TextureRect>("MarginContainer/Pattern");
                _titleLabel = GetNode<Label>("MarginContainer/VBoxContainer/Title");
                _buttonsContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");

                // Get shader materials
                _backgroundMaterial = (ShaderMaterial)_background.Material;
                _patternMaterial = (ShaderMaterial)_pattern.Material;

                // Setup audio
                SetupAudio();

                // Connect button signals for hover effects
                ConnectButtonHoverSignals();

                // Setup mouse effect container
                SetupMouseEffects();

                // Setup timers
                SetupTimers();

                // Set initial shader parameters
                _backgroundMaterial.SetShaderParameter("distortion_intensity", 0.5f);
                _backgroundMaterial.SetShaderParameter("noise_intensity", 0.3f);
                _patternMaterial.SetShaderParameter("distortion_intensity", 0.3f);

                // Update title text
                _titleLabel.Text = "Batak Mythology";

                // Create an innovative horror-themed button layout
                CreateHorrorMenuLayout();

                // Apply horror styling to buttons
                StyleButtons();

                // Connect button handlers safely
                try
                {
                    ConnectButtonSignals();
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Error connecting button signals: {e.Message}");
                }

                _rng.Randomize();

                // Start background effects
                StartBackgroundEffects();

                // Apply horror fonts to all text elements
                FontManager.ApplyHorrorFontsToScene(this);

                GD.Print("WelcomeScene initialized successfully");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in _Ready: {e.Message}");
            }
        }

        public override void _Process(double delta)
        {
            // Update mouse position for shaders
            UpdateMousePosition();

            // Update intensity based on audio
            UpdateIntensityFromAudio((float)delta);

            // Apply intensity to shaders
            ApplyIntensityToShaders();

            // Check for button hovering
            CheckButtonHovering();

            // Update distortion based on mouse position
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

            // Calculate normalized mouse position (0-1)
            float normalizedX = mousePos.X / viewportSize.X;
            float normalizedY = mousePos.Y / viewportSize.Y;

            // Update shader parameters based on mouse position
            _backgroundMaterial.SetShaderParameter(
                "mouse_position",
                new Vector2(normalizedX, normalizedY)
            );
            _patternMaterial.SetShaderParameter(
                "mouse_position",
                new Vector2(normalizedX, normalizedY)
            );

            // Randomly trigger effects
            if (_rng.Randf() < 0.0005f && !_isFlickering)
            {
                TriggerFlickerEffect();
            }

            if (_rng.Randf() < 0.0002f)
            {
                TriggerDistortionPulse();
            }
        }

        private void SetupAudio()
        {
            try
            {
                GD.Print("Setting up audio...");

                // Background music
                _backgroundMusic = new AudioStreamPlayer();
                AddChild(_backgroundMusic);
                _backgroundMusic.Stream = GD.Load<AudioStream>(
                    "res://Assets/Sounds/Horror/horror_ambience.mp3"
                );
                _backgroundMusic.VolumeDb = -10;
                _backgroundMusic.Play();

                // Heartbeat sound
                _heartbeatSound = new AudioStreamPlayer();
                AddChild(_heartbeatSound);
                _heartbeatSound.Stream = GD.Load<AudioStream>("res://Assets/Sounds/heartbeat.mp3");
                _heartbeatSound.VolumeDb = -5;

                // Siren sound
                _sirenSound = new AudioStreamPlayer();
                AddChild(_sirenSound);
                _sirenSound.Stream = GD.Load<AudioStream>("res://Assets/Sounds/siren.mp3");
                _sirenSound.VolumeDb = -15;

                // Hover sound
                _hoverSound = new AudioStreamPlayer();
                AddChild(_hoverSound);
                _hoverSound.Stream = GD.Load<AudioStream>("res://Assets/Sounds/hover.mp3");
                _hoverSound.VolumeDb = -5;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error setting up audio: {e.Message}");
            }
        }

        private void SetupMouseEffects()
        {
            // Create container for mouse effects
            _mouseEffectContainer = new Control();
            _mouseEffectContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            AddChild(_mouseEffectContainer);
        }

        private void SetupTimers()
        {
            // Create flicker timer
            _flickerTimer = new Timer();
            _flickerTimer.WaitTime = 0.05f; // Fast flicker
            _flickerTimer.Autostart = false;
            AddChild(_flickerTimer);
            _flickerTimer.Timeout += OnFlickerTimeout;

            // Create effect timer for periodic effects
            _effectTimer = new Timer();
            _effectTimer.WaitTime = 15.0f; // Every 15 seconds
            _effectTimer.Connect("timeout", new Callable(this, nameof(OnEffectTimerTimeout)));
            _effectTimer.Autostart = true;
            AddChild(_effectTimer);

            // Create glitch timer for shader glitches
            _glitchTimer = new Timer();
            _glitchTimer.WaitTime = 0.1f;
            _glitchTimer.Connect("timeout", new Callable(this, nameof(OnGlitchTimerTimeout)));
            _glitchTimer.Autostart = false;
            _glitchTimer.OneShot = true;
            AddChild(_glitchTimer);

            // Create heartbeat timer
            _heartbeatTimer = new Timer();
            _heartbeatTimer.WaitTime = _rng.RandfRange(20.0f, 40.0f); // Random time between 20-40 seconds
            _heartbeatTimer.Connect("timeout", new Callable(this, nameof(OnHeartbeatTimerTimeout)));
            _heartbeatTimer.Autostart = true;
            _heartbeatTimer.OneShot = true;
            AddChild(_heartbeatTimer);

            // Create random effect timers
            CreateRandomEffectTimers();
        }

        private void CreateRandomEffectTimers()
        {
            // Create 5 random effect timers
            for (int i = 0; i < 5; i++)
            {
                var timer = new Timer();
                timer.WaitTime = _rng.RandfRange(8.0f, 25.0f);
                timer.Connect("timeout", new Callable(this, nameof(OnRandomEffectTimeout)));
                timer.Autostart = true;
                timer.AddToGroup("random_effect_timer");
                AddChild(timer);
                _randomEffectTimers.Add(timer);
            }
        }

        private void StyleButtons()
        {
            try
            {
                GD.Print("Styling buttons with advanced horror effects");

                // Apply horror styling to all buttons in the container
                foreach (Node child in _buttonsContainer.GetChildren())
                {
                    if (child is Button button)
                    {
                        // Skip the title label
                        if (button.Name == "Title")
                            continue;

                        // Create a horror-themed font effect
                        // Dark red blood-like color with shadow
                        button.AddThemeColorOverride("font_color", new Color(0.6f, 0.05f, 0.05f));
                        button.AddThemeColorOverride(
                            "font_hover_color",
                            new Color(0.9f, 0.0f, 0.0f)
                        );

                        // Add shadow effect to text
                        button.AddThemeConstantOverride("outline_size", 1);
                        button.AddThemeColorOverride(
                            "font_outline_color",
                            new Color(0.1f, 0.0f, 0.0f, 0.8f)
                        );

                        // Increase font size for more dramatic effect
                        button.AddThemeConstantOverride("font_size", 28);

                        // Store original position to prevent unwanted movement
                        button.SetMeta("_original_position", button.Position);

                        // Add random subtle animation to each button
                        // But avoid the shake effect that causes positioning issues
                        int effectType = _rng.RandiRange(0, 2);
                        switch (effectType)
                        {
                            case 0:
                                // Flickering text effect
                                ApplyFlickerEffect(button);
                                break;
                            case 1:
                                // Blood drip effect
                                ApplyBloodEffect(button);
                                break;
                            case 2:
                                // Distortion effect
                                ApplyDistortionEffect(button);
                                break;
                        }

                        // Log button found
                        GD.Print($"Styled horror button: {button.Name}");
                    }
                }

                // Style the title label with horror effect
                StyleTitleLabel();

                GD.Print("Horror button styling completed");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error styling buttons: {e.Message}");
            }
        }

        // Apply a flickering text effect to a button
        private void ApplyFlickerEffect(Button button)
        {
            // Create a timer for the flicker effect
            Timer flickerTimer = new Timer();
            flickerTimer.WaitTime = _rng.RandfRange(1.0f, 3.0f);
            flickerTimer.Autostart = true;

            flickerTimer.Timeout += () =>
            {
                // Only flicker if not being hovered
                if (!button.IsHovered())
                {
                    // Create a sequence of flickers
                    int flickerCount = _rng.RandiRange(2, 5);
                    float flickerDelay = 0.1f;

                    for (int i = 0; i < flickerCount; i++)
                    {
                        GetTree().CreateTimer(i * flickerDelay).Timeout += () =>
                        {
                            // Toggle visibility
                            button.Modulate = button.Modulate with
                            {
                                A = button.Modulate.A < 0.5f ? 1.0f : 0.3f,
                            };
                        };
                    }

                    // Reset visibility after sequence
                    GetTree().CreateTimer(flickerCount * flickerDelay).Timeout += () =>
                    {
                        button.Modulate = button.Modulate with { A = 1.0f };
                    };

                    // Reset timer with random interval
                    flickerTimer.WaitTime = _rng.RandfRange(3.0f, 8.0f);
                    flickerTimer.Start();
                }
            };

            button.AddChild(flickerTimer);
        }

        // Apply a blood drip effect to a button
        private void ApplyBloodEffect(Button button)
        {
            // Create a timer for occasional blood drip
            Timer bloodTimer = new Timer();
            bloodTimer.WaitTime = _rng.RandfRange(5.0f, 15.0f);
            bloodTimer.Autostart = true;

            bloodTimer.Timeout += () =>
            {
                // Create a blood drip animation
                if (!button.IsHovered())
                {
                    // Temporarily change color to simulate blood dripping
                    Color originalColor = button.GetThemeColor("font_color");
                    Color bloodColor = new Color(0.8f, 0.0f, 0.0f);

                    // Animate the color change
                    float duration = 1.5f;
                    Tween tween = GetTree().CreateTween();
                    tween.TweenProperty(
                        button,
                        "theme_override_colors/font_color",
                        bloodColor,
                        duration / 3
                    );
                    tween.TweenProperty(
                        button,
                        "theme_override_colors/font_color",
                        originalColor,
                        duration * 2 / 3
                    );

                    // Reset timer with random interval
                    bloodTimer.WaitTime = _rng.RandfRange(8.0f, 20.0f);
                    bloodTimer.Start();
                }
            };

            button.AddChild(bloodTimer);
        }

        // Apply a distortion effect to a button
        private void ApplyDistortionEffect(Button button)
        {
            // Create a timer for occasional distortion
            Timer distortTimer = new Timer();
            distortTimer.WaitTime = _rng.RandfRange(4.0f, 10.0f);
            distortTimer.Autostart = true;

            distortTimer.Timeout += () =>
            {
                // Create a distortion animation
                if (!button.IsHovered())
                {
                    // Apply scale distortion
                    Vector2 originalScale = button.Scale;
                    Vector2 distortedScale = new Vector2(
                        originalScale.X + _rng.RandfRange(-0.1f, 0.1f),
                        originalScale.Y + _rng.RandfRange(-0.05f, 0.05f)
                    );

                    // Animate the distortion
                    float duration = 0.3f;
                    Tween tween = GetTree().CreateTween();
                    tween.TweenProperty(button, "scale", distortedScale, duration / 2);
                    tween.TweenProperty(button, "scale", originalScale, duration / 2);

                    // Reset timer with random interval
                    distortTimer.WaitTime = _rng.RandfRange(6.0f, 15.0f);
                    distortTimer.Start();
                }
            };

            button.AddChild(distortTimer);
        }

        // Style the title label with horror effects
        private void StyleTitleLabel()
        {
            if (_titleLabel != null)
            {
                // Set horror font for title
                _titleLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.1f, 0.1f));
                _titleLabel.AddThemeConstantOverride("font_size", 72);
                _titleLabel.AddThemeConstantOverride("outline_size", 3);
                _titleLabel.AddThemeColorOverride(
                    "font_outline_color",
                    new Color(0.1f, 0.0f, 0.0f, 0.9f)
                );

                // Create pulsating effect for title
                Timer pulseTimer = new Timer();
                pulseTimer.WaitTime = 0.05f;
                pulseTimer.Autostart = true;

                float pulsePhase = 0.0f;
                float pulseSpeed = 1.5f;
                float pulseAmount = 0.05f;

                pulseTimer.Timeout += () =>
                {
                    pulsePhase += 0.05f * pulseSpeed;
                    float pulseFactor = 1.0f + Mathf.Sin(pulsePhase) * pulseAmount;

                    // Apply subtle scale pulsation
                    _titleLabel.Scale = new Vector2(pulseFactor, pulseFactor);
                };

                _titleLabel.AddChild(pulseTimer);
            }
        }

        // Apply visual hover effect to a button
        private void ApplyButtonHoverEffect(Button button)
        {
            try
            {
                // Apply hover effect
                button.Modulate = new Color(1.2f, 1.2f, 1.2f, 1.0f);
                button.Scale = new Vector2(1.05f, 1.05f);

                // Store the original rotation for reference
                float originalRotation = button.Rotation;

                // Add subtle rotation animation
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(
                    button,
                    "rotation",
                    originalRotation + _rng.RandfRange(-0.1f, 0.1f),
                    0.3f
                );

                // Increase distortion around button
                _distortionIntensity += 0.2f;

                // Add blood drip effect on hover
                Color bloodColor = new Color(0.9f, 0.0f, 0.0f);
                button.AddThemeColorOverride("font_color", bloodColor);

                // Add text distortion effect
                button.AddThemeConstantOverride("outline_size", 2);

                // Add a custom stylebox for hover state with more intense blood effect
                StyleBoxFlat hoverStyle = CreateHorrorStylebox(0.6f);
                button.AddThemeStyleboxOverride("hover", hoverStyle);

                // Trigger a small screen shake
                TriggerDistortionPulse(0.3f);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in ApplyButtonHoverEffect: {e.Message}");
            }
        }

        // Reset button to non-hovered state
        private void ResetButtonHoverEffect(Button button)
        {
            try
            {
                // Reset button appearance with animation
                Tween tween = GetTree().CreateTween();
                tween.TweenProperty(button, "scale", new Vector2(1.0f, 1.0f), 0.2f);

                // Reset rotation
                if (button.HasMeta("_original_rotation"))
                {
                    float originalRotation = (float)button.GetMeta("_original_rotation");
                    tween.TweenProperty(button, "rotation", originalRotation, 0.3f);
                }

                // Reset distortion
                _distortionIntensity -= 0.2f;
                if (_distortionIntensity < 0.0f)
                    _distortionIntensity = 0.0f;

                // Reset button color
                button.AddThemeColorOverride("font_color", new Color(0.6f, 0.05f, 0.05f));

                // Reset outline
                button.AddThemeConstantOverride("outline_size", 1);

                // Reset hover style
                button.AddThemeStyleboxOverride("hover", CreateHorrorStylebox(0.4f));
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error in ResetButtonHoverEffect: {e.Message}");
            }
        }

        private void StartBackgroundEffects()
        {
            // Start background music
            if (_backgroundMusic != null && !_backgroundMusic.Playing)
            {
                _backgroundMusic.Play();
            }

            // Schedule first siren sound after a delay
            var sirenDelayTimer = new Timer();
            sirenDelayTimer.WaitTime = _rng.RandfRange(10.0f, 20.0f);
            sirenDelayTimer.Connect("timeout", new Callable(this, nameof(PlaySirenSound)));
            sirenDelayTimer.Autostart = true;
            sirenDelayTimer.OneShot = true;
            AddChild(sirenDelayTimer);
        }

        private void UpdateMousePosition()
        {
            if (_backgroundMaterial != null && _patternMaterial != null)
            {
                // Get viewport size and mouse position
                var viewportSize = GetViewport().GetVisibleRect().Size;
                var mousePos = GetViewport().GetMousePosition();

                // Convert to normalized coordinates (0.0 - 1.0)
                var normalizedMousePos = new Vector2(
                    mousePos.X / viewportSize.X,
                    mousePos.Y / viewportSize.Y
                );

                // Smoothly interpolate mouse position for more natural movement
                _lastMousePosition = _lastMousePosition.Lerp(normalizedMousePos, 0.1f);

                // Set shader parameters
                _backgroundMaterial.SetShaderParameter("mouse_position", _lastMousePosition);
                _patternMaterial.SetShaderParameter("mouse_position", _lastMousePosition);

                // Increase tracking intensity when mouse moves quickly
                float mouseDistance = _lastMousePosition.DistanceTo(normalizedMousePos);
                if (mouseDistance > 0.01f)
                {
                    _mouseTrackingIntensity = Math.Min(
                        _mouseTrackingIntensity + mouseDistance * 5.0f,
                        1.0f
                    );

                    // Chance to trigger heartbeat on fast mouse movement
                    if (mouseDistance > 0.05f && _rng.Randf() < 0.1f && !_heartbeatSound.Playing)
                    {
                        _heartbeatSound.Play();
                    }
                }
                else
                {
                    // Gradually decrease intensity
                    _mouseTrackingIntensity = Math.Max(_mouseTrackingIntensity - 0.01f, 0.3f);
                }
            }
        }

        private void UpdateIntensityFromAudio(float delta)
        {
            // Increase intensity when certain sounds are playing
            if (_sirenSound != null && _sirenSound.Playing)
            {
                _intensityLevel = Math.Min(_intensityLevel + delta * 0.5f, 1.0f);
            }
            else if (_heartbeatSound != null && _heartbeatSound.Playing)
            {
                _intensityLevel = Math.Min(_intensityLevel + delta * 0.2f, 0.8f);
            }
            else
            {
                // Gradually decrease intensity
                _intensityLevel = Math.Max(_intensityLevel - delta * 0.1f, 0.0f);
            }
        }

        private void ApplyIntensityToShaders()
        {
            if (_backgroundMaterial != null)
            {
                // Apply combined intensity to shaders
                float combinedIntensity = Math.Max(_intensityLevel, _mouseTrackingIntensity);
                _backgroundMaterial.SetShaderParameter(
                    "distortion_intensity",
                    0.3f + combinedIntensity * 0.7f
                );

                // Update time offset for variation
                float currentTime = (float)Time.GetTicksMsec() / 1000.0f;
                _backgroundMaterial.SetShaderParameter("time_offset", currentTime);

                // Set intense mode flag
                _backgroundMaterial.SetShaderParameter(
                    "intense_mode",
                    _isIntenseModeActive ? 1.0f : 0.0f
                );
            }

            if (_patternMaterial != null)
            {
                // Apply intensity to pattern shader
                _patternMaterial.SetShaderParameter(
                    "distortion_intensity",
                    0.3f + _intensityLevel * 0.7f
                );

                // Update time offset for variation
                float currentTime = (float)Time.GetTicksMsec() / 1000.0f;
                _patternMaterial.SetShaderParameter("time_offset", currentTime);
            }
        }

        private void TriggerGlitchEffect(float duration)
        {
            // Set glitch parameters in shaders
            if (_backgroundMaterial != null)
            {
                _backgroundMaterial.SetShaderParameter("glitch_intensity", 1.0f);
            }

            if (_patternMaterial != null)
            {
                _patternMaterial.SetShaderParameter("glitch_intensity", 1.0f);
            }

            // Set timer to end glitch
            _glitchTimer.WaitTime = duration;
            _glitchTimer.Start();
        }

        private void EndGlitchEffect()
        {
            // Reset glitch parameters
            if (_backgroundMaterial != null)
            {
                _backgroundMaterial.SetShaderParameter("glitch_intensity", 0.0f);
            }

            if (_patternMaterial != null)
            {
                _patternMaterial.SetShaderParameter("glitch_intensity", 0.0f);
            }
        }

        private void EndIntenseMode()
        {
            _isIntenseModeActive = false;
        }

        private void PlaySirenSound()
        {
            if (_sirenSound != null && !_sirenSound.Playing)
            {
                GD.Print("Playing siren sound");
                _sirenSound.Play();

                // Trigger a mild glitch effect when siren starts
                TriggerGlitchEffect(0.2f);
            }
        }

        private void PlayHeartbeatSequence()
        {
            if (_heartbeatSound != null && !_heartbeatSound.Playing)
            {
                GD.Print("Playing heartbeat sequence");
                _heartbeatSound.Play();

                // Schedule the next heartbeat
                _heartbeatTimer.WaitTime = _rng.RandfRange(20.0f, 40.0f);
                _heartbeatTimer.Start();
            }
        }

        // Event handlers
        private void OnEffectTimerTimeout()
        {
            // Choose a random effect
            float rand = _rng.Randf();

            if (rand < 0.3f)
            {
                // Trigger mild glitch
                TriggerGlitchEffect(0.1f);
            }
            else if (rand < 0.6f && !_sirenSound.Playing)
            {
                // Play siren
                PlaySirenSound();
            }
            else if (rand < 0.8f)
            {
                // Increase intensity briefly
                _intensityLevel = Math.Min(_intensityLevel + 0.3f, 0.9f);
            }
            else
            {
                // Do nothing this time
            }
        }

        private void OnGlitchTimerTimeout()
        {
            EndGlitchEffect();
        }

        private void OnHeartbeatTimerTimeout()
        {
            PlayHeartbeatSequence();
        }

        private void OnButtonHovered()
        {
            // Increase intensity slightly when hovering over buttons
            _intensityLevel = Math.Min(_intensityLevel + 0.1f, 0.7f);

            // Small chance to trigger heartbeat
            if (_rng.Randf() < 0.3f && !_heartbeatSound.Playing)
            {
                _heartbeatSound.Play();
            }

            // Play hover sound with cooldown to prevent rapid firing
            float currentTime = (float)Time.GetTicksMsec() / 1000.0f;
            if (currentTime - _lastHoverSoundTime > 0.1f) // 100ms cooldown
            {
                if (_hoverSound != null && _hoverSound.Stream != null)
                {
                    _hoverSound.Play();
                    _lastHoverSoundTime = currentTime;
                }
            }

            // Get the button that triggered this event
            Button button = GetTree().Root.GetViewport().GetNode<Button>(".") as Button;
            if (button != null)
            {
                // Apply horror-themed hover effect
                button.Scale = new Vector2(1.05f, 1.05f);

                // Change text color to blood red
                Color bloodColor = new Color(0.8f, 0.0f, 0.0f);
                button.AddThemeColorOverride("font_color", bloodColor);

                // Update the last hovered button
                _lastHoveredButton = button;
            }
        }

        private void OnButtonExited()
        {
            // Reset button to normal state
            Button button = _lastHoveredButton;
            if (button != null)
            {
                // Reset scale
                button.Scale = new Vector2(1.0f, 1.0f);

                // Reset overrides
                button.RemoveThemeColorOverride("font_color");

                // Clear the last hovered button
                _lastHoveredButton = null;
            }
        }

        private void OnFlickerTimeout()
        {
            // Toggle visibility for flicker effect
            _background.Visible = !_background.Visible;
            _pattern.Visible = !_pattern.Visible;

            // Count flickers and stop after a few
            if (_rng.Randi() % 10 == 0)
            {
                _flickerTimer.Stop();
                _background.Visible = true;
                _pattern.Visible = true;
                _isFlickering = false;
            }
        }

        private void TriggerFlickerEffect()
        {
            _isFlickering = true;
            _flickerTimer.Start();
        }

        private void TriggerDistortionPulse(float intensity = 1.0f)
        {
            // Temporarily increase distortion
            float originalDistortion = (float)
                _backgroundMaterial.GetShaderParameter("distortion_intensity");
            _backgroundMaterial.SetShaderParameter(
                "distortion_intensity",
                originalDistortion * intensity
            );

            // Create a tween to restore original value
            var tween = CreateTween();
            tween.TweenProperty(
                _backgroundMaterial,
                "shader_parameter/distortion_intensity",
                originalDistortion,
                0.5f
            );
        }

        private void OnRandomEffectTimeout()
        {
            // Choose a random subtle effect
            float rand = _rng.Randf();

            if (rand < 0.2f)
            {
                // Brief glitch
                TriggerGlitchEffect(0.05f);
            }
            else if (rand < 0.4f)
            {
                // Subtle intensity increase
                _intensityLevel = Math.Min(_intensityLevel + 0.1f, 0.7f);
            }
            else if (
                rand < 0.5f
                && _heartbeatSound != null
                && !_heartbeatSound.Playing
                && _sirenSound != null
                && !_sirenSound.Playing
            )
            {
                // Play heartbeat
                _heartbeatSound.Play();
            }

            // Reset timer with new random time
            Timer timer = GetTree().GetNodesInGroup("random_effect_timer").PickRandom() as Timer;
            if (timer != null)
            {
                timer.WaitTime = _rng.RandfRange(8.0f, 25.0f);
                timer.Start();
            }
        }

        private void OnStartButtonPressed()
        {
            // Transition to the game scene
            GetTree().ChangeSceneToFile("res://Scenes/GameScene.tscn");
        }

        private void TransitionToGameScene()
        {
            // Transition to the main game scene
            GetTree().ChangeSceneToFile("res://Scenes/IntroScene.tscn");
        }

        private void OnExitButtonPressed()
        {
            // Exit the game
            GetTree().Quit();
        }

        private void OnBtnKarya1Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya1.tscn");
        }

        private void OnBtnKarya2Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya2.tscn");
        }

        private void OnBtnKarya3Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya3.tscn");
        }

        private void OnBtnKarya4Pressed()
        {
            NavigateToScene("res://Scenes/UI/Karya4.tscn");
        }

        private void OnBtnAboutPressed()
        {
            NavigateToScene("res://Scenes/UI/About.tscn");
        }

        private void OnBtnGuidePressed()
        {
            NavigateToScene("res://Scenes/UI/Guide.tscn");
        }

        private void NavigateToScene(string scenePath)
        {
            if (GetTree().ChangeSceneToFile(scenePath) != Error.Ok)
            {
                GD.PrintErr($"Failed to load scene: {scenePath}");
            }
        }

        // Safely connect button signals
        private void ConnectButtonSignals()
        {
            // Dictionary of button names (not paths) and their handlers
            var buttonHandlers = new Dictionary<string, string>
            {
                { "BtnExit", nameof(OnExitButtonPressed) },
                { "BtnKarya1", nameof(OnBtnKarya1Pressed) },
                { "BtnKarya2", nameof(OnBtnKarya2Pressed) },
                { "BtnKarya3", nameof(OnBtnKarya3Pressed) },
                { "BtnKarya4", nameof(OnBtnKarya4Pressed) },
                { "BtnAbout", nameof(OnBtnAboutPressed) },
                { "BtnGuide", nameof(OnBtnGuidePressed) },
            };

            // Connect each button using direct node search
            foreach (var buttonEntry in buttonHandlers)
            {
                try
                {
                    Button button = null;

                    // First try to find the button in the horror menu container
                    var menuContainer = GetNodeOrNull<Control>(
                        "MarginContainer/VBoxContainer/HorrorMenuContainer"
                    );
                    if (menuContainer != null)
                    {
                        // Look through all children of the menu container
                        foreach (Node child in menuContainer.GetChildren())
                        {
                            if (child is Button childButton && childButton.Name == buttonEntry.Key)
                            {
                                button = childButton;
                                break;
                            }
                        }
                    }

                    // If not found, try the traditional path approach
                    if (button == null)
                    {
                        string buttonPath = $"MarginContainer/VBoxContainer/{buttonEntry.Key}";
                        button = GetNodeOrNull<Button>(buttonPath);
                    }

                    if (button == null)
                    {
                        GD.PrintErr($"Button not found: {buttonEntry.Key}");
                        continue;
                    }

                    // First, try to disconnect any existing connections to avoid duplicates
                    try
                    {
                        if (button.IsConnected("pressed", new Callable(this, buttonEntry.Value)))
                        {
                            button.Disconnect("pressed", new Callable(this, buttonEntry.Value));
                            GD.Print($"Disconnected existing signal for {buttonEntry.Key}");
                        }
                    }
                    catch
                    {
                        // Ignore errors during disconnection attempt
                    }

                    // Now connect with Error checking
                    Error error = button.Connect(
                        "pressed",
                        new Callable(this, buttonEntry.Value),
                        (uint)ConnectFlags.OneShot
                    );
                    if (error == Error.Ok)
                    {
                        GD.Print(
                            $"Successfully connected {buttonEntry.Key} to {buttonEntry.Value}"
                        );
                    }
                    else if (error == Error.InvalidParameter)
                    {
                        GD.Print($"Signal already connected for {buttonEntry.Key}");
                    }
                    else
                    {
                        GD.PrintErr($"Error connecting {buttonEntry.Key}: {error}");
                    }

                    // Connect mouse enter/exit signals for hover sound functionality
                    // Individual button connections are now handled in the ConnectButtonHoverSignals method
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Exception connecting {buttonEntry.Key}: {e.Message}");
                }
            }

            // Also connect hover signals for any buttons in the main container that might not be in the dictionary
            if (_buttonsContainer != null)
            {
                foreach (Node child in _buttonsContainer.GetChildren())
                {
                    if (child is Button button && !button.Name.Equals("Title"))
                    {
                        // Individual button connections are now handled in the ConnectButtonHoverSignals method
                    }
                }
            }
        }

        // Check which button is currently being hovered and play sound if it changes
        private void CheckButtonHovering()
        {
            // Get current mouse position
            Vector2 mousePos = GetViewport().GetMousePosition();

            // Check if we're hovering over any buttons
            Button currentHoveredButton = null;

            // Check buttons in the horror menu container
            var menuContainer = GetNodeOrNull<Control>(
                "MarginContainer/VBoxContainer/HorrorMenuContainer"
            );
            if (menuContainer != null)
            {
                foreach (Node child in menuContainer.GetChildren())
                {
                    if (child is Button button)
                    {
                        Rect2 buttonRect = button.GetGlobalRect();
                        if (buttonRect.HasPoint(mousePos))
                        {
                            currentHoveredButton = button;
                            break;
                        }
                    }
                }
            }

            // Check main buttons if none found in container
            if (currentHoveredButton == null)
            {
                var buttons = new string[]
                {
                    "BtnExit",
                    "BtnKarya1",
                    "BtnKarya2",
                    "BtnKarya3",
                    "BtnKarya4",
                    "BtnAbout",
                    "BtnGuide",
                };
                foreach (var buttonName in buttons)
                {
                    string buttonPath = $"MarginContainer/VBoxContainer/{buttonName}";
                    var button = GetNodeOrNull<Button>(buttonPath);
                    if (button != null)
                    {
                        Rect2 buttonRect = button.GetGlobalRect();
                        if (buttonRect.HasPoint(mousePos))
                        {
                            currentHoveredButton = button;
                            break;
                        }
                    }
                }
            }

            // If we're hovering over a different button than before, play the hover sound
            if (currentHoveredButton != null && currentHoveredButton != _lastHoveredButton)
            {
                // Prevent sound from playing too frequently (add a small cooldown)
                float currentTime = (float)Time.GetTicksMsec() / 1000.0f;
                if (currentTime - _lastHoverSoundTime > 0.1f) // 100ms cooldown
                {
                    if (_hoverSound != null && _hoverSound.Stream != null)
                    {
                        _hoverSound.Play();
                        _lastHoverSoundTime = currentTime;
                    }
                }

                // Apply hover effect to the button
                ApplyButtonHoverEffect(currentHoveredButton);

                // Reset hover effect on previous button
                if (_lastHoveredButton != null)
                {
                    ResetButtonHoverEffect(_lastHoveredButton);
                }
            }
            else if (currentHoveredButton == null && _lastHoveredButton != null)
            {
                // Mouse is not hovering over any button now, reset the last button
                ResetButtonHoverEffect(_lastHoveredButton);
            }

            // Update the last hovered button
            _lastHoveredButton = currentHoveredButton;
        }

        // Connect mouse enter/exit signals for buttons to handle hover effects
        private void ConnectButtonHoverSignals()
        {
            try
            {
                // Connect buttons in the main container
                if (_buttonsContainer != null)
                {
                    foreach (Node child in _buttonsContainer.GetChildren())
                    {
                        if (child is Button button)
                        {
                            // First disconnect any existing connections to avoid duplicates
                            if (
                                button.IsConnected(
                                    "mouse_entered",
                                    new Callable(this, nameof(OnButtonMouseEntered))
                                )
                            )
                            {
                                button.Disconnect(
                                    "mouse_entered",
                                    new Callable(this, nameof(OnButtonMouseEntered))
                                );
                            }

                            if (
                                button.IsConnected(
                                    "mouse_exited",
                                    new Callable(this, nameof(OnButtonMouseExited))
                                )
                            )
                            {
                                button.Disconnect(
                                    "mouse_exited",
                                    new Callable(this, nameof(OnButtonMouseExited))
                                );
                            }

                            // Connect mouse signals
                            button.Connect(
                                "mouse_entered",
                                new Callable(this, nameof(OnButtonMouseEntered))
                            );
                            button.Connect(
                                "mouse_exited",
                                new Callable(this, nameof(OnButtonMouseExited))
                            );
                        }
                    }
                }

                GD.Print("Button hover signals connected successfully");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error connecting button hover signals: {e.Message}");
            }
        }

        // Handle mouse entering a button
        private void OnButtonMouseEntered()
        {
            // Play hover sound
            float currentTime = (float)Time.GetTicksMsec() / 1000.0f;
            if (currentTime - _lastHoverSoundTime > 0.1f) // 100ms cooldown
            {
                if (_hoverSound != null && _hoverSound.Stream != null)
                {
                    _hoverSound.Play();
                    _lastHoverSoundTime = currentTime;
                }
            }

            // Get the button that triggered this event
            if (GetViewport().GetMousePosition() != Vector2.Zero)
            {
                CheckButtonHovering(); // This will apply the hover effect
            }
        }

        // Handle mouse exiting a button
        private void OnButtonMouseExited()
        {
            // Reset hover effect on the last hovered button
            if (_lastHoveredButton != null)
            {
                ResetButtonHoverEffect(_lastHoveredButton);
                _lastHoveredButton = null;
            }
        }

        // Create a horror-themed stylebox
        private StyleBoxFlat CreateHorrorStylebox(Color baseColor)
        {
            StyleBoxFlat style = new StyleBoxFlat();
            style.BgColor = new Color(
                baseColor.R * 0.3f,
                baseColor.G * 0.3f,
                baseColor.B * 0.3f,
                0.4f
            );
            style.BorderColor = baseColor;
            style.BorderWidthBottom = 2;
            style.BorderWidthLeft = 1;
            style.BorderWidthRight = 1;
            style.BorderWidthTop = 1;
            style.CornerRadiusBottomLeft = 4;
            style.CornerRadiusBottomRight = 4;
            style.CornerRadiusTopLeft = 4;
            style.CornerRadiusTopRight = 4;
            return style;
        }

        // Create an innovative horror-themed menu layout inspired by popular horror games
        private void CreateHorrorMenuLayout()
        {
            try
            {
                GD.Print("Creating innovative horror menu layout");

                // First, style the title label with horror effects
                if (_titleLabel != null)
                {
                    // Make title larger and more prominent
                    _titleLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.1f, 0.1f));
                    _titleLabel.AddThemeConstantOverride("font_size", 72);
                    _titleLabel.AddThemeConstantOverride("outline_size", 3);
                    _titleLabel.AddThemeColorOverride(
                        "font_outline_color",
                        new Color(0.1f, 0.0f, 0.0f, 0.9f)
                    );

                    // Add a slight rotation to the title for unsettling effect
                    _titleLabel.Rotation = _rng.RandfRange(-0.05f, 0.05f);

                    // Create pulsating effect for title
                    Timer pulseTimer = new Timer();
                    pulseTimer.WaitTime = 0.05f;
                    pulseTimer.Autostart = true;

                    float pulsePhase = 0.0f;
                    float pulseSpeed = 1.5f;
                    float pulseAmount = 0.05f;

                    pulseTimer.Timeout += () =>
                    {
                        pulsePhase += 0.05f * pulseSpeed;
                        float pulseFactor = 1.0f + Mathf.Sin(pulsePhase) * pulseAmount;

                        // Apply subtle scale pulsation
                        _titleLabel.Scale = new Vector2(pulseFactor, pulseFactor);
                    };

                    _titleLabel.AddChild(pulseTimer);
                }

                // Create a new container for our innovative layout
                Control menuContainer = new Control();
                menuContainer.Name = "HorrorMenuContainer";
                menuContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                menuContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                menuContainer.CustomMinimumSize = new Vector2(800, 500);
                _buttonsContainer.AddChild(menuContainer);

                // Get the viewport size for positioning
                Vector2 viewportSize = GetViewportRect().Size;
                Vector2 containerCenter = new Vector2(viewportSize.X / 2, viewportSize.Y / 2 + 100);

                // Define positions for each button in a pentagram/asymmetric pattern
                // This creates a more interesting and horror-themed layout
                Dictionary<string, Vector2> buttonPositions = new Dictionary<string, Vector2>
                {
                    { "BtnKarya1", new Vector2(-200, -50) }, // Top-left
                    { "BtnKarya2", new Vector2(200, -50) }, // Top-right
                    { "BtnKarya3", new Vector2(-150, 100) }, // Bottom-left
                    { "BtnKarya4", new Vector2(150, 100) }, // Bottom-right
                    { "BtnAbout", new Vector2(-250, 200) }, // Far bottom-left
                    { "BtnGuide", new Vector2(250, 200) }, // Far bottom-right
                    { "BtnExit", new Vector2(0, 250) }, // Bottom
                };

                // IMPORTANT: Instead of removing buttons from the container, we'll just reposition them
                // This way, the original node paths remain valid for signal connections
                foreach (string buttonName in buttonPositions.Keys)
                {
                    string buttonPath = $"MarginContainer/VBoxContainer/{buttonName}";
                    var button = GetNodeOrNull<Button>(buttonPath);

                    if (button == null)
                    {
                        GD.PrintErr($"Button not found: {buttonPath}");
                        continue;
                    }

                    // Reparent the button to our horror menu container
                    _buttonsContainer.RemoveChild(button);
                    menuContainer.AddChild(button);

                    // Set button size
                    button.CustomMinimumSize = new Vector2(200, 60);

                    // Position the button
                    Vector2 position = buttonPositions[buttonName];

                    // Center the button at its position
                    button.Position =
                        position
                        + new Vector2(
                            menuContainer.Size.X / 2 - button.Size.X / 2,
                            menuContainer.Size.Y / 2 - button.Size.Y / 2
                        );

                    // Add a slight random rotation for unsettling effect
                    button.Rotation = _rng.RandfRange(-0.1f, 0.1f);

                    // Store original position and rotation for hover effects
                    button.SetMeta("_original_position", button.Position);
                    button.SetMeta("_original_rotation", button.Rotation);

                    // Add a subtle floating animation
                    AddFloatingAnimation(button);

                    GD.Print($"Positioned button {button.Name} at {position}");
                }

                // Get all the buttons for adding decorations and connecting lines
                List<Button> buttons = new List<Button>();
                foreach (Node child in menuContainer.GetChildren())
                {
                    if (child is Button button)
                    {
                        buttons.Add(button);
                    }
                }

                // Add blood stain decorations around buttons
                AddBloodStainDecorations(menuContainer);

                // Add pentagram-like connecting lines between buttons for horror theme
                if (buttons.Count >= 5)
                {
                    AddConnectingLines(menuContainer, buttons);
                }

                GD.Print("Horror menu layout created");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error creating horror menu layout: {e.Message}");
            }
        }

        // Add floating animation to a button
        private void AddFloatingAnimation(Button button)
        {
            Timer floatTimer = new Timer();
            floatTimer.WaitTime = 0.05f;
            floatTimer.Autostart = true;

            float floatPhase = _rng.RandfRange(0, Mathf.Pi * 2); // Random starting phase
            float floatSpeed = _rng.RandfRange(0.5f, 1.5f);
            float floatAmount = _rng.RandfRange(2.0f, 5.0f);

            Vector2 originalPosition = button.Position;

            floatTimer.Timeout += () =>
            {
                if (!button.IsHovered())
                {
                    floatPhase += 0.05f * floatSpeed;
                    float offsetY = Mathf.Sin(floatPhase) * floatAmount;

                    // Apply subtle floating motion
                    button.Position = originalPosition + new Vector2(0, offsetY);
                }
            };

            button.AddChild(floatTimer);
        }

        // Add blood stain decorations around buttons
        private void AddBloodStainDecorations(Control container)
        {
            // Create 3-5 blood stains at random positions
            int numStains = _rng.RandiRange(3, 5);

            for (int i = 0; i < numStains; i++)
            {
                TextureRect bloodStain = new TextureRect();
                bloodStain.Name = $"BloodStain{i}";

                // Load a blood stain texture - you'll need to create this asset
                string soundPath = "res://Assets/Images/blood_stain.png";
                var texture = GD.Load<Texture2D>(soundPath);
                if (texture == null)
                {
                    // Create a placeholder if texture doesn't exist
                    texture = new GradientTexture2D();
                    GradientTexture2D gradTexture = (GradientTexture2D)texture;
                    Gradient gradient = new Gradient();
                    gradient.AddPoint(0, new Color(0.5f, 0.0f, 0.0f, 0.7f));
                    gradient.AddPoint(1, new Color(0.3f, 0.0f, 0.0f, 0.0f));
                    gradTexture.Gradient = gradient;
                    gradTexture.Width = 100;
                    gradTexture.Height = 100;
                    gradTexture.FillFrom = new Vector2(0.5f, 0.5f);
                    gradTexture.FillTo = new Vector2(1.0f, 1.0f);
                }

                bloodStain.Texture = texture;
                bloodStain.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                bloodStain.StretchMode = TextureRect.StretchModeEnum.Scale;
                bloodStain.CustomMinimumSize = new Vector2(
                    _rng.RandfRange(50, 150),
                    _rng.RandfRange(50, 150)
                );

                // Position randomly
                bloodStain.Position = new Vector2(
                    _rng.RandfRange(50, container.Size.X - 50),
                    _rng.RandfRange(50, container.Size.Y - 50)
                );

                // Random rotation
                bloodStain.Rotation = _rng.RandfRange(0, Mathf.Pi * 2);

                // Random transparency
                bloodStain.Modulate = bloodStain.Modulate with
                {
                    A = _rng.RandfRange(0.3f, 0.7f),
                };

                // Set z-index to be below buttons
                bloodStain.ZIndex = -1;

                container.AddChild(bloodStain);
            }
        }

        // Add connecting lines between buttons for pentagram-like effect
        private void AddConnectingLines(Control container, List<Button> buttons)
        {
            if (buttons.Count < 5)
                return;

            // Create a custom control for drawing lines
            PentagramLines lineDrawer = new PentagramLines();
            lineDrawer.Name = "ConnectingLines";
            lineDrawer.ZIndex = -1; // Below buttons

            // Add button centers as points
            List<Vector2> points = new List<Vector2>();
            foreach (Button button in buttons)
            {
                // Skip exit button
                if (button.Name == "BtnExit")
                    continue;

                // Add center point of button
                points.Add(button.Position + button.Size / 2);
            }

            // Set points to draw
            lineDrawer.Points = points;
            lineDrawer.LineColor = new Color(0.5f, 0.0f, 0.0f, 0.3f); // Subtle blood color
            lineDrawer.LineWidth = 2.0f;

            container.AddChild(lineDrawer);
        }

        // Custom control for drawing connecting lines
        private partial class PentagramLines : Control
        {
            public List<Vector2> Points { get; set; } = new List<Vector2>();
            public Color LineColor { get; set; } = new Color(1, 0, 0, 0.5f);
            public float LineWidth { get; set; } = 2.0f;

            public override void _Draw()
            {
                if (Points.Count < 2)
                    return;

                // Draw lines connecting points in sequence
                for (int i = 0; i < Points.Count; i++)
                {
                    // Connect to next point (or back to first for last point)
                    int nextIndex = (i + 1) % Points.Count;
                    DrawLine(Points[i], Points[nextIndex], LineColor, LineWidth);
                }

                // Draw additional connections for pentagram effect if we have enough points
                if (Points.Count >= 5)
                {
                    // Connect non-adjacent points for pentagram effect
                    for (int i = 0; i < Points.Count; i++)
                    {
                        int skipIndex = (i + 2) % Points.Count;
                        DrawLine(Points[i], Points[skipIndex], LineColor, LineWidth);
                    }
                }
            }
        }

        // Create a horror-themed stylebox for buttons
        private StyleBoxFlat CreateHorrorStylebox(float intensity)
        {
            StyleBoxFlat style = new StyleBoxFlat();

            // Dark, blood-like background with more horror-themed colors
            style.BgColor = new Color(0.15f + (intensity * 0.1f), 0.01f, 0.01f, 0.8f);

            // Add border with blood-like appearance
            style.BorderWidthBottom = 4;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderColor = new Color(0.5f + (intensity * 0.3f), 0.05f, 0.05f, 0.9f);

            // Add shadow for depth
            style.ShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.8f);
            style.ShadowSize = 8;
            style.ShadowOffset = new Vector2(3, 3);

            // Add some padding for better text display
            style.ContentMarginLeft = 25;
            style.ContentMarginRight = 25;
            style.ContentMarginTop = 12;
            style.ContentMarginBottom = 12;

            return style;
        }
    }
}
