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

        private static string DefaultName;
        public static Font Default;
        private static Dictionary<string, Font> _Fonts = new Dictionary<string, Font>();

        public static void AddFont(string Name, BitmapFont BMFont, int Height)
        {
            if (_Fonts.ContainsKey(Name))
            {
                if (DefaultName == Name)
                    Default.AddSize(BMFont, Height);
                else
                    _Fonts[Name].AddSize(BMFont, Height);
            }
            else
            {
                Font Font = new Font();
                Font.AddSize(BMFont, Height);
                if (Default == null)
                {
                    DefaultName = Name;
                    Default = Font;
                }
                _Fonts.Add(Name, Font);
            }
        }

        public static Font GetFont(string Name)
        {
            if (_Fonts.ContainsKey(Name))
                return _Fonts[Name];
            return Default;
        }

        #endregion

        public List<FontInfo> Sizes = new List<FontInfo>();

        public void AddSize(BitmapFont BMFont, int Height)
        {
            FontInfo Font = new FontInfo { BMFont = BMFont, Height = Height };
            for (int i = 0; i < Sizes.Count; i++)
                if (Sizes[i].Height > Height)
                {
                    Sizes.Insert(i, Font);
                    return;
                }
            Sizes.Add(Font);
        }

        public FontInfo GetSized(double Size)
        {
            for (int i = 0; i < Sizes.Count; i++)
            {
                FontInfo Font = Sizes[i];
                if (Font.Height > Size)
                {
                    if (i == 0)
                        return Font;
                    else
                    {
                        FontInfo Lower = Sizes[i - 1];
                        return Font.Height - Size > Size - Lower.Height ? Lower : Font;
                    }
                }
            }
            return Sizes[Sizes.Count - 1];
        }

        public Location MeasureString(string Text, double Height)
        {
            FontInfo Info = GetSized(Height);
            Size2 Size = Info.BMFont.MeasureString(Text);
            double Scalar = Height / Info.Height;
            return new Location(Size.Width * Scalar, Size.Height * Scalar);
        }

        public static implicit operator Font(string Name) => GetFont(Name);
    }

    public class FontInfo : IComparable<FontInfo>
    {
        public BitmapFont BMFont;
        public int Height;

        public int CompareTo(FontInfo other)
        {
            if (Height > other.Height)
                return 1;
            else if (Height < other.Height)
                return -1;
            else
                return 0;
        }
    }
}
