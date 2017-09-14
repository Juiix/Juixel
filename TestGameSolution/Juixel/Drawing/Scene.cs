using Juixel.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Threading;

namespace Juixel.Drawing
{
    public delegate void OnResize(double X, double Y);
    public class Scene : Node
    {
        #region Properties

        /// <summary>
        /// On Window Resize
        /// </summary>
        public event OnResize OnResize;

        public override Location Size
        {
            get => base.Size;
            set { base.Size = value; OnResize?.Invoke(Size.X, Size.Y); }
        }

        /// <summary>
        /// The camera of the scene, used to direct the focus of the scene
        /// </summary>
        public Camera Camera = new Camera();

        /// <summary>
        /// The UI layer, add a <see cref="Node"/> to display UI
        /// </summary>
        public Node UI = new Node();

        /// <summary>
        /// All updating Nodes
        /// </summary>
        private List<IUpdatable> UpdatingNodes = new List<IUpdatable>();

        #endregion

        #region Init

        public Scene(Location Size)
        {
            this.Size = Size;
            UI.Layer = int.MaxValue;
            UI.Parent = this;
            UI.Scene = this;
        }

        #endregion

        #region Methods

        public virtual void Update(JuixelTime Time)
        {
            for (int i = 0; i < UpdatingNodes.Count; i++)
                UpdatingNodes[i].Update(Time);
        }

        public virtual void Draw(JuixelTime Time, SpriteBatch SpriteBatch)
        {
            if (ChildrenNeedSort)
            {
                SortChildren();
                ChildrenNeedSort = false;
            }

            for (int i = 0; i < Children.Count; i++)
                Children[i].Draw(Time, SpriteBatch, Camera.DrawOffset, 0, Location.One, 1);
            UI.Draw(Time, SpriteBatch, UI.Position, 0, Location.One, 1);
        }

        /// <summary>
        /// Adds an <see cref="IUpdatable"/> to the scene
        /// </summary>
        /// <param name="System">The <see cref="IUpdatable"/> to add</param>
        public void AddUpdatable(IUpdatable Updatable)
        {
            UpdatingNodes.Add(Updatable);
        }

        /// <summary>
        /// Removes an <see cref="IUpdatable"/> from the scene
        /// </summary>
        /// <param name="System">The <see cref="IUpdatable"/> to remove</param>
        public void RemoveParticleSystem(IUpdatable Updatable)
        {
            UpdatingNodes.Remove(Updatable);
        }

        #endregion

        #region Interaction



        #endregion
    }
}
