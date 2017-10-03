using Juixel;
using Juixel.Drawing;
using Juixel.Drawing.Actions;
using Juixel.Drawing.Interaction;
using Juixel.Drawing.Textures;
using Juixel.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace WorldBuilder
{
    public class LoadingScene : Scene
    {
        private SpriteNode LoadingBar;
        private SpriteNode LoadingBack;

        public LoadingScene(Location Size) : base(Size)
        {
            char[] NumeralCharFilter = "0123456789".ToCharArray();

            TextField WidthField = new TextField("Map Width", Font.Default, new Location(200, 40), 20)
            {
                Position = new Location(Size.X / 2, Size.Y * 0.2),
                AnchorPoint = 0.5
            };
            WidthField.CharFilter = NumeralCharFilter;
            WidthField.CustomizeBackground(() => new SpriteNode(TextureLibrary.Square) { Size = new Location(0, 4), AnchorPoint = new Location(0, 1) }, (Node, BackSize) => Node.Size = new Location(BackSize.X, Node.Size.Y));
            AddChild(WidthField);

            TextField HeightField = new TextField("Map Height", Font.Default, new Location(200, 40), 20)
            {
                Position = new Location(Size.X / 2, Size.Y * 0.25),
                AnchorPoint = 0.5
            };
            HeightField.CharFilter = NumeralCharFilter;
            HeightField.CustomizeBackground(() => new SpriteNode(TextureLibrary.Square) { Size = new Location(0, 4), AnchorPoint = new Location(0, 1) }, (Node, BackSize) => Node.Size = new Location(BackSize.X, Node.Size.Y));
            AddChild(HeightField);

            TextField TypeField = new TextField("Map Fill Type", Font.Default, new Location(200, 40), 20)
            {
                Position = new Location(Size.X / 2, Size.Y * 0.3),
                AnchorPoint = 0.5
            };
            TypeField.CharFilter = NumeralCharFilter;
            TypeField.MaxCharacters = 4;
            TypeField.CustomizeBackground(() => new SpriteNode(TextureLibrary.Square) { Size = new Location(0, 4), AnchorPoint = new Location(0, 1) }, (Node, BackSize) => Node.Size = new Location(BackSize.X, Node.Size.Y));
            AddChild(TypeField);

            Button Button = new Button(WBGame.ButtonSprite, "Create Map", 20, () =>
            {
                int Width = int.Parse(WidthField.Text);
                int Height = int.Parse(HeightField.Text);
                ushort Type = ushort.Parse(TypeField.Text);
                JuixelGame.Shared.ChangeScene(new MapScene(Width, Height, Type, Size));
            })
            {
                Position = Size / 2
            };
            AddChild(Button);

            Button.RunAction("fade", JAction.FadeIn(2));
        }
    }
}
