using System;
using Godot;

namespace Scenes
{
    /// <summary>
    /// A horror opening scene featuring the Batak mythology of Begu Ganjang.
    /// </summary>

    /// <summary>
    /// Simplified enumeration of horror sound themes for the intro scene
    /// </summary>
    public enum HorrorSoundTheme
    {
        Ambient, // Background atmospheric sounds
        Effect, // Various horror effects (whispers, breathing, footsteps)
        Siren, // Siren/alarm sounds (replacing jumpscare)
        Typing, // Typing sound effects
        Waiting, // Sound for "press enter to continue" state
    }

    public partial class IntroScene : Control
    {
        // Text content for each frame
        private readonly string[] _storyFrames = new string[]
        {
            "Dulu, di sebuah desa terpencil di tanah Batak, seorang dukun tamak menginginkan kekuatan yang tak terbatas. Ia melakukan ritual terlarang, mempersembahkan nyawa manusia untuk memanggil roh dari dunia lain. Namun, roh itu bukan sekadar hantu… itu adalah Begu Ganjang. Makhluk tinggi menjulang, bermata hitam pekat, yang haus akan nyawa. Dukun itu lenyap tanpa jejak… tapi makhluk itu tetap tinggal, mengintai, menunggu mangsa baru...",
            "Begu Ganjang tak bisa mati. Setiap malam, ia memburu siapa saja yang melihatnya. Semakin kau menatap, semakin tinggi ia tumbuh, hingga kepalamu tak lagi bisa menengadah… lalu bayangan panjangnya menyelimuti tubuhmu… dan esoknya, yang tersisa darimu hanyalah tubuh membiru dan senyum yang membeku. Kini, kau sudah melihatnya… dan ia telah melihatmu…",
        };

        // Enhanced background properties
        private ColorRect _backgroundRect;
        private ColorRect _noiseTexture;
        private ColorRect _vignetteEffect;
        private ShaderMaterial _backgroundShader;

        // UI elements
        private Label _textLabel;
        private AnimationPlayer _animationPlayer;
        private ColorRect _fogEffect; // Using ColorRect instead of TextureRect
        private ColorRect _shadowFigure; // Using ColorRect instead of TextureRect
        private AudioStreamPlayer _ambientSound;
        private AudioStreamPlayer _effectSound;
        private AudioStreamPlayer _typingSound;
        private AudioStreamPlayer _sirenSound; // Renamed from jumpscare to siren
        private AudioStreamPlayer _waitingSound; // Sound for "press enter to continue" state
        private Timer _typeTimer;
        private Timer _pulseTimer;
        private Timer _horrorEffectTimer; // Timer for random horror effects
        private Timer _sirenTimer; // Timer for playing siren sounds periodically
        private Tween _shadowTween;
        private ShaderMaterial _fogShader; // Shader for fog effect
        private ShaderMaterial _shadowShader; // Shader for shadow effect

        // External shader paths - make sure these match the actual file locations
        // Note: Godot paths use forward slashes regardless of OS
        // Using absolute paths to ensure shaders are found
        private const string NoiseShaderPath = "res://Scenes/Shaders/noise.gdshader";
        private const string VignetteShaderPath = "res://Scenes/Shaders/vignette.gdshader";
        private const string FogShaderPath = "res://Scenes/Shaders/fog.gdshader";
        private const string ShadowShaderPath = "res://Scenes/Shaders/shadow.gdshader";

        // Shader parameter names for easy reference
        private const string PARAM_PULSE_INTENSITY = "pulse_intensity";
        private const string PARAM_FOG_DENSITY = "fog_density";
        private const string PARAM_SHADOW_OPACITY = "shadow_opacity";
        private const string PARAM_SHADOW_POSITION = "shadow_position";
        private const string PARAM_DISTORTION = "distortion";

        // Simplified sound paths - update these with your actual sound file paths
        private readonly string[] _ambientSounds = new string[]
        {
            "res://Assets/Sounds/wind_howl.mp3",
            "res://Assets/Sounds/creaking.mp3",
        };

        private readonly string[] _effectSounds = new string[]
        {
            "res://Assets/Sounds/whisper.mp3", // Whispers
            "res://Assets/Sounds/breath.mp3", // Breathing
            "res://Assets/Sounds/sudden_noise.mp3", // Voices
        };

        private readonly string[] _sirenSounds = new string[]
        {
            "res://Assets/Sounds/siren.mp3", // Replace with actual siren sound path
            "res://Assets/Sounds/alarm.mp3", // Replace with actual alarm sound path
        };

        private readonly string[] _waitingSounds = new string[]
        {
            "res://Assets/Sounds/wait_for_key.mp3", // Replace with actual waiting sound path
            "res://Assets/Sounds/heartbeat.mp3", // Replace with actual heartbeat sound path
        };

        private readonly string[] _typingSounds = new string[]
        {
            "res://Assets/Sounds/typing.mp3",
            "res://Assets/Sounds/typing.mp3",
        };

        // State variables
        private int _currentFrame = 0;
        private int _currentCharIndex = 0;
        private bool _canAdvance = false; // Whether the user can advance to the next frame
        private bool _isTyping = false; // Whether text is currently being typed
        private float _typingSpeed = 0.07f; // seconds per character
        private float _pulseIntensity = 0.0f; // Intensity of the pulsing vignette effect
        private float _shadowOpacity = 0.0f; // Opacity of the shadow figure
        private float _shadowScale = 0.5f; // Scale of the shadow figure
        private Vector2 _shadowPosition = new Vector2(0, 0); // Position of the shadow figure
        private bool _waitingSoundConnected = false; // Whether the waiting sound handler is connected
        private Random _random = new Random();

        public override void _Ready()
        {
            GD.Print("IntroScene: Starting initialization");

            // Initialize UI elements
            InitializeUI();

            // Start the first frame
            StartFrame(_currentFrame);

            // Process input to ensure Enter key works
            ProcessMode = ProcessModeEnum.Always;

            GD.Print("IntroScene: Initialization complete");
        }

