using Juixel.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Threading;

namespace Juixel.Drawing
{
    public class Node
    {

        #region Properties

        private Location _Position;
        /// <summary>
        /// The position of this <see cref="Node"/>
        /// </summary>
        public Location Position
        {
            get => _Position;
            set
            {
                if (Position != value)
                    OnPositionWillChange(value);
                _Position = value;
            }
        }

        public Location SceneLocation
        {
            get
            {
                if (Parent != null)
                    return Position * Parent.Scale + Parent.SceneLocation;
                else
                    return Location.Zero;
            }
        }

        private Node _Parent;
        /// <summary>
        /// The parent of this <see cref="Node"/>
        /// </summary>
        public Node Parent
        {
            get => _Parent;
            set
            {
                bool Changed = _Parent != value;
                _Parent = value;
                if (Changed)
                {
                    if (_Parent != null)
                        OnParentAdd();
                    else
                        OnParentRemove();
                }
            }
        }

        private Scene _Scene;
        /// <summary>
        /// The containing <see cref="Drawing.Scene"/> of the <see cref="Node"/>
        /// </summary>
        public Scene Scene
        {
            get => _Scene;
            set
            {
                bool Changed = _Scene != value;
                Scene Old = _Scene;
                _Scene = value;
                if (Changed)
                {
                    if (_Scene != null)
                        OnAddScene();
                    else
                        OnRemoveScene(Old);
                }
            }
        }

        /// <summary>
        /// The size of this <see cref="Node"/>
        /// </summary>
        public virtual Location Size { get; set; }

        /// <summary>
        /// The X and Y scale of this <see cref="Node"/> and children
        /// </summary>
        public Location Scale = Location.One;

        /// <summary>
        /// The opacity of this <see cref="Node"/>
        /// </summary>
        public float Alpha = 1;

        /// <summary>
        /// Determines if this <see cref="Node"/> is drawn
        /// </summary>
        public bool Hidden = false;

        /// <summary>
        /// The color shift of this <see cref="Node"/>
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// The current rotation of this <see cref="Node"/> and children
        /// </summary>
        public Angle Rotation = 0;

        /// <summary>
        /// <see langword="True"/> if this <see cref="Node"/> responds to Clicks/Touches. Defaults to <see langword="False"/>
        /// </summary>
        public bool Interactive = false;

        /// <summary>
        /// The current children of this <see cref="Node"/>
        /// </summary>
        public List<Node> Children = new List<Node>();

        /// <summary>
        /// Count when this <see cref="Node"/> was added
        /// </summary>
        public int AddOrder;

        /// <summary>
        /// Current children add count, used to assign <see cref="AddOrder"/>
        /// </summary>
        private int CurrentAddCount;

        private double _Layer = 0;
        /// <summary>
        /// The layer or drawing order of this <see cref="Node"/>
        /// </summary>
        public double Layer
        {
            get => _Layer;
            set
            {
                bool ReSort = _Layer != value;
                _Layer = value;
                if (ReSort && Parent != null)
                    Parent.ChildrenNeedSort = true;
            }
        }

        public virtual int NodeCount
        {
            get
            {
                int Count = Children.Count;
                for (int i = 0; i < Children.Count; i++)
                    Count += Children[i].NodeCount;
                return Count;
            }
        }

        /// <summary>
        /// <see landword="True"/> if the children of this <see cref="Node"/> need to be sorted by layer
        /// </summary>
        public bool ChildrenNeedSort = false;

        #endregion

        #region Initialization

        public virtual void Dispose()
        {
            RemoveFromParent();
            RemoveAllChildren();

            Alpha = 1;
            Position = Location.Zero;
            Scale = Location.One;
            CurrentAddCount = 0;
            Color = Color.White;
            Layer = 0;
        }

        #endregion

        #region Events

        /// <summary>
        /// Called when this <see cref="Node"/> is added to a <see cref="Drawing.Scene"/>
        /// </summary>
        protected virtual void OnAddScene()
        {
            Node[] Cs = Children.ToArray();
            for (int i = 0; i < Cs.Length; i++)
                Cs[i].Scene = Scene;
        }

        /// <summary>
        /// Called when this <see cref="Node"/> is removed from the <see cref="Drawing.Scene"/>
        /// </summary>
        /// <param name="RemovedScene">The <see cref="Scene"/> removed from</param>
        protected virtual void OnRemoveScene(Scene RemovedScene)
        {
            Node[] Cs = Children.ToArray();
            for (int i = 0; i < Cs.Length; i++)
                Cs[i].Scene = Scene;
        }

        /// <summary>
        /// Called when the <see cref="Parent"/> is set
        /// </summary>
        protected virtual void OnParentAdd()
        {

        }

        /// <summary>
        /// Called when the <see cref="Parent"/> is removed
        /// </summary>
        protected virtual void OnParentRemove()
        {

        }

        /// <summary>
        /// Called when <see cref="Position"/> is changed
        /// </summary>
        /// <param name="To">The new position of this <see cref="Node"/></param>
        protected virtual void OnPositionWillChange(Location To)
        {

        }

        #endregion

        #region Children

        /// <summary>
        /// Removes all children of this <see cref="Node"/>
        /// </summary>
        public void RemoveAllChildren()
        {
            Node[] Cs = Children.ToArray();
            for (int i = 0; i < Cs.Length; i++)
                RemoveChild(Cs[i]);
        }

        /// <summary>
        /// Adds a child to this <see cref="Node"/>
        /// </summary>
        /// <param name="Child">The <see cref="Node"/> to add</param>
        public void RemoveChild(Node Child)
        {
            if (Child.Parent == this)
            {
                Child.Parent = null;
                Child.Scene = null;
                Children.Remove(Child);
                ChildrenNeedSort = true;
            }
        }

        /// <summary>
        /// Removes this <see cref="Node"/> from its parent (If it has one)
        /// </summary>
        public void RemoveFromParent()
        {
            if (Parent != null)
                Parent.RemoveChild(this);
        }

        /// <summary>
        /// Adds a child this <see cref="Node"/>
        /// </summary>
        /// <param name="Child">The <see cref="Node"/> to add as a child</param>
        public virtual void AddChild(Node Child)
        {
            if (Child.Parent == null)
            {
                Child.Parent = this;
                if (this is Scene)
                    Child.Scene = (Scene)this;
                else
                    Child.Scene = Scene;
                Child.AddOrder = CurrentAddCount++;
                Children.Add(Child);
                ChildrenNeedSort = true;
            }
        }

        /// <summary>
        /// Sort the children by <see cref="Layer"/> and <see cref="AddOrder"/>
        /// </summary>
        protected virtual void SortChildren()
        {
            Children.Sort((A, B) => A.Layer == B.Layer ? A.AddOrder.CompareTo(B.AddOrder) : A.Layer.CompareTo(B.Layer));
        }

        #endregion

        #region Drawing

        public virtual void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            if (!Hidden)
            {
                if (ChildrenNeedSort)
                {
                    SortChildren();
                    ChildrenNeedSort = false;
                }

                for (int i = 0; i < Children.Count; i++)
                    Children[i].Draw(Time, SpriteBatch, Position + this.Position, Rotation + this.Rotation, Scale * this.Scale, Alpha * this.Alpha);
            }
        }

        #endregion

        #region Interaction



        #endregion
    }
}
