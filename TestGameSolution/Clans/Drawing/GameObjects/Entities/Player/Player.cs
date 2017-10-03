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

        private double _Health = 1.0;
        public double Health
        {
            get => _Health;
            set
            {
                value = Math.Min(1.0, Math.Max(0, value));
                _Health = value;
                BloodDrops.DropsPerSecond = (1 - value) * 15;
                BloodTrail.DropsPerSecond = BloodDrops.DropsPerSecond * 8;
            }
        }

        private DropBloodEffect BloodDrops;
        private GroundBloodEffect BloodTrail;

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
            BloodDrops.DropsPerSecond = 0;
            AddChild(BloodDrops);

            BloodTrail = new GroundBloodEffect(this, 4, 5);
            BloodTrail.DropsPerSecond = 0;

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

        protected override void OnParentAdd()
        {
            base.OnParentAdd();
            Parent.AddChild(BloodTrail);
        }

        protected override void OnParentRemove()
        {
            base.OnParentRemove();
            BloodTrail.RemoveFromParent();
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