        // Called to ensure all UI elements are properly sized and positioned
        private void UpdateLayout()
        {
            // Make sure all ColorRects fill the entire viewport
            Vector2 viewportSize = GetViewportRect().Size;

            _backgroundRect.Size = viewportSize;
            _noiseTexture.Size = viewportSize;
            _fogEffect.Size = viewportSize;
            _shadowFigure.Size = viewportSize;
            _vignetteEffect.Size = viewportSize;

            // Force all shaders to be visible
            _noiseTexture.Visible = true;
            _fogEffect.Visible = true;
            _shadowFigure.Visible = true;
            _vignetteEffect.Visible = true;

            // Make sure all ColorRects have proper colors
            _noiseTexture.Color = Colors.White;
            _fogEffect.Color = Colors.White;
            _shadowFigure.Color = Colors.White;
            _vignetteEffect.Color = Colors.White;

            GD.Print($"Updated layout with viewport size: {viewportSize}");
        }

        // Called when the window is resized
        public override void _Notification(int what)
        {
            if (what == NotificationResized)
            {
                // Update layout when window is resized
                UpdateLayout();
            }
        }

        public override void _Input(InputEvent @event)
        {
            // Check for Enter key press to advance to next frame
            if (
                @event is InputEventKey keyEvent
                && keyEvent.Pressed
                && keyEvent.Keycode == Key.Enter
            )
            {
                GD.Print("Enter key pressed");
                if (_canAdvance)
                {
                    GD.Print("Advancing to next frame");
                    AdvanceToNextFrame();
                }
                else if (_isTyping)
                {
                    GD.Print("Completing typing animation");
                    // Skip the typing animation and show the full text
                    CompleteTypingAnimation();
                }
            }
        }

        private void InitializeUI()
        {
            // Create a terrifying background with procedural effects
            _backgroundRect = new ColorRect
            {
                Color = new Color(0.01f, 0.01f, 0.02f),
                AnchorRight = 1,
                AnchorBottom = 1,
                ZIndex = -10, // Ensure it's the bottom-most layer
            };
            AddChild(_backgroundRect);

            // Create procedural noise texture using external shader
            _backgroundShader = new ShaderMaterial();
            var noiseShader = ResourceLoader.Load<Shader>(NoiseShaderPath);
            if (noiseShader == null)
            {
                GD.PrintErr($"Failed to load noise shader from {NoiseShaderPath}");
            }
            else
            {
                GD.Print($"Successfully loaded noise shader from {NoiseShaderPath}");
                _backgroundShader.Shader = noiseShader;
                _backgroundShader.SetShaderParameter("noise_density", 0.6f);
                _backgroundShader.SetShaderParameter("noise_speed", 0.05f);
                _backgroundShader.SetShaderParameter("noise_scale", new Vector2(2.0f, 2.0f));
            }

            _noiseTexture = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = _backgroundShader,
                ZIndex = -9, // Update Z-index to be above background but below other elements
                Color = Colors.White, // Make sure color is fully opaque
                MouseFilter = Control.MouseFilterEnum.Ignore, // Don't block mouse events
            };
            AddChild(_noiseTexture);

            // Make sure the ColorRect fills the entire viewport
            _noiseTexture.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            // Create vignette effect using external shader
            ShaderMaterial vignetteShader = new ShaderMaterial();
            var vignette = ResourceLoader.Load<Shader>(VignetteShaderPath);
            if (vignette == null)
            {
                GD.PrintErr($"Failed to load vignette shader from {VignetteShaderPath}");
            }
            else
            {
                GD.Print($"Successfully loaded vignette shader from {VignetteShaderPath}");
                vignetteShader.Shader = vignette;
                vignetteShader.SetShaderParameter(PARAM_PULSE_INTENSITY, 0.0f);
                vignetteShader.SetShaderParameter("vignette_opacity", 0.5f);
                vignetteShader.SetShaderParameter("pulse_speed", 1.0f);
            }

            _vignetteEffect = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = vignetteShader,
                ZIndex = -7, // Update Z-index to be above fog but below text
                Color = Colors.White, // Make sure color is fully opaque
                MouseFilter = Control.MouseFilterEnum.Ignore, // Don't block mouse events
            };
            AddChild(_vignetteEffect);

            // Make sure the ColorRect fills the entire viewport
            _vignetteEffect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            // Create fog effect using external shader
            _fogShader = new ShaderMaterial();
            var fogShader = ResourceLoader.Load<Shader>(FogShaderPath);
            if (fogShader == null)
            {
                GD.PrintErr($"Failed to load fog shader from {FogShaderPath}");
            }
            else
            {
                GD.Print($"Successfully loaded fog shader from {FogShaderPath}");
                _fogShader.Shader = fogShader;
                _fogShader.SetShaderParameter(PARAM_FOG_DENSITY, 0.3f);
                _fogShader.SetShaderParameter("speed", 0.03f);
                _fogShader.SetShaderParameter("time_factor", 1.0f);
                _fogShader.SetShaderParameter("fog_color", new Vector3(0.05f, 0.05f, 0.05f));
            }

