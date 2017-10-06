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
using Juixel.Drawing.Textures;
using Utilities.Tools;
using System.IO;
using Utilities.Net.Tools;
using Juixel.Tools;
using Juixel.Drawing.Actions;
using Juixel.Drawing.Interaction;
using WorldBuilderLib;
using WorldBuilderLib.IO;

namespace WorldBuilder
{
    public class Map : Node, IMouseHandler, IKeyHandler
    {
        private const int Chunk_Size = 32;

        private const double Min_Zoom = 0.25;
        private const double Max_Zoom = 64;

        private const int Max_Brush_Size = 300;

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
                    case MapAction.Random:
                        return "Chance: " + (CurrentRandom + 1);
                }
                return "";
            }
        }

        public event Action OnBlockLoad;

        public MapDataLoader MapLoader;
        private Selection Selection;
        public IntLocation ViewSize;

        public int ChunkLength;
        public int Width;
        public int Height;

        private Chunk[,] Chunks;
        private TileData[] Tiles;
        private Dictionary<ushort, TileData> TileDatas;

        public Map(MapDataLoader MapLoader, IntLocation ViewSize, TileData[] Tiles)
        {
            this.MapLoader = MapLoader;
            this.Tiles = Tiles;
            TileDatas = Tiles.ToDictionary(_ => _.Type);
            this.ViewSize = ViewSize;
            Scale = Min_Zoom;
            DrawTile = Tiles[1];

            ChunkLength = MapDescriptorFile.Block_Size / Chunk_Size;

            Chunks = new Chunk[ChunkLength, ChunkLength];
            for (int y = 0; y < ChunkLength; y++)
                for (int x = 0; x < ChunkLength; x++)
                    Chunks[x, y] = new Chunk(Chunk_Size, new IntLocation(x * Chunk_Size, y * Chunk_Size));

            LoadBlock(0, 0);

            Interactive = true;
            Selection = new Selection();
            AddChild(Selection);
            MakeBrush();

            Input.ListenForKey(Keys.M, this);
            Input.ListenForKey(Keys.D, this);
            Input.ListenForKey(Keys.R, this);
            Input.ListenForKey(Keys.S, this);

            Input.ListenForKey(Keys.OemPlus, this);
            Input.ListenForKey(Keys.OemMinus, this);

            Input.ListenForKey(Keys.OemPeriod, this);
            Input.ListenForKey(Keys.OemComma, this);
        }

        public int BlockIndex(int X, int Y) => Y * MapLoader.BlockWidth + X;

        public int BlockX = 0;
        public int BlockY = 0;

        public void LoadBlock(int X, int Y)
        {
            DispatchQueue.DispatchIO(() =>
            {
                BlockX = X;
                BlockY = Y;

                BlockFile Block = MapLoader.LoadBlock(X, Y);

                DispatchQueue.DispatchMain(() =>
                {
                    Width = Block.Width;
                    Height = Block.Height;

                    for (int y = 0; y < ChunkLength; y++)
                        for (int x = 0; x < ChunkLength; x++)
                        {
                            Chunk Chunk = Chunks[x, y];
                            for (int yy = 0; yy < Chunk_Size; yy++)
                                for (int xx = 0; xx < Chunk_Size; xx++)
                                {
                                    int GlobalX = x * Chunk_Size + xx;
                                    int GlobalY = y * Chunk_Size + yy;
                                    if (GlobalX < Block.Width && GlobalY < Block.Height)
                                        Chunk.Set(xx, yy, new Tile(TileDatas[Block.Tiles[GlobalX, GlobalY]]), 0);
                                }
                            Chunk.UpdateTexture();
                        }

                    Position = new Location(JuixelGame.WindowWidth / 2 - Width * Scale.X / 2, JuixelGame.WindowHeight / 2 - Height * Scale.Y / 2);
                    OnBlockLoad?.Invoke();
                });
            });
        }

        public void SaveTempBlock(Action Callback = null)
        {
            string FileName = Path.Combine(CreationScene.MapTempPath, MapLoader.MapFile.Blocks[BlockX, BlockY]);
            BlockFile Block = new BlockFile()
            {
                Width = (ushort)Width,
                Height = (ushort)Height
            };
            Block.Tiles = new ushort[Width, Height];
            for (int y = 0; y < ChunkLength; y++)
                for (int x = 0; x < ChunkLength; x++)
                {
                    Chunk Chunk = Chunks[x, y];
                    for (int yy = 0; yy < Chunk_Size; yy++)
                        for (int xx = 0; xx < Chunk_Size; xx++)
                        {
                            int GlobalX = x * Chunk_Size + xx;
                            int GlobalY = y * Chunk_Size + yy;
                            if (GlobalX < Block.Width && GlobalY < Block.Height)
                                Block.Tiles[GlobalX, GlobalY] = Chunk.Get(xx, yy).Value.Type;
                        }
                }
            DispatchQueue.DispatchIO(() =>
            {
                using (MemoryStream Stream = new MemoryStream())
                {
                    NetworkWriter w = new NetworkWriter(Stream);
                    Block.Write(w);
                    File.WriteAllBytes(FileName, Stream.ToArray());
                }
                Callback?.Invoke();
            });
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            if (Hidden)
                return;

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
            Selection.Draw(Time, SpriteBatch, new Location(Position.X, Position.Y), 0, Scale, Alpha * this.Alpha);
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
            ActionTick++;
            switch (CurrentAction)
            {
                case MapAction.Move:
                    MoveActionStart(Location);
                    break;
                case MapAction.Draw:
                    DrawActionStart(Location);
                    break;
                case MapAction.Random:
                    RandomActionStart(Location);
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
                case MapAction.Random:
                    RandomActionDrag(Location);
                    break;
            }
        }

        public override bool HitTest(Location Location)
        {
            return true;
        }

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            var Pairs = HeldKeys.ToArray();
            foreach (var Pair in Pairs)
            {
                var Info = Pair.Value;
                if (Pair.Value.Count > 5)
                {
                    Info.Call();
                    Info.Call();
                }
                else
                    Info.Count++;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            for (int y = 0; y < Height / Chunk_Size; y++)
                for (int x = 0; x < Width / Chunk_Size; x++)
                    Chunks[x, y].Dispose();
            Chunks = null;
        }

        #region Map Actions

        private int ActionTick = 0;

        private void MapActionPlus()
        {
            switch (CurrentAction)
            {
                case MapAction.Draw:
                    BrushSize++;
                    if (BrushSize > Max_Brush_Size)
                        BrushSize = Max_Brush_Size;
                    MakeBrush();
                    break;
                case MapAction.Random:
                    CurrentRandom++;
                    if (CurrentRandom > 99)
                        CurrentRandom = 99;
                    break;
            }
        }

        private void MapActionMinus()
        {
            switch (CurrentAction)
            {
                case MapAction.Draw:
                    BrushSize--;
                    if (BrushSize < 1)
                        BrushSize = 1;
                    MakeBrush();
                    break;
                case MapAction.Random:
                    CurrentRandom--;
                    if (CurrentRandom < 0)
                        CurrentRandom = 0;
                    break;
            }
        }

        #region Moving

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

        #endregion

        #region Drawing

        private int BrushSize = 4;
        private IntLocation[] Brush;
        public TileData DrawTile;

        private void MakeBrush()
        {
            List<IntLocation> Brush = new List<IntLocation>();
            int Offset = BrushSize / 2;
            for (double y = 0; y < BrushSize; y += 1)
                for (double x = 0; x < BrushSize; x += 1)
                    if (new Location(x - Offset, y - Offset).Length < (double)BrushSize / 2 || BrushSize <= 4)
                        Brush.Add(new IntLocation((int)x - Offset, (int)y - Offset));
            this.Brush = Brush.ToArray();
            Selection.UpdateSelection(this.Brush, DrawTile);
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
                SetTile(X + Offset.X, Y + Offset.Y, new Tile(DrawTile));
            }

            Chunk[] Chunks = AffectedChunks.ToArray();
            AffectedChunks = new HashSet<Chunk>();
            for (int i = 0; i < Chunks.Length; i++)
                Chunks[i].UpdateTexture();
        }

        private HashSet<Chunk> AffectedChunks = new HashSet<Chunk>();

        private void SetTile(int X, int Y, Tile Tile)
        {
            Chunk Chunk = ChunkFromPosition(X, Y);
            int TX = X % Chunk_Size;
            int TY = Y % Chunk_Size;
            if (Chunk != null && Chunk.RTicks[TX, TY] != ActionTick)
            {
                Chunk.Set(TX, TY, Tile, ActionTick);
                AffectedChunks.Add(Chunk);
            }
        }

        private void VoidTile(int X, int Y)
        {
            Chunk Chunk = ChunkFromPosition(X, Y);
            int TX = X % Chunk_Size;
            int TY = Y % Chunk_Size;
            if (Chunk != null && Chunk.RTicks[TX, TY] != ActionTick)
                Chunk.RTicks[TX, TY] = ActionTick;
        }

        private void SetDrawTile(TileData Data)
        {
            DrawTile = Data;
            Selection.UpdateSelection(Brush, DrawTile);
            ActionTick++;
        }

        #endregion

        #region Random
        
        private int CurrentRandom = 19;

        private void RandomActionStart(Location Location)
        {
            DrawRandom((int)Location.X, (int)Location.Y);
        }

        private void RandomActionDrag(Location Location)
        {
            DrawRandom((int)Location.X, (int)Location.Y);
        }

        private void DrawRandom(int GlobalX, int GlobalY)
        {
            int X = (int)((GlobalX - Position.X) / Scale.X);
            int Y = (int)((GlobalY - Position.Y) / Scale.Y);

            for (int i = 0; i < Brush.Length; i++)
            {
                IntLocation Offset = Brush[i];
                if (JRandom.Next(100) <= CurrentRandom)
                    SetTile(X + Offset.X, Y + Offset.Y, new Tile(DrawTile));
                else
                    VoidTile(X + Offset.X, Y + Offset.Y);
            }

            Chunk[] Chunks = AffectedChunks.ToArray();
            AffectedChunks = new HashSet<Chunk>();
            for (int i = 0; i < Chunks.Length; i++)
                Chunks[i].UpdateTexture();
        }

        #endregion

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

        public void MouseAltDown(int Id, Location Location)
        {
            if (Scene != null && Scene.InteractiveAtLocation(Location) == this)
            {
                int X = (int)((Location.X - Position.X) / Scale.X);
                int Y = (int)((Location.Y - Position.Y) / Scale.Y);

                Chunk Chunk = ChunkFromPosition(X, Y);
                if (Chunk != null)
                {
                    Tile? T = Chunk.Get(X % Chunk_Size, Y % Chunk_Size);
                    if (T != null)
                        SetDrawTile(TileDatas[T.Value.Type]);
                }
            }
        }

        public void MouseAltUp(int Id, Location Location)
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

        private Dictionary<Keys, HoldInfo> HeldKeys = new Dictionary<Keys, HoldInfo>();

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
                case Keys.R:
                    CurrentAction = MapAction.Random;
                    break;
                case Keys.S:
                    if (Input.KeyIsDown(Keys.LeftControl))
                    {
                        SaveTempBlock(() =>
                        {
                            DispatchQueue.DispatchMain(() =>
                            {
                                System.Windows.Forms.SaveFileDialog Save = new System.Windows.Forms.SaveFileDialog();
                                Save.Filter = "Map Descriptor|*.mdes";
                                if (Save.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    string Directory = Path.GetDirectoryName(Save.FileName);
                                    string BaseName = Path.GetFileNameWithoutExtension(Save.FileName);

                                    MapDescriptorFile NewMapFile = new MapDescriptorFile();
                                    NewMapFile.BlockWidth = MapLoader.MapFile.BlockWidth;
                                    NewMapFile.BlockHeight = MapLoader.MapFile.BlockHeight;
                                    NewMapFile.Blocks = new string[NewMapFile.BlockWidth, NewMapFile.BlockHeight];
                                    for (int y = 0; y < NewMapFile.BlockHeight; y++)
                                        for (int x = 0; x < NewMapFile.BlockWidth; x++)
                                        {
                                            string BlockName = BaseName + "_" + (y * NewMapFile.BlockWidth + x) + ".mdata";
                                            File.Copy(Path.Combine(MapLoader.FileDirectory, MapLoader.MapFile.Blocks[x, y]), Path.Combine(Directory, BlockName), true);
                                            NewMapFile.Blocks[x, y] = BlockName;
                                        }

                                    using (MemoryStream Stream = new MemoryStream())
                                    {
                                        NetworkWriter w = new NetworkWriter(Stream);
                                        NewMapFile.Write(w);
                                        File.WriteAllBytes(Path.Combine(Directory, BaseName + ".mdes"), Stream.ToArray());
                                    }

                                    Node Holder = new Node();
                                    Holder.Position = new Location(Scene.Size.X / 2, Scene.Size.Y * 0.25);
                                    Scene.UI.AddChild(Holder);

                                    LabelNode Label = new LabelNode(Font.Default, "Saved Block!", 20);
                                    Label.AnchorPoint = 0.5;
                                    Holder.AddChild(Label);

                                    SpriteNode Back = new SpriteNode(TextureLibrary.Square);
                                    Back.Size = Label.Size + 10;
                                    Back.AnchorPoint = 0.5;
                                    Back.Color = new Color(24, 24, 24);
                                    Back.Layer = -1;
                                    Holder.AddChild(Back);

                                    Holder.RunAction("m", JAction.Sequence(JAction.MoveByY(-20, 1, StepType.Linear), JAction.FadeOutToAngle(-90, 40, 2, StepType.Linear), JAction.RemoveFromParent(true)));
                                }
                            });
                        });
                    }
                    break;
                case Keys.OemPlus:
                    MapActionPlus();
                    HeldKeys.Add(Key, new HoldInfo { Call = MapActionPlus, Count = 0 });
                    break;
                case Keys.OemMinus:
                    MapActionMinus();
                    HeldKeys.Add(Key, new HoldInfo { Call = MapActionMinus, Count = 0 });
                    break;
                case Keys.OemPeriod:
                    for (int i = 0; i < Tiles.Length; i++)
                        if (Tiles[i].Type == DrawTile.Type)
                        {
                            if (i == Tiles.Length - 1)
                                i = -1;
                            SetDrawTile(Tiles[i + 1]);
                            break;
                        }
                    break;
                case Keys.OemComma:
                    for (int i = 0; i < Tiles.Length; i++)
                        if (Tiles[i].Type == DrawTile.Type)
                        {
                            if (i == 0)
                                i = Tiles.Length;
                            SetDrawTile(Tiles[i - 1]);
                            break;
                        }
                    break;
            }
        }

        public void KeyUp(Keys Key)
        {
            if (HeldKeys.ContainsKey(Key))
                HeldKeys.Remove(Key);
        }

        private class HoldInfo
        {
            public Action Call;
            public int Count;
        }

        #endregion
    }
}
