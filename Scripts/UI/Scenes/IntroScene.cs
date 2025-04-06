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

        // Shader code for procedural horror effects
        private const string FogShaderCode =
            @"
            shader_type canvas_item;
            
            uniform float density: hint_range(0.0, 1.0) = 0.5;
            uniform float time_scale: hint_range(0.0, 2.0) = 0.2;
            uniform vec4 fog_color: source_color = vec4(0.0, 0.0, 0.02, 1.0);
            
            float random(vec2 uv) {
                return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123);
            }
            
            void fragment() {
                vec2 uv = SCREEN_UV;
                float noise = random(uv + TIME * time_scale);
                
                // Create swirling fog effect
                float fog_factor = sin(uv.x * 10.0 + TIME * 0.5) * sin(uv.y * 10.0 + TIME * 0.3) * density;
                fog_factor = clamp(fog_factor + noise * 0.1, 0.0, 1.0);
                
                COLOR = mix(vec4(0.0), fog_color, fog_factor);
            }
        ";

        private const string ShadowShaderCode =
            @"
            shader_type canvas_item;
            
            uniform float shadow_intensity: hint_range(0.0, 1.0) = 0.8;
            uniform float distortion: hint_range(0.0, 5.0) = 1.5;
            uniform float time_scale: hint_range(0.0, 1.0) = 0.1;
            
            void fragment() {
                vec2 uv = SCREEN_UV;
                
                // Create distorted shadow shape
                float shadow = 0.0;
                vec2 center = vec2(0.5, 0.5);
                float dist = length(uv - center);
                
                // Tall humanoid shape
                float body = smoothstep(0.1, 0.12, abs(uv.x - 0.5)) * smoothstep(0.0, 0.5, 1.0 - uv.y);
                
                // Add distortion
                float time_offset = TIME * time_scale;
                float distort_x = sin(uv.y * 20.0 + time_offset) * distortion * 0.01;
                float distort_y = cos(uv.x * 20.0 + time_offset) * distortion * 0.01;
                
                shadow = 1.0 - body - distort_x - distort_y;
                shadow = clamp(shadow, 0.0, 1.0) * shadow_intensity;
                
                COLOR = vec4(0.0, 0.0, 0.0, shadow);
            }
        ";

        // State variables
        private int _currentFrame = 0;
        private int _currentCharIndex = 0;
        private bool _isTyping = false;
        private bool _canAdvance = false;
        private float _typingSpeed = 0.07f; // seconds per character
        private float _pulseIntensity = 0.0f;
        private float _shadowOpacity = 0.0f;
        private float _shadowScale = 0.5f;
        private Vector2 _shadowPosition = new Vector2(0, 0);
        private Random _random = new Random();

        public override void _Ready()
        {
            // Initialize UI elements
            InitializeUI();

            // Start the first frame
            StartFrame(_currentFrame);
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
                if (_canAdvance)
                {
                    AdvanceToNextFrame();
                }
                else if (_isTyping)
                {
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
            };

            // Create procedural noise effect
            ShaderMaterial noiseShader = new ShaderMaterial();
            noiseShader.Shader = new Shader()
            {
                Code =
                    @"
                shader_type canvas_item;
                
                uniform float noise_density: hint_range(0.0, 1.0) = 0.05;
                uniform float time_scale: hint_range(0.0, 2.0) = 0.1;
                
                float random(vec2 uv) {
                    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123);
                }
                
                void fragment() {
                    vec2 uv = SCREEN_UV;
                    float noise = random(uv * 500.0 + TIME * time_scale);
                    COLOR = vec4(noise * noise_density);
                }
            ",
            };

            _noiseTexture = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = noiseShader,
            };

            // Create procedural vignette effect
            ShaderMaterial vignetteShader = new ShaderMaterial();
            vignetteShader.Shader = new Shader()
            {
                Code =
                    @"
                shader_type canvas_item;
                
                uniform float vignette_intensity: hint_range(0.0, 5.0) = 0.4;
                uniform float vignette_opacity: hint_range(0.0, 1.0) = 0.5;
                uniform vec4 vignette_color: source_color = vec4(0.0, 0.0, 0.0, 1.0);
                
                void fragment() {
                    vec2 uv = SCREEN_UV;
                    vec2 center = vec2(0.5, 0.5);
                    float dist = length(uv - center);
                    
                    // Create pulsing vignette
                    float vignette = smoothstep(0.8, 0.2, dist * (vignette_intensity + sin(TIME * 0.2) * 0.1));
                    vignette = pow(vignette, 2.0) * vignette_opacity;
                    
                    COLOR = mix(vec4(0.0), vignette_color, 1.0 - vignette);
                }
            ",
            };

            _vignetteEffect = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = vignetteShader,
            };

            // Create procedural fog effect
            _fogShader = new ShaderMaterial();
            _fogShader.Shader = new Shader() { Code = FogShaderCode };

            _fogEffect = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = _fogShader,
            };

            // Create procedural shadow figure
            _shadowShader = new ShaderMaterial();
            _shadowShader.Shader = new Shader() { Code = ShadowShaderCode };

            _shadowFigure = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = _shadowShader,
                Modulate = new Color(1, 1, 1, 0),
            };

            // Text label for story
            _textLabel = new Label
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                Modulate = new Color(0.9f, 0.9f, 0.95f),
            };

            // Set custom font
            // _textLabel.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://path_to_horror_font.ttf"));
            _textLabel.AddThemeFontSizeOverride("font_size", 24);
            _textLabel.Position = new Vector2(50, 50);

            // Use set_deferred to avoid warning non-equal opposite anchors
            _textLabel.SetDeferred(
                "size",
                new Vector2(GetViewportRect().Size.X - 100, GetViewportRect().Size.Y - 100)
            );

            // Animation player for various effects
            _animationPlayer = new AnimationPlayer();

            // Simplified audio players with horror-themed configuration
            _ambientSound = new AudioStreamPlayer { VolumeDb = 3.0f, Bus = "Ambient" };
            _effectSound = new AudioStreamPlayer { VolumeDb = -5.0f, Bus = "Effects" };
            _typingSound = new AudioStreamPlayer { VolumeDb = 5f, Bus = "UI" };
            _sirenSound = new AudioStreamPlayer { VolumeDb = -5.0f, Bus = "Effects" };
            _waitingSound = new AudioStreamPlayer { VolumeDb = 10.0f, Bus = "UI" };

            // Timer for random horror effects
            _horrorEffectTimer = new Timer { WaitTime = 5.0f, OneShot = false };
            _horrorEffectTimer.Timeout += OnHorrorEffectTimeout;

            // Timer for typing effect
            _typeTimer = new Timer { WaitTime = _typingSpeed, OneShot = true };
            _typeTimer.Timeout += OnTypeTimerTimeout;

            // Timer for pulsing light effect
            _pulseTimer = new Timer { WaitTime = 0.05f, OneShot = false };
            _pulseTimer.Timeout += OnPulseTimerTimeout;
            _pulseTimer.Start();

            // Add all elements to the scene
            AddChild(_backgroundRect);
            AddChild(_noiseTexture);
            AddChild(_vignetteEffect);
            AddChild(_fogEffect);
            AddChild(_shadowFigure);
            AddChild(_textLabel);
            AddChild(_ambientSound);
            AddChild(_effectSound);
            AddChild(_typingSound);
            AddChild(_sirenSound);
            AddChild(_waitingSound);
            AddChild(_typeTimer);
            AddChild(_pulseTimer);
            AddChild(_horrorEffectTimer);
            AddChild(_animationPlayer);

            // Initialize siren timer
            _sirenTimer = new Timer { WaitTime = 15.0f, OneShot = true };
            _sirenTimer.Timeout += PlaySirenSound;
            AddChild(_sirenTimer);

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
            // Create a pulsing light effect
            _pulseIntensity = Math.Max(_pulseIntensity - 0.01f, 0);
            float noise = (float)_random.NextDouble() * 0.05f;

            // Apply to background color
            Color bgColor = new Color(
                0.02f + _pulseIntensity + noise,
                0.02f + _pulseIntensity * 0.7f + noise * 0.5f,
                0.03f + _pulseIntensity * 0.5f + noise * 0.3f
            );
            _backgroundRect.Color = bgColor;

            // Also affect text brightness
            _textLabel.Modulate = new Color(
                0.8f + _pulseIntensity * 0.2f,
                0.8f + _pulseIntensity * 0.2f,
                0.8f + _pulseIntensity * 0.2f
            );
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

            // Make the waiting sound loop
            _waitingSound.Finished += () =>
            {
                if (_canAdvance) // Only replay if we're still waiting for input
                    PlayHorrorSound(HorrorSoundTheme.Waiting);
            };
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
            // Only create random effects if we're in the intro scene
            if (_currentFrame >= _storyFrames.Length)
                return;

            // Random horror effect selection
            float rand = (float)_random.NextDouble();

            if (rand < 0.3f)
            {
                // Play a random horror sound effect
                PlayHorrorSound(HorrorSoundTheme.Effect);

                // Increase pulse intensity for visual effect
                _pulseIntensity = Math.Min(_pulseIntensity + 0.2f, 0.8f);
            }
            else if (rand < 0.4f)
            {
                // Distort the shadow figure temporarily
                if (_shadowShader != null && _shadowFigure.Modulate.A > 0)
                {
                    float currentDistortion = _shadowShader
                        .GetShaderParameter("distortion")
                        .AsSingle();
                    _shadowShader.SetShaderParameter("distortion", currentDistortion * 3.0f);

                    // Reset after a short time
                    Timer resetTimer = new Timer();
                    resetTimer.WaitTime = 0.5f;
                    resetTimer.OneShot = true;
                    resetTimer.Timeout += () =>
                    {
                        _shadowShader.SetShaderParameter("distortion", currentDistortion);
                        resetTimer.QueueFree();
                    };
                    AddChild(resetTimer);
                    resetTimer.Start();
                }
            }
            else if (rand < 0.5f)
            {
                // Increase fog density temporarily
                if (_fogShader != null)
                {
                    float currentDensity = _fogShader.GetShaderParameter("density").AsSingle();
                    _fogShader.SetShaderParameter("density", Math.Min(currentDensity * 2.0f, 0.9f));

                    // Reset after a short time
                    Timer resetTimer = new Timer();
                    resetTimer.WaitTime = 1.0f;
                    resetTimer.OneShot = true;
                    resetTimer.Timeout += () =>
                    {
                        _fogShader.SetShaderParameter("density", currentDensity);
                        resetTimer.QueueFree();
                    };
                    AddChild(resetTimer);
                    resetTimer.Start();
                }
            }

            // Randomize the next effect interval
            _horrorEffectTimer.WaitTime = 3.0f + (float)_random.NextDouble() * 7.0f;
            _horrorEffectTimer.Start();
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

            // Schedule second siren after a delay
            Timer secondSirenTimer = new Timer();
            secondSirenTimer.WaitTime = 3.0f;
            secondSirenTimer.OneShot = true;
            secondSirenTimer.Timeout += () =>
            {
                if (_currentFrame < _storyFrames.Length) // Only play if still in intro scene
                {
                    PlayHorrorSound(HorrorSoundTheme.Siren);
                    _pulseIntensity = 1.0f;
                }
                secondSirenTimer.QueueFree();
            };
            AddChild(secondSirenTimer);
            secondSirenTimer.Start();
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
            _shadowTween = CreateTween();
            _shadowTween.SetEase(Tween.EaseType.InOut);

            switch (frameIndex)
            {
                case 0:
                    // Frame 1: Shadow slowly appears in the distance
                    _shadowFigure.Scale = new Vector2(0.5f, 0.5f);
                    _shadowFigure.Position = new Vector2(
                        GetViewportRect().Size.X * 0.8f,
                        GetViewportRect().Size.Y * 0.2f
                    );

                    _shadowTween.TweenProperty(_shadowFigure, "modulate:a", 0.1f, 5.0f);
                    _shadowTween.TweenProperty(
                        _shadowFigure,
                        "scale",
                        new Vector2(0.6f, 0.6f),
                        15.0f
                    );
                    _shadowTween
                        .Parallel()
                        .TweenProperty(
                            _shadowFigure,
                            "position:y",
                            GetViewportRect().Size.Y * 0.25f,
                            15.0f
                        );
                    break;

                case 1:
                    // Frame 2: Shadow grows larger and comes closer
                    _shadowFigure.Scale = new Vector2(0.7f, 0.7f);
                    _shadowFigure.Position = new Vector2(
                        GetViewportRect().Size.X * 0.5f,
                        GetViewportRect().Size.Y * 0.3f
                    );

                    _shadowTween.TweenProperty(_shadowFigure, "modulate:a", 0.2f, 3.0f);
                    _shadowTween.TweenProperty(
                        _shadowFigure,
                        "scale",
                        new Vector2(1.2f, 1.2f),
                        20.0f
                    );
                    _shadowTween
                        .Parallel()
                        .TweenProperty(
                            _shadowFigure,
                            "position:y",
                            GetViewportRect().Size.Y * 0.4f,
                            20.0f
                        );

                    // Add a sudden movement at the end for a jump scare
                    _shadowTween.TweenInterval(15.0f);
                    _shadowTween.TweenProperty(_shadowFigure, "modulate:a", 0.5f, 0.5f);
                    _shadowTween.TweenProperty(
                        _shadowFigure,
                        "scale",
                        new Vector2(2.0f, 2.0f),
                        0.5f
                    );
                    _shadowTween.TweenProperty(
                        _shadowFigure,
                        "position:y",
                        GetViewportRect().Size.Y * 0.6f,
                        0.5f
                    );

                    // Schedule a siren sound at the exact moment of the sudden movement
                    Timer sirenEffectTimer = new Timer();
                    sirenEffectTimer.WaitTime = 15.0f;
                    sirenEffectTimer.OneShot = true;
                    sirenEffectTimer.Timeout += () =>
                    {
                        PlayHorrorSound(HorrorSoundTheme.Siren);

                        // Increase pulse intensity for visual effect
                        _pulseIntensity = 1.0f;

                        // Temporarily increase fog density for added effect
                        if (_fogShader != null)
                        {
                            float currentDensity = _fogShader
                                .GetShaderParameter("density")
                                .AsSingle();
                            _fogShader.SetShaderParameter("density", 0.9f);

                            // Reset after a short time
                            Timer resetTimer = new Timer();
                            resetTimer.WaitTime = 1.5f;
                            resetTimer.OneShot = true;
                            resetTimer.Timeout += () =>
                            {
                                _fogShader.SetShaderParameter("density", currentDensity);
                                resetTimer.QueueFree();
                            };
                            AddChild(resetTimer);
                            resetTimer.Start();
                        }

                        // Schedule second siren after a delay
                        Timer secondSirenTimer = new Timer();
                        secondSirenTimer.WaitTime = 3.0f;
                        secondSirenTimer.OneShot = true;
                        secondSirenTimer.Timeout += () =>
                        {
                            PlayHorrorSound(HorrorSoundTheme.Siren);
                            _pulseIntensity = 1.0f;
                            secondSirenTimer.QueueFree();
                        };
                        AddChild(secondSirenTimer);
                        secondSirenTimer.Start();

                        sirenEffectTimer.QueueFree();
                    };
                    AddChild(sirenEffectTimer);
                    sirenEffectTimer.Start();
                    break;
            }
        }

        private void AnimateShadowFigure(float delta)
        {
            // Gradually increase shadow opacity
            _shadowOpacity = Math.Min(_shadowOpacity + delta * 0.05f, 0.8f);
            _shadowFigure.Modulate = new Color(1, 1, 1, _shadowOpacity);

            // Make shadow grow larger
            _shadowScale = Math.Min(_shadowScale + delta * 0.01f, 1.2f);
            _shadowFigure.Scale = new Vector2(_shadowScale, _shadowScale);

            // Move shadow closer
            _shadowPosition.Y -= delta * 10;
            _shadowFigure.Position = _shadowPosition;
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
