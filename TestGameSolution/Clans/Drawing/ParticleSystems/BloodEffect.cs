using Juixel;
using Juixel.Drawing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing.Textures;
using Utilities;
using Utilities.Tools;

namespace Clans.Drawing.ParticleSystems
{
    public class BloodEffect : ParticleSystem
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

        private Node Follow;

        /// <summary>
        /// The range of values the blood will drop from
        /// </summary>
        private int SpawnRadius;

        private double PixelScale;

        public BloodEffect(Node Follow, int SpawnRadius, double PixelScale) : base(true)
        {
            this.Follow = Follow;
            this.SpawnRadius = SpawnRadius;
            this.PixelScale = PixelScale;
        }

        public override void Update(JuixelTime Time)
        {
            Position = Follow.Position;

            base.Update(Time);

            if (DropsPerSecond < 0)
                DropsPerSecond = 0;
            if (DropsPerSecond > 4000)
                DropsPerSecond = 4000;

            SpawnCD -= Time.ElapsedSec;
            while (SpawnCD <= 0)
            {
                SpawnCD += 1 / DropsPerSecond;
                AddBloodParticle();
            }
        }

        private void AddBloodParticle()
        {
            var Blood = new BloodParticle(new IntLocation((int)(-SpawnRadius * PixelScale) + (int)(SpawnRadius * 2 * PixelScale * JRandom.NextDouble()), (int)(-SpawnRadius * PixelScale) + (int)(SpawnRadius * 2 * PixelScale * JRandom.NextDouble())))
            {
                Scale = PixelScale
            };
            AddParticle(Blood);
            Blood.Position = (Blood.Position / PixelScale).Floor() * PixelScale - PixelScale / 2;
        }

        private class BloodParticle : Particle
        {
            private double FadeSpeed = 0.5;

            public BloodParticle(IntLocation Position) : base(TextureLibrary.Square)
            {
                this.Position = new Location(Position.X, Position.Y);
                Color = new Color(181, 37, 37);
            }

            public override Location Start()
            {
                Alpha = 0.8f;
                FadeSpeed = JRandom.Next(0.15, 0.45);
                return Position;
            }

            public override Location Update(JuixelTime Time, Location CurrentLocal, out bool Stop)
            {
                Alpha -= (float)(Time.ElapsedSec * FadeSpeed);
                Stop = Alpha <= 0;
                return 0;
            }
        }
    }
}
