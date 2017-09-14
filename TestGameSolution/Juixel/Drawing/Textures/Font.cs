using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Juixel.Drawing.Textures
{
    public class Font
    {
        #region Static

        private static Font Default;
        private static Dictionary<string, Font> _Fonts = new Dictionary<string, Font>();

        public static void AddFont(string Name, Font Font)
        {
            if (Default == null)
                Default = Font;
            _Fonts.Add(Name, Font);
        }

        public static Font GetFont(string Name)
        {
            if (_Fonts.ContainsKey(Name))
                return _Fonts[Name];
            return Default;
        }

        #endregion

        public Font(BitmapFont Font, int Height)
        {
            BaseFont = Font;
            this.Height = Height;
        }

        public BitmapFont BaseFont;
        public int Height;

        public Size2 MeasureString(string Text) => BaseFont.MeasureString(Text);
    }
}
