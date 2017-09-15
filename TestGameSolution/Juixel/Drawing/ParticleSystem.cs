using Juixel.Drawing.Textures;
using Juixel.Extensions;
using Juixel.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Math;

namespace Juixel.Drawing
{
    public class ParticleSystem : Node, IUpdatable
    {
        #region Properties

        private bool _UseParentCoordinates;

        protected virtual bool RemoveWhenEmpty => true;

        #endregion

        #region Init

        public ParticleSystem(bool UseParentCoordinates)
        {
            _UseParentCoordinates = UseParentCoordinates;
        }

        public virtual void Emit()
        {

        }

        public virtual void Update(JuixelTime Time)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Particle Particle = (Particle)Children[i];
                bool Remove = false;
                if (_UseParentCoordinates)
                    Particle.Position += Particle.Update(Time, Particle.Position - Position, out Remove);
                else
                    Particle.Position += Particle.Update(Time, Particle.Position, out Remove);

                if (Remove)
                {
                    RemoveParticle(Particle);
                    i--;
                }
            }
        }

        #endregion

        #region Drawing

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            SortChildren();

            if (!_UseParentCoordinates)
                for (int i = 0; i < Children.Count; i++)
                    Children[i].Draw(Time, SpriteBatch, Position + this.Position, Rotation + this.Rotation, Scale * this.Scale, Alpha * this.Alpha);
        }

        protected override void SortChildren()
        {
            Children.Sort((A, B) => A.Position.Y == B.Position.Y ? A.AddOrder.CompareTo(B.AddOrder) : A.Position.Y.CompareTo(B.Position.Y));
        }

        #endregion

        #region Particles

        public void AddParticle(Particle Particle)
        {
            if (Particle.System == null)
            {
                Children.Add(Particle);
                if (_UseParentCoordinates && Parent != null)
                    Parent.AddChild(Particle);

                Particle.System = this;

                Location Pos = Particle.Start();
                if (_UseParentCoordinates)
                    Particle.Position = Position + Pos;
                else
                    Particle.Position = Pos;
            }
        }

        public void RemoveParticle(Particle Particle)
        {
            if (Particle.System == this)
            {
                Children.Remove(Particle);
                if (_UseParentCoordinates && Parent != null)
                    Parent.RemoveChild(Particle);

                Particle.System = null;
                Particle.Dispose();

                if (Children.Count == 0 && RemoveWhenEmpty)
                    Dispose();
            }
        }

        protected override void OnParentAdd()
        {
            if (_UseParentCoordinates)
            {
                for (int i = 0; i < Children.Count; i++)
                    Parent.AddChild(Children[i]);
            }
        }

        protected override void OnParentRemove()
        {
            if (_UseParentCoordinates)
            {
                for (int i = 0; i < Children.Count; i++)
                    Children[i].RemoveFromParent();
            }
        }

        protected override void OnAddScene()
        {
            base.OnAddScene();
            Scene.AddUpdatable(this);
        }

        protected override void OnRemoveScene(Scene RemovedScene)
        {
            base.OnRemoveScene(RemovedScene);
            RemovedScene.AddUpdatable(this);
        }

        #endregion

        public override void AddChild(Node Child)
        {
            throw new InvalidOperationException("Particle Systems cannot have children!"); // Sterile, probably...
        }
    }

    public class Particle : Node
    {
        /// <summary>
        /// The <see cref="Sprite"/> to draw
        /// </summary>
        public Sprite Sprite;

        /// <summary>
        /// <see langword="True"/> if the sprite will be flipped horizontally
        /// </summary>
        public bool Reversed = false;

        /// <summary>
        /// The height of the sprite.
        /// This property will raise the <see cref="Particle"/> without changing it's layer
        /// </summary>
        public double Height = 0;

        /// <summary>
        /// The <see cref="ParticleSystem"/> that contains this <see cref="Particle"/>
        /// </summary>
        public ParticleSystem System;

        /// <summary>
        /// Initializes this <see cref="Particle"/> with a <see cref="Sprite"/>
        /// </summary>
        /// <param name="Sprite">The <see cref="Sprite"/> to display</param>
        public Particle(Sprite Sprite)
        {
            this.Sprite = Sprite;
        }

        public virtual Location Start()
        {
            return 0;
        }

        public virtual Location Update(JuixelTime Time, Location CurrentLocal, out bool Stop)
        {
            Stop = true;
            return 0;
        }

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            Location DrawPosition = Position + this.Position * Scale - new Location(0, Height * Scale.Y);
            Scale *= this.Scale;
            SpriteBatch.Draw(Sprite.Texture, DrawPosition.ToVector2(), Sprite.Source, Color * (Alpha * this.Alpha), (float)(Rotation.Radians + this.Rotation.Radians),
                new Vector2(Sprite.Source.Width * 0.5f, Sprite.Source.Height * 0.5f), Scale.ToVector2(), Reversed ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            base.Draw(Time, SpriteBatch, Position, Rotation, Scale, Alpha);
        }

        public override void Dispose()
        {
            base.Dispose();

            Sprite = null;
            System = null;
        }
    }
}
