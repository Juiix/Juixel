using Juixel.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Juixel;
using Utilities.Logging;
using Microsoft.Xna.Framework.Input;
using Juixel.Interaction;

namespace WorldBuilder
{
    public class Map : Node, IMouseHandler, IKeyHandler
    {
        private const int Chunk_Size = 32;

        private const double Min_Zoom = 0.25;
        private const double Max_Zoom = 64;

        public override Location Size { get => new Location(Width, Height) * Scale; set { } }

        protected override bool Updates => true;

        public MapAction CurrentAction = MapAction.Move;

        public string Details
        {
            get
            {
                switch (CurrentAction)
                {
                    case MapAction.Draw:
                        return "BrushSize: " + BrushSize;
                }
                return "";
            }
        }

        public IntLocation ViewSize;

        public int Width;
        public int Height;

        private Chunk[,] Chunks;

        public Map(int Width, int Height, IntLocation ViewSize, ushort FillType)
        {
            this.ViewSize = ViewSize;
            Scale = Min_Zoom;

            int XChunks = (int)Math.Ceiling((double)Width / Chunk_Size);
            int YChunks = (int)Math.Ceiling((double)Height / Chunk_Size);

            this.Width = XChunks * Chunk_Size;
            this.Height = YChunks * Chunk_Size;

            Chunks = new Chunk[XChunks, YChunks];
            for (int y = 0; y < YChunks; y++)
                for (int x = 0; x < XChunks; x++)
                    Chunks[x, y] = new Chunk(Chunk_Size, new IntLocation(x * Chunk_Size, y * Chunk_Size), FillType);

            Interactive = true;
            MakeBrush();

            Input.ListenForKey(Keys.M, this);
            Input.ListenForKey(Keys.D, this);

            Input.ListenForKey(Keys.OemPlus, this);
            Input.ListenForKey(Keys.OemMinus, this);
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            Position += this.Position;
            Scale *= this.Scale;
            Location Start = Position * -1;
            Location BlockSize = new Location(Chunk_Size * Scale.X, Chunk_Size * Scale.Y);
            for (double y = Start.Y; y < Start.Y + ViewSize.Y + BlockSize.Y; y += BlockSize.Y)
                for (double x = Start.X; x < Start.X + ViewSize.X + BlockSize.X; x += BlockSize.X)
                {
                    Chunk Chunk = ChunkFromPosition((int)(x / Scale.X), (int)(y / Scale.Y));
                    if (Chunk != null)
                        Chunk.Draw(Time, SpriteBatch, new Location(Position.X, Position.Y), 0, Scale, Alpha * this.Alpha);
                }
        }

        private Chunk ChunkFromPosition(int X, int Y)
        {
            if (X < 0 || Y < 0)
                return null;

            X = X / Chunk_Size;
            Y = Y / Chunk_Size;

            if (X >= 0 && X < Width / Chunk_Size &&
                Y >= 0 && Y < Height / Chunk_Size)
                return Chunks[X, Y];
            return null;
        }

        public override void OnSelectDown(int Id, Location Location)
        {
            switch (CurrentAction)
            {
                case MapAction.Move:
                    MoveActionStart(Location);
                    break;
                case MapAction.Draw:
                    DrawActionStart(Location);
                    break;
            }
        }

        public override void OnSelectMoved(int Id, Location Location)
        {
            switch (CurrentAction)
            {
                case MapAction.Move:
                    MoveActionDrag(Location);
                    break;
                case MapAction.Draw:
                    DrawActionDrag(Location);
                    break;
            }
        }

        public override bool HitTest(Location Location)
        {
            return true;
        }

        #region Map Actions

        private void MapActionPlus()
        {
            switch (CurrentAction)
            {
                case MapAction.Draw:
                    BrushSize += 1;
                    MakeBrush();
                    if (BrushSize > 20)
                        BrushSize = 20;
                    break;
            }
        }

        private void MapActionMinus()
        {
            switch (CurrentAction)
            {
                case MapAction.Draw:
                    BrushSize -= 1;
                    MakeBrush();
                    if (BrushSize < 1)
                        BrushSize = 1;
                    break;
            }
        }

