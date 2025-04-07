using System;
using Godot;

namespace Scenes
{
    public partial class GuideScene : Control
    {
        private Label titleLabel;
        private RichTextLabel guideContentLabel;
        private Button backButton;
        private AudioStreamPlayer creepyAudio;
        private AudioStreamPlayer pageFlipAudio;
        private AudioStreamPlayer buttonHoverAudio;
        private AudioStreamPlayer clickSound;
        private RandomNumberGenerator rng = new RandomNumberGenerator();
        private float flickerTimer = 0f;
        private float scaryEventTimer = 0f;
        private bool isTransitioning = false;
        private string targetScene = "res://Scenes/UI/Welcome.tscn";
        private int currentPage = 0;
        private string[] pageContents;

        public override void _Ready()
        {
            // Get references to UI elements
            titleLabel = GetNode<Label>("MarginContainer/VBoxContainer/Title");
            guideContentLabel = GetNode<RichTextLabel>(
                "MarginContainer/VBoxContainer/ScrollContainer/GuideContent"
            );
            backButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/BtnBack");

            // Setup page navigation buttons
            var prevButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/BtnPrev");
            var nextButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/BtnNext");

            // Connect button signals
            if (!backButton.IsConnected("pressed", new Callable(this, nameof(OnBtnBackPressed))))
            {
                backButton.Pressed += OnBtnBackPressed;
            }

            if (
                !backButton.IsConnected(
                    "mouse_entered",
                    new Callable(this, nameof(OnButtonHovered))
                )
            )
            {
                backButton.MouseEntered += OnButtonHovered;
            }

            if (!prevButton.IsConnected("pressed", new Callable(this, nameof(OnBtnPrevPressed))))
            {
                prevButton.Pressed += OnBtnPrevPressed;
            }

            if (
                !prevButton.IsConnected(
                    "mouse_entered",
                    new Callable(this, nameof(OnButtonHovered))
                )
            )
            {
                prevButton.MouseEntered += OnButtonHovered;
            }

            if (!nextButton.IsConnected("pressed", new Callable(this, nameof(OnBtnNextPressed))))
            {
                nextButton.Pressed += OnBtnNextPressed;
            }

            if (
                !nextButton.IsConnected(
                    "mouse_entered",
                    new Callable(this, nameof(OnButtonHovered))
                )
            )
            {
                nextButton.MouseEntered += OnButtonHovered;
            }

            // Setup creepy ambient sound
            creepyAudio = new AudioStreamPlayer();
            AddChild(creepyAudio);

            // Try to load horror ambient sound
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

            // Setup page flip sound
            pageFlipAudio = new AudioStreamPlayer();
            AddChild(pageFlipAudio);
            var pageFlipResource = GD.Load<AudioStream>("res://Assets/Sounds/page_flip.mp3");
            if (pageFlipResource != null)
            {
                pageFlipAudio.Stream = pageFlipResource;
                pageFlipAudio.VolumeDb = -5;
                pageFlipAudio.Bus = "Master";
            }

            // Setup button hover sound
            buttonHoverAudio = new AudioStreamPlayer();
            AddChild(buttonHoverAudio);
            var buttonHoverResource = GD.Load<AudioStream>("res://Assets/Sounds/hover.mp3");
            if (buttonHoverResource != null)
            {
                buttonHoverAudio.Stream = buttonHoverResource;
                buttonHoverAudio.VolumeDb = -8;
                buttonHoverAudio.Bus = "Master";
            }

            // Setup click sound
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
            scaryEventTimer = rng.RandfRange(5f, 10f);

            // Setup page contents
            InitializePageContents();

            // Display the first page
            UpdatePageContent();
        }

