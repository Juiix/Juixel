using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Microsoft.Xna.Framework.Graphics;
using Utilities.Math;
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
        /// The point within the sprite that it will center on 0.0 - 1.0 values.
        /// (0, 0) will be the top left corner
        /// </summary>
        public Location AnchorPoint;

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
            set => Scale = new Location(Sprite.Source.Width / value.X, Sprite.Source.Y / value.Y);
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
            Location DrawPosition = Position + Geometry.RotateAroundOrigin(Rotation, this.Position * Scale) - new Location(0, Height * Scale.Y);
            if (SnapToPixel) DrawPosition = DrawPosition.Round();
            Scale *= this.Scale;
            SpriteBatch.Draw(Sprite.Texture, DrawPosition.ToVector2(), Sprite.Source, Color * (Alpha * this.Alpha), (float)(Rotation.Radians + this.Rotation.Radians),
                new Vector2(Sprite.Source.Width * (float)AnchorPoint.X, Sprite.Source.Height * (float)AnchorPoint.Y), Scale.ToVector2(), Reversed ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            base.Draw(Time, SpriteBatch, Position, Rotation, Scale, Alpha);
        }

        #endregion

        #region Methods



        #endregion
    }
}
