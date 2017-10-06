using Juixel;
using Juixel.Drawing;
using Juixel.Drawing.Actions;
using Juixel.Drawing.Interaction;
using Juixel.Drawing.Textures;
using Juixel.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Net.Tools;
using WorldBuilderLib;
using WorldBuilderLib.IO;

namespace WorldBuilder
{
    public class CreationScene : Scene
    {
        private TextField WidthField;
        private TextField HeightField;
        private Button CreateMapButton;

        private Location CreateMapPosition;

        private SpriteNode LoadingBack;
        private SpriteNode LoadingBar;
        private LabelNode LoadingInfo;

        public CreationScene(TileData[] Tiles, Location Size) : base(Size)
        {
            Button BackButton = new Button(WBGame.ButtonSprite, "Back", 15, () =>
            {
                JuixelGame.Shared.ChangeScene(new SelectionScene(Size));
            });
            BackButton.Position = new Location(10 + BackButton.Size.X / 2, 10 + BackButton.Size.Y / 2);
            UI.AddChild(BackButton);

            char[] NumeralCharFilter = "0123456789".ToCharArray();

            WidthField = new TextField("Map Width", Font.Default, new Location(200, 40), 20)
            {
                Position = new Location(Size.X / 2, Size.Y * 0.2),
                AnchorPoint = 0.5,
                UsePlaceholderAsTitle = true,
                MaxCharacters = 5,
                CharFilter = NumeralCharFilter
            };
            WidthField.OnTextChanged += CheckReady;
            WidthField.Text = GameSettings.GetString("map_width", "");
            WidthField.CustomizeBackground(() => new SpriteNode(TextureLibrary.Square) { Size = new Location(0, 4), AnchorPoint = new Location(0, 1) }, (Node, BackSize) => Node.Size = new Location(BackSize.X, Node.Size.Y));
            AddChild(WidthField);

            HeightField = new TextField("Map Height", Font.Default, new Location(200, 40), 20)
            {
                Position = new Location(Size.X / 2, Size.Y * 0.3),
                AnchorPoint = 0.5,
                UsePlaceholderAsTitle = true,
                MaxCharacters = 5,
                CharFilter = NumeralCharFilter
            };
            HeightField.OnTextChanged += CheckReady;
            HeightField.Text = GameSettings.GetString("map_height", "");
            HeightField.CustomizeBackground(() => new SpriteNode(TextureLibrary.Square) { Size = new Location(0, 4), AnchorPoint = new Location(0, 1) }, (Node, BackSize) => Node.Size = new Location(BackSize.X, Node.Size.Y));
            AddChild(HeightField);

            CreateMapButton = new Button(WBGame.ButtonSprite, "Create Map", 30, () =>
            {
                CheckReady();
                if (_Ready)
                {
                    int Width = int.Parse(WidthField.Text);
                    int Height = int.Parse(HeightField.Text);
                    GameSettings.SetString("map_width", WidthField.Text);
                    GameSettings.SetString("map_height", HeightField.Text);
                    LoadingBack.Hidden = false;
                    CreateMap(Width, Height, Tiles, UpdateLoading);
                }
            })
            {
                Position = new Location(Size.X / 2, Size.Y * 0.6),
                Alpha = 0
            };
            AddChild(CreateMapButton);

            CreateMapPosition = CreateMapButton.Position;

            LoadingBack = new SpriteNode(TextureLibrary.Square);
            LoadingBack.Size = new Location(300, 30);
            LoadingBack.AnchorPoint = 0.5;
            LoadingBack.Color = new Color(24, 24, 24);
            LoadingBack.Position = new Location(Size.X / 2, Size.Y * 0.75);
            LoadingBack.Hidden = true;
            AddChild(LoadingBack);

            LoadingBar = new SpriteNode(TextureLibrary.Square);
            LoadingBar.AnchorPoint = LoadingBack.AnchorPoint;
            LoadingBar.Color = new Color(54, 54, 54);
            LoadingBar.Scale.X = 0;
            LoadingBack.AddChild(LoadingBar);

            LoadingInfo = new LabelNode(Font.Default, "", 16);
            LoadingInfo.AnchorPoint = new Location(0.5, 1);
            LoadingInfo.Position = new Location(LoadingBack.Position.X, LoadingBack.Position.Y - 20);
            AddChild(LoadingInfo);

            _CanReady = true;
            CheckReady();
        }

        private bool _CanReady = false;
        private bool _Ready = false;

