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
using Microsoft.Xna.Framework.Input;
using Clans.Drawing.ParticleSystems;
using Juixel.Drawing.Tiles;
using Clans.Drawing.GameObjects;
using Clans.Data;
using Utilities.JMath;
using Microsoft.Xna.Framework.Graphics;
using Clans.Networking;
using Utilities.Logging;
using Juixel.Interaction;

namespace Clans.Scenes
{
    public class MainScene : Scene, IKeyHandler
    {
        #region Init

        private const double Pixels_Per_Second = 10 * 40;

        private Player Player;
        private TileManager Tiles;
        private SnowEffect Snow;
        private LabelNode Info;

        private List<Player> Players = new List<Player>();

        private Client Client;

        public MainScene(Location Size) : base(Size)
        {
            ConnectionClient Connection = new ConnectionClient();
            Connection.Connect((Port) =>
            {
                if (Port != 0)
                {
                    Logger.Log("Connecting on Port " + Port);
                    Client = new Client(Port);
                }
                else
                    Logger.Log("Failed to connect to remote host");
                Connection.Dispose();
            });

            Player = new Player();
            Player.Scale = 10;
            Player.Layer = 10;
            AddChild(Player);
            Players.Add(Player);

            Tiles = new TileManager(2048, 2048, ClansGame.TestTileTexture, 8);
            Tiles.Scale = 5;
            
            for (int Y = 0; Y < Tiles.Height; Y++)
                for (int X = 0; X < Tiles.Width; X++)
                    Tiles.Set(X, Y, 3 + JRandom.Next(3), 0, 1, 0);

            Tiles.Layer = -1;
            AddChild(Tiles);

            Camera.Target = Player;

            Snow = new SnowEffect(2);
            Snow.Intensity = 0;
            UI.AddChild(Snow);

            Info = new LabelNode(Font.Default, "WASD to move + and - to change the bleeding", 20);
            Info.Position = new Location(Size.X / 2, Size.Y * 0.3);
            Info.AnchorPoint.X = 0.5;
            Info.Color = new Color(128, 128, 130);
            UI.AddChild(Info);

            Input.ListenForKey(Keys.I, this);
            Input.ListenForKey(Keys.O, this);
            Input.ListenForKey(Keys.P, this);
        }

        #endregion

        #region Methods

        private Tile LastTile;
        private double TargetIntensity = 0;

        private const double Weather_Change_Per_Second = 2;
        private const int Weather_Detect_Radius = 12;

        public override void Update(JuixelTime Time)
        {
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
                Tiles.Focus = (Player.Position / 40).Int;
            }

            base.Update(Time);

            if (Input.KeyIsDown(Keys.Y))
            {
                var Player = new Player();
                Player.Scale = 10;
                Player.Position = Location.Random * 1000;
                Player.Layer = Player.Position.Y;
                AddChild(Player);
                Players.Add(Player);

                Player = new Player();
                Player.Scale = 10;
                Player.Position = Location.Random * 1000;
                Player.Layer = Player.Position.Y;
                AddChild(Player);
                Players.Add(Player);
            }

            
            if (Input.KeyIsDown(Keys.OemPlus))
                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].Health += 0.01;
                }

            if (Input.KeyIsDown(Keys.OemMinus))
                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].Health -= 0.01;
                }


            IntLocation TilePosition = (Player.Position / Tiles.Scale / 8).IntFloor;
            Tile CurrentTile = Tiles.Get(TilePosition.X, TilePosition.Y);

            int TileCount = 0;
            int WeatherCount = 0;

            if (CurrentTile != LastTile)
            {
                for (int Y = -Weather_Detect_Radius; Y <= Weather_Detect_Radius; Y++)
                    for (int X = -Weather_Detect_Radius; X <= Weather_Detect_Radius; X++)
                    {
                        Tile Tile = Tiles.Get(TilePosition.X + X, TilePosition.Y + Y);
                        if (Tile != null)
                        {
                            WeatherCount += Tile.WeatherIntensity;
                            TileCount++;
                        }
                    }
                LastTile = CurrentTile;
                TargetIntensity = (WeatherCount / (double)TileCount) * 4;
            }

            if (Snow.Intensity != TargetIntensity)
            {
                double Difference = TargetIntensity - Snow.Intensity;
                double Change = Weather_Change_Per_Second * Time.ElapsedSec;
                if (Math.Abs(Difference) < Change)
                    Snow.Intensity = TargetIntensity;
                else
                    Snow.Intensity += Change * (Difference / Math.Abs(Difference));
            }

            JuixelGame.ShowDebugText(NodeCount.ToString(), 2);

            if (Client != null)
                Info.Text = Client.Latency.ToString();
        }

        private ushort WeaponIndex = 0;

        public void KeyDown(Keys Key)
        {
            switch (Key)
            {
                case Keys.O:
                    if (Player.EquippedArmor.FileIndex == 0)
                        Player.EquippedArmor = new ItemData()
                        {
                            FileIndex = 2
                        };
                    else
                        Player.EquippedArmor = new ItemData()
                        {
                            FileIndex = 0
                        };
                    break;
                case Keys.P:
                    WeaponIndex = (ushort)((WeaponIndex + 1) % 3);
                    Player.EquippedWeapon = new ItemData()
                    {
                        FileIndex = WeaponIndex
                    };
                    break;
            }
        }

        public void KeyUp(Keys Key)
        {
            
        }

        #endregion
    }
}
