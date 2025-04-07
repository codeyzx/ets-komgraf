using System;
using Godot;

namespace Scenes
{
    public partial class AboutScene : Control
    {
        private Label titleLabel;
        private Label identityLabel;
        private Panel profileFrame;
        private TextureRect profileImage;
        private Button backButton;
        private AudioStreamPlayer creepyAudio;
        private AudioStreamPlayer wolfHowlAudio;
        private AudioStreamPlayer buttonHoverAudio;
        private AudioStreamPlayer clickSound;
        private RandomNumberGenerator rng = new RandomNumberGenerator();
        private float flickerTimer = 0f;
        private float profileShakeTimer = 0f;
        private float profileShakeIntensity = 0f;
        private Vector2 originalProfilePosition;
        private float wolfHowlTimer = 0f;
        private float profileJumpTimer = 0f;
        private HBoxContainer profileContainer;
        private VBoxContainer textContainer;
        private Control parentContainer;
        private float scaryEventTimer = 0f;
        private bool isTransitioning = false;
        private string targetScene = "res://Scenes/UI/Welcome.tscn";

        public override void _Ready()
        {
            // Get references to UI elements
            titleLabel = GetNode<Label>("MarginContainer/VBoxContainer/Title");
            identityLabel = GetNode<Label>(
                "MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/LabelIdentitas"
            );
            profileFrame = GetNode<Panel>(
                "MarginContainer/VBoxContainer/HBoxContainer/ProfileFrame"
            );
            profileImage = GetNode<TextureRect>(
                "MarginContainer/VBoxContainer/HBoxContainer/ProfileFrame/TextureRect"
            );
            backButton = GetNode<Button>("MarginContainer/VBoxContainer/BtnBack");
            profileContainer = GetNode<HBoxContainer>(
                "MarginContainer/VBoxContainer/HBoxContainer"
            );
            textContainer = GetNode<VBoxContainer>(
                "MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer"
            );
            parentContainer = GetNode<Control>("MarginContainer/VBoxContainer");

            // Setup creepy ambient sound
            creepyAudio = new AudioStreamPlayer();
            AddChild(creepyAudio);

            // Try to load horror ambient sound - note that these files need to be added to the project
            var ambientResource = GD.Load<AudioStream>(
                "res://Assets/Sounds/Horror/horror_ambience.mp3"
            );
            if (ambientResource != null)
            {
                creepyAudio.Stream = ambientResource;
                creepyAudio.VolumeDb = -10; // Lower volume
                creepyAudio.Autoplay = true;
                creepyAudio.Bus = "Master";
            }

            // Setup wolf howl sound
            wolfHowlAudio = new AudioStreamPlayer();
            AddChild(wolfHowlAudio);

            // Try to load wolf howl sound
            var wolfHowlResource = GD.Load<AudioStream>("res://Assets/Sounds/Horror/wolf_howl.mp3");
            if (wolfHowlResource != null)
            {
                wolfHowlAudio.Stream = wolfHowlResource;
                wolfHowlAudio.VolumeDb = -5; // Slightly louder than ambient
                wolfHowlAudio.Bus = "Master";
            }

            // Setup button hover sound
            buttonHoverAudio = new AudioStreamPlayer();
            AddChild(buttonHoverAudio);

            // Try to load button hover sound
            var buttonHoverResource = GD.Load<AudioStream>("res://Assets/Sounds/hover.mp3");
            if (buttonHoverResource != null)
            {
                buttonHoverAudio.Stream = buttonHoverResource;
                buttonHoverAudio.VolumeDb = -8;
                buttonHoverAudio.Bus = "Master";
            }

            // Setup click sound and add it to scene tree immediately
            clickSound = new AudioStreamPlayer();
            AddChild(clickSound);
            var clickResource = GD.Load<AudioStream>("res://Assets/Sounds/hover.mp3");
            if (clickResource != null)
            {
                clickSound.Stream = clickResource;
                clickSound.VolumeDb = -5;
                clickSound.Bus = "Master";
            }

            // Initialize random timers
            rng.Randomize();
            flickerTimer = rng.RandfRange(0.5f, 1.5f);
            wolfHowlTimer = rng.RandfRange(5f, 15f); // First howl between 5-15 seconds
            profileJumpTimer = rng.RandfRange(3f, 8f); // First profile jump between 3-8 seconds
            scaryEventTimer = rng.RandfRange(5f, 10f); // First scary event between 5-10 seconds

            // Store original profile position for shake effect
            originalProfilePosition = profileFrame.Position;

            // IMPORTANT: Only connect signals if not already connected
            // This avoids the "Signal already connected" error
            if (!backButton.IsConnected("pressed", new Callable(this, nameof(OnBtnBackPressed))))
            {
                backButton.Pressed += OnBtnBackPressed;
            }

            if (
                !backButton.IsConnected(
                    "mouse_entered",
                    new Callable(this, nameof(OnBtnBackHovered))
                )
            )
            {
                backButton.MouseEntered += OnBtnBackHovered;
            }

            // Set up initial profile position
            SetRandomProfilePosition(false);

            // Make sure profile image size is correct
            AdjustProfileImageSize();
        }

