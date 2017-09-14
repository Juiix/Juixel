using Juixel.Drawing;
using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Juixel;
using Utilities.Tools;
using Juixel.Input;
using Microsoft.Xna.Framework.Input;
using Clans.Drawing.ParticleSystems;
using Clans.Drawing.GameObjects.Entities;
using Juixel.Drawing.Tiles;

namespace Clans.Scenes
{
    public class LoadingScene : Scene
    {
        #region Init

        private Player Player;
        private TileManager Tiles;
        private SnowEffect Snow;

        public LoadingScene(Location Size) : base(Size)
        {
            Player = new Player();
            Player.Scale = 10;
            AddChild(Player);

            Tiles = new TileManager(2048, 2048, ClansGame.TestTileTexture, 8);
            Tiles.Scale = 5;
            for (int Y = 0; Y < Tiles.Height; Y++)
                for (int X = 0; X < Tiles.Width; X++)
                    Tiles.Set(X, Y, JRandom.Next(3));
            Tiles.Layer = -1;
            AddChild(Tiles);

            Camera.Target = Player;

            Snow = new SnowEffect(2);
            UI.AddChild(Snow);
        }

        #endregion

        #region Methods

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            Location MoveVector = 0;

            if (Input.KeyIsDown(Keys.W))
                MoveVector += new Location(0, -10);
            if (Input.KeyIsDown(Keys.S))
                MoveVector += new Location(0, 10);

            if (Input.KeyIsDown(Keys.A))
                MoveVector += new Location(-10, 0);
            if (Input.KeyIsDown(Keys.D))
                MoveVector += new Location(10, 0);

            if (Input.KeyIsDown(Keys.OemPlus))
                Snow.Intensity += 2 * Time.ElapsedSec;
            if (Input.KeyIsDown(Keys.OemMinus))
                Snow.Intensity -= 2 * Time.ElapsedSec;

            Player.Position += MoveVector;
            Tiles.Focus = (Player.Position / 40).Int();

            JuixelGame.ShowDebugText(Snow.Intensity.ToString(), 2);
        }

        #endregion
    }
}
