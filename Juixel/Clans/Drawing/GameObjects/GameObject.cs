using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing.Textures;
using Juixel;
using Juixel.Tools;

namespace Clans.Drawing.GameObjects
{
    public class GameObject : Node, IUpdatable
    {
        /// <summary>
        /// The internal <see cref="SpriteNode"/>
        /// </summary>
        protected SpriteNode Sprite;

        /// <summary>
        /// The current <see cref="AnimatedSprite"/>, if it has one
        /// </summary>
        protected AnimatedSprite Animation;

        /// <summary>
        /// The time spent on each animation frame
        /// </summary>
        protected double FrameRate = 0.4;
        
        /// <summary>
        /// The base object of all objects in the map
        /// </summary>
        public GameObject()
        {

        }

        public override void Update(JuixelTime Time)
        {
            if (Animation != null)
                Animation.Update(Time, FrameRate, out Sprite.Reversed, out Sprite.AnchorPoint);
        }
    }
}
