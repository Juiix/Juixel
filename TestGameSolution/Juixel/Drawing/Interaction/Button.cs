using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing.Textures;
using Utilities;
using Utilities.Logging;
using Microsoft.Xna.Framework;

namespace Juixel.Drawing.Interaction
{
    public class Button : Node
    {

        private SpriteNode SpriteNode;
        private LabelNode Label;

        private bool Selected = false;

        public Button(Sprite Sprite, string Title, double Height, Action OnSelect)
        {
            SpriteNode = new SpriteNode(Sprite);
            SpriteNode.AnchorPoint = 0.5;
            SpriteNode.Interactive = true;
            SpriteNode.CenterRect = new Rectangle(Sprite.Source.Width / 2 - 1, Sprite.Source.Height / 2 - 1, 2, 2);
            SpriteNode.SelectDown += (Node) =>
            {
                SpriteNode.Color = Color.DarkGray;
                Selected = true;
            };
            SpriteNode.SelectUp += (Node, Contains) =>
            {
                SpriteNode.Color = Color.White;
                Selected = false;
                if (Contains)
                    OnSelect?.Invoke();
            };
            SpriteNode.HoverEnter += (Node) =>
            {
                if (!Selected)
                    SpriteNode.Color = Color.LightGray;
            };
            SpriteNode.HoverOut += (Node) =>
            {
                if (!Selected)
                    SpriteNode.Color = Color.White;
            };
            AddChild(SpriteNode);

            Label = new LabelNode(Font.Default, Title, Height);
            Label.Color = Color.Black;
            Label.AnchorPoint = 0.5;
            AddChild(Label);

            Location LabelSize = Label.Size;
            SpriteNode.Size = new Location(LabelSize.X + 80, LabelSize.Y + 40);
            Logger.Log(SpriteNode.Scale);
        }
    }
}
