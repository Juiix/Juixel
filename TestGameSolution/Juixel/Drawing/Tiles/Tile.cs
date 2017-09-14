using Juixel.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Math;

namespace Juixel.Drawing.Tiles
{
    public class Tile
    {
        public Rectangle Source;

        public Tile(int TexX, int TexY, int Size)
        {
            Source = new Rectangle(TexX, TexY, Size, Size);
        }

        public void Draw(Texture2D Texture, JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            Location DrawPosition = Position + Geometry.RotateAroundOrigin(Rotation, Scale);
            SpriteBatch.Draw(Texture, DrawPosition.ToVector2(), Source, Color.White * Alpha, (float)Rotation.Radians,
                Vector2.Zero, Scale.ToVector2(), SpriteEffects.None, 0);
        }
    }
}
