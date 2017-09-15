using Juixel.Drawing.Assets;
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
using Utilities.Tools;

namespace Juixel.Drawing.Tiles
{
    public class Tile
    {
        /// <summary>
        /// The <see cref="Rectangle"/> source of the tile sprite within the sheet
        /// </summary>
        public Rectangle Source;

        /// <summary>
        /// Determines when to draw blend, each index is equal to <see cref="TileDirection"/>
        /// </summary>
        private bool[] Blend = new bool[9];

        /// <summary>
        /// The index of the tile blend mask to use
        /// </summary>
        private int MaskIndex = JRandom.Next(2);

        /// <summary>
        /// Used to determine when this tile will draw blend
        /// </summary>
        public int BlendLayer = 0;

        public Tile(int TexX, int TexY, int Size)
        {
            Source = new Rectangle(TexX, TexY, Size, Size);
        }

        public void Draw(Texture2D Texture, JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Location Scale, float Alpha)
        {
            Location DrawPosition = Position;
            SpriteBatch.Draw(Texture, DrawPosition.ToVector2(), Source, Color.White * Alpha, 0,
                Vector2.Zero, Scale.ToVector2(), SpriteEffects.None, 0);
        }

        public void CompareTo(Tile Other, int BlendIndex, int OtherBlendIndex)
        {
            if (Other == null)
                Blend[BlendIndex] = false;
            else if (Other.BlendLayer == -1 || BlendLayer == -1)
            {
                Blend[BlendIndex] = false;
                Other.Blend[OtherBlendIndex] = false;
            }
            else if (Other.BlendLayer > BlendLayer)
            {
                Blend[BlendIndex] = false;
                Other.Blend[OtherBlendIndex] = true;
            }
            else if (Other.BlendLayer < BlendLayer)
            {
                Blend[BlendIndex] = true;
                Other.Blend[OtherBlendIndex] = false;
            }
            else
            {
                Blend[BlendIndex] = false;
                Other.Blend[OtherBlendIndex] = false;
            }
        }

        public void DrawBlend(Texture2D Texture, JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Location Scale, float Alpha)
        {
            Vector2 BasePosition = new Vector2(MaskIndex % 5 * 24, MaskIndex / 5 * 24);
            for (int i = 0; i < Blend.Length; i++)
                if (Blend[i])
                {
                    IntLocation Offset = TileManager.TileDirectionToPoint((TileDirection)i);
                    Effects.TileBlendEffect.Parameters["MaskBasePosition"].SetValue(new Vector2((BasePosition.X + Offset.X * 8) / Masks.TileBlendMask.Width, (BasePosition.Y + Offset.Y * 8) / Masks.TileBlendMask.Height));
                    Effects.TileBlendEffect.Parameters["TileBasePosition"].SetValue(new Vector2((float)Source.X / Texture.Width, (float)Source.Y / Texture.Height));
                    Location DrawPosition = Position + new Location((Offset.X - 1) * Source.Width, (Offset.Y - 1) * Source.Height) * Scale;
                    SpriteBatch.Draw(Texture, DrawPosition.ToVector2(), Source, Color.White * Alpha, 0,
                        Vector2.Zero, Scale.ToVector2(), SpriteEffects.None, 0);
                }
        }
    }
}
