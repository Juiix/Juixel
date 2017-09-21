using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utilities;

namespace Juixel.Drawing.Textures
{
    public class AnimatedSprite : Sprite
    {
        public int CurrentIndex = 0;
        public double CurrentTime = 0;

        public IntLocation Offset;
        private Frame[] _Sources;

        public AnimatedSprite(Texture2D Texture, params Frame[] Sources) : base(Texture, (Sources.Length > 0 ? Sources[0].Source : Texture.Bounds))
        {
            _Sources = Sources;
        }

        public void Update(JuixelTime Time, double FrameRate, out bool Reverse, out Location AnchorPoint)
        {
            CurrentTime += Time.ElapsedSec;
            if (CurrentTime > FrameRate)
            {
                CurrentTime = 0;
                CurrentIndex++;
                if (CurrentIndex >= _Sources.Length)
                    CurrentIndex = 0;
            }

            Frame CurrentFrame = _Sources[CurrentIndex];
            Source = CurrentFrame.Source;
            Source.Offset(Offset.X, Offset.Y);
            AnchorPoint = CurrentFrame.AnchorPoint;
            Reverse = CurrentFrame.Reverse;
        }

        public void Reset()
        {
            CurrentTime = 0;
            CurrentIndex = 0;
        }

        public void Set(int Index, double Time)
        {
            CurrentIndex = Index % _Sources.Length;
            CurrentTime = Time;
        }
    }

    public class Frame
    {
        public Rectangle Source;
        public Location AnchorPoint;
        public bool Reverse;

        public Frame(Rectangle Source, Location AnchorPoint, bool Reverse)
        {
            this.Source = Source;
            this.AnchorPoint = AnchorPoint;
            this.Reverse = Reverse;
        }
    }
}
