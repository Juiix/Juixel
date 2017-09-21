using Juixel.Drawing;
using Juixel.Drawing.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Clans.Drawing.GameObjects
{
    public partial class Player
    {
        private SpriteNode WeaponSprite;
        private SpriteNode HelmSprite;

        private void SetupSprites()
        {
            WeaponAnimation = WeaponAnimations.Animation(AnimationStatus.Still_Right);
            WeaponSprite = new SpriteNode(WeaponAnimation);
            WeaponSprite.Layer = 1;
            AddChild(WeaponSprite);

            HelmAnimation = HelmAnimations.Animation(AnimationStatus.Still_Right);
            HelmSprite = new SpriteNode(HelmAnimation);
            HelmSprite.Position = new Location(0, -4);
            HelmSprite.Layer = -1;
            AddChild(HelmSprite);
        }
    }
}
