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

            FadeCircular = Content.Load<Texture2D>("FadeCircular");
        }

        #endregion
    }
}
