using Juixel;
using Juixel.Drawing;
using Juixel.Drawing.Textures;
using Juixel.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Tools;

namespace WorldBuilder
{
    public class Chunk : Node
    {
        private const int Render_Tiles_Zoom = 4;
        private const int Tile_Size = 8;

        private Texture2D Texture;
        private Color[] TextureData;

        public IntLocation Location;

        private Tile?[,] Tiles;
        public int[,] RTicks;

        private int TileCount;

        public Chunk(int Size, IntLocation Location)
        {
            this.Location = Location;
            Texture = new Texture2D(JuixelGame.Shared.GraphicsDevice, Size, Size);
            TextureData = new Color[Size * Size];

            TileCount = Size;
            Tiles = new Tile?[TileCount, TileCount];
            RTicks = new int[TileCount, TileCount];

            Texture.SetData(TextureData);
        }

        public override void Dispose()
        {
            base.Dispose();

            Texture.Dispose();
        }

        public Tile? Get(int X, int Y) => Tiles[X, Y];

        public void Set(int X, int Y, Tile Tile, int Tick)
        {
            RTicks[X, Y] = Tick;
            Tiles[X, Y] = Tile;
            TextureData[Y * TileCount + X] = Tile.MainColor;
        }

        public void UpdateTexture()
        {
            Texture.SetData(TextureData);
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            if (Scale.X < Render_Tiles_Zoom)
            {
                Position = new Location(Position.X + Location.X * Scale.X, Position.Y + Location.Y * Scale.Y);
                Scale *= this.Scale;
                SpriteBatch.Draw(Texture, Position.ToVector2(), null, Color, 0, Vector2.Zero, Scale.ToVector2(), SpriteEffects.None, 0);
            }
            else
            {
                Position = new Location(Position.X + Location.X * Scale.X, Position.Y + Location.Y * Scale.Y);
                Scale *= this.Scale;
                for (int y = 0; y < TileCount; y++)
                    for (int x = 0; x < TileCount; x++)
                    {
                        var NTile = Tiles[x, y];
                        if (NTile != null)
                        {
                            var Tile = NTile.Value;
                            SpriteBatch.Draw(Tile.Sprite.Texture, new Location(Position.X + x * Scale.X, Position.Y + y * Scale.Y).ToVector2(), Tile.Sprite.Source, Color, 0, Vector2.Zero, (Scale / 8).ToVector2(), SpriteEffects.None, 0);
                        }
                    }
            }
        }
    }
}