        private void AdjustProfileImageSize()
        {
            if (profileImage != null && profileImage.Texture != null)
            {
                // Set expand mode to keep size
                profileImage.ExpandMode = TextureRect.ExpandModeEnum.KeepSize;

                // Ensure the profile frame size is appropriate
                if (profileFrame != null)
                {
                    // Set the profile frame to use size flags
                    profileFrame.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
                    profileFrame.SizeFlagsVertical = SizeFlags.ShrinkCenter;

                    // Set custom minimum size based on texture
                    var textureSize = profileImage.Texture.GetSize();
                    profileImage.CustomMinimumSize = textureSize;
                }
            }
        }

        public override void _Process(double delta)
        {
            // Skip processing if we're transitioning to another scene
            if (isTransitioning)
                return;

            // Handle title flickering
            flickerTimer -= (float)delta;
            if (flickerTimer <= 0)
            {
                // Randomly change title color to create flickering effect
                float r = rng.RandfRange(0.7f, 0.9f);
                float g = rng.RandfRange(0.0f, 0.2f);
                float b = rng.RandfRange(0.0f, 0.1f);
                titleLabel.AddThemeColorOverride("font_color", new Color(r, g, b));

                // Reset timer with random interval
                flickerTimer = rng.RandfRange(0.1f, 0.5f);
            }

            // Handle profile frame shaking
            if (profileShakeTimer > 0)
            {
                profileShakeTimer -= (float)delta;
                float offsetX = rng.RandfRange(-profileShakeIntensity, profileShakeIntensity);
                float offsetY = rng.RandfRange(-profileShakeIntensity, profileShakeIntensity);
                profileFrame.Position = originalProfilePosition + new Vector2(offsetX, offsetY);

                if (profileShakeTimer <= 0)
                {
                    // Reset position when shake is done
                    profileFrame.Position = originalProfilePosition;

                    // Schedule next shake after random interval
                    profileShakeTimer = rng.RandfRange(3f, 8f);
                    profileShakeIntensity = rng.RandfRange(1f, 5f);
                }
            }
            else
            {
                // Start a new shake after random interval
                profileShakeTimer = rng.RandfRange(3f, 8f);
                profileShakeIntensity = rng.RandfRange(1f, 5f);
            }

            // Handle wolf howl sound timing
            if (wolfHowlAudio != null && !wolfHowlAudio.Playing)
            {
                wolfHowlTimer -= (float)delta;
                if (wolfHowlTimer <= 0)
                {
                    // Play wolf howl sound
                    wolfHowlAudio.Play();

                    // Reset timer with random interval for next howl
                    wolfHowlTimer = rng.RandfRange(10f, 30f);
                }
            }

            // Handle profile jumping to random positions
            profileJumpTimer -= (float)delta;
            if (profileJumpTimer <= 0)
            {
                // Make profile jump to a new position
                SetRandomProfilePosition(true);

                // Reset timer with random interval for next jump
                profileJumpTimer = rng.RandfRange(5f, 15f);
            }

            // Handle random scary events
            scaryEventTimer -= (float)delta;
            if (scaryEventTimer <= 0)
            {
                // Trigger a random scary event
                TriggerRandomScaryEvent();

                // Reset timer with random interval for next event
                scaryEventTimer = rng.RandfRange(7f, 20f);
            }
        }

