using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace forms
{
    public class PlaySpace
    {
        private int _width;
        private int _height;

        public int Width { get { return (int)((float)_width * ScaleMultiplier); } set { _width = Math.Abs(value); } }
        public int Height { get { return (int)((float)_height * ScaleMultiplier); } set { _height = Math.Abs(value); } }
        public int WidthRaw { get { return _width; } }
        public int HeightRaw { get { return _height; } }
        private int _x;
        private int _y;

        public int Xpos { get { return _x; } set { _x = value; } }
        public int Ypos { get { return _y; } set { _y = value; } }
        public float ScaleMultiplier;

        public int OffsetX => Width / 2;
        public int OffsetY => Height / 2;

        public int[] Corner => new int[2] { Xpos - OffsetX, Ypos - OffsetY };

        public List<Card> cards;
        public PlaySpace(int Xpos, int Ypos, int Width, int Height, float ScaleMultiplier = 1)
        {
            _width = Width;
            _height = Height;
            this.Xpos = Xpos;
            this.Ypos = Ypos;
            this.ScaleMultiplier = ScaleMultiplier;
        }
    }
}
