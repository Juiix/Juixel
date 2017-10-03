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

namespace WorldBuilder
{
    public class MapScene : Scene
    {
        private LabelNode Zoom;
        private LabelNode Action;
        private Map Map;

        public MapScene(int Width, int Height, ushort FillType, Location Size) : base(Size)
        {
            /*
            Node Holder = new Node();
            Holder.Position = Size / 2;
            AddChild(Holder);
            */
            Map = new Map(Width, Height, new IntLocation((int)Size.X, (int)Size.Y), FillType);
            Map.Position = Size / 2 - Map.Size / 2;
            AddChild(Map);

            OnResize += OnWindowResize;

            Zoom = new LabelNode(Font.Default, "", 30);
            Zoom.Color = Color.Red;
            Zoom.Position = 10;
            AddChild(Zoom);

            Action = new LabelNode(Font.Default, "", 30);
            Action.Color = Color.Red;
            Action.Position = new Location(10, 40);
            AddChild(Action);
        }

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            Zoom.Text = $"x{Map.Scale.X}";
            Action.Text = Map.CurrentAction.ToString() + " " + Map.Details;
        }

        private void OnWindowResize(double X, double Y)
        {
            Map.ViewSize = new IntLocation((int)X, (int)Y);
        }
    }
}
