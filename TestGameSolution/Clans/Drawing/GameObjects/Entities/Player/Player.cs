using Juixel.Drawing.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel;
using Juixel.Drawing;
using Microsoft.Xna.Framework;
using Utilities;
using Utilities.Tools;
using Clans.Drawing.ParticleSystems;
using Juixel.Drawing.Animation;
using Clans.Data;

namespace Clans.Drawing.GameObjects
{
    public partial class Player : Entity
    {
        #region Init

        public DropBloodEffect BloodDrops;

        public Player() : base()
        {
            Animation = Animations.Animation(AnimationStatus.Still_Right);
            Sprite = new SpriteNode(Animation);
            AddChild(Sprite);

            FrameRate = 0.2;

            SetupOtherAnimations();
            SetupSprites();

            BloodDrops = new DropBloodEffect(new Location(-3, -4), new Location(5, 2), 0.5);
            BloodDrops.Position = new Location(-2.5, 0);
            AddChild(BloodDrops);

            EquippedArmor = new ItemData()
            {
                FileIndex = 2
            };

            EquippedHelm = new ItemData()
            {
                FileIndex = 1
            };

            EquippedWeapon = new ItemData()
            {
                FileIndex = 0
            };
        }

        #endregion

        #region Updating

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);
            UpdateAnimations(Time);
        }

        #endregion
    }
}