        private void SetRandomProfilePosition(bool playSound)
        {
            // Remove the profile from its current parent
            if (profileFrame.GetParent() != null)
            {
                profileFrame.GetParent().RemoveChild(profileFrame);
            }

            // Decide where to place the profile (randomly)
            int position = rng.RandiRange(0, 3);

            switch (position)
            {
                case 0: // Left side (original position)
                    profileContainer.AddChild(profileFrame);
                    profileContainer.MoveChild(profileFrame, 0); // Move to first position
                    break;
                case 1: // Right side
                    profileContainer.AddChild(profileFrame);
                    profileContainer.MoveChild(profileFrame, 1); // Move to last position
                    break;
                case 2: // Above text
                    parentContainer.AddChild(profileFrame);
                    // Position above the HBoxContainer
                    profileFrame.Position = new Vector2(
                        rng.RandfRange(50, parentContainer.Size.X - 150),
                        rng.RandfRange(50, profileContainer.Position.Y - 150)
                    );
                    break;
                case 3: // Below text
                    parentContainer.AddChild(profileFrame);
                    // Position below the HBoxContainer
                    profileFrame.Position = new Vector2(
                        rng.RandfRange(50, parentContainer.Size.X - 150),
                        profileContainer.Position.Y
                            + profileContainer.Size.Y
                            + rng.RandfRange(20, 100)
                    );
                    break;
            }

            // Store the new original position for shake effect
            originalProfilePosition = profileFrame.Position;

            // Make sure profile image size is correct after repositioning
            AdjustProfileImageSize();

            // Make the profile larger briefly for a jumpscare effect if this is not initial setup
            if (playSound)
            {
                float originalScale = profileFrame.Scale.X;
                profileFrame.Scale = new Vector2(1.5f, 1.5f);

                // Create a timer to reset the scale
                var timer = GetTree().CreateTimer(0.2);
                timer.Timeout += () =>
                    profileFrame.Scale = new Vector2(originalScale, originalScale);
            }
        }

        private void TriggerRandomScaryEvent()
        {
            int eventType = rng.RandiRange(0, 3);

            switch (eventType)
            {
                case 0: // Briefly flash the screen
                    FlashScreen();
                    break;
                case 1: // Distort the text
                    DistortText();
                    break;
                case 2: // Make the profile image "glitch"
                    GlitchProfile();
                    break;
                case 3: // Briefly show a scary message
                    ShowScaryMessage();
                    break;
            }
        }

        private void FlashScreen()
        {
            // Create a full-screen white panel for the flash
            var flashPanel = new Panel();
            AddChild(flashPanel);
            flashPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);

            // Set it to white
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(1, 1, 1, 0.7f);
            flashPanel.AddThemeStyleboxOverride("panel", styleBox);

            // Remove after a short delay
            var timer = GetTree().CreateTimer(0.1);
            timer.Timeout += () =>
            {
                RemoveChild(flashPanel);
                flashPanel.QueueFree();
            };
        }

