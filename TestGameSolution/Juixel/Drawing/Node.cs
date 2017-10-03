using Juixel.Drawing.Actions;
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
using Utilities.JMath;
using Utilities.Logging;
using Utilities.Threading;

namespace Juixel.Drawing
{
    public delegate void InteractionAction(Node Node);
    public delegate void InteractionActionRelease(Node Node, bool Contains);

    public class Node : IUpdatable
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
        /// Determines if this <see cref="Node"/> can be interactive with by multiple interactions. Example: multiple touches
        /// </summary>
        public bool AllowsMultiInput = false;

        /// <summary>
        /// The current id of the interaction interacting with this <see cref="Node"/>. <see langword="-1"/> equals no interaction
        /// </summary>
        public int CurrentInterationId = -1;

        /// <summary>
        /// Determines if <see cref="Update(JuixelTime)"/> will be called each update
        /// </summary>
        protected virtual bool Updates => false;

        /// <summary>
        /// The point within the <see cref="Node"/> that it will center on. 0.0 - 1.0 values.
        /// (0, 0) will be the top left corner
        /// </summary>
        public Location AnchorPoint;

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
            RemoveAllActions();

            /*Alpha = 1;
            Position = Location.Zero;
            Scale = Location.One;
            CurrentAddCount = 0;
            Color = Color.White;
            Layer = 0;*/
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

            if (Updates || Actions.Count > 0)
                Scene.AddUpdatable(this);
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

            if (Updates || Actions.Count > 0)
                RemovedScene.RemoveUpdatable(this);
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

        public event InteractionAction SelectDown;

        public event InteractionActionRelease SelectMoved;

        public event InteractionActionRelease SelectUp;

        public event InteractionAction HoverEnter;

        public event InteractionAction HoverMoved;

        public event InteractionAction HoverOut;

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

        public Node InteractiveAtLocation(Location Location)
        {
            if (Hidden) return null;
            Node Node = null;
            if (Children.Count > 0)
                for (int i = Children.Count - 1; i >= 0; i--)
                {
                    Node = Children[i].InteractiveAtLocation(Geometry.RotateAroundOrigin(new Angle(-Rotation.Radians), (Location - Position) / Scale));
                    if (Node != null)
                        return Node;
                }
            if (HitTest(Location) && Interactive && (CurrentInterationId == -1 || AllowsMultiInput))
                Node = this;
            return Node;
        }

        public virtual bool HitTest(Location Location)
        {
            Location = Geometry.RotateAroundOrigin(new Angle(-Rotation.Radians), Location);
            Location Size = this.Size;
            Location Position = this.Position - Size * AnchorPoint;
            return Location.X >= Position.X && Location.X < Position.X + Size.X &&
                Location.Y >= Position.Y && Location.Y < Position.Y + Size.Y;
        }

        /// <summary>
        /// Called when a touch or click down occurs on this <see cref="Node"/>
        /// </summary>
        /// <param name="Id">The id of the interaction</param>
        /// <param name="Location">The <see cref="Location"/> of the interaction</param>
        public virtual void OnSelectDown(int Id, Location Location)
        {
            SelectDown?.Invoke(this);
        }

        /// <summary>
        /// Called when a touch or click moves that has previously called <see cref="OnSelectDown(int, Location)"/>
        /// </summary>
        /// <param name="Id">The id of the interaction</param>
        /// <param name="Location">The <see cref="Location"/> of the interaction</param>
        public virtual void OnSelectMoved(int Id, Location Location)
        {
            SelectMoved?.Invoke(this, HitTest(Scene.FromScene(Location, Parent)));
        }

        /// <summary>
        /// Called when a touch of click is released that has previously called <see cref="OnSelectDown(int, Location)"/>
        /// </summary>
        /// <param name="Id">The id of the interaction</param>
        /// <param name="Location">The <see cref="Location"/> of the interaction</param>
        public virtual void OnSelectUp(int Id, Location Location)
        {
            SelectUp?.Invoke(this, HitTest(Scene.FromScene(Location, Parent)));
        }

        /// <summary>
        /// Called when the mouse hovers this <see cref="Node"/>
        /// </summary>
        /// <param name="Location">The <see cref="Location"/> of the interaction</param>
        public virtual void OnHoverEnter(Location Location)
        {
            HoverEnter?.Invoke(this);
        }

        /// <summary>
        /// Called when the mouse moves over this <see cref="Node"/>
        /// </summary>
        /// <param name="Location">The <see cref="Location"/> of the interaction</param>
        public virtual void OnHoverMove(Location Location)
        {
            HoverMoved?.Invoke(this);
        }

        /// <summary>
        /// Called when the mouse hovers out of this <see cref="Node"/>
        /// </summary>
        /// <param name="Location">The <see cref="Location"/> of the interaction</param>
        public virtual void OnHoverLeave(Location Location)
        {
            HoverOut?.Invoke(this);
        }

        public virtual void Update(JuixelTime Time)
        {
            if (Actions.Count > 0)
            {
                var Actions = this.Actions.ToArray();
                foreach (var Action in Actions)
                    if (Action.Value.Run(this, Time.ElapsedSec))
                        this.Actions.Remove(Action.Key);
            }
        }

        #endregion

        #region Actions

        private Dictionary<string, JAction> Actions = new Dictionary<string, JAction>();

        public void RunAction(string Key, JAction Action)
        {
            if (Actions.Count == 0 && Scene != null && !Updates)
                Scene.AddUpdatable(this);
            Actions[Key] = Action;
        }

        public void RemoveAction(string Key)
        {
            if (Actions.ContainsKey(Key))
            {
                Actions.Remove(Key);
                if (Actions.Count == 0 && Scene != null && !Updates)
                    Scene.RemoveUpdatable(this);
            }
        }

        public void RemoveAllActions()
        {
            if (Actions.Count > 0)
            {
                Actions.Clear();
                if (Scene != null && !Updates)
                    Scene.RemoveUpdatable(this);
            }
        }

        #endregion
    }
}
