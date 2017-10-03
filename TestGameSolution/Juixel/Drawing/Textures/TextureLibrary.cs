using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juixel.Drawing.Textures
{
    public class TextureLibrary
    {
        #region Textures

        public static Texture2D FadeCircular;

        public static Texture2D Square;

        #endregion

        #region Methods

        public static void Setup(GraphicsDevice GraphicsDevice, ContentManager Content)
        {
            Square = new Texture2D(GraphicsDevice, 1, 1);
            Square.SetData(new Color[] { Color.White });

            int D = 100;
            float Radius = D / 2.0f;
            FadeCircular = new Texture2D(GraphicsDevice, D, D);
            Color[] Colors = new Color[10000];
            for (int y = 0; y < D; y++)
                for (int x = 0; x < D; x++)
                {
                    float dx = (x - Radius) / Radius;
                    float dy = (y - Radius) / Radius;
                    Colors[y * (D) + x] = Color.White * (float)(1 - Math.Sqrt(dx * dx + dy * dy) / Radius);
                }
        }

        #endregion
    }
}