        private Location LastLocation;

        private void MoveActionStart(Location Location)
        {
            LastLocation = Location;
        }

        private void MoveActionDrag(Location Location)
        {
            Position += Location - LastLocation;
            LastLocation = Location;
        }

        private int BrushSize = 4;
        private IntLocation[] Brush;
        private ushort DrawType = 0;

        private void MakeBrush()
        {
            List<IntLocation> Brush = new List<IntLocation>();
            int Offset = BrushSize / 2;
            for (double y = 0; y < BrushSize; y += 1)
                for (double x = 0; x < BrushSize; x += 1)
                    if (new Location(x - Offset, y - Offset).Length < (double)BrushSize / 2 || BrushSize <= 4)
                        Brush.Add(new IntLocation((int)x - Offset, (int)y - Offset));
            this.Brush = Brush.ToArray();
        }

        private void DrawActionStart(Location Location)
        {
            Draw((int)Location.X, (int)Location.Y);
        }

        private void DrawActionDrag(Location Location)
        {
            Draw((int)Location.X, (int)Location.Y);
        }

        private void Draw(int GlobalX, int GlobalY)
        {
            int X = (int)((GlobalX - Position.X) / Scale.X);
            int Y = (int)((GlobalY - Position.Y) / Scale.Y);

            for (int i = 0; i < Brush.Length; i++)
            {
                IntLocation Offset = Brush[i];
                SetTile(X + Offset.X, Y + Offset.Y, new Tile(DrawType));
            }

            /*for (int y = -Radius; y <= Radius; y++)
                for (int x = -Radius; x <= Radius; x++)
                {
                    SetTile(X + x, Y + y, new Tile(0, 0));
                }*/
        }

        private void SetTile(int X, int Y, Tile Tile)
        {
            Chunk Chunk = ChunkFromPosition(X, Y);
            if (Chunk != null)
            {
                Chunk.Set(X % Chunk_Size, Y % Chunk_Size, Tile);
                Chunk.UpdateTexture();
            }
        }

        #endregion

        #region Mouse Handler

        protected override void OnAddScene()
        {
            base.OnAddScene();

            Input.ListenForMouse(this);
        }

        protected override void OnRemoveScene(Scene RemovedScene)
        {
            base.OnRemoveScene(RemovedScene);

            Input.RemoveMouseListener(this);
        }

        public void MouseDown(int Id, Location Location)
        {
            
        }

        public void MouseMoved(int Id, Location Location)
        {
            
        }

        public void MouseUp(int Id, Location Location)
        {
            
        }

        public void MouseScroll(int Amount)
        {
            if (Input.MouseOnScreen)
            {
                Location LastScale = Scale;

                double Scalar = Math.Pow(2, Amount / Math.Abs(Amount));
                Scale *= Scalar;

                if (Scale.X > Max_Zoom)
                    Scale.X = Max_Zoom;
                if (Scale.X < Min_Zoom)
                    Scale.X = Min_Zoom;

                if (Scale.Y > Max_Zoom)
                    Scale.Y = Max_Zoom;
                if (Scale.Y < Min_Zoom)
                    Scale.Y = Min_Zoom;

                if (Scale != LastScale)
                {
                    CenterMap(Input.MouseX, Input.MouseY, Scalar);
                    CenterMap(Input.MouseX, Input.MouseY, 1);
                }
            }
        }

        private void CenterMap(int X, int Y, double Scalar)
        {
            Location Offset = new Location(X, Y) - Position;
            Position = Offset * Scalar + new Location(X, Y);

        }

        #endregion

        #region Key Handler

        public void KeyDown(Keys Key)
        {
            switch (Key)
            {
                case Keys.M:
                    CurrentAction = MapAction.Move;
                    break;
                case Keys.D:
                    CurrentAction = MapAction.Draw;
                    break;
                case Keys.OemPlus:
                    MapActionPlus();
                    break;
                case Keys.OemMinus:
                    MapActionMinus();
                    break;
            }
        }

        public void KeyUp(Keys Key)
        {
            
        }

        #endregion
    }
}
