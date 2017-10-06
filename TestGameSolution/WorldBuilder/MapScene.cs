using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Juixel;
using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using Juixel.Drawing.Interaction;
using WorldBuilderLib;
using WorldBuilderLib.IO;

namespace WorldBuilder
{
    public class MapScene : Scene
    {
        private LabelNode Zoom;
        private LabelNode Action;
        private Map Map;

        private Button BlockUp;
        private Button BlockDown;
        private Button BlockLeft;
        private Button BlockRight;

        public LabelNode Loading;

        #region Tools

        private SpriteNode DrawToolPreview;

        #endregion

        public MapScene(MapDataLoader Loader, TileData[] Tiles, Location Size) : base(Size)
        {
            Loading = new LabelNode(Font.Default, "Loading...", 30);
            Loading.Position = Size / 2;
            Loading.AnchorPoint = 0.5;
            AddChild(Loading);

            Map = new Map(Loader, new IntLocation((int)Size.X, (int)Size.Y), Tiles);
            Map.OnBlockLoad += StopLoading;
            Map.Position = Size / 2 - Map.Size / 2;
            AddChild(Map);

            OnResize += OnWindowResize;

            Zoom = new LabelNode(Font.Default, "", 20);
            Zoom.Color = Color.LightGray;
            Zoom.Position = new Location(100, 10);
            UI.AddChild(Zoom);

            Action = new LabelNode(Font.Default, "", 20);
            Action.Color = Color.LightGray;
            Action.Position = new Location(100, 30);
            UI.AddChild(Action);

            SpriteNode InfoBack = new SpriteNode(TextureLibrary.Square);
            InfoBack.Size = new Location(220, 55);
            InfoBack.Layer = -1;
            InfoBack.Color = Color.Black;
            InfoBack.Alpha = 0.4f;
            InfoBack.Position = Zoom.Position - 5;
            UI.AddChild(InfoBack);

            DrawToolPreview = new SpriteNode(new Sprite(WBGame.TileSheet, GetDrawPreviewRect()))
            {
                AnchorPoint = new Location(1, 0),
                Size = 32,
                Position = new Location(Size.X - 10, 10)
            };
            UI.AddChild(DrawToolPreview);

            Button BackButton = new Button(WBGame.ButtonSprite, "Back", 15, () =>
            {
                JuixelGame.Shared.ChangeScene(new SelectionScene(Size));
            });
            BackButton.Position = new Location(10 + BackButton.Size.X / 2, 10 + BackButton.Size.Y / 2);
            UI.AddChild(BackButton);

            BlockUp = new Button(WBGame.ButtonSprite, "Block Up", 15, () =>
            {
                if (Loading.Hidden)
                {
                    StartLoading();
                    Map.SaveTempBlock();
                    Map.LoadBlock(Map.BlockX, Map.BlockY + 1);
                }
            });
            BlockUp.Position = new Location(Size.X / 2, 30);
            UI.AddChild(BlockUp);

            BlockDown = new Button(WBGame.ButtonSprite, "Block Down", 15, () =>
            {
                if (Loading.Hidden)
                {
                    StartLoading();
                    Map.SaveTempBlock();
                    Map.LoadBlock(Map.BlockX, Map.BlockY - 1);
                }
            });
            BlockDown.Position = new Location(Size.X / 2, Size.Y - 30);
            UI.AddChild(BlockDown);

            BlockLeft = new Button(WBGame.ButtonSprite, "Block Left", 15, () =>
            {
                if (Loading.Hidden)
                {
                    StartLoading();
                    Map.SaveTempBlock();
                    Map.LoadBlock(Map.BlockX - 1, Map.BlockY);
                }
            });
            BlockLeft.Rotation = -90;
            BlockLeft.Position = new Location(30, Size.Y / 2);
            UI.AddChild(BlockLeft);

            BlockRight = new Button(WBGame.ButtonSprite, "Block Right", 15, () =>
            {
                if (Loading.Hidden)
                {
                    StartLoading();
                    Map.SaveTempBlock();
                    Map.LoadBlock(Map.BlockX + 1, Map.BlockY);
                }
            });
            BlockRight.Rotation = 90;
            BlockRight.Position = new Location(Size.X - 30, Size.Y / 2);
            UI.AddChild(BlockRight);

            UpdateBlockButtons();
        }

        private void StopLoading()
        {
            Loading.Hidden = true;
            Map.Hidden = false;
            UpdateBlockButtons();
        }

        private void StartLoading()
        {
            Loading.Hidden = false;
            Map.Hidden = true;
        }

        private void UpdateBlockButtons()
        {
            if (Map.BlockX != 0)
            {
                BlockLeft.Hidden = false;
                BlockLeft.Title = "Block " + Map.BlockIndex(Map.BlockX - 1, Map.BlockY);
            }
            else
                BlockLeft.Hidden = true;

            if (Map.BlockX != Map.MapLoader.BlockWidth - 1)
            {
                BlockRight.Hidden = false;
                BlockRight.Title = "Block " + Map.BlockIndex(Map.BlockX + 1, Map.BlockY);
            }
            else
                BlockRight.Hidden = true;

            if (Map.BlockY != 0)
            {
                BlockDown.Hidden = false;
                BlockDown.Title = "Block " + Map.BlockIndex(Map.BlockX, Map.BlockY - 1);
            }
            else
                BlockDown.Hidden = true;

            if (Map.BlockY != Map.MapLoader.BlockHeight - 1)
            {
                BlockUp.Hidden = false;
                BlockUp.Title = "Block " + Map.BlockIndex(Map.BlockX, Map.BlockY + 1);
            }
            else
                BlockUp.Hidden = true;
        }

        private Rectangle GetDrawPreviewRect()
        {
            int Index = Map.DrawTile.Indexes[0];
            int XLength = WBGame.TileSheet.Width / 8;
            return new Rectangle(Index % XLength * 8, Index / XLength * 8, 8, 8);
        }

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            Zoom.Text = $"Block {Map.BlockIndex(Map.BlockX, Map.BlockY)}: x{Map.Scale.X}";
            Action.Text = Map.CurrentAction.ToString() + " " + Map.Details;

            DrawToolPreview.Sprite.Source = GetDrawPreviewRect();
        }

        private void OnWindowResize(double X, double Y)
        {
            Map.ViewSize = new IntLocation((int)X, (int)Y);
        }
    }
}
