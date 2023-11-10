using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace forms
{
    public class Transform
    {
        public PlaySpace PlaySpace { get; set; }

        private int _width;
        private int _height;

        private float playSpaceM => PlaySpace.ScaleMultiplier;
        public int Width { get{ return (int)((float)_width * ScaleMultiplier * playSpaceM); } set { _width = Math.Abs(value); } }
        public int Height { get { return(int)((float) _height * ScaleMultiplier * playSpaceM); } set { _height = Math.Abs(value); } }
        public int WidthRaw {  get { return _width; } }
        public int HeightRaw {  get { return _height; } }
        private int _x;
        private int _y;

        public int Xpos { get { return _x + PlaySpace.Corner[0]; } set { _x = value; } }
        public int Ypos { get { return _y + PlaySpace.Corner[1]; } set { _y = value; } }
        public float ScaleMultiplier;

        public int OffsetX => Width / 2;
        public int OffsetY => Height / 2;

        public int[] Corner => new int[2] { Xpos - Width / 2, Ypos - Height / 2 };
        public Transform(int Xpos, int Ypos, int Width, int Height, float ScaleMultiplier = 1)
        {
            PlaySpace = new PlaySpace(0,0,10,10,1);
            _width = Width;
            _height = Height;
            this.Xpos = Xpos;
            this.Ypos = Ypos;
            this.ScaleMultiplier = ScaleMultiplier;
        }
        public Transform(Transform transform) : this(transform.Xpos, transform.Ypos, transform.Width, transform.Height, transform.ScaleMultiplier) { }
        //public Transform(PlaySpace playspace) : this(0,0,10,10,1,playspace) { }
        public Transform() : this(0,0,10,10,1) { }
    }
}