        private void InitializePageContents()
        {
            pageContents = new string[]
            {
                // Page 1: Introduction
                "[center][color=#AA0000][font_size=24]PANDUAN MENGARUNGI MIMPI BURUK[/font_size][/color][/center]\n\n"
                    + "[color=#CCCCCC]Selamat datang di dunia mimpi buruk ini. Aplikasi ini memiliki 4 karya yang akan membawa Anda ke dalam kegelapan yang semakin dalam. Setiap karya memiliki tingkat keseraman yang berbeda.[/color]\n\n"
                    + "[color=#AA0000]PERINGATAN:[/color] [color=#CCCCCC]Jangan pernah menatap terlalu lama. Mereka bisa melihat Anda melalui layar.[/color]",
                // Page 2: Karya 1
                "[center][color=#AA0000][font_size=24]KARYA 1: GARIS KEHAMPAAN[/font_size][/color][/center]\n\n"
                    + "[color=#CCCCCC]Karya pertama hanya menampilkan outline rumah dalam kegelapan. Garis-garis tipis yang membentuk struktur rumah adalah awal dari perjalanan Anda.[/color]\n\n"
                    + "[color=#AA0000]Cara Menggunakan:[/color]\n"
                    + "[color=#CCCCCC]1. Pilih Karya 1 dari menu utama\n"
                    + "2. Amati garis-garis rumah yang terbentuk\n"
                    + "3. Perhatikan detail pada outline\n"
                    + "4. Jangan terlalu fokus pada sudut gelap rumah[/color]",
                // Page 3: Karya 2
                "[center][color=#AA0000][font_size=24]KARYA 2: WARNA DARAH[/font_size][/color][/center]\n\n"
                    + "[color=#CCCCCC]Karya kedua menampilkan rumah dengan warna. Warna-warna yang muncul membawa energi tersendiri, menarik Anda lebih dalam ke dunia ini.[/color]\n\n"
                    + "[color=#AA0000]Cara Menggunakan:[/color]\n"
                    + "[color=#CCCCCC]1. Pilih Karya 2 dari menu utama\n"
                    + "2. Perhatikan perubahan warna pada struktur rumah\n"
                    + "3. Rasakan atmosfer yang tercipta dari kombinasi warna\n"
                    + "4. Jika warna merah mulai mendominasi, segera alihkan pandangan[/color]",
                // Page 4: Karya 3
                "[center][color=#AA0000][font_size=24]KARYA 3: GERAKAN DALAM GELAP[/font_size][/color][/center]\n\n"
                    + "[color=#CCCCCC]Karya ketiga menampilkan outline rumah dengan animasi dan sosok-sosok yang bergerak. Mereka menyadari kehadiran Anda.[/color]\n\n"
                    + "[color=#AA0000]Cara Menggunakan:[/color]\n"
                    + "[color=#CCCCCC]1. Pilih Karya 3 dari menu utama\n"
                    + "2. Amati gerakan yang terjadi pada outline rumah\n"
                    + "3. Perhatikan sosok-sosok yang muncul di sekitar rumah\n"
                    + "4. Jika sosok-sosok tersebut mulai mendekat, jangan panik[/color]\n\n"
                    + "[color=#AA0000]PERINGATAN:[/color] [color=#CCCCCC]Jangan pernah memanggil nama mereka.[/color]",
                // Page 5: Karya 4
                "[center][color=#AA0000][font_size=24]KARYA 4: PUNCAK TEROR[/font_size][/color][/center]\n\n"
                    + "[color=#CCCCCC]Karya keempat adalah pengalaman penuh. Rumah berwarna dengan animasi dan sosok-sosok yang berinteraksi dengan Anda. Ini adalah puncak dari perjalanan Anda.[/color]\n\n"
                    + "[color=#AA0000]Cara Menggunakan:[/color]\n"
                    + "[color=#CCCCCC]1. Pilih Karya 4 dari menu utama\n"
                    + "2. Saksikan rumah yang kini hidup sepenuhnya\n"
                    + "3. Berinteraksilah dengan elemen-elemen yang muncul\n"
                    + "4. Jika Anda mendengar bisikan, itu bukan dari komputer Anda[/color]",
                // Page 6: Peringatan
                "[center][color=#AA0000][font_size=24]PERINGATAN TERAKHIR[/font_size][/color][/center]\n\n"
                    + "[color=#CCCCCC]Setiap karya memiliki rahasia tersendiri. Temukan mereka jika berani. Namun ingat, setiap rahasia yang terungkap membawa konsekuensi.[/color]\n\n"
                    + "[color=#AA0000]Aturan Keselamatan:[/color]\n"
                    + "[color=#CCCCCC]1. Jangan gunakan aplikasi ini sendirian di malam hari\n"
                    + "2. Jika layar berkedip tanpa sebab, segera tutup aplikasi\n"
                    + "3. Jika Anda melihat sosok yang tidak seharusnya ada, jangan sapa\n"
                    + "4. Jika Anda merasa diawasi setelah menggunakan aplikasi ini, itu normal[/color]\n\n"
                    + "[color=#AA0000]Selamat menikmati mimpi buruk Anda.[/color]",
            };
        }

