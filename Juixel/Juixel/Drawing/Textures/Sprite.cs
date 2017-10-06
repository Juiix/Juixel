using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juixel.Drawing.Textures
{
    public class Sprite
    {
        #region Properties

        /// <summary>
        /// The base <see cref="Texture2D"/> for drawing
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Source of the sprite within the base <see cref="Texture2D"/>
        /// </summary>
        public Rectangle Source;

        #endregion

        /// <summary>
        /// Initializes the <see cref="SpriteNode"/> with a base <see cref="Texture2D"/>
        /// </summary>
        /// <param name="Texture">The base <see cref="Texture2D"/></param>
        public Sprite(Texture2D Texture)
        {
            Initialize(Texture, new Rectangle(0, 0, Texture.Width, Texture.Height));
        }

        /// <summary>
        /// Initializes the <see cref="SpriteNode"/> with a base <see cref="Texture2D"/> and a sprite source
        /// </summary>
        /// <param name="Texture">The base <see cref="Texture2D"/></param>
        /// <param name="Source">The source of this sprite within the base <see cref="Texture2D"/></param>
        public Sprite(Texture2D Texture, Rectangle Source)
        {
            Initialize(Texture, Source);
        }

        private void Initialize(Texture2D Texture, Rectangle Source)
        {
            this.Texture = Texture;
            this.Source = Source;
        }

        public static implicit operator Sprite(Texture2D Texture) => new Sprite(Texture);
    }
}
