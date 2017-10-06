using Juixel;
using Juixel.Drawing;
using Juixel.Drawing.Interaction;
using Juixel.Drawing.Textures;
using Juixel.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Logging;
using WorldBuilderLib;

namespace WorldBuilder
{
    public class LoadScene : Scene
    {
        private bool HasImporter = false;
        private IImporter Importer;
        private Button ImportButton;
        private TextField ImporterName;
        private TileData[] Tiles;

        private Button LoadButton;

        private SpriteNode LoadingBack;
        private SpriteNode LoadingBar;
        private LabelNode LoadingInfo;

        public LoadScene(TileData[] Tiles, Location Size) : base(Size)
        {
            Button BackButton = new Button(WBGame.ButtonSprite, "Back", 15, () =>
            {
                JuixelGame.Shared.ChangeScene(new SelectionScene(Size));
            });
            BackButton.Position = new Location(10 + BackButton.Size.X / 2, 10 + BackButton.Size.Y / 2);
            UI.AddChild(BackButton);

            this.Tiles = Tiles;
            ImporterName = new TextField("Importer", Font.Default, new Location(200, 40), 20)
            {
                Position = new Location(Size.X * 0.3, Size.Y * 0.4),
                AnchorPoint = 0.5,
                UsePlaceholderAsTitle = true,
                Interactive = false
            };
            ImporterName.CustomizeBackground(() => new SpriteNode(TextureLibrary.Square) { Size = new Location(0, 4), AnchorPoint = new Location(0, 1) }, (Node, BackSize) => Node.Size = new Location(BackSize.X, Node.Size.Y));
            AddChild(ImporterName);

            ImportButton = new Button(WBGame.ButtonSprite, "Select Importer", 20, ImportPressed);
            ImportButton.Position = new Location(Size.X * 0.3, Size.Y * 0.4 + ImporterName.Size.Y + 20);
            AddChild(ImportButton);

            LoadButton = new Button(WBGame.ButtonSprite, "Load Map", 20, LoadPressed);
            LoadButton.Position = new Location(Size.X * 0.7, ImportButton.Position.Y);
            AddChild(LoadButton);

            SpriteNode Divider = new SpriteNode(TextureLibrary.Square);
            Divider.Size = new Location(6, 600);
            Divider.AnchorPoint = 0.5;
            Divider.Position = Size / 2;
            AddChild(Divider);

            string DefImporter = GameSettings.GetString("default_importer", "");
            if (DefImporter != "" && File.Exists(DefImporter))
                LoadImporterFromFile(DefImporter);

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
        }

        private void LoadPressed()
        {
            if (Importing)
                return;

            Importing = true;
            System.Windows.Forms.OpenFileDialog Open = new System.Windows.Forms.OpenFileDialog();
            Open.Filter = "Map Descriptor|*.mdes";
            Open.Multiselect = false;
            if (Open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MapDataLoader Loader = new MapDataLoader(Open.FileName);
                for (int y = 0; y < Loader.BlockHeight; y++)
                    for (int x = 0; x < Loader.BlockWidth; x++)
                    {
                        string Block = Loader.MapFile.Blocks[x, y];
                        File.Copy(Path.Combine(Loader.FileDirectory, Block), Path.Combine(CreationScene.MapTempPath, Block));
                    }
                string Descriptor = Loader.FileBaseName + ".mdes";
                File.Copy(Path.Combine(Loader.FileDirectory, Descriptor), Path.Combine(CreationScene.MapTempPath, Descriptor));
                Loader.FileDirectory = CreationScene.MapTempPath;
                JuixelGame.Shared.ChangeScene(new MapScene(Loader, Tiles, JuixelGame.Shared.CurrentScene.Size));
            }
        }

        private void LoadImporter()
        {
            System.Windows.Forms.OpenFileDialog Open = new System.Windows.Forms.OpenFileDialog();
            Open.Filter = "Class Library|*.dll";
            Open.Multiselect = false;
            if (Open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadImporterFromFile(Open.FileName);
        }

        private void LoadImporterFromFile(string FileName)
        {
            var As = Assembly.LoadFile(FileName);
            ImporterName.Text = Path.GetFileName(FileName);
            GameSettings.SetString("default_importer", FileName);
            var CheckType = typeof(IImporter);
            var Definers = As.GetTypes().Where(_ => CheckType.IsAssignableFrom(_)).ToArray();
            if (Definers.Length > 0)
            {
                Importer = (IImporter)Activator.CreateInstance(Definers[0]);
                ImportButton.Title = "Import";
                HasImporter = true;
            }
        }

        private bool Importing = false;

        private void ImportPressed()
        {
            if (Importing)
                return;

            if (!HasImporter)
                LoadImporter();
            else
            {
                Importing = true;
                System.Windows.Forms.OpenFileDialog Open = new System.Windows.Forms.OpenFileDialog();
                Open.Filter = Importer.GetFileFilter();
                Open.Multiselect = false;
                if (Open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Importer.LoadFile(File.OpenRead(Open.FileName));
                    LoadingBack.Hidden = false;
                    CreationScene.CreateMap(Importer.GetWidth(), Importer.GetHeight(), Tiles, UpdateLoading, Importer.GetTiles());
                }
            }
        }

        private void UpdateLoading(double Percent, string Text)
        {
            LoadingBar.Scale.X = Percent;
            LoadingInfo.Text = Text;
        }
    }
}
