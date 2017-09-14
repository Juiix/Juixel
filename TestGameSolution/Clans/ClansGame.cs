using Juixel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Utilities;
using Clans.Scenes;
using Juixel.Drawing.Textures;
using MonoGame.Extended.BitmapFonts;

namespace Clans
{
    public class ClansGame : JuixelGame
    {
        public static Texture2D TestTexture;
        public static Texture2D TestTileTexture;
        public static Texture2D Particles_8x8;

        public ClansGame(Game Game, DeviceType Device) : base(Game, Device)
        {
            LockFrameRate = false;

            BackgroundColor = Color.Black;
            //TintColor = new Color(232, 244, 252);
            //TintIntensity = 0.1f;
        }

        public override void LoadContent()
        {
            TestTexture = Content.Load<Texture2D>("Characters_8x8");
            TestTileTexture = Content.Load<Texture2D>("Tiles_8x8");
            Particles_8x8 = Content.Load<Texture2D>("Particles_8x8");

            base.LoadContent();

            CurrentScene = new LoadingScene(new Location(WindowWidth, WindowHeight));
        }

        public override void LoadFonts()
        {
            Font.AddFont("px", new Font(Content.Load<BitmapFont>("Fonts/PixelFont-20"), 20));
        }
    }
}
