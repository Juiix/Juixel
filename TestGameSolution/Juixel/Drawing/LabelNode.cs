using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Utilities;
using MonoGame.Extended.BitmapFonts;
using Juixel.Extensions;
using Utilities.JMath;
using Microsoft.Xna.Framework;
using Juixel.Drawing.Textures;

namespace Juixel.Drawing
{
    public class LabelNode : Node
    {
        #region Properties

        /// <summary>
        /// The text displayed
        /// </summary>
        public string Text;
        
        /// <summary>
        /// The height of the text
        /// </summary>
        public double TextHeight
        {
            get => Font.Height * Scale.X;
            set => Scale.X = value / Font.Height;
        }

        /// <summary>
        /// The point within the sprite that it will center on 0.0 - 1.0 values.
        /// (0, 0) will be the top left corner
        /// </summary>
        public Location AnchorPoint;

        /// <summary>
        /// The font to be drawn
        /// </summary>
        private Font Font;

        public override Location Size { get => new Location(Font.MeasureString(Text).Width * Scale.X, TextHeight * Scale.X); set { } }

        #endregion

        #region Init

        /// <summary>
        /// Initializes a <see cref="LabelNode"/> with text, height, and a font
        /// </summary>
        /// <param name="Font">The font to draw</param>
        /// <param name="Text">The text to display</param>
        /// <param name="TextHeight">The height of the text</param>
        public LabelNode(Font Font, string Text, double TextHeight)
        {
            this.Font = Font;
            this.Text = Text;
            this.TextHeight = TextHeight;
        }

        /// <summary>
        /// Initializes a <see cref="LabelNode"/> with text, height, and a font
        /// </summary>
        /// <param name="FontName">The name of the font to draw</param>
        /// <param name="Text">The text to display</param>
        /// <param name="TextHeight">The height of the text</param>
        public LabelNode(string FontName, string Text, double TextHeight)
        {
            Font = Font.GetFont(FontName);
            this.Text = Text;
            this.TextHeight = TextHeight;
        }

        #endregion

        #region Drawing

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            Location DrawPosition = Position + Geometry.RotateAroundOrigin(Rotation, this.Position * Scale);
            Scale *= this.Scale;
            Vector2 NormSize = Font.MeasureString(Text);
            SpriteBatch.DrawString(Font.BaseFont, Text, DrawPosition.ToVector2(), Color * (Alpha * this.Alpha), (float)(Rotation.Radians + this.Rotation.Radians),
                new Vector2(NormSize.X * (float)AnchorPoint.X, NormSize.Y * (float)AnchorPoint.Y), (float)(Scale * this.Scale).X, SpriteEffects.None, 0);
            base.Draw(Time, SpriteBatch, Position, Rotation, Scale, Alpha);
        }

        #endregion
    }
}
