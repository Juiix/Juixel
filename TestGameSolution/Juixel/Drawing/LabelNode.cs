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

        private double _TextHeight;
        /// <summary>
        /// The height of the text
        /// </summary>
        public double TextHeight
        {
            get => _TextHeight;
            set => _TextHeight = value;
        }

        /// <summary>
        /// The font to be drawn
        /// </summary>
        private Font Font;

        public override SamplerState UsedSampler => SamplerState.LinearClamp;

        public override Location Size { get { return Font.MeasureString(Text, _TextHeight * Scale.X); } set { } }

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
            if (!Hidden)
            {
                CheckSamplerState(SpriteBatch);
                Location DrawPosition = Position + Geometry.RotateAroundOrigin(Rotation, this.Position * Scale);
                var UseScale = Scale * this.Scale;
                FontInfo Info = Font.GetSized(_TextHeight * UseScale.X);
                Vector2 NormSize = Info.BMFont.MeasureString(Text);
                SpriteBatch.DrawString(Info.BMFont, Text, DrawPosition.ToVector2(), Color * (Alpha * this.Alpha), (float)(Rotation.Radians + this.Rotation.Radians),
                    new Vector2(NormSize.X * (float)AnchorPoint.X, NormSize.Y * (float)AnchorPoint.Y), (float)(UseScale * (_TextHeight / Info.Height)).X, SpriteEffects.None, 0);
                base.Draw(Time, SpriteBatch, Position, Rotation, Scale, Alpha);
            }
        }

        #endregion
    }
}
