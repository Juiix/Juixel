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
using Utilities.Tools;

namespace Clans.Drawing.ParticleSystems
{
    public class DropBloodEffect : ParticleSystem
    {
        protected override bool RemoveWhenEmpty => false;

        private double _DropsPerSecond = 3;
        /// <summary>
        /// How many blood drops spawn per second
        /// </summary>
        public double DropsPerSecond
        {
            get => _DropsPerSecond;
            set
            {
                _DropsPerSecond = value;
                double CD = 1 / DropsPerSecond;
                if (SpawnCD > CD)
                    SpawnCD = CD;
            }
        }

        /// <summary>
        /// Time till next blood drop
        /// </summary>
        public double SpawnCD = 0;

        /// <summary>
        /// The start of the range of values the blood will drop from
        /// </summary>
        private Location SpawnStart;

        /// <summary>
        /// The range of values the blood will drop from
        /// </summary>
        private Location SpawnRange;

        /// <summary>
        /// The current scale of pixels
        /// </summary>
        private double PixelScale;

        public DropBloodEffect(Location SpawnStart, Location SpawnRange, double PixelScale) : base(false)
        {
            this.SpawnStart = SpawnStart;
            this.SpawnRange = SpawnRange;
            this.PixelScale = PixelScale;
        }

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            if (DropsPerSecond < 0)
                DropsPerSecond = 0;
            if (DropsPerSecond > 100)
                DropsPerSecond = 100;

            SpawnCD -= Time.ElapsedSec;
            while (SpawnCD <= 0)
            {
                SpawnCD += 1 / DropsPerSecond;
                AddBloodParticle();
            }
        }

        private void AddBloodParticle()
        {
            //Location Position = new Location(SpawnStart.X + SpawnRange.X * JRandom.NextDouble(), SpawnStart.Y + SpawnRange.Y * JRandom.NextDouble());
            //Position = (Position / PixelScale).Round() * PixelScale; // Snaps the particle to a pixel position defined by the pixel scale
            var Blood = new BloodParticle(new Location(JRandom.Next(10) / 2.0, -4 + JRandom.Next(4) / 2.0))
            {
                Scale = PixelScale
            };
            AddParticle(Blood);
        }

        private class BloodParticle : Particle
        {
            private const double Drop_Acceleration_Acceleration = 16;
            private const double Drop_Initial_Acceleration = 0;
            private const double Drop_Initial_Velocity = 0;

            private double Acceleration;
            private double Velocity;
            private double StartY;

            public BloodParticle(Location Position) : base(TextureLibrary.Square)
            {
                this.Position = Position;
                Color = new Color(181, 37, 37);
                StartY = this.Position.Y;
                Origin = Vector2.Zero;
            }

            public override Location Start()
            {
                Alpha = 0.8f;
                Velocity = Drop_Initial_Velocity;
                Acceleration = Drop_Initial_Acceleration;
                return Position;
            }

            public override Location Update(JuixelTime Time, Location CurrentLocal, out bool Stop)
            {
                Alpha = 0.8f * (float)(Position.Y / StartY);
                Stop = Alpha <= 0;
                Acceleration += Drop_Acceleration_Acceleration * Time.ElapsedSec;
                Velocity += Acceleration * Time.ElapsedSec;
                return new Location(0, Velocity * Time.ElapsedSec);
            }
        }
    }
}
