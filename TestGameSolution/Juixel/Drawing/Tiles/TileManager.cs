using Juixel.Drawing.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Utilities.Math;

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

        public void Set(int X, int Y, int Index)
        {
            Tiles[X, Y] = new Tile((Index % MaxX) * TileSize, (Index / MaxX) * TileSize, TileSize);
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            Location BaseLocation = Position + this.Position;
            Angle BaseRotation = Rotation + this.Rotation;
            Location BaseScale = Scale * this.Scale;
            float BaseAlpha = Alpha * this.Alpha;

            for (int Y = Focus.Y - ViewRadius.Y; Y < Focus.Y + ViewRadius.Y; Y++)
                for (int X = Focus.X - ViewRadius.X; X < Focus.X + ViewRadius.X; X++)
                    if (X >= 0 && Y >= 0 && X < Width && Y < Height)
                    {
                        Tile Tile = Tiles[X, Y];
                        if (Tile != null)
                            Tile.Draw(Texture, Time, SpriteBatch, BaseLocation + new Location(X * TileSize * BaseScale.X, Y * TileSize * BaseScale.Y), BaseRotation, BaseScale, BaseAlpha);
                    }
        }
    }
}
