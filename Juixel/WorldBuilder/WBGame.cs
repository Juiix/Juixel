using Juixel;
using Juixel.Drawing;
using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace WorldBuilder
{
    public class WBGame : JuixelGame
    {
        public static Texture2D ButtonSprite;
        public static Texture2D TileSheet;

        public override string DataSavePath => "WorldBuilder\\Data\\";

        public WBGame(Game Game, DeviceType Device) : base(Game, Device)
        {
            LockFrameRate = false;

            BackgroundColor = Color.Black;
        }

        protected override Scene MakeFirstScene()
        {
            return new SelectionScene(new Location(WindowWidth, WindowHeight));
        }

        public override void LoadContent()
        {
            ButtonSprite = Content.Load<Texture2D>("Button-Rect");

            base.LoadContent();
        }

        public override void LoadFonts()
        {
            Font.AddFont("nordic", Content.Load<BitmapFont>("Fonts/Nordic/Nordic_20px"), 20);
            Font.AddFont("nordic", Content.Load<BitmapFont>("Fonts/Nordic/Nordic_40px"), 40);
            Font.AddFont("nordic", Content.Load<BitmapFont>("Fonts/Nordic/Nordic_60px"), 60);
        }
    }
}
