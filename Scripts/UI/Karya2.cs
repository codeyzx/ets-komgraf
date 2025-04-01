using System;
using System.Collections.Generic;
using Core;
using Godot;

namespace UI
{
    public partial class Karya2 : Node2D
    {
        private BentukDasar _bentukDasar = new BentukDasar(); // Objek untuk menggambar bentuk dasar
        private const int MarginLeft = 50; // Margin kiri
        private const int MarginTop = 50; // Margin atas

        /// <summary>
        /// Metode yang dipanggil saat node siap untuk digunakan.
        /// </summary>
        public override void _Ready()
        {
            // Tidak ada inisialisasi tambahan yang diperlukan.
        }

        /// <summary>
        /// Metode yang dipanggil untuk menggambar elemen-elemen di layar.
        /// </summary>
        public override void _Draw()
        {
            // Dapatkan ukuran layar saat ini
            Vector2 windowSize = GetViewportRect().Size;

            // Hitung faktor skala berdasarkan ukuran layar
            float scaleFactor = Math.Min(windowSize.X / 800f, windowSize.Y / 600f);

            // Gambar margin dan garis Kartesian
            MarginPixel(
                MarginLeft,
                MarginTop,
                (int)windowSize.X - MarginLeft,
                (int)windowSize.Y - MarginTop
            );

            // Gambar rumah Bolon
            DrawRumahBolon(MarginLeft, MarginTop, scaleFactor);
        }

        /// <summary>
        /// Metode untuk menggambar rumah Bolon Batak.
        /// </summary>
        /// <param name="marginLeft">Margin kiri.</param>
        /// <param name="marginTop">Margin atas.</param>
        /// <param name="scaleFactor">Faktor skala untuk penyesuaian ukuran.</param>
        private void DrawRumahBolon(int marginLeft, int marginTop, float scaleFactor)
        {
            // Dapatkan ukuran layar dan titik pusat layar
            Vector2 windowSize = GetViewportRect().Size;
            Vector2 centerScreen = new Vector2(windowSize.X / 2, windowSize.Y / 2);

            // Warna-warna tradisional Batak
            Color warnaKayu = new Color("#8B4513"); // Coklat kayu
            Color warnaAtap = new Color("#A52A2A"); // Coklat merah
            Color warnaUkiran = new Color("#FFD700"); // Emas untuk ukiran
            Color warnaDasar = new Color("#F5DEB3"); // Warna dasar krem

            // Ukuran dasar rumah
            float rumahWidth = 300 * scaleFactor;
            float rumahHeight = 200 * scaleFactor;
            float atapHeight = 120 * scaleFactor;

            // Posisi dasar rumah
            Vector2 rumahPos = centerScreen - new Vector2(rumahWidth / 2, rumahHeight / 2);

            // 1. Gambar dasar rumah (lantai yang ditinggikan)
            List<Vector2> dasarRumah = new List<Vector2>
            {
                new Vector2(rumahPos.X, rumahPos.Y + rumahHeight),
                new Vector2(rumahPos.X + rumahWidth, rumahPos.Y + rumahHeight),
                new Vector2(
                    rumahPos.X + rumahWidth - 20 * scaleFactor,
                    rumahPos.Y + rumahHeight - 30 * scaleFactor
                ),
                new Vector2(
                    rumahPos.X + 20 * scaleFactor,
                    rumahPos.Y + rumahHeight - 30 * scaleFactor
                ),
            };
            DrawColoredPolygon(dasarRumah.ToArray(), warnaKayu);

            // 2. Gambar dinding rumah
            List<Vector2> dinding = new List<Vector2>
            {
                new Vector2(
                    rumahPos.X + 20 * scaleFactor,
                    rumahPos.Y + rumahHeight - 30 * scaleFactor
                ),
                new Vector2(
                    rumahPos.X + rumahWidth - 20 * scaleFactor,
                    rumahPos.Y + rumahHeight - 30 * scaleFactor
                ),
                new Vector2(
                    rumahPos.X + rumahWidth - 20 * scaleFactor,
                    rumahPos.Y + 50 * scaleFactor
                ),
                new Vector2(rumahPos.X + 20 * scaleFactor, rumahPos.Y + 50 * scaleFactor),
            };
            DrawColoredPolygon(dinding.ToArray(), warnaDasar);

            // 3. Gambar atap (bentuk segitiga dengan ekstensi di ujungnya)
            List<Vector2> atap = new List<Vector2>
            {
                new Vector2(rumahPos.X - 30 * scaleFactor, rumahPos.Y + 50 * scaleFactor),
                new Vector2(
                    rumahPos.X + rumahWidth / 2,
                    rumahPos.Y + 50 * scaleFactor - atapHeight
                ),
                new Vector2(
                    rumahPos.X + rumahWidth + 30 * scaleFactor,
                    rumahPos.Y + 50 * scaleFactor
                ),
                new Vector2(
                    rumahPos.X + rumahWidth - 20 * scaleFactor,
                    rumahPos.Y + 50 * scaleFactor
                ),
                new Vector2(
                    rumahPos.X + rumahWidth / 2,
                    rumahPos.Y + 30 * scaleFactor - atapHeight / 2
                ),
                new Vector2(rumahPos.X + 20 * scaleFactor, rumahPos.Y + 50 * scaleFactor),
            };
            DrawColoredPolygon(atap.ToArray(), warnaAtap);

            // 4. Gambar tiang penyangga
            float tiangWidth = 10 * scaleFactor;
            for (int i = 0; i < 5; i++)
            {
                float xPos =
                    rumahPos.X + 40 * scaleFactor + i * (rumahWidth - 80 * scaleFactor) / 4;
                List<Vector2> tiang = new List<Vector2>
                {
                    new Vector2(xPos, rumahPos.Y + rumahHeight),
                    new Vector2(xPos + tiangWidth, rumahPos.Y + rumahHeight),
                    new Vector2(xPos + tiangWidth, rumahPos.Y + rumahHeight - 50 * scaleFactor),
                    new Vector2(xPos, rumahPos.Y + rumahHeight - 50 * scaleFactor),
                };
                DrawColoredPolygon(tiang.ToArray(), warnaKayu);
            }

            // 5. Gambar ukiran tradisional (simplified)
            // Ukiran di atap
            for (int i = 0; i < 3; i++)
            {
                float xPos = rumahPos.X + 80 * scaleFactor + i * 80 * scaleFactor;
                List<Vector2> ukiran = new List<Vector2>
                {
                    new Vector2(xPos, rumahPos.Y + 20 * scaleFactor),
                    new Vector2(xPos + 30 * scaleFactor, rumahPos.Y + 40 * scaleFactor),
                    new Vector2(xPos, rumahPos.Y + 60 * scaleFactor),
                    new Vector2(xPos - 30 * scaleFactor, rumahPos.Y + 40 * scaleFactor),
                };
                DrawColoredPolygon(ukiran.ToArray(), warnaUkiran);
            }

            // 6. Gambar tangga
            List<Vector2> tangga = new List<Vector2>
            {
                new Vector2(
                    rumahPos.X + rumahWidth / 2 - 30 * scaleFactor,
                    rumahPos.Y + rumahHeight
                ),
                new Vector2(
                    rumahPos.X + rumahWidth / 2 + 30 * scaleFactor,
                    rumahPos.Y + rumahHeight
                ),
                new Vector2(
                    rumahPos.X + rumahWidth / 2 + 20 * scaleFactor,
                    rumahPos.Y + rumahHeight + 40 * scaleFactor
                ),
                new Vector2(
                    rumahPos.X + rumahWidth / 2 - 20 * scaleFactor,
                    rumahPos.Y + rumahHeight + 40 * scaleFactor
                ),
            };
            DrawColoredPolygon(tangga.ToArray(), warnaKayu);
        }

