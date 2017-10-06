using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Microsoft.Xna.Framework.Graphics;
using Utilities.JMath;
using Juixel.Extensions;
using Utilities.Logging;

namespace Juixel.Drawing
{
    public class SpriteNode : Node
    {
        #region Properties

        /// <summary>
        /// The <see cref="Sprite"/> to draw
        /// </summary>
        public Sprite Sprite;

        /// <summary>
        /// The center rect that is scaled, corners will not scale
        /// </summary>
        public Rectangle? CenterRect;

        /// <summary>
        /// The height of the sprite.
        /// This property will raise the sprite without changing it's layer
        /// </summary>
        public double Height = 0;

        /// <summary>
        /// If <see langword="True"/>, the sprite will be flipped horizontally
        /// </summary>
        public bool Reversed = false;

        /// <summary>
        /// Determines if this Sprite will draw with a rounded position
        /// </summary>
        protected virtual bool SnapToPixel => true;

        /// <summary>
        /// The size of this <see cref="SpriteNode"/> with scale applied.
        /// Setting this will change its <see cref="Node.Scale"/>
        /// </summary>
        public override Location Size
        {
            get => new Location(Sprite.Source.Width * Scale.X, Sprite.Source.Height * Scale.Y);
            set => Scale = new Location(value.X / Sprite.Source.Width, value.Y / Sprite.Source.Height);
        }

        #endregion

        #region Init

        public SpriteNode(Sprite Sprite)
        {
            this.Sprite = Sprite;
        }

        public override void Dispose()
        {
            base.Dispose();

            Sprite = null;
        }

        #endregion

        #region Drawing

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            if (!Hidden)
            {
                CheckSamplerState(SpriteBatch);
                Location DrawPosition = Position + Geometry.RotateAroundOrigin(Rotation, this.Position * Scale) - new Location(0, Height * Scale.Y);
                if (SnapToPixel) DrawPosition = DrawPosition.Round();
                Location Size = this.Size * Scale;
                var UseScale = Scale * this.Scale;
                Color Color = this.Color * (Alpha * this.Alpha);
                Rotation += this.Rotation;
                float Radians = (float)Rotation.Radians;

                if (DrawPosition.X + Size.X >= 0 && DrawPosition.Y + Size.X >= 0 && DrawPosition.X - Size.X < JuixelGame.WindowWidth && DrawPosition.Y - Size.X < JuixelGame.WindowHeight)
                {
                    if (!CenterRect.HasValue)
                        SpriteBatch.Draw(Sprite.Texture, DrawPosition.ToVector2(), Sprite.Source, Color, (float)(Rotation.Radians + this.Rotation.Radians),
                            new Vector2(Sprite.Source.Width * (float)AnchorPoint.X, Sprite.Source.Height * (float)AnchorPoint.Y), UseScale.ToVector2(), Reversed ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                    else
                    {
                        DrawPosition -= Geometry.RotateAroundOrigin(Rotation, Size * AnchorPoint);
                        Rectangle ScaleRect = CenterRect.Value;

                        Vector2 centerScale = new Vector2((float)(Size.X - ScaleRect.X * 2) / ScaleRect.Width, (float)(Size.Y - ScaleRect.Y * 2) / ScaleRect.Height);
                        Vector2 centerSize = new Vector2(ScaleRect.Width * centerScale.X, ScaleRect.Height * centerScale.Y);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(ScaleRect.X, ScaleRect.Y))).ToVector2(), ScaleRect, Color, Radians, Vector2.Zero, centerScale, SpriteEffects.None, 0);

                        Rectangle topLeft = new Rectangle(0, 0, ScaleRect.X, ScaleRect.Y);
                        SpriteBatch.Draw(Sprite.Texture, DrawPosition.ToVector2(), topLeft, Color, Radians, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

                        Rectangle topCenter = new Rectangle(ScaleRect.X, 0, ScaleRect.Width, ScaleRect.Y);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(ScaleRect.X, 0))).ToVector2(), topCenter, Color, Radians, Vector2.Zero, new Vector2(centerScale.X, 1), SpriteEffects.None, 0);

                        Rectangle topRight = new Rectangle(ScaleRect.X + ScaleRect.Width, 0, ScaleRect.X, ScaleRect.Y);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(ScaleRect.X + centerSize.X, 0))).ToVector2(), topRight, Color, Radians, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

                        Rectangle leftCenter = new Rectangle(0, ScaleRect.Y, ScaleRect.X, ScaleRect.Height);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(0, ScaleRect.Y))).ToVector2(), leftCenter, Color, Radians, Vector2.Zero, new Vector2(1, centerScale.Y), SpriteEffects.None, 0);

                        Rectangle rightCenter = new Rectangle(ScaleRect.X + ScaleRect.Width, ScaleRect.Y, ScaleRect.X, ScaleRect.Height);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(ScaleRect.X + centerSize.X, ScaleRect.Y))).ToVector2(), rightCenter, Color, Radians, Vector2.Zero, new Vector2(1, centerScale.Y), SpriteEffects.None, 0);

                        Rectangle botLeft = new Rectangle(0, ScaleRect.Y + ScaleRect.Height, ScaleRect.X, ScaleRect.Y);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(0, ScaleRect.Y + centerSize.Y))).ToVector2(), botLeft, Color, Radians, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);

                        Rectangle botCenter = new Rectangle(ScaleRect.X, ScaleRect.Y + ScaleRect.Height, ScaleRect.Width, ScaleRect.Y);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(ScaleRect.X, ScaleRect.Y + centerSize.Y))).ToVector2(), botCenter, Color, Radians, Vector2.Zero, new Vector2(centerScale.X, 1), SpriteEffects.None, 0);

                        Rectangle botRight = new Rectangle(ScaleRect.X + ScaleRect.Width, ScaleRect.Y + ScaleRect.Height, ScaleRect.X, ScaleRect.Y);
                        SpriteBatch.Draw(Sprite.Texture, (DrawPosition + Geometry.RotateAroundOrigin(Rotation, new Location(ScaleRect.X + centerSize.X, ScaleRect.Y + centerSize.Y))).ToVector2(), botRight, Color, Radians, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
                    }
                }
                base.Draw(Time, SpriteBatch, Position, Rotation, Scale, Alpha);
            }
        }

        #endregion

        #region Methods



        #endregion
    }
}