        private void UpdatePageContent()
        {
            guideContentLabel.Text = pageContents[currentPage];
            titleLabel.Text = $"Panduan - Halaman {currentPage + 1}/{pageContents.Length}";
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

        private void TriggerRandomScaryEvent()
        {
            int eventType = rng.RandiRange(0, 2);

            switch (eventType)
            {
                case 0: // Briefly flash the screen
                    FlashScreen();
                    break;
                case 1: // Distort the text
                    DistortText();
                    break;
                case 2: // Briefly show a scary message
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
            string originalContent = guideContentLabel.Text;

            // Create distorted version with random BBCode tags
            string distortedContent = originalContent;

            // Add random color tags
            if (rng.Randf() < 0.5f)
            {
                distortedContent = distortedContent.Replace("[color=#CCCCCC]", "[color=#FF0000]");
            }

            // Add random shake tags
            if (rng.Randf() < 0.3f)
            {
                distortedContent = "[shake rate=20 level=10]" + distortedContent + "[/shake]";
            }

            // Add random wave tags
            if (rng.Randf() < 0.3f)
            {
                distortedContent = "[wave amp=50 freq=5]" + distortedContent + "[/wave]";
            }

            // Set distorted text
            guideContentLabel.Text = distortedContent;

            // Reset after a short delay
            var timer = GetTree().CreateTimer(0.5);
            timer.Timeout += () =>
            {
                guideContentLabel.Text = originalContent;
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
                "MEREKA MENGAWASIMU",
                "JANGAN LIHAT KE BELAKANG",
                "LARI",
                "SUDAH TERLAMBAT",
                "AKU MELIHATMU",
                "TOLONG AKU",
                "MEREKA DATANG",
                "KELUAR SEKARANG",
                "DIA ADA DI BELAKANGMU",
                "RUMAH ITU HIDUP",
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

        private void OnBtnPrevPressed()
        {
            if (currentPage > 0)
            {
                currentPage--;
                UpdatePageContent();

                // Play page flip sound
                if (pageFlipAudio != null && !pageFlipAudio.Playing)
                {
                    pageFlipAudio.Play();
                }
            }
        }

        private void OnBtnNextPressed()
        {
            if (currentPage < pageContents.Length - 1)
            {
                currentPage++;
                UpdatePageContent();

                // Play page flip sound
                if (pageFlipAudio != null && !pageFlipAudio.Playing)
                {
                    pageFlipAudio.Play();
                }
            }
        }

        private void OnBtnBackPressed()
        {
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
                    GetTree().ChangeSceneToFile(targetScene);
                }
            };

            transitionTimer.Start();
        }

        private void OnButtonHovered()
        {
            // Play hover sound when button is hovered
            if (buttonHoverAudio != null && !buttonHoverAudio.Playing)
            {
                buttonHoverAudio.Play();
            }
        }
    }
}