        private void DistortText()
        {
            // Save original text
            string originalTitle = titleLabel.Text;
            string originalIdentity = identityLabel.Text;

            // Distort text with random characters
            string distortedTitle = "";
            string distortedIdentity = "";

            for (int i = 0; i < originalTitle.Length; i++)
            {
                if (rng.Randf() < 0.3f)
                {
                    // Replace with a random character
                    char randomChar = (char)rng.RandiRange(33, 126); // ASCII printable characters
                    distortedTitle += randomChar;
                }
                else
                {
                    distortedTitle += originalTitle[i];
                }
            }

            for (int i = 0; i < originalIdentity.Length; i++)
            {
                if (rng.Randf() < 0.3f)
                {
                    // Replace with a random character
                    char randomChar = (char)rng.RandiRange(33, 126);
                    distortedIdentity += randomChar;
                }
                else
                {
                    distortedIdentity += originalIdentity[i];
                }
            }

            // Set distorted text
            titleLabel.Text = distortedTitle;
            identityLabel.Text = distortedIdentity;

            // Reset after a short delay
            var timer = GetTree().CreateTimer(0.5);
            timer.Timeout += () =>
            {
                titleLabel.Text = originalTitle;
                identityLabel.Text = originalIdentity;
            };
        }

        private void GlitchProfile()
        {
            // Save original modulation
            Color originalModulation = profileFrame.Modulate;

            // Apply glitch effect (color distortion)
            profileFrame.Modulate = new Color(
                rng.RandfRange(0.5f, 1.5f),
                rng.RandfRange(0.0f, 0.5f),
                rng.RandfRange(0.0f, 0.5f),
                1.0f
            );

            // Apply random rotation
            float originalRotation = profileFrame.Rotation;
            profileFrame.Rotation = rng.RandfRange(-0.2f, 0.2f);

            // Increase shake intensity temporarily
            profileShakeIntensity = 10f;

            // Reset after a short delay
            var timer = GetTree().CreateTimer(0.3);
            timer.Timeout += () =>
            {
                profileFrame.Modulate = originalModulation;
                profileFrame.Rotation = originalRotation;
                profileShakeIntensity = rng.RandfRange(1f, 5f);
            };
        }

        private void ShowScaryMessage()
        {
            // Create a label for the scary message
            var scaryLabel = new Label();
            AddChild(scaryLabel);

            // Position it randomly on screen
            scaryLabel.Position = new Vector2(
                rng.RandfRange(100, GetViewportRect().Size.X - 300),
                rng.RandfRange(100, GetViewportRect().Size.Y - 100)
            );

            // Choose a random scary message
            string[] scaryMessages =
            {
                "BEHIND YOU",
                "DON'T LOOK",
                "RUN",
                "IT'S TOO LATE",
                "I SEE YOU",
                "HELP ME",
                "THEY'RE COMING",
                "GET OUT",
            };

            scaryLabel.Text = scaryMessages[rng.RandiRange(0, scaryMessages.Length - 1)];

            // Style the label to be scary
            scaryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.0f, 0.0f));
            scaryLabel.AddThemeFontSizeOverride("font_size", 40);

            // Remove after a short delay
            var timer = GetTree().CreateTimer(0.7);
            timer.Timeout += () =>
            {
                RemoveChild(scaryLabel);
                scaryLabel.QueueFree();
            };
        }

        // Button event handlers
        public void OnBtnBackPressed()
        {
            GD.Print("Back button pressed!");

            // Mark that we're transitioning to prevent further processing
            isTransitioning = true;

            // Play click sound before transitioning
            if (clickSound != null && !clickSound.Playing)
            {
                clickSound.Play();
            }

            // Use a timer to handle scene transition properly
            Timer transitionTimer = new Timer();
            transitionTimer.OneShot = true;
            transitionTimer.WaitTime = 0.2f; // Short delay for sound to play
            AddChild(transitionTimer);

            transitionTimer.Timeout += () =>
            {
                // Make sure the node and scene tree are still valid before changing scenes
                if (IsInstanceValid(this) && GetTree() != null)
                {
                    GD.Print("Changing scene to: " + targetScene);
                    GetTree().ChangeSceneToFile(targetScene);
                }
            };

            transitionTimer.Start();
            GD.Print("Transition timer started");
        }

        public void OnBtnBackHovered()
        {
            // Play hover sound when button is hovered
            if (buttonHoverAudio != null && !buttonHoverAudio.Playing)
            {
                buttonHoverAudio.Play();
            }
        }
    }
}
