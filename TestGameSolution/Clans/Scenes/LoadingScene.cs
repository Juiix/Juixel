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

        private const double Pixels_Per_Second = 10 * 40;

        private Player Player;
        private TileManager Tiles;
        private SnowEffect Snow;
        private BloodEffect Blood;

        public LoadingScene(Location Size) : base(Size)
        {
            Player = new Player();
            Player.Scale = 10;
            Player.Layer = 10;
            AddChild(Player);

            Blood = new BloodEffect(Player, 4, 5);
            Blood.DropsPerSecond = 10;
            AddChild(Blood);

            Tiles = new TileManager(2048, 2048, ClansGame.TestTileTexture, 8);
            Tiles.Scale = 5;
            for (int Y = 0; Y < Tiles.Height; Y++)
                for (int X = 0; X < Tiles.Width; X++)
                    Tiles.Set(X, Y, JRandom.Next(3), 1);

            for (int Y = 0; Y < Tiles.Height; Y++)
                for (int X = 0; X < Tiles.Width; X++)
                    if (JRandom.NextDouble() < 0.02)
                    {
                        int Width = JRandom.Next(3, 6);
                        int Height = JRandom.Next(3, 6);
                        for (int Y2 = 0; Y2 < Width; Y2++)
                            for (int X2 = 0; X2 < Height; X2++)
                            {
                                int XNew = X + X2;
                                int YNew = Y + Y2;
                                if (JRandom.NextDouble() < 0.8 && XNew >= 0 && XNew < Tiles.Width && YNew >= 0 && YNew < Tiles.Height)
                                    Tiles.Set(XNew, YNew, 3 + JRandom.Next(3), 0);
                            }
                    }

            Tiles.Layer = -1;
            AddChild(Tiles);

            Camera.Target = Player;

            Snow = new SnowEffect(2);
            UI.AddChild(Snow);

            LabelNode Info = new LabelNode(Font.Default, "WASD to move, + and - to change the variable", 20);
            Info.Position = new Location(Size.X / 2, Size.Y * 0.3);
            Info.AnchorPoint.X = 0.5;
            Info.Color = new Color(128, 128, 130);
            UI.AddChild(Info);
        }

        #endregion

        #region Methods

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            Location MoveVector = 0;

            if (Input.KeyIsDown(Keys.W))
                MoveVector += new Location(0, -1);
            if (Input.KeyIsDown(Keys.S))
                MoveVector += new Location(0, 1);

            if (Input.KeyIsDown(Keys.A))
                MoveVector += new Location(-1, 0);
            if (Input.KeyIsDown(Keys.D))
                MoveVector += new Location(1, 0);

            if (MoveVector.X != 0 || MoveVector.Y != 0)
            {
                Player.Position += MoveVector.Angle.Location * Pixels_Per_Second * Time.ElapsedSec;
                Tiles.Focus = (Player.Position / 40).Int();
            }

            if (Input.KeyIsDown(Keys.OemPlus))
                Blood.DropsPerSecond *= 1.05;
            if (Input.KeyIsDown(Keys.OemMinus))
                Blood.DropsPerSecond *= 1 / 1.05;
            Blood.DropsPerSecond = (int)(Blood.DropsPerSecond * 1000) / 1000.0;

            JuixelGame.ShowDebugText(Blood.DropsPerSecond.ToString(), 2);
        }

        #endregion
    }
}
