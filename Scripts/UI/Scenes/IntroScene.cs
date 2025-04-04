using System;
using Godot;

namespace Scenes
{
    /// <summary>
    /// A horror opening scene featuring the Batak mythology of Begu Ganjang.
    /// </summary>
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
        private TextureRect _noiseTexture;
        private ColorRect _vignetteEffect;
        private ShaderMaterial _backgroundShader;

        // UI elements
        private Label _textLabel;
        private AnimationPlayer _animationPlayer;
        private TextureRect _fogEffect;
        private TextureRect _shadowFigure;
        private AudioStreamPlayer _ambientSound;
        private AudioStreamPlayer _effectSound;
        private AudioStreamPlayer _typingSound;
        private Timer _typeTimer;
        private Timer _pulseTimer;
        private Tween _shadowTween;

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
            // Create a more terrifying background
            _backgroundRect = new ColorRect
            {
                Color = new Color(0.01f, 0.01f, 0.02f),
                AnchorRight = 1,
                AnchorBottom = 1,
            };

            // Add noise texture for a grainy effect
            _noiseTexture = new TextureRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Modulate = new Color(1, 1, 1, 0.1f),
            };
            // _noiseTexture.Texture = ResourceLoader.Load<Texture2D>("res://path_to_noise_texture.png");

            // Add vignette effect
            _vignetteEffect = new ColorRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Material = new ShaderMaterial(),
            };
            // _vignetteEffect.Material.Shader = ResourceLoader.Load<Shader>("res://path_to_vignette_shader.shader");

            // Add shadow figure
            _shadowFigure = new TextureRect
            {
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                Modulate = new Color(1, 1, 1, 0),
                Scale = new Vector2(0.5f, 0.5f),
            };
            // _shadowFigure.Texture = ResourceLoader.Load<Texture2D>("res://path_to_shadow_texture.png");

            // Fog effect texture
            _fogEffect = new TextureRect
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                Modulate = new Color(1, 1, 1, 0.2f),
            };
            // Note: You'll need to set the texture in the editor or load it from a file
            // _fogEffect.Texture = ResourceLoader.Load<Texture2D>("res://path_to_fog_texture.png");

            // Text label for the story
            _textLabel = new Label
            {
                AnchorRight = 1,
                AnchorBottom = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word,
                Text = "",
                Modulate = new Color(0.8f, 0.8f, 0.8f), // Slightly gray text
            };

            // Set up custom font and size
            // Note: You'll need to load the font in the editor or from a file
            // _textLabel.AddThemeFont("font", ResourceLoader.Load<Font>("res://path_to_creepy_font.ttf"));
            // _textLabel.AddThemeFontSize("font_size", 24);

            // Add padding around the text
            _textLabel.Position = new Vector2(50, 50);
            // Use set_deferred untuk menghindari warning non-equal opposite anchors
            _textLabel.SetDeferred(
                "size",
                new Vector2(GetViewportRect().Size.X - 100, GetViewportRect().Size.Y - 100)
            );

            // Animation player for various effects
            _animationPlayer = new AnimationPlayer();
            AddChild(_animationPlayer);

            // Sound players
            _ambientSound = new AudioStreamPlayer();
            // _ambientSound.Stream = ResourceLoader.Load<AudioStream>("res://path_to_ambient_sound.wav");
            _ambientSound.VolumeDb = -10; // Lower volume
            AddChild(_ambientSound);

            _effectSound = new AudioStreamPlayer();
            // _effectSound.Stream = ResourceLoader.Load<AudioStream>("res://path_to_effect_sound.wav");
            _effectSound.VolumeDb = -5;
            AddChild(_effectSound);

            // Typing sound
            _typingSound = new AudioStreamPlayer
            {
                Stream = ResourceLoader.Load<AudioStream>("res://Assets/Sounds/typing.wav"),
                VolumeDb = -15,
                PitchScale = 0.9f,
            };
            AddChild(_typingSound);

            // Timer for typing effect
            _typeTimer = new Timer { WaitTime = _typingSpeed, OneShot = true };
            _typeTimer.Timeout += OnTypeTimerTimeout;
            AddChild(_typeTimer);

            // Timer for pulsing light effect
            _pulseTimer = new Timer { WaitTime = 0.05f, OneShot = false };
            _pulseTimer.Timeout += OnPulseTimerTimeout;
            AddChild(_pulseTimer);
            _pulseTimer.Start();

            // Add all elements to scene
            AddChild(_backgroundRect);
            AddChild(_noiseTexture);
            AddChild(_vignetteEffect);
            AddChild(_shadowFigure);
            AddChild(_fogEffect);
            AddChild(_textLabel);
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
                _typingSound.Play();

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
        }

        private void AppendContinueHint()
        {
            _textLabel.Text += "\n\n[Press Enter to continue]";
        }

        private void PlayFrameAmbientSound(int frameIndex)
        {
            // Stop any currently playing sounds
            _ambientSound.Stop();
            _effectSound.Stop();

            // Play appropriate ambient sound based on the frame
            switch (frameIndex)
            {
                case 0:
                    // Frame 1: Distant whispers or ancient ritual sounds
                    // _ambientSound.Stream = ResourceLoader.Load<AudioStream>("res://path_to_whispers_sound.wav");
                    _ambientSound.Stream = ResourceLoader.Load<AudioStream>(
                        "res://Assets/Sounds/whisper.mp3"
                    );
                    break;
                case 1:
                    // Frame 2: Heavy breathing and distant scream
                    _ambientSound.Stream = ResourceLoader.Load<AudioStream>(
                        "res://Assets/Sounds/breath.mp3"
                    );
                    break;
            }

            _ambientSound.Play();
        }

        private void PlayTypingSound()
        {
            // Play a subtle typing sound effect
            // Vary the pitch slightly for more organic feel
            _effectSound.PitchScale = 0.9f + (float)_random.NextDouble() * 0.2f;
            _effectSound.Play();
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
            // Skip all animations and transitions, immediately queue free this scene
            // This will trigger the TreeExiting signal which Karya3 is listening for
            QueueFree();
        }
    }
}
