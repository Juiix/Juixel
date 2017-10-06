using Juixel;
using Juixel.Drawing.Animation;
using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Tools;

namespace Clans.Drawing.GameObjects
{
    public partial class Player
    {
        #region Animation

        private AnimationGroup HelmAnimations;
        private AnimationGroup WeaponAnimations;

        private AnimatedSprite HelmAnimation;
        private AnimatedSprite WeaponAnimation;

        #endregion

        #region Animation Init

        private Location NormAnchorLeft = new Location(0.5625, 1);
        private Location NormAnchorRight = new Location(0.4375, 1);

        private Location LargerAnchor = new Location(0.25, 1);
        private Location LargerAnchorReverse = new Location(0.75, 1);

        protected override AnimatedSprite AnimationFor(AnimationStatus Status)
        {
            switch (Status)
            {
                case AnimationStatus.Still_Down:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(0, 1), NormAnchorRight, false)
                    );
                case AnimationStatus.Still_Left:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(0, 0), NormAnchorLeft, true)
                    );
                case AnimationStatus.Still_Right:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(0, 0), NormAnchorRight, false)
                    );
                case AnimationStatus.Still_Up:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(0, 2), NormAnchorLeft, false)
                    );
                case AnimationStatus.Moving_Down:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(1, 1), NormAnchorRight, false),
                        new Frame(SourceFrom(2, 1), NormAnchorRight, false)
                    );
                case AnimationStatus.Moving_Left:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(0, 0), NormAnchorLeft, true),
                        new Frame(SourceFrom(1, 0), NormAnchorLeft, true)
                    );
                case AnimationStatus.Moving_Right:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(0, 0), NormAnchorRight, false),
                        new Frame(SourceFrom(1, 0), NormAnchorRight, false)
                    );
                case AnimationStatus.Moving_Up:
                    return new AnimatedSprite(ClansGame.PlayerParts_1,
                        new Frame(SourceFrom(1, 2), NormAnchorLeft, false),
                        new Frame(SourceFrom(2, 2), NormAnchorLeft, false)
                    );
            }
            return base.AnimationFor(Status);
        }

        private void SetupOtherAnimations()
        {
            HelmAnimations = new AnimationGroup(AnimationFor);
            WeaponAnimations = new AnimationGroup(AnimationFor);
            SetAnimationTexture(WeaponAnimations, ClansGame.Weapons_1);
        }

        private Rectangle SourceFrom(int X, int Y)
        {
            int Width = X == 4 ? 16 : 8;
            int Height = 8;
            return new Rectangle(X * 8, Y * 8, Width, Height);
        }

        private void SetAnimationTexture(AnimationGroup Group, Texture2D Texture)
        {
            foreach (var Ani in Group.AllAnimations())
                Ani.Value.Texture = Texture;
        }

        private void SetAnimationOffset(AnimationGroup Group, int Index, bool HasBow)
        {
            IntLocation Norm = new IntLocation(Index % 16 * 64, Index / 16 * 24);
            IntLocation Bow = new IntLocation(Index % 16 * 64 + 24, Index / 16 * 24);

            foreach (var Ani in Group.AllAnimations())
            {
                if ((int)Ani.Key >= 8)
                {
                    if (HasBow)
                        Ani.Value.Offset = Bow;
                    else
                        Ani.Value.Offset = Norm;
                }
                else
                    Ani.Value.Offset = Norm;
            }
        }

        #endregion

        #region Updating

        protected override Sprite SpriteForStatus(AnimationStatus Status)
        {
            HelmAnimation = HelmAnimations.Animation(Status);
            HelmSprite.Sprite = HelmAnimation;
            WeaponAnimation = WeaponAnimations.Animation(Status);
            WeaponSprite.Sprite = WeaponAnimation;
            return Animations.Animation(Status);
        }

        public void UpdateAnimations(JuixelTime Time)
        {
            HelmAnimation.Update(Time, FrameRate, out HelmSprite.Reversed, out HelmSprite.AnchorPoint);
            WeaponAnimation.Update(Time, FrameRate, out WeaponSprite.Reversed, out WeaponSprite.AnchorPoint);
        }

        protected override void SetDirection(Angle Angle)
        {
            base.SetDirection(Angle);
            HelmAnimation.Set(Animation.CurrentIndex, Animation.CurrentTime);
            WeaponAnimation.Set(Animation.CurrentIndex, Animation.CurrentTime);
        }

        protected override void UpdateDirection()
        {
            base.UpdateDirection();

            switch (FacingDirection)
            {
                case Direction.Right:
                    WeaponSprite.Layer = 1;
                    break;
                case Direction.Left:
                    WeaponSprite.Layer = 1;
                    break;
                case Direction.Up:
                    WeaponSprite.Layer = -1;
                    break;
                case Direction.Down:
                    WeaponSprite.Layer = 1;
                    break;
            }
        }

        #endregion
    }
}
