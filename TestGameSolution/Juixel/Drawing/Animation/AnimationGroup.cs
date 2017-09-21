using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Tools;

namespace Juixel.Drawing.Animation
{
    public class AnimationGroup
    {
        private Dictionary<AnimationStatus, AnimatedSprite> Animations = new Dictionary<AnimationStatus, AnimatedSprite>();

        private static AnimationStatus[] AnimationStatuses = (AnimationStatus[])Enum.GetValues(typeof(AnimationStatus));

        /// <summary>
        /// Used to group animations by direction
        /// </summary>
        /// <param name="AnimationFactory">Creates an animation for a given direction. Return <see langword="null"/> for no animation</param>
        public AnimationGroup(Func<AnimationStatus, AnimatedSprite> AnimationFactory)
        {
            for (int i = 0; i < AnimationStatuses.Length; i++)
            {
                AnimationStatus Status = AnimationStatuses[i];
                AnimatedSprite Sprite = AnimationFactory(Status);
                if (Sprite != null)
                    Animations[Status] = Sprite;
            }
        }

        /// <summary>
        /// Retreive an <see cref="AnimatedSprite"/>
        /// </summary>
        /// <param name="Status">The status of the Animation</param>
        /// <returns></returns>
        public AnimatedSprite Animation(AnimationStatus Status) => Animations[Status];

        public KeyValuePair<AnimationStatus, AnimatedSprite>[] AllAnimations() => Animations.ToArray();
    }
}
