using System;
using System.Collections.Generic;
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
        private AudioStreamPlayer _jumpscareSound;
        private Timer _flickerTimer;
        private Timer _jumpscareTimer;
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

                // Setup mouse effect container
                SetupMouseEffects();

                // Setup timers
                SetupTimers();

                // Set initial shader parameters
                _backgroundMaterial.SetShaderParameter("distortion_intensity", 0.5f);
                _backgroundMaterial.SetShaderParameter("noise_intensity", 0.3f);
                _patternMaterial.SetShaderParameter("distortion_intensity", 0.3f);

                // Update title text
                _titleLabel.Text = "Selamat Siang";

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

                // Trigger a small jumpscare after a short delay to test sound
                GetTree().CreateTimer(2.0f).Timeout += () => TriggerJumpscare();

                _rng.Randomize();

                // Start background effects
                StartBackgroundEffects();

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

                // Create background music player
                _backgroundMusic = new AudioStreamPlayer();

                // Load the audio file with error checking
                var backgroundStream = ResourceLoader.Load<AudioStream>(
                    "res://Assets/Sounds/wind_howl.mp3"
                );
                if (backgroundStream == null)
                {
                    GD.PrintErr(
                        "Failed to load background music: res://Assets/Sounds/wind_howl.mp3"
                    );
                }
                else
                {
                    _backgroundMusic.Stream = backgroundStream;
                    _backgroundMusic.VolumeDb = 0; // Increased volume
                    _backgroundMusic.Autoplay = true; // Auto play on scene load
                    AddChild(_backgroundMusic);
                    _backgroundMusic.Play();
                    GD.Print("Background music started playing");
                }

                // Create jumpscare sound player
                _jumpscareSound = new AudioStreamPlayer();

                // Load the jumpscare sound with error checking
                var jumpscareStream = ResourceLoader.Load<AudioStream>(
                    "res://Assets/Sounds/sudden_noise.mp3"
                );
                if (jumpscareStream == null)
                {
                    GD.PrintErr(
                        "Failed to load jumpscare sound: res://Assets/Sounds/sudden_noise.mp3"
                    );
                }
                else
                {
                    _jumpscareSound.Stream = jumpscareStream;
                    _jumpscareSound.VolumeDb = 5; // Louder for jumpscare effect
                    AddChild(_jumpscareSound);
                    GD.Print("Jumpscare sound loaded successfully");
                }

                // Add a heartbeat sound for additional atmosphere
                _heartbeatSound = new AudioStreamPlayer();
                var heartbeatStream = ResourceLoader.Load<AudioStream>(
                    "res://Assets/Sounds/heartbeat.mp3"
                );
                if (heartbeatStream != null)
                {
                    _heartbeatSound.Stream = heartbeatStream;
                    _heartbeatSound.VolumeDb = 3;
                    _heartbeatSound.Autoplay = true;
                    AddChild(_heartbeatSound);
                    GD.Print("Heartbeat sound added");
                }

                // Create siren sound player
                _sirenSound = new AudioStreamPlayer();
                var sirenStream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/siren.mp3");
                if (sirenStream == null)
                {
                    GD.PrintErr("Failed to load siren sound: res://Assets/Sounds/siren.mp3");
                }
                else
                {
                    _sirenSound.Stream = sirenStream;
                    _sirenSound.VolumeDb = 0; // Louder for jumpscare effect
                    AddChild(_sirenSound);
                    GD.Print("Siren sound loaded successfully");
                }
            }
            catch (System.Exception e)
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

            // Create jumpscare timer
            _jumpscareTimer = new Timer();
            _jumpscareTimer.WaitTime = _rng.RandfRange(15.0f, 30.0f); // Random initial time
            _jumpscareTimer.Autostart = true;
            AddChild(_jumpscareTimer);
            _jumpscareTimer.Timeout += OnJumpscareTimeout;

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
                GD.Print("Styling buttons");

                // Apply horror styling to all buttons in the container
                foreach (Node child in _buttonsContainer.GetChildren())
                {
                    if (child is Button button)
                    {
                        // Set horror-themed font and colors
                        button.AddThemeColorOverride("font_color", new Color(0.8f, 0.1f, 0.1f));
                        button.AddThemeColorOverride(
                            "font_hover_color",
                            new Color(1.0f, 0.0f, 0.0f)
                        );

                        // Log button found
                        GD.Print($"Styled button: {button.Name}");
                    }
                }

                GD.Print("Button styling completed");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error styling buttons: {e.Message}");
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

        private void TriggerJumpscare()
        {
            GD.Print("Triggering jumpscare");

            // Play jumpscare sound
            if (_jumpscareSound != null)
            {
                _jumpscareSound.Play();
            }

            // Activate intense mode
            _isIntenseModeActive = true;

            // Create a timer to turn off intense mode
            var intenseModeTimer = new Timer();
            intenseModeTimer.WaitTime = 2.0f;
            intenseModeTimer.Connect("timeout", new Callable(this, nameof(EndIntenseMode)));
            intenseModeTimer.Autostart = true;
            intenseModeTimer.OneShot = true;
            AddChild(intenseModeTimer);

            // Trigger glitch effect
            TriggerGlitchEffect(0.5f);

            // Reset jumpscare timer
            _jumpscareTimer.WaitTime = _rng.RandfRange(45.0f, 90.0f);
            _jumpscareTimer.Start();
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

        private void OnJumpscareTimeout()
        {
            // Random chance for jumpscare
            if (_rng.Randf() < 0.3f)
            {
                TriggerJumpscare();
            }

            // Randomize next jumpscare time
            _jumpscareTimer.WaitTime = _rng.RandfRange(20.0f, 60.0f);
            _jumpscareTimer.Start();
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
        }

        private void OnButtonExited()
        {
            // No specific action needed
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
            // Trigger a jumpscare before transitioning
            TriggerJumpscare();

            // Create a timer to transition after the jumpscare
            var transitionTimer = new Timer();
            transitionTimer.WaitTime = 1.5f;
            transitionTimer.Timeout += TransitionToGameScene;
            transitionTimer.Autostart = true;
            transitionTimer.OneShot = true;
            AddChild(transitionTimer);
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
            // Dictionary of button paths and their handlers
            var buttonHandlers = new Dictionary<string, string>
            {
                { "MarginContainer/VBoxContainer/BtnStart", nameof(OnStartButtonPressed) },
                { "MarginContainer/VBoxContainer/BtnExit", nameof(OnExitButtonPressed) },
                { "MarginContainer/VBoxContainer/BtnKarya1", nameof(OnBtnKarya1Pressed) },
                { "MarginContainer/VBoxContainer/BtnKarya2", nameof(OnBtnKarya2Pressed) },
                { "MarginContainer/VBoxContainer/BtnKarya3", nameof(OnBtnKarya3Pressed) },
                { "MarginContainer/VBoxContainer/BtnKarya4", nameof(OnBtnKarya4Pressed) },
                { "MarginContainer/VBoxContainer/BtnAbout", nameof(OnBtnAboutPressed) },
                { "MarginContainer/VBoxContainer/BtnGuide", nameof(OnBtnGuidePressed) },
            };

            // Connect each button using TryConnect to avoid duplicate connections
            foreach (var buttonEntry in buttonHandlers)
            {
                try
                {
                    var button = GetNodeOrNull<Button>(buttonEntry.Key);
                    if (button == null)
                    {
                        GD.PrintErr($"Button not found: {buttonEntry.Key}");
                        continue;
                    }

                    // First, try to disconnect any existing connections to avoid duplicates
                    // This is a safety measure in case the signal is already connected
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
                }
                catch (Exception e)
                {
                    GD.PrintErr($"Exception connecting {buttonEntry.Key}: {e.Message}");
                }
            }
        }
    }
}
