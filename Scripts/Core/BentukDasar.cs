using System;
using System.Collections.Generic;
using Godot;

namespace Core
{
    [GlobalClass]
    public partial class BentukDasar : Node2D, IDisposable
    {
        private Primitif _primitif = new Primitif();

        public Primitif Primitif => _primitif;

        public override void _Ready() { }

        public List<Vector2> Margin(
            int MarginLeft,
            int MarginTop,
            int MarginRight,
            int MarginBottom
        )
        {
            if (_primitif == null)
            {
                GD.PrintErr("Node Primitif belum di-assign!");
                return new List<Vector2>();
            }
            List<Vector2> res = new List<Vector2>();
            res.AddRange(_primitif.LineBresenham(MarginLeft, MarginTop, MarginRight, MarginTop));
            res.AddRange(
                _primitif.LineBresenham(MarginLeft, MarginBottom, MarginRight, MarginBottom)
            );
            res.AddRange(_primitif.LineBresenham(MarginLeft, MarginTop, MarginLeft, MarginBottom));
            res.AddRange(
                _primitif.LineBresenham(MarginRight, MarginTop, MarginRight, MarginBottom)
            );
            return res;
        }

        public List<Vector2> Persegi(float x, float y, float ukuran)
        {
            return PersegiPanjang(x, y, ukuran, ukuran);
        }

        public List<Vector2> PersegiPanjang(float x, float y, float panjang, float lebar)
        {
            if (_primitif == null)
            {
                GD.PrintErr("Node Primitif belum di-assign!");
                return new List<Vector2>();
            }
            List<Vector2> garis1 = _primitif.LineBresenham(x, y, x + panjang, y);
            List<Vector2> garis2 = _primitif.LineBresenham(x + panjang, y, x + panjang, y + lebar);
            List<Vector2> garis3 = _primitif.LineBresenham(x + panjang, y + lebar, x, y + lebar);
            List<Vector2> garis4 = _primitif.LineBresenham(x, y + lebar, x, y);
            List<Vector2> persegiPanjang = new List<Vector2>();
            persegiPanjang.AddRange(garis1);
            persegiPanjang.AddRange(garis2.GetRange(1, garis2.Count - 1));
            persegiPanjang.AddRange(garis3.GetRange(1, garis3.Count - 1));
            persegiPanjang.AddRange(garis4.GetRange(1, garis4.Count - 1));
            return persegiPanjang;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _primitif?.Dispose();
                _primitif = null;
            }
        }
    }
}