        private void CheckReady()
        {
            if (!_CanReady)
                return;

            bool Ready = WidthField.Text != "" && HeightField.Text != "" && WBGame.TileSheet != null;
            if (Ready && !_Ready)
            {
                CreateMapButton.Position = CreateMapPosition;
                CreateMapButton.RunAction("fade", JAction.FadeInFromAngle(90, 30, 0.8, StepType.EaseInSin));
            }
            else if (!Ready && _Ready)
            {
                CreateMapButton.Position = CreateMapPosition;
                CreateMapButton.RunAction("fade", JAction.FadeOutToAngle(90, 30, 0.8, StepType.EaseOutSin));
            }
            _Ready = Ready;
        }

        private void UpdateLoading(double Percent, string Text)
        {
            LoadingBar.Scale.X = Percent;
            LoadingInfo.Text = Text;
        }

        #region Map Creation

        public static string MapTempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorldBuilder\\Temp\\");
        public const string MapTempName = "TempMap.mdes";
        public const string MapDataTempName = "TempMap%.mdata";

        public static void CreateMap(int Width, int Height, TileData[] Tiles, Action<double ,string> UpdateLoading, ushort[,] DefaultTiles = null)
        {
            UpdateLoading(0.1, $"Creating Map Descriptor...");

            DispatchQueue.DispatchIO(() =>
            {
                if (!Directory.Exists(MapTempPath))
                    Directory.CreateDirectory(MapTempPath);

                MapDescriptorFile MapFile = new MapDescriptorFile()
                {
                    BlockWidth = (uint)Math.Ceiling((double)Width / MapDescriptorFile.Block_Size),
                    BlockHeight = (uint)Math.Ceiling((double)Height / MapDescriptorFile.Block_Size)
                };

                int BlocksDone = 0;
                int Blocks = (int)MapFile.BlockHeight * (int)MapFile.BlockWidth;

                MapFile.Blocks = new string[MapFile.BlockWidth, MapFile.BlockHeight];
                for (int y = 0; y < MapFile.BlockHeight; y++)
                    for (int x = 0; x < MapFile.BlockWidth; x++)
                    {
                        UpdateLoading(0.1 + 0.5 * (((double)BlocksDone + 1) / Blocks), $"Creating Block {BlocksDone + 1} / {Blocks}...");
                        int StartX = x * MapDescriptorFile.Block_Size;
                        int StartY = y * MapDescriptorFile.Block_Size;
                        BlockFile Block = new BlockFile()
                        {
                            Width = (ushort)((Width - StartX < MapDescriptorFile.Block_Size) ? Width - StartX : MapDescriptorFile.Block_Size),
                            Height = (ushort)((Height - StartY < MapDescriptorFile.Block_Size) ? Height - StartY : MapDescriptorFile.Block_Size)
                        };
                        Block.Tiles = new ushort[Block.Width, Block.Height];
                        for (int yy = 0; yy < Block.Height; yy++)
                            for (int xx = 0; xx < Block.Width; xx++)
                            {
                                if (DefaultTiles == null)
                                    Block.Tiles[xx, yy] = Tiles[0].Type;
                                else
                                    Block.Tiles[xx, yy] = DefaultTiles[StartX + xx, StartY + yy];
                            }
                        string FileName = MapDataTempName.Replace("%", $"_{y * MapFile.BlockWidth + x}");
                        MapFile.Blocks[x, y] = FileName;
                        using (MemoryStream Stream = new MemoryStream())
                        {
                            NetworkWriter w = new NetworkWriter(Stream);
                            Block.Write(w);
                            File.WriteAllBytes(Path.Combine(MapTempPath, FileName), Stream.ToArray());
                        }

                        BlocksDone++;
                    }

                UpdateLoading(0.7, $"Saving Map Descriptor...");

                using (MemoryStream Stream = new MemoryStream())
                {
                    NetworkWriter w = new NetworkWriter(Stream);
                    MapFile.Write(w);
                    File.WriteAllBytes(Path.Combine(MapTempPath, MapTempName), Stream.ToArray());
                }

                UpdateLoading(0.8, $"Rasterizing First Block...");

                DispatchQueue.DispatchMain(() =>
                {
                    JuixelGame.Shared.ChangeScene(new MapScene(new MapDataLoader(Path.Combine(MapTempPath, MapTempName)),
                        Tiles, JuixelGame.Shared.CurrentScene.Size));
                });
            });
        }

        #endregion
    }
}