            _fogEffect = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = _fogShader,
                ZIndex = -8, // Update Z-index to be above noise but below vignette
                Color = Colors.White, // Make sure color is fully opaque
                MouseFilter = Control.MouseFilterEnum.Ignore, // Don't block mouse events
            };
            AddChild(_fogEffect);

            // Make sure the ColorRect fills the entire viewport
            _fogEffect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            // Create procedural shadow figure using external shader
            _shadowShader = new ShaderMaterial();
            var shadowShader = ResourceLoader.Load<Shader>(ShadowShaderPath);
            if (shadowShader == null)
            {
                GD.PrintErr($"Failed to load shadow shader from {ShadowShaderPath}");
            }
            else
            {
                GD.Print($"Successfully loaded shadow shader from {ShadowShaderPath}");
                _shadowShader.Shader = shadowShader;
                _shadowShader.SetShaderParameter(PARAM_SHADOW_OPACITY, 0.0f); // Start invisible
                _shadowShader.SetShaderParameter(PARAM_SHADOW_POSITION, new Vector2(0.5f, 0.5f));
                _shadowShader.SetShaderParameter(PARAM_DISTORTION, 0.2f);
                _shadowShader.SetShaderParameter("shadow_height", 1.5f);
                _shadowShader.SetShaderParameter("shadow_width", 0.5f);
                _shadowShader.SetShaderParameter("time_scale", 1.0f);
            }

            _shadowFigure = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = _shadowShader,
                ZIndex = 5, // Increase Z-index to ensure it's visible above text
                Color = Colors.White, // Make sure color is fully opaque
                MouseFilter = Control.MouseFilterEnum.Ignore, // Don't block mouse events
            };
            AddChild(_shadowFigure);

            // Make sure the ColorRect fills the entire viewport
            _shadowFigure.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            // Text label for the story
            _textLabel = new Label
            {
                AnchorLeft = 0,
                AnchorTop = 0,
                AnchorRight = 1,
                AnchorBottom = 1,
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Modulate = new Color(0.9f, 0.9f, 0.95f),
            };
            AddChild(_textLabel);

            // Set custom font
            // _textLabel.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://path_to_horror_font.ttf"));
            _textLabel.AddThemeFontSizeOverride("font_size", 24);

            // Set position and size with proper margins
            _textLabel.Position = new Vector2(50, 50);
            _textLabel.Size = new Vector2(
                GetViewportRect().Size.X - 100,
                GetViewportRect().Size.Y - 100
            );

            // Animation player for various effects
            _animationPlayer = new AnimationPlayer();
            AddChild(_animationPlayer);

            // Simplified audio players with horror-themed configuration
            _ambientSound = new AudioStreamPlayer { VolumeDb = 3.0f, Bus = "Ambient" };
            _effectSound = new AudioStreamPlayer { VolumeDb = -5.0f, Bus = "Effects" };
            _typingSound = new AudioStreamPlayer { VolumeDb = 5f, Bus = "UI" };
            _sirenSound = new AudioStreamPlayer { VolumeDb = -5.0f, Bus = "Effects" };
            _waitingSound = new AudioStreamPlayer { VolumeDb = 10.0f, Bus = "UI" };

            // Add audio players
            AddChild(_ambientSound);
            AddChild(_effectSound);
            AddChild(_typingSound);
            AddChild(_sirenSound);
            AddChild(_waitingSound);

            // Timer for random horror effects
            _horrorEffectTimer = new Timer { WaitTime = 5.0f, OneShot = false };
            _horrorEffectTimer.Timeout += OnHorrorEffectTimeout;
            AddChild(_horrorEffectTimer);

            // Timer for typing effect
            _typeTimer = new Timer { WaitTime = _typingSpeed, OneShot = true };
            _typeTimer.Timeout += OnTypeTimerTimeout;
            AddChild(_typeTimer);

            // Timer for pulsing light effect
            _pulseTimer = new Timer { WaitTime = 0.05f, OneShot = false };
            _pulseTimer.Timeout += OnPulseTimerTimeout;
            AddChild(_pulseTimer);

            // Initialize siren timer
            _sirenTimer = new Timer { WaitTime = 15.0f, OneShot = true };
            _sirenTimer.Timeout += PlaySirenSound;
            AddChild(_sirenTimer);

            // Force update the layout
            UpdateLayout();

            // Now that all timers are added to the scene tree, we can start the pulse timer
            _pulseTimer.Start();

            // Start the horror effect timer
            _horrorEffectTimer.Start();
        }

        private void StartFrame(int frameIndex)
        {
            if (frameIndex >= _storyFrames.Length)
            {
                EndIntroSequence();
                return;
            }

            // Reset state
            _currentCharIndex = 0;
            _textLabel.Text = "";
            _isTyping = true;
            _canAdvance = false;

            // Play appropriate ambient sound for the frame
            PlayFrameAmbientSound(frameIndex);

            // Start the typing effect
            _typeTimer.Start();

            // Start shadow animation
            AnimateShadowFigure(frameIndex);
        }

        private void OnTypeTimerTimeout()
        {
            if (_currentCharIndex < _storyFrames[_currentFrame].Length)
            {
                // Add the next character to the text
                _currentCharIndex++;
                _textLabel.Text = _storyFrames[_currentFrame].Substring(0, _currentCharIndex);

                // Play typing sound
                PlayTypingSound();

                // Add random pauses for dramatic effect
                float nextTypeDelay = _typingSpeed;
                if (
                    _storyFrames[_currentFrame][_currentCharIndex - 1] == '.'
                    || _storyFrames[_currentFrame][_currentCharIndex - 1] == ','
                    || _storyFrames[_currentFrame][_currentCharIndex - 1] == '…'
                )
                {
                    nextTypeDelay *= 3; // Longer pause at punctuation
                }
                else if (_random.NextDouble() > 0.7)
                {
                    nextTypeDelay *= 2; // Random pauses
                }

                // Continue typing
                _typeTimer.WaitTime = nextTypeDelay;
                _typeTimer.Start();
            }
            else
            {
                // Typing finished
                _isTyping = false;
                _canAdvance = true;
                // Stop typing sound
                _typingSound.Stop();

                // Show "Press Enter to continue" hint
                ShowContinueHint();
            }
        }

        private void OnPulseTimerTimeout()
        {
            if (_pulseIntensity > 0)
            {
                // Decrease pulse intensity over time
                _pulseIntensity = Math.Max(0, _pulseIntensity - 0.02f);

                // Get the vignette shader material
                ShaderMaterial vignetteShader = _vignetteEffect.Material as ShaderMaterial;
                if (vignetteShader != null)
                {
                    // Set pulse intensity directly in the shader
                    vignetteShader.SetShaderParameter(PARAM_PULSE_INTENSITY, _pulseIntensity);
                }

                // Also update fog density based on pulse intensity
                if (_fogShader != null)
                {
                    // Make fog denser during intense moments
                    float baseDensity = 0.3f;
                    float density = baseDensity + _pulseIntensity * 0.4f;
                    _fogShader.SetShaderParameter(PARAM_FOG_DENSITY, density);
                }

                // Update shadow distortion based on pulse intensity
                if (_shadowShader != null && _shadowOpacity > 0.1f)
                {
                    float baseDistortion = 0.2f;
                    float distortion = baseDistortion + _pulseIntensity * 0.3f;
                    _shadowShader.SetShaderParameter(PARAM_DISTORTION, distortion);
                }
            }
        }

        private void CompleteTypingAnimation()
        {
            // Show the full text immediately
            _textLabel.Text = _storyFrames[_currentFrame];
            _currentCharIndex = _storyFrames[_currentFrame].Length;
            _isTyping = false;
            _canAdvance = true;
            _typeTimer.Stop();
            // Stop typing sound
            _typingSound.Stop();

            // Show "Press Enter to continue" hint
            ShowContinueHint();
        }

        private void AdvanceToNextFrame()
        {
            // Prevent multiple rapid advancements
            if (!_canAdvance)
                return;

            // Disable advancement until next frame is ready
            _canAdvance = false;

            // Stop the waiting sound
            _waitingSound.Stop();

            _currentFrame++;

            if (_currentFrame < _storyFrames.Length)
            {
                // Skip the fade animation and start the next frame immediately
                StartFrame(_currentFrame);
            }
            else
            {
                // End the intro sequence
                EndIntroSequence();
            }
        }

        private void FadeOutCurrentFrame(Action onComplete)
        {
            // Create a tween to fade out the current frame
            Tween tween = CreateTween();
            tween.TweenProperty(_textLabel, "modulate:a", 0.0f, 1.0f);
            tween.TweenProperty(_fogEffect, "modulate:a", 0.0f, 1.0f);
            tween.TweenProperty(_shadowFigure, "modulate:a", 0.0f, 1.0f);

            // When complete, call the callback
            tween.Finished += () => onComplete?.Invoke();
        }

        private void ShowContinueHint()
        {
            // Add a blinking "Press Enter to continue" hint at the bottom of the screen
            // This could be implemented with a separate Label and AnimationPlayer
            // For simplicity, we'll just append it to the current text with a delay
            CallDeferred(nameof(AppendContinueHint));

            // Stop typing sound
            _typingSound.Stop();

            // Play waiting sound in a loop
            PlayHorrorSound(HorrorSoundTheme.Waiting);

            // Make the waiting sound loop - use a one-time connection to avoid duplicate connections
            // We'll use a try-catch to handle the case where there's no existing connection
            try
            {
                // Track if we've already connected the handler
                if (!_waitingSoundConnected)
                {
                    _waitingSound.Finished += OnWaitingSoundFinished;
                    _waitingSoundConnected = true;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error connecting waiting sound handler: {ex.Message}");
            }
        }

        private void AppendContinueHint()
        {
            _textLabel.Text += "\n\n[Press Enter to continue]";
        }

        /// <summary>
        /// Plays a sound effect from the specified horror theme
        /// </summary>
        /// <param name="theme">The horror sound theme to play</param>
        /// <param name="soundIndex">Optional index to select a specific sound from the theme (default: random)</param>
        /// <param name="volume">Optional volume adjustment in decibels (default: 0)</param>
        private void PlayHorrorSound(HorrorSoundTheme theme, int soundIndex = -1, float volume = 0f)
        {
            string[] soundOptions;
            AudioStreamPlayer targetPlayer;

            // Select the appropriate sound array and player based on theme
            switch (theme)
            {
                case HorrorSoundTheme.Ambient:
                    soundOptions = _ambientSounds;
                    targetPlayer = _ambientSound;
                    break;
                case HorrorSoundTheme.Effect:
                    soundOptions = _effectSounds;
                    targetPlayer = _effectSound;
                    break;
                case HorrorSoundTheme.Siren:
                    soundOptions = _sirenSounds;
                    targetPlayer = _sirenSound;
                    break;
                case HorrorSoundTheme.Typing:
                    soundOptions = _typingSounds;
                    targetPlayer = _typingSound;
                    break;
                case HorrorSoundTheme.Waiting:
                    soundOptions = _waitingSounds;
                    targetPlayer = _waitingSound;
                    break;
                default:
                    soundOptions = _ambientSounds;
                    targetPlayer = _ambientSound;
                    break;
            }

            // If no sounds are available for this theme, return
            if (soundOptions.Length == 0)
                return;

            // Determine which sound to play
            int index = soundIndex;
            if (index < 0 || index >= soundOptions.Length)
            {
                // Pick a random sound if index is invalid
                index = _random.Next(0, soundOptions.Length);
            }

            // Load and play the sound
            string soundPath = soundOptions[index];
            try
            {
                targetPlayer.Stream = ResourceLoader.Load<AudioStream>(soundPath);
                float originalVolume = targetPlayer.VolumeDb;
                targetPlayer.VolumeDb += volume; // Apply volume adjustment
                targetPlayer.Play();

                // Reset volume after playing
                if (volume != 0f)
                {
                    Timer resetVolumeTimer = new Timer();
                    resetVolumeTimer.WaitTime = 0.1f;
                    resetVolumeTimer.OneShot = true;
                    resetVolumeTimer.Timeout += () =>
                    {
                        targetPlayer.VolumeDb = originalVolume;
                        resetVolumeTimer.QueueFree();
                    };
                    AddChild(resetVolumeTimer);
                    resetVolumeTimer.Start();
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load sound: {soundPath}. Error: {e.Message}");
            }
        }

        /// <summary>
        /// Handler for the horror effect timer - creates random horror effects
        /// </summary>
        private void OnHorrorEffectTimeout()
        {
            try
            {
                // Choose a random horror effect
                int effectType = _random.Next(0, 7); // Increased range for more variety

                switch (effectType)
                {
                    case 0:
                        // Play a random ambient or effect sound
                        if (_random.NextDouble() < 0.7)
                        {
                            // 70% chance of effect sound
                            PlayHorrorSound(HorrorSoundTheme.Effect, -1, -10.0f);

                            // Stronger pulse with effect sound
                            _pulseIntensity = 0.3f;
                            
                            // Add subtle screen shake
                            Vector2 originalPosition = Position;
                            Tween screenShakeTween = CreateTween();
                            screenShakeTween.SetTrans(Tween.TransitionType.Sine);
                            
                            // Just a few subtle shakes
                            for (int i = 0; i < 3; i++)
                            {
                                float intensity = 2.0f;
                                Vector2 offset = new Vector2(
                                    (float)_random.NextDouble() * intensity - intensity/2,
                                    (float)_random.NextDouble() * intensity - intensity/2
                                );
                                screenShakeTween.TweenProperty(this, "position", originalPosition + offset, 0.1f);
                            }
                            // Return to original position
                            screenShakeTween.TweenProperty(this, "position", originalPosition, 0.1f);
                        }
                        else
                        {
                            // 30% chance of ambient sound
                            PlayHorrorSound(HorrorSoundTheme.Ambient, -1, -5.0f);
                        }
                        break;

                    case 1:
                        // Adjust fog density with visual feedback
                        if (_fogShader != null)
                        {
                            try
                            {
                                float currentDensity = _fogShader
                                    .GetShaderParameter(PARAM_FOG_DENSITY)
                                    .AsSingle();
                                float newDensity =
                                    currentDensity + (float)(_random.NextDouble() * 0.3 - 0.1);
                                newDensity = Math.Clamp(newDensity, 0.2f, 0.7f);

                                // Create a tween for smooth transition
                                Tween fogTween = CreateTween();
                                fogTween.SetTrans(Tween.TransitionType.Cubic);
                                fogTween.SetEase(Tween.EaseType.InOut);
                                
                                // Make sure we add the tween method before using it
                                fogTween.TweenMethod(
                                    Callable.From<float>(
                                        (density) =>
                                            _fogShader.SetShaderParameter(
                                                PARAM_FOG_DENSITY,
                                                density
                                            )
                                    ),
                                    currentDensity,
                                    newDensity,
                                    2.0f
                                );

                                // Play a subtle ambient sound with fog change
                                if (newDensity > currentDensity && newDensity > 0.5f)
                                {
                                    PlayHorrorSound(HorrorSoundTheme.Effect, 1, -15.0f); // Breathing sound
                                    
                                    // If fog is getting denser, make the vignette darker too
                                    ShaderMaterial vignetteShader = _vignetteEffect.Material as ShaderMaterial;
                                    if (vignetteShader != null)
                                    {
                                        float currentOpacity = vignetteShader.GetShaderParameter("vignette_opacity").AsSingle();
                                        float newOpacity = Math.Min(0.8f, currentOpacity + 0.1f);
                                        
                                        Tween vignetteTween = CreateTween();
                                        vignetteTween.TweenMethod(
                                            Callable.From<float>(opacity => vignetteShader.SetShaderParameter("vignette_opacity", opacity)),
                                            currentOpacity,
                                            newOpacity,
                                            3.0f
                                        );
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                GD.PrintErr($"Error adjusting fog density: {ex.Message}");
                            }
                        }
                        break;

                    case 2:
                        // Pulse vignette effect with sound
                        _pulseIntensity = 0.3f + (float)_random.NextDouble() * 0.2f;
                        PlayHorrorSound(HorrorSoundTheme.Effect, 0, -15.0f); // Whisper sound
                        
                        // Add subtle background color shift
                        if (_backgroundRect != null)
                        {
                            Tween bgTween = CreateTween();
                            bgTween.SetTrans(Tween.TransitionType.Sine);
                            
                            // Shift to slightly blue-ish color for a cold feeling
                            Color originalColor = _backgroundRect.Color;
                            Color coldColor = new Color(0.01f, 0.01f, 0.05f);
                            
                            bgTween.TweenProperty(_backgroundRect, "color", coldColor, 1.0f);
                            bgTween.TweenProperty(_backgroundRect, "color", originalColor, 2.0f);
                        }
                        break;

                    case 3:
                        // Shadow movement effect
                        if (_shadowShader != null && _shadowOpacity > 0.1f) // Only if shadow is visible
                        {
                            try
                            {
                                // Get current position
                                Vector2 currentPos = _shadowShader
                                    .GetShaderParameter(PARAM_SHADOW_POSITION)
                                    .AsVector2();

                                // Calculate new position with small random movement
                                Vector2 newPos = new Vector2(
                                    currentPos.X + ((float)_random.NextDouble() * 0.2f - 0.1f),
                                    currentPos.Y + ((float)_random.NextDouble() * 0.1f - 0.05f)
                                );

                                // Ensure position stays in reasonable bounds
                                newPos.X = Math.Clamp(newPos.X, 0.2f, 0.8f);
                                newPos.Y = Math.Clamp(newPos.Y, 0.2f, 0.7f);

                                // Create a tween for smooth movement
                                Tween shadowTween = CreateTween();
                                shadowTween.SetTrans(Tween.TransitionType.Cubic);
                                shadowTween.SetEase(Tween.EaseType.Out);
                                
                                // Make sure we add the tween method before using it
                                shadowTween.TweenMethod(
                                    Callable.From<Vector2>(UpdateShadowPosition),
                                    currentPos,
                                    newPos,
                                    1.0f + (float)_random.NextDouble() * 2.0f
                                );

                                // Play a subtle sound with shadow movement
                                if (_random.NextDouble() < 0.5f)
                                {
                                    PlayHorrorSound(HorrorSoundTheme.Effect, 2, -15.0f); // Distant voices
                                    
                                    // Occasionally make the shadow more visible
                                    if (_random.NextDouble() < 0.3f)
                                    {
                                        float currentOpacity = _shadowShader.GetShaderParameter(PARAM_SHADOW_OPACITY).AsSingle();
                                        float targetOpacity = Math.Min(0.9f, currentOpacity + 0.1f);
                                        
                                        Tween opacityTween = CreateTween();
                                        opacityTween.TweenMethod(
                                            Callable.From<float>(UpdateShadowOpacity),
                                            currentOpacity,
                                            targetOpacity,
                                            0.5f
                                        );
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                GD.PrintErr($"Error in shadow movement effect: {ex.Message}");
                            }
                        }
                        break;

                    case 4:
                        // Combination effect - increase intensity briefly
                        _pulseIntensity = 0.4f;

                        if (_shadowShader != null && _shadowOpacity > 0.1f)
                        {
                            try
                            {
                                // Increase shadow distortion temporarily
                                float currentDistortion = _shadowShader
                                    .GetShaderParameter(PARAM_DISTORTION)
                                    .AsSingle();

                                Tween distortTween = CreateTween();
                                distortTween.SetTrans(Tween.TransitionType.Bounce);
                                distortTween.SetEase(Tween.EaseType.Out);
                                
                                // Make sure we add the tween methods before using the tween
                                distortTween.TweenMethod(
                                    Callable.From<float>(UpdateShadowDistortion),
                                    currentDistortion,
                                    currentDistortion + 0.3f, // More extreme distortion
                                    0.5f
                                );
                                distortTween.TweenMethod(
                                    Callable.From<float>(UpdateShadowDistortion),
                                    currentDistortion + 0.3f,
                                    currentDistortion,
                                    1.5f
                                );
                            }
                            catch (Exception ex)
                            {
                                GD.PrintErr($"Error in shadow distortion effect: {ex.Message}");
                            }
                        }

                        // Play a subtle effect sound
                        PlayHorrorSound(
                            HorrorSoundTheme.Effect,
                            _random.Next(0, _effectSounds.Length),
                            -10.0f
                        );
                        break;
                        
                    case 5:
                        // Flickering light effect with sound
                        ShaderMaterial flickerVignetteShader = _vignetteEffect.Material as ShaderMaterial;
                        if (flickerVignetteShader != null)
                        {
                            // Play electrical interference sound
                            PlayHorrorSound(HorrorSoundTheme.Effect, 3, -5.0f);
                            
                            // Create rapid flickering effect
                            Tween flickerTween = CreateTween();
                            flickerTween.SetTrans(Tween.TransitionType.Linear);
                            
                            // Store original values
                            float originalPulse = _pulseIntensity;
                            float originalOpacity = flickerVignetteShader.GetShaderParameter("vignette_opacity").AsSingle();
                            
                            // Create 8-10 rapid flickers
                            int flickers = _random.Next(8, 11);
                            for (int i = 0; i < flickers; i++)
                            {
                                // Randomize flicker intensity
                                float pulseValue = (float)_random.NextDouble() * 0.8f + 0.2f;
                                float opacityValue = (float)_random.NextDouble() * 0.4f + 0.4f;
                                
                                // Set pulse intensity directly
                                flickerTween.TweenCallback(Callable.From(() => {
                                    _pulseIntensity = pulseValue;
                                    flickerVignetteShader.SetShaderParameter("vignette_opacity", opacityValue);
                                }));
                                
                                // Short duration for each flicker
                                flickerTween.TweenInterval(0.05f + (float)_random.NextDouble() * 0.1f);
                            }
                            
                            // Reset to original values
                            flickerTween.TweenCallback(Callable.From(() => {
                                _pulseIntensity = originalPulse;
                                flickerVignetteShader.SetShaderParameter("vignette_opacity", originalOpacity);
                            }));
                            
                            // Add subtle background color flash
                            if (_backgroundRect != null)
                            {
                                Tween bgFlashTween = CreateTween();
                                Color originalColor = _backgroundRect.Color;
                                
                                // Flash to blueish-white
                                bgFlashTween.TweenProperty(_backgroundRect, "color", new Color(0.2f, 0.2f, 0.3f), 0.1f);
                                // Back to original
                                bgFlashTween.TweenProperty(_backgroundRect, "color", originalColor, 0.3f);
                            }
                        }
                        break;
                        
                    case 6:
                        // Shadow figure appears briefly in a different position
                        if (_shadowShader != null)
                        {
                            // Get current position and opacity
                            Vector2 currentPos = _shadowShader
                                .GetShaderParameter(PARAM_SHADOW_POSITION)
                                .AsVector2();
                            float currentOpacity = _shadowShader
                                .GetShaderParameter(PARAM_SHADOW_OPACITY)
                                .AsSingle();
                            
                            // Choose a random position at the edge of the screen
                            Vector2 edgePos;
                            int edge = _random.Next(4);
                            switch (edge)
                            {
                                case 0: // Top
                                    edgePos = new Vector2((float)_random.NextDouble() * 0.6f + 0.2f, 0.1f);
                                    break;
                                case 1: // Right
                                    edgePos = new Vector2(0.9f, (float)_random.NextDouble() * 0.6f + 0.2f);
                                    break;
                                case 2: // Bottom
                                    edgePos = new Vector2((float)_random.NextDouble() * 0.6f + 0.2f, 0.9f);
                                    break;
                                default: // Left
                                    edgePos = new Vector2(0.1f, (float)_random.NextDouble() * 0.6f + 0.2f);
                                    break;
                            }
                            
                            // Play a distant sound
                            PlayHorrorSound(HorrorSoundTheme.Effect, 2, -20.0f); // Very quiet distant sound
                            
                            // Create sequence: briefly appear at edge, then return to original position
                            Tween shadowJumpTween = CreateTween();
                            
                            // First save original position
                            Vector2 originalPos = currentPos;
                            float originalOpacity = currentOpacity;
                            
                            // Jump to edge position and increase opacity
                            shadowJumpTween.TweenCallback(Callable.From(() => {
                                UpdateShadowPosition(edgePos);
                                UpdateShadowOpacity(0.7f);
                            }));
                            
                            // Hold briefly
                            shadowJumpTween.TweenInterval(0.3f);
                            
                            // Return to original position and opacity
                            shadowJumpTween.TweenCallback(Callable.From(() => {
                                UpdateShadowPosition(originalPos);
                                UpdateShadowOpacity(originalOpacity);
                            }));
                        }
                        break;
                }

                // Set next timeout to a random duration
                _horrorEffectTimer.WaitTime = 5.0f + (float)_random.NextDouble() * 10.0f;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in horror effect timeout: {ex.Message}");
                // Make sure we reset the timer even if there's an error
                _horrorEffectTimer.WaitTime = 5.0f;
            }
        }

        private void PlayFrameAmbientSound(int frameIndex)
        {
            // Stop any currently playing ambient sounds and waiting sound
            _ambientSound.Stop();
            _waitingSound.Stop();

            // Play appropriate ambient sound based on the frame
            switch (frameIndex)
            {
                case 0:
                    // Frame 1: Ambient sounds with occasional whispers
                    PlayHorrorSound(HorrorSoundTheme.Ambient, 0);

                    // Schedule occasional whisper effects
                    Timer whisperTimer = new Timer();
                    whisperTimer.WaitTime = 5.0f;
                    whisperTimer.OneShot = false;
                    whisperTimer.Timeout += () =>
                    {
                        if (_currentFrame == 0) // Only play if still on this frame
                        {
                            PlayHorrorSound(HorrorSoundTheme.Effect, 0, -5.0f); // Whisper sound
                            whisperTimer.WaitTime = 4.0f + (float)_random.NextDouble() * 8.0f; // Random interval
                            whisperTimer.Start();
                        }
                        else
                            whisperTimer.QueueFree();
                    };
                    AddChild(whisperTimer);
                    whisperTimer.Start();

                    // Schedule first siren sound after a delay
                    _sirenTimer.WaitTime = 8.0f;
                    _sirenTimer.Start();
                    break;

                case 1:
                    // Frame 2: More intense ambient with breathing effects
                    PlayHorrorSound(HorrorSoundTheme.Ambient, 1);

                    // Add breathing sound
                    PlayHorrorSound(HorrorSoundTheme.Effect, 1); // Breathing sound

                    // Schedule first siren sound after a delay
                    _sirenTimer.WaitTime = 5.0f;
                    _sirenTimer.Start();
                    break;
            }
        }

        /// <summary>
        /// Plays siren sound and schedules the second siren
        /// </summary>
        private void PlaySirenSound()
        {
            // Play siren sound
            PlayHorrorSound(HorrorSoundTheme.Siren);

            // Increase pulse intensity for visual effect
            _pulseIntensity = 1.0f;

            // Create a dramatic visual effect synchronized with the siren
            // Temporarily increase fog density
            if (_fogShader != null)
            {
                // Create a pulsating fog effect that syncs with siren
                Tween fogPulseTween = CreateTween();
                fogPulseTween.SetTrans(Tween.TransitionType.Sine);
                fogPulseTween.SetEase(Tween.EaseType.InOut);
                fogPulseTween.TweenMethod(
                    Callable.From<float>(density => _fogShader.SetShaderParameter(PARAM_FOG_DENSITY, density)),
                    0.3f, // Start with normal density
                    0.9f, // Peak at very dense fog
                    0.5f  // Duration of half a second (quick rise)
                );
                // Then pulse between high and medium values
                for (int i = 0; i < 4; i++) // Create 4 pulses
                {
                    fogPulseTween.TweenMethod(
                        Callable.From<float>(density => _fogShader.SetShaderParameter(PARAM_FOG_DENSITY, density)),
                        0.9f, 
                        0.6f,
                        0.3f
                    );
                    fogPulseTween.TweenMethod(
                        Callable.From<float>(density => _fogShader.SetShaderParameter(PARAM_FOG_DENSITY, density)),
                        0.6f,
                        0.9f,
                        0.3f
                    );
                }
            }

            // Make shadow more visible and distorted
            if (_shadowShader != null)
            {
                // Dramatic shadow reveal
                Tween shadowTween = CreateTween();
                shadowTween.SetTrans(Tween.TransitionType.Cubic);
                shadowTween.SetEase(Tween.EaseType.Out);
                
                // Rapidly increase opacity for jump scare effect
                shadowTween.TweenMethod(
                    Callable.From<float>(UpdateShadowOpacity),
                    _shadowShader.GetShaderParameter(PARAM_SHADOW_OPACITY).AsSingle(),
                    0.95f,
                    0.3f
                );
                
                // Extreme distortion during siren
                shadowTween.TweenMethod(
                    Callable.From<float>(UpdateShadowDistortion),
                    _shadowShader.GetShaderParameter(PARAM_DISTORTION).AsSingle(),
                    0.7f, // More extreme distortion
                    0.2f
                );

                // Move shadow position for a jump scare effect - make it appear closer
                Vector2 currentPos = _shadowShader
                    .GetShaderParameter(PARAM_SHADOW_POSITION)
                    .AsVector2();
                
                // First jerk to one side quickly
                shadowTween.TweenMethod(
                    Callable.From<Vector2>(UpdateShadowPosition),
                    currentPos,
                    new Vector2(currentPos.X + 0.15f, currentPos.Y - 0.15f),
                    0.2f
                );
                
                // Then move closer to center in a threatening way
                shadowTween.TweenMethod(
                    Callable.From<Vector2>(UpdateShadowPosition),
                    new Vector2(currentPos.X + 0.15f, currentPos.Y - 0.15f),
                    new Vector2(0.5f, 0.4f), // Move toward center of screen
                    0.8f
                );
            }

            // Add screen shake effect
            Tween screenShakeTween = CreateTween();
            screenShakeTween.SetTrans(Tween.TransitionType.Sine);
            Vector2 originalPosition = Position;
            
            // Create multiple rapid shakes
            for (int i = 0; i < 10; i++)
            {
                float intensity = (10 - i) * 0.5f; // Decreasing intensity
                Vector2 offset = new Vector2(
                    (float)_random.NextDouble() * intensity - intensity/2,
                    (float)_random.NextDouble() * intensity - intensity/2
                );
                screenShakeTween.TweenProperty(this, "position", originalPosition + offset, 0.05f);
            }
            // Return to original position
            screenShakeTween.TweenProperty(this, "position", originalPosition, 0.1f);

            // Add background color flash for siren effect
            if (_backgroundRect != null)
            {
                Tween bgFlashTween = CreateTween();
                bgFlashTween.SetTrans(Tween.TransitionType.Sine);
                
                // Flash to reddish color
                bgFlashTween.TweenProperty(_backgroundRect, "color", new Color(0.3f, 0.01f, 0.02f), 0.2f);
                // Back to dark
                bgFlashTween.TweenProperty(_backgroundRect, "color", new Color(0.01f, 0.01f, 0.02f), 0.3f);
                // Another flash
                bgFlashTween.TweenProperty(_backgroundRect, "color", new Color(0.25f, 0.01f, 0.02f), 0.2f);
                // Back to dark
                bgFlashTween.TweenProperty(_backgroundRect, "color", new Color(0.01f, 0.01f, 0.02f), 0.5f);
            }

            // Schedule second siren after a delay
            Timer secondSirenTimer = new Timer();
            secondSirenTimer.WaitTime = 3.0f;
            secondSirenTimer.OneShot = true;
            secondSirenTimer.Timeout += () =>
            {
                // The PlaySirenSound method now handles all the visual effects
                PlaySirenSound();
                secondSirenTimer.QueueFree();
            };
            AddChild(secondSirenTimer);
            secondSirenTimer.Start();

            // Reset effects after a delay
            Timer resetTimer = new Timer();
            resetTimer.WaitTime = 5.0f;
            resetTimer.OneShot = true;
            resetTimer.Timeout += () =>
            {
                if (_fogShader != null)
                {
                    _fogShader.SetShaderParameter(PARAM_FOG_DENSITY, 0.3f);
                }

                if (_shadowShader != null)
                {
                    // Don't reset opacity - we want the shadow to stay visible once revealed
                    _shadowShader.SetShaderParameter(PARAM_DISTORTION, 0.2f);
                }

                resetTimer.QueueFree();
            };
            AddChild(resetTimer);
            resetTimer.Start();
        }

        private void PlayTypingSound()
        {
            // Play a horror-themed typing sound
            // Vary the pitch slightly for more organic and creepy feel
            _typingSound.PitchScale = 0.85f + (float)_random.NextDouble() * 0.3f;

            // Use the horror typing sounds with randomized selection
            PlayHorrorSound(HorrorSoundTheme.Typing);
        }

        private void AnimateShadowFigure(int frameIndex)
        {
            // Cancel any existing tween
            if (_shadowTween != null && _shadowTween.IsValid())
            {
                _shadowTween.Kill();
            }

            // Reset shadow position and opacity
            _shadowFigure.Modulate = new Color(1, 1, 1, 0);

            // Create new tween for shadow animation
            switch (frameIndex)
            {
                case 0:
                    // First frame: subtle shadow movement in the background
                    // Start with shadow barely visible
                    _shadowShader.SetShaderParameter(PARAM_SHADOW_OPACITY, 0.0f);
                    _shadowShader.SetShaderParameter(
                        PARAM_SHADOW_POSITION,
                        new Vector2(0.7f, 0.5f)
                    );

                    // Gradually reveal the shadow - make sure we add the tween method before using it
                    try
                    {
                        _shadowTween = CreateTween();
                        // Add the tween method before using it
                        _shadowTween.TweenMethod(
                            Callable.From<float>(UpdateShadowOpacity),
                            0.0f,
                            0.3f,
                            8.0f
                        );
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Error creating shadow tween: {ex.Message}");
                    }

                    // Schedule occasional shadow movement
                    Timer shadowMoveTimer = new Timer();
                    shadowMoveTimer.WaitTime = 10.0f;
                    shadowMoveTimer.OneShot = true;
                    shadowMoveTimer.Timeout += () =>
                    {
                        if (_currentFrame == 0) // Only play if still on this frame
                        {
                            try
                            {
                                // Move shadow to a different position
                                Vector2 currentPos = _shadowShader
                                    .GetShaderParameter(PARAM_SHADOW_POSITION)
                                    .AsVector2();
                                Tween positionTween = CreateTween();
                                // Make sure we add the tween method before using it
                                positionTween.TweenMethod(
                                    Callable.From<Vector2>(UpdateShadowPosition),
                                    currentPos,
                                    new Vector2(0.3f, 0.5f),
                                    5.0f
                                );

                                // Play a subtle effect sound
                                PlayHorrorSound(HorrorSoundTheme.Effect, 0, -10.0f);
                            }
                            catch (Exception ex)
                            {
                                GD.PrintErr($"Error in shadow movement: {ex.Message}");
                            }
                        }
                        shadowMoveTimer.QueueFree();
                    };
                    AddChild(shadowMoveTimer);
                    shadowMoveTimer.Start();
                    break;

                case 1:
                    try
                    {
                        // Second frame: more aggressive shadow movement
                        // Make shadow more visible
                        float currentOpacity = 0.0f;
                        try
                        {
                            currentOpacity = _shadowShader
                                .GetShaderParameter(PARAM_SHADOW_OPACITY)
                                .AsSingle();
                        }
                        catch
                        {
                            // If we can't get the current value, use a default
                            currentOpacity = 0.0f;
                        }

                        Tween opacityTween = CreateTween();
                        opacityTween.TweenMethod(
                            Callable.From<float>(UpdateShadowOpacity),
                            currentOpacity,
                            0.6f,
                            3.0f
                        );

                        // Move shadow closer to center
                        Vector2 startPos = new Vector2(0.5f, 0.5f); // Default position
                        try
                        {
                            startPos = _shadowShader
                                .GetShaderParameter(PARAM_SHADOW_POSITION)
                                .AsVector2();
                        }
                        catch
                        {
                            // If we can't get the current position, use the default
                        }

                        Tween posTween = CreateTween();
                        posTween.TweenMethod(
                            Callable.From<Vector2>(UpdateShadowPosition),
                            startPos,
                            new Vector2(0.5f, 0.4f),
                            8.0f
                        );

                        // Increase distortion over time
                        Tween distortTween = CreateTween();
                        distortTween.TweenMethod(
                            Callable.From<float>(UpdateShadowDistortion),
                            0.2f,
                            0.4f,
                            15.0f
                        );
                    }
                    catch (Exception ex)
                    {
                        GD.PrintErr($"Error in shadow animation frame 1: {ex.Message}");
                    }

                    // Schedule a siren sound with synchronized visual effects
                    Timer sirenEffectTimer = new Timer();
                    sirenEffectTimer.WaitTime = 15.0f;
                    sirenEffectTimer.OneShot = true;
                    sirenEffectTimer.Timeout += () =>
                    {
                        // The PlaySirenSound method now handles all the visual effects
                        PlaySirenSound();
                        sirenEffectTimer.QueueFree();
                    };
                    AddChild(sirenEffectTimer);
                    sirenEffectTimer.Start();
                    break;
            }
        }

        // Helper methods for tweening shader parameters
        private void UpdateShadowOpacity(float opacity)
        {
            if (_shadowShader != null)
            {
                _shadowShader.SetShaderParameter(PARAM_SHADOW_OPACITY, opacity);
                _shadowOpacity = opacity; // Update the tracking variable
            }
        }

        private void UpdateShadowPosition(Vector2 position)
        {
            if (_shadowShader != null)
            {
                _shadowShader.SetShaderParameter(PARAM_SHADOW_POSITION, position);
            }
        }

        private void UpdateShadowDistortion(float distortion)
        {
            if (_shadowShader != null)
            {
                _shadowShader.SetShaderParameter(PARAM_DISTORTION, distortion);
            }
        }

        // This method is no longer needed as we're using shader parameters directly
        // Keeping an empty implementation to avoid errors in case it's called elsewhere
        private void AnimateShadowFigure(float delta)
        {
            // Implementation moved to shader parameter tweening
        }

        // Handler for waiting sound finished event
        private void OnWaitingSoundFinished()
        {
            if (_canAdvance) // Only replay if we're still waiting for input
                PlayHorrorSound(HorrorSoundTheme.Waiting);
        }

        private void EndIntroSequence()
        {
            // Stop all sounds before ending
            _ambientSound.Stop();
            _effectSound.Stop();
            _typingSound.Stop();
            _sirenSound.Stop();
            _waitingSound.Stop();

            // Skip all animations and transitions, immediately queue free this scene
            // This will trigger the TreeExiting signal which Karya3 is listening for
            QueueFree();
        }
    }
}
