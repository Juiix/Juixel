using Juixel;
using Juixel.Drawing;
using Juixel.Drawing.Textures;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Logging;
using Utilities.Tools;

namespace Clans.Drawing.ParticleSystems
{
    public class SnowEffect : ParticleSystem
    {
        private Sprite[] Sprites;

        private double _Intensity = 0;
        /// <summary>
        /// The intensity of the snow. 1.0 = 10 Particles per second. Changing this affects speed and count
        /// </summary>
        public double Intensity
        {
            get => _Intensity;
            set
            {
                if (value > 0 || _Intensity != 0)
                    Enabled = true;
                _Intensity = value;
                double CD = 1 / (Math.Pow(Intensity, 2) * 20);
                if (SpawnCD > CD)
                    SpawnCD = CD;
            }
        }

        /// <summary>
        /// The current spawn cooldown in Seconds
        /// </summary>
        public double SpawnCD = 0;

        private Color TintColor = new Color(198, 215, 226);

        private bool Enabled = true;

        public SnowEffect(double Intensity) : base(false)
        {
            this.Intensity = Intensity;

            Sprites = new Sprite[] 
            {
                new Sprite(ClansGame.Particles_8x8, new Rectangle(0, 0, 8, 8)),
                new Sprite(ClansGame.Particles_8x8, new Rectangle(8, 0, 8, 8))
            };
        }

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            if (Intensity < 0)
                Intensity = 0;

            if (Enabled)
            {
                JuixelGame.TintColor = TintColor;
                JuixelGame.TintIntensity = 0.1f + (float)(Math.Log10(Intensity) / Math.E) * 0.9f;

                if (_Intensity == 0)
                    Enabled = false;
            }

            SpawnCD -= Time.ElapsedSec;
            while (SpawnCD <= 0)
            {
                double ParticlesPerSecond = Math.Pow(Intensity, 2) * 20;
                SpawnCD += 1 / ParticlesPerSecond;

                AddSnowParticle();
            }
        }

        private void AddSnowParticle()
        {
            Sprite Sprite = Sprites[JRandom.Next(Sprites.Length)];
            double Speed = 120 * Intensity * JRandom.Next(0.7, 1.3);
            AddParticle(new SnowParticle(new Angle((Math.PI / 2) * JRandom.NextDouble() + Math.PI / 4).Location * Speed, new Location(JRandom.Next(JuixelGame.WindowWidth), -Sprite.Source.Height),
                1 + Math.Log10(Intensity / Math.E), JuixelGame.WindowHeight + Sprite.Source.Height, Sprite));
        }

        public override void Dispose()
        {
            base.Dispose();

            Sprites = null;
        }

        #region Particle

        private class SnowParticle : Particle
        {
            private Location Speed;
            private double MaxY;

            public SnowParticle(Location Speed, Location Position, double Size, double MaxY, Sprite Sprite) : base(Sprite)
            {
                this.Speed = Speed;
                this.Position = Position;
                this.MaxY = MaxY;
                Scale = Size;
            }

            public override Location Start()
            {
                Scale *= JRandom.Next(0.6, 1.0);
                Alpha = (float)JRandom.Next(0.8, 1);
                return Position;
            }

            public override Location Update(JuixelTime Time, Location CurrentLocal, out bool Stop)
            {
                Stop = Position.Y > MaxY;
                return Speed * Time.ElapsedSec;
            }
        }

        private class TintParticle : Particle
        {
            public TintParticle(Color Color) : base(TextureLibrary.Square)
            {
                this.Color = Color;

                Scale = new Location(JuixelGame.WindowWidth + 2, JuixelGame.WindowHeight + 2);
            }

            public override Location Start()
            {
                return Scale / 2;
            }

            public override Location Update(JuixelTime Time, Location CurrentLocal, out bool Stop)
            {
                Stop = false;
                return 0;
            }
        }

        #endregion
    }
}
