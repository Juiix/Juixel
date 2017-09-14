using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing.Textures;
using Utilities;
using Juixel;

namespace Clans.Drawing.ParticleSystems
{
    public class TestParticleSystem : ParticleSystem
    {
        private const int Particle_Count = 1;
        private Sprite Sprite;

        public TestParticleSystem(Sprite Sprite) : base(true)
        {
            this.Sprite = Sprite;
        }

        public override void Emit()
        {
            for (int i = 0; i < Particle_Count; i++)
                AddParticle(new TestParticle(Sprite));
        }

        public override void Dispose()
        {
            base.Dispose();

            Sprite = null;
        }

        #region Particle

        private class TestParticle : Particle
        {
            public TestParticle(Sprite Sprite) : base(Sprite)
            {
                
            }

            public override Location Start()
            {
                Scale = 0.2;
                Height = 15;
                return (Location.Random - 0.5) * new Location(30, 20);
            }

            public override Location Update(JuixelTime Time, Location CurrentLocal, out bool Stop)
            {
                Alpha -= (float)Time.ElapsedSec;
                Height -= Time.ElapsedSec * 10;
                Stop = Alpha <= 0;
                Layer = Position.Y;
                return 0;
            }
        }

        #endregion
    }
}
