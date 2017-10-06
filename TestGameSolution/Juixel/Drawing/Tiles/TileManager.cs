using Juixel.Drawing.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Utilities.JMath;
using Juixel.Drawing.Assets;

namespace Juixel.Drawing.Tiles
{
    public class TileManager : Node
    {
        /// <summary>
        /// The tiles ordered in a 2d array
        /// </summary>
        private Tile[,] Tiles;

        /// <summary>
        /// The width of the tile map
        /// </summary>
        public int Width;

        /// <summary>
        /// The height if the tile map
        /// </summary>
        public int Height;

        /// <summary>
        /// The focus of the tiles. Defaults to (0, 0)
        /// </summary>
        public IntLocation Focus = new IntLocation(0, 0);

        /// <summary>
        /// How many tiles are drawn relative to <see cref="Focus"/>. Defaults to (16, 16)
        /// </summary>
        public IntLocation ViewRadius = new IntLocation(16, 16);

        /// <summary>
        /// Determines if tiles will blend into blank areas
        /// </summary>
        public bool BlendToBlank = false;

        /// <summary>
        /// The <see cref="Texture2D"/> containing all tiles
        /// </summary>
        private Texture2D Texture;

        /// <summary>
        /// The width and height of each tile
        /// </summary>
        private int TileSize;

        /// <summary>
        /// How many tiles fit on the X axis of the <see cref="Texture"/>
        /// </summary>
        private int MaxX;

        public TileManager(int Width, int Height, Texture2D Texture, int TileSize)
        {
            this.Texture = Texture;
            this.TileSize = TileSize;
            this.Width = Width;
            this.Height = Height;

            Tiles = new Tile[Width, Height];

            MaxX = Texture.Width / TileSize;
        }

        public void Set(int X, int Y, int Index, int BlendLayer, ushort Type, int WeatherIntensity)
        {
            Tile Tile = new Tile((Index % MaxX) * TileSize, (Index / MaxX) * TileSize, TileSize, Type, WeatherIntensity);
            Tile.BlendLayer = BlendLayer;
            Tiles[X, Y] = Tile;

            for (int YAdd = -1; YAdd < 2; YAdd++)
                for (int XAdd = -1; XAdd < 2; XAdd++)
                {
                    if (YAdd == 0 && XAdd == 0) continue;
                    int XNew = X + XAdd;
                    int YNew = Y + YAdd;
                    if (XNew >= 0 && XNew < Width && YNew >= 0 && YNew < Height)
                    {
                        TileDirection Direction = PointToTileDirection(new IntLocation(XAdd + 1, YAdd + 1));
                        Tile.CompareTo(Tiles[XNew, YNew], (int)Direction, (int)OppositeTileDirection(Direction));
                    }
                }
        }

        public Tile Get(int X, int Y)
        {
            if (X >= 0 && X < Width && Y >= 0 && Y < Height)
                return Tiles[X, Y];
            return null;
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            if (!Hidden)
            {
                CheckSamplerState(SpriteBatch);

                Location BaseLocation = Position + this.Position;
                Location BaseScale = Scale * this.Scale;
                float BaseAlpha = Alpha * this.Alpha;

                for (int Y = Focus.Y - ViewRadius.Y; Y < Focus.Y + ViewRadius.Y; Y++)
                    for (int X = Focus.X - ViewRadius.X; X < Focus.X + ViewRadius.X; X++)
                        if (X >= 0 && Y >= 0 && X < Width && Y < Height)
                        {
                            Tile Tile = Tiles[X, Y];
                            if (Tile != null)
                                Tile.Draw(Texture, Time, SpriteBatch, BaseLocation + new Location(X * TileSize * BaseScale.X, Y * TileSize * BaseScale.Y), BaseScale, BaseAlpha);
                        }

                // Draw blend

                SpriteBatch.End();
                SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, effect: Effects.TileBlendEffect);

                for (int Y = Focus.Y - ViewRadius.Y; Y < Focus.Y + ViewRadius.Y; Y++)
                    for (int X = Focus.X - ViewRadius.X; X < Focus.X + ViewRadius.X; X++)
                        if (X >= 0 && Y >= 0 && X < Width && Y < Height)
                        {
                            Tile Tile = Tiles[X, Y];
                            if (Tile != null)
                                Tile.DrawBlend(Texture, Time, SpriteBatch, BaseLocation + new Location(X * TileSize * BaseScale.X, Y * TileSize * BaseScale.Y), BaseScale, BaseAlpha);
                        }

                SpriteBatch.End();
                SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            }
        }

        public static IntLocation TileDirectionToPoint(TileDirection Direction)
        {
            int Value = (int)Direction;
            return new IntLocation(Value % 3, Value / 3);
        }

        public static TileDirection PointToTileDirection(IntLocation Point) => (TileDirection)(Point.Y * 3 + Point.X);

        public static TileDirection OppositeTileDirection(TileDirection Original) => (TileDirection)(8 - (int)Original);
    }

    public enum TileDirection : byte
    {
        TopLeft,
        TopCenter,
        TopRight,
        MidLeft,
        MidCenter,
        MidRight,
        BotLeft,
        BotCenter,
        BotRight
    }
}
