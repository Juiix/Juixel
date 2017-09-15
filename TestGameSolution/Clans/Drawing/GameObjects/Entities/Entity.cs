using Juixel;
using Juixel.Drawing.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Logging;
using Utilities.Tools;

namespace Clans.Drawing.GameObjects.Entities
{
    public class Entity : GameObject
    {
        #region Properties

        public virtual Direction FacingDirection { get; set; } = Direction.Left;

        protected override bool Updates => true;

        protected bool Moving = false;
        protected bool WasMoving = false;

        protected bool Attacking = false;
        protected bool WasAttacking = false;

        #endregion

        #region Init

        public Entity() : base()
        {
            SetupAnimation();
        }

        protected virtual void SetupAnimation()
        {

        }

        #endregion

        #region Movement

        protected virtual void SetDirection(Angle Angle)
        {
            Direction Old = FacingDirection;
            int AngleValue = (int)((Angle + 45).Degrees + 0.5);
            FacingDirection = DirectionFromAngle(AngleValue);

            if (Old != FacingDirection)
                UpdateDirection();
            else if (!WasMoving && Moving && Animation != null)
            {
                UpdateDirection();
                Animation.Set(1, 0);
            }
        }

        protected void UpdateDirection()
        {
            Sprite New = SpriteForDirection(FacingDirection, Moving);
            Sprite.Sprite = New;

            if (New is AnimatedSprite)
            {
                AnimatedSprite NewAnimation = (AnimatedSprite)New;
                if (Animation != null)
                    NewAnimation.Set(Animation.CurrentIndex, Animation.CurrentTime);
                else
                    NewAnimation.Reset();
                Animation = NewAnimation;
            }
        }

        private Direction DirectionFromAngle(int Degrees)
        {
            if (Degrees <= 90 || Degrees == 360)
                return Direction.Left;
            else if (Degrees < 180)
                return Direction.Down;
            else if (Degrees <= 270)
                return Direction.Right;
            return Direction.Up;
        }

        protected virtual Sprite SpriteForDirection(Direction Direction, bool Moving)
        {
            return null;
        }

        protected override void OnPositionWillChange(Location To)
        {
            Moving = true;
            SetDirection((To - Position).Angle);
        }

        #endregion

        #region Animation

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            if (Animation != null)
                Animation.Update(Time, FrameRate, out Sprite.Reversed, out Sprite.AnchorPoint);

            if (Moving)
            {
                Moving = false;
                WasMoving = true;
            }
            else if (WasMoving)
            {
                WasMoving = false;
                UpdateDirection();
            }
        }

        #endregion
    }
}
