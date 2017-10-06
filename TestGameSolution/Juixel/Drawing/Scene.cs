using Juixel.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;
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

        public override void Update(JuixelTime Time)
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
        public void RemoveUpdatable(IUpdatable Updatable)
        {
            UpdatingNodes.Remove(Updatable);
        }

        #endregion

        #region Interaction

        private Dictionary<int, Node> CurrentInterations = new Dictionary<int, Node>();
        private Node CurrentHover;

        public override void OnSelectDown(int Id, Location Location)
        {
            Node N = UI.InteractiveAtLocation(Location);
            if (N == null)
                N = InteractiveAtLocation(Location);
            if (N != null)
            {
                CurrentInterations[Id] = N;
                N.CurrentInterationId = Id;
                N.OnSelectDown(Id, Location);
            }
        }

        public override void OnSelectMoved(int Id, Location Location)
        {
            if (CurrentInterations.ContainsKey(Id))
                CurrentInterations[Id].OnSelectMoved(Id, Location);
            else
            {
                Node N = UI.InteractiveAtLocation(Location);
                if (N == null)
                    N = InteractiveAtLocation(Location);
                if (N != CurrentHover)
                {
                    if (CurrentHover != null)
                        CurrentHover.OnHoverLeave(Location);
                    CurrentHover = N;
                    if (N != null)
                        N.OnHoverEnter(Location);
                }
                else if (CurrentHover != null)
                    CurrentHover.OnHoverMove(Location);
            }
        }

        public override void OnSelectUp(int Id, Location Location)
        {
            if (CurrentInterations.ContainsKey(Id))
            {
                Node N = CurrentInterations[Id];
                N.OnSelectUp(Id, Location);
                CurrentInterations.Remove(Id);
                N.CurrentInterationId = -1;
            }
        }

        #endregion

        #region Point Manipulation

        /// <summary>
        /// Converts a point from a given Node's coordinates to a another Node's coordinates.
        /// </summary>
        /// <param name="Point">The point to convert</param>
        /// <param name="From">Coordinate space to convert from</param>
        /// <param name="To">Coordinate space to convert to</param>
        /// <returns></returns>
        public Location Convert(Location Point, Node From, Node To)
        {
            // Get the scene position of the Point
            Node Parent = From;
            while (Parent != null)
            {
                Point = Parent.Position + Geometry.RotateAroundOrigin(Parent.Rotation, Point * Parent.Scale);
                Parent = From.Parent;
            }

            return FromScene(Point, To); // Return the point from the scene
        }

        /// <summary>
        /// Return a point manipulated into the To Node's coordinate space from Scene coordinates
        /// </summary>
        /// <param name="Point">The point to convert</param>
        /// <param name="To">Coordinate space to convert to</param>
        /// <returns></returns>
        public Location FromScene(Location Point, Node To)
        {
            // Get the To Node's parent tree
            List<Node> Parents = new List<Node>();
            Node Parent = To;
            while (Parent != null)
            {
                Parents.Add(Parent);
                Parent = Parent.Parent;
            }

            // Convert point down the parent tree
            for (int i = Parents.Count - 1; i >= 0; i--)
            {
                Parent = Parents[i];
                Point = Geometry.RotateAroundOrigin(new Angle(-Parent.Rotation.Radians), (Point - Parent.Position) / Parent.Scale);
            }

            return Point;
        }

        #endregion
    }
}
