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

namespace Clans.Drawing.GameObjects.Entities
{
    public class Player : Entity
    {
        #region Animation

        private AnimatedSprite Ani_Walk_Left;
        private AnimatedSprite Ani_Walk_Right;
        private AnimatedSprite Ani_Walk_Up;
        private AnimatedSprite Ani_Walk_Down;

        private AnimatedSprite Ani_Still_Left;
        private AnimatedSprite Ani_Still_Right;
        private AnimatedSprite Ani_Still_Up;
        private AnimatedSprite Ani_Still_Down;

        #endregion

        #region Init

        public Player() : base()
        {
            Sprite = new SpriteNode(Ani_Walk_Left);
            AddChild(Sprite);

            Animation = Ani_Still_Left;

            FrameRate = 0.2;
        }

        protected override void SetupAnimation()
        {
            Location NormAnchor = new Location(0.5, 1);
            Location LargerAnchor = new Location(0.25, 1);
            Location LargerAnchorReverse = new Location(0.75, 1);

            // Walking

            Ani_Walk_Left = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(0, 0), NormAnchor, false),
                new Frame(SourceFrom(1, 0), NormAnchor, false)
            );

            Ani_Walk_Right = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(0, 0), NormAnchor, true),
                new Frame(SourceFrom(1, 0), NormAnchor, true)
            );

            Ani_Walk_Up = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(1, 2), NormAnchor, false),
                new Frame(SourceFrom(2, 2), NormAnchor, false)
            );

            Ani_Walk_Down = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(1, 1), NormAnchor, false),
                new Frame(SourceFrom(2, 1), NormAnchor, false)
            );

            // Still

            Ani_Still_Left = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(0, 0), NormAnchor, false)
            );

            Ani_Still_Right = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(0, 0), NormAnchor, true)
            );

            Ani_Still_Up = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(0, 2), NormAnchor, false)
            );

            Ani_Still_Down = new AnimatedSprite(ClansGame.TestTexture,
                new Frame(SourceFrom(0, 1), NormAnchor, false)
            );
        }

        private Rectangle SourceFrom(int X, int Y)
        {
            int Width = X == 4 ? 16 : 8;
            int Height = 8;
            return new Rectangle(X * 8, Y * 8, Width, Height);
        }

        #endregion

        #region Updating

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);
        }

        protected override Sprite SpriteForDirection(Direction Direction, bool Moving)
        {
            if (Moving)
                switch (Direction)
                {
                    case Direction.Right: return Ani_Walk_Right;
                    case Direction.Up: return Ani_Walk_Up;
                    case Direction.Down: return Ani_Walk_Down;
                    default: return Ani_Walk_Left;
                }
            else
                switch (Direction)
                {
                    case Direction.Right: return Ani_Still_Right;
                    case Direction.Up: return Ani_Still_Up;
                    case Direction.Down: return Ani_Still_Down;
                    default: return Ani_Still_Left;
                }
        }

        #endregion
    }
}
