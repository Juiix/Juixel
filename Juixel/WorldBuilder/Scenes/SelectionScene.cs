using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing.Interaction;
using Utilities;
using System.Reflection;
using WorldBuilderLib;
using Juixel.Drawing.Actions;
using System.IO;
using Juixel.Tools;
using Juixel;
using Utilities.Logging;
using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework.Graphics;

namespace WorldBuilder
{
    public class SelectionScene : Scene
    {
        private TileData[] Tiles;

        private Button LoadDescriptor;
        private Button LoadButton;
        private Button CreateButton;

        public SelectionScene(Location Size) : base(Size)
        {
            LoadDescriptor = new Button(WBGame.ButtonSprite, "Custom Descriptor", 20, LoadDefiner);
            LoadDescriptor.Position = new Location(Size.X / 2, Size.Y * 0.25);
            AddChild(LoadDescriptor);

            SpriteNode AssetDisplay = new SpriteNode(TextureLibrary.Square);
            AssetDisplay.Hidden = true;
            AssetDisplay.AnchorPoint.Y = 0.5;
            AddChild(AssetDisplay);

            Button AssetButton = new Button(WBGame.ButtonSprite, "Load Tile Asset", 20, () =>
            {
                System.Windows.Forms.OpenFileDialog Open = new System.Windows.Forms.OpenFileDialog();
                Open.Filter = "PNG Image|*.png";
                Open.Multiselect = false;
                if (Open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    WBGame.TileSheet = Texture2D.FromStream(JuixelGame.Shared.GraphicsDevice, File.OpenRead(Open.FileName));
                    AssetDisplay.Sprite = WBGame.TileSheet;
                    AssetDisplay.Hidden = false;
                    AssetDisplay.Size = 100;
                    GameSettings.SetString("map_image", Open.FileName);
                    ShowButtons();
                }
            })
            {
                Position = new Location(Size.X / 2, Size.Y * 0.4),
            };
            AddChild(AssetButton);
            AssetDisplay.Position = AssetButton.Position + new Location(150, 0);

            string AssetFile = GameSettings.GetString("map_image", "");
            if (AssetFile != "" && File.Exists(AssetFile))
            {
                WBGame.TileSheet = Texture2D.FromStream(JuixelGame.Shared.GraphicsDevice, File.OpenRead(AssetFile));
                AssetDisplay.Sprite = WBGame.TileSheet;
                AssetDisplay.Hidden = false;
                AssetDisplay.Size = 100;
            }

            LoadButton = new Button(WBGame.ButtonSprite, "Load", 20, () =>
            {
                JuixelGame.Shared.ChangeScene(new LoadScene(Tiles, Size));
            });
            LoadButton.Position = new Location(Size.X * 0.3, Size.Y * 0.6);
            LoadButton.Hidden = true;
            LoadButton.Alpha = 0;
            AddChild(LoadButton);

            CreateButton = new Button(WBGame.ButtonSprite, "Create", 20, () =>
            {
                JuixelGame.Shared.ChangeScene(new CreationScene(Tiles, Size));
            });
            CreateButton.Position = new Location(Size.X * 0.7, Size.Y * 0.6);
            CreateButton.Hidden = true;
            CreateButton.Alpha = 0;
            AddChild(CreateButton);

            string DefDefiner = GameSettings.GetString("default_definer", "");
            if (DefDefiner != "" && File.Exists(DefDefiner))
                LoadDefinerFromFile(DefDefiner);
        }

        private void LoadDefiner()
        {
            System.Windows.Forms.OpenFileDialog Open = new System.Windows.Forms.OpenFileDialog();
            Open.Filter = "Class Library|*.dll";
            Open.Multiselect = false;
            if (Open.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadDefinerFromFile(Open.FileName);
            }
        }

        private void LoadDefinerFromFile(string FileName)
        {
            var As = Assembly.LoadFile(FileName);
            LoadDescriptor.Title = Path.GetFileName(FileName);
            GameSettings.SetString("default_definer", FileName);
            var CheckType = typeof(IDefiner);
            var Definers = As.GetTypes().Where(_ => CheckType.IsAssignableFrom(_)).ToArray();
            if (Definers.Length > 0)
            {
                IDefiner Definer = (IDefiner)Activator.CreateInstance(Definers[0]);
                Tiles = Definer.GetTileDefintions();
                ShowButtons();
            }
        }

        private void ShowButtons()
        {
            if (Tiles != null && WBGame.TileSheet != null && LoadButton.Hidden)
            {
                LoadButton.Hidden = false;
                CreateButton.Hidden = false;

                LoadButton.RunAction("enter", JAction.FadeInFromAngle(90, 30, 1, StepType.EaseInSin));
                CreateButton.RunAction("enter", JAction.FadeInFromAngle(90, 30, 1, StepType.EaseInSin));
            }
        }
    }
}