        /// <summary>
        /// Metode untuk menggambar margin di sekitar layar.
        /// </summary>
        private void MarginPixel(int marginLeft, int marginTop, int marginRight, int marginBottom)
        {
            // Tentukan warna margin
            Godot.Color color = new Godot.Color("#32CD30");

            // Gambar margin menggunakan metode Margin dari objek _bentukDasar
            PutPixelAll(
                _bentukDasar.Margin(marginLeft, marginTop, marginRight, marginBottom),
                color
            );
        }

        /// <summary>
        /// Metode untuk menggambar semua piksel dalam daftar.
        /// </summary>
        public void PutPixelAll(List<Vector2> dots, Godot.Color? color = null)
        {
            foreach (Vector2 point in dots)
            {
                PutPixel(point.X, point.Y, color);
            }
        }

        /// <summary>
        /// Metode untuk menggambar satu piksel.
        /// </summary>
        private void PutPixel(float x, float y, Godot.Color? color = null)
        {
            Godot.Color actualColor = color ?? Godot.Colors.White;
            Vector2[] points = { new Vector2(Mathf.Round(x), Mathf.Round(y)) };
            DrawPrimitive(points, new Godot.Color[] { actualColor }, null);
        }

        /// <summary>
        /// Metode untuk menggambar polygon berwarna.
        /// </summary>
        private void DrawColoredPolygon(Vector2[] points, Color color)
        {
            DrawPolygon(points, new Color[] { color }, null, null);
        }

        /// <summary>
        /// Metode yang dipanggil saat node keluar dari pohon scene.
        /// </summary>
        public override void _ExitTree()
        {
            _bentukDasar?.Dispose();
            _bentukDasar = null;
            base._ExitTree();
        }
    }
}
