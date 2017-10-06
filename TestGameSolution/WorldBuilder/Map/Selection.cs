using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel;
using Microsoft.Xna.Framework.Graphics;
using Utilities;
using Microsoft.Xna.Framework;
using Juixel.Extensions;
using Juixel.Interaction;
using WorldBuilderLib;

namespace WorldBuilder
{
    public class Selection : Node
    {
        private const int Render_Tiles_Zoom = 4;
        private const int Tile_Size = 8;

        private Texture2D Texture;
        private Color[] TextureData;
        private Tile?[,] Tiles;

        private int TileCount;

        private Rectangle Bounds;

        public Selection()
        {
            Alpha = 0.4f;
        }

        public void UpdateSelection(IntLocation[] Brush, TileData Data)
        {
            if (Texture != null)
            {
                Texture.Dispose();
            }

            Bounds = GetBounds(Brush);
            Tiles = new Tile?[Bounds.Width, Bounds.Height];

            Texture = new Texture2D(JuixelGame.Shared.GraphicsDevice, Bounds.Width, Bounds.Height);
            TextureData = new Color[Bounds.Width * Bounds.Height];

            foreach (var Point in Brush)
            {
                var Tile = new Tile(Data);
                Tiles[Point.X - Bounds.X, Point.Y - Bounds.Y] = Tile;
                TextureData[(Point.Y - Bounds.Y) * Bounds.Width + Point.X - Bounds.X] = Tile.MainColor;
            }

            Texture.SetData(TextureData);
        }

        private Rectangle GetBounds(IntLocation[] Points)
        {
            int LowX = int.MaxValue;
            int HighX = int.MinValue;
            int LowY = int.MaxValue;
            int HighY = int.MinValue;

            foreach (var Point in Points)
            {
                LowX = Math.Min(Point.X, LowX);
                HighX = Math.Max(Point.X, HighX);
                LowY = Math.Min(Point.Y, LowY);
                HighY = Math.Max(Point.Y, HighY);
            }

            return new Rectangle(LowX, LowY, HighX - LowX + 1, HighY - LowY + 1);
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            Location Location = Scene.FromScene(new Location(Input.MouseX, Input.MouseY), this).Floor() + new Location(Bounds.X, Bounds.Y);
            Position = new Location(Position.X + Location.X * Scale.X, Position.Y + Location.Y * Scale.Y);
            Scale *= this.Scale;
            if (Scale.X < Render_Tiles_Zoom)
                SpriteBatch.Draw(Texture, Position.ToVector2(), null, Color * this.Alpha, 0, Vector2.Zero, Scale.ToVector2(), SpriteEffects.None, 0);
            else
                for (int y = 0; y < Bounds.Height; y++)
                    for (int x = 0; x < Bounds.Width; x++)
                    {
                        var NTile = Tiles[x, y];
                        if (NTile != null)
                        {
                            var Tile = NTile.Value;
                            SpriteBatch.Draw(Tile.Sprite.Texture, new Location(Position.X + x * Scale.X, Position.Y + y * Scale.Y).ToVector2(), Tile.Sprite.Source, Color * this.Alpha, 0, Vector2.Zero, (Scale / 8).ToVector2(), SpriteEffects.None, 0);
                        }
                    }
        }
    }
}
