using Juixel.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Logging;

namespace Juixel.Drawing.Actions
{
    public class JAction
    {
        private Func<Node, double, bool> _RunAction;
        private Action<Node> _ResetAction;
        private double _Time = 0;

        public JAction(Func<Node, double, bool> RunAction, Action<Node> ResetAction = null)
        {
            _RunAction = RunAction;
            _ResetAction = ResetAction;
        }

        public bool Run(Node Node, double ElapsedSec)
        {
            _Time += ElapsedSec;
            return _RunAction.Invoke(Node, _Time);
        }

        private void Reset(Node Node)
        {
            _Time = 0;
            _ResetAction?.Invoke(Node);
        }

        #region Base Actions

        public static JAction Wait(double Duration)
        {
            return new JAction((Node, Step) =>
            {
                return Step >= Duration;
            });
        }

        public static JAction RemoveFromParent(bool DoDispose = false)
        {
            return new JAction((Node, Step) =>
            {
                DispatchQueue.DispatchMain(() =>
                {
                    if (DoDispose)
                        Node.Dispose();
                    else
                        Node.RemoveFromParent();
                });
                return true;
            });
        }

        public static JAction MoveToX(double To, double Duration, StepType Type = StepType.Linear)
        {
            double StartX = 0;
            bool First = true;
            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    StartX = Node.Position.X;
                }
                double Distance = To - StartX;
                double RealStep = ProcessStep(Step / Duration, Type);
                Node.Position = new Location(StartX + Distance * RealStep, Node.Position.Y);
                if (RealStep >= 1)
                {
                    Node.Position = new Location(To, Node.Position.Y);
                    return true;
                }
                return false;
            }, (Node) => { First = true; });
        }

        public static JAction MoveToY(double To, double Duration, StepType Type = StepType.Linear)
        {
            double StartY = 0;
            bool First = true;
            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    StartY = Node.Position.Y;
                }
                double Distance = To - StartY;
                double RealStep = ProcessStep(Step / Duration, Type);
                Node.Position = new Location(Node.Position.X, StartY + Distance * RealStep);
                if (RealStep >= 1)
                {
                    Node.Position = new Location(Node.Position.X, To);
                    return true;
                }
                return false;
            }, (Node) => { First = true; });
        }

        public static JAction MoveTo(Location To, double Duration, StepType Type = StepType.Linear)
        {
            return Group(MoveToX(To.X, Duration, Type), MoveToY(To.Y, Duration, Type));
        }

        public static JAction MoveByX(double To, double Duration, StepType Type = StepType.Linear)
        {
            double StartX = 0;
            bool First = true;
            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    StartX = Node.Position.X;
                }
                double Distance = To;
                double RealStep = ProcessStep(Step / Duration, Type);
                Node.Position = new Location(StartX + Distance * RealStep, Node.Position.Y);
                if (RealStep >= 1)
                {
                    Node.Position = new Location(StartX + To, Node.Position.Y);
                    return true;
                }
                return false;
            }, (Node) => { First = true; });
        }

        public static JAction MoveByY(double To, double Duration, StepType Type = StepType.Linear)
        {
            double StartY = 0;
            bool First = true;
            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    StartY = Node.Position.Y;
                }
                double Distance = To;
                double RealStep = ProcessStep(Step / Duration, Type);
                Node.Position = new Location(Node.Position.X, StartY + Distance * RealStep);
                if (RealStep >= 1)
                {
                    Node.Position = new Location(Node.Position.X, StartY + To);
                    return true;
                }
                return false;
            }, (Node) => { First = true; });
        }

        public static JAction MoveBy(Location To, double Duration, StepType Type = StepType.Linear)
        {
            return Group(MoveByX(To.X, Duration, Type), MoveByY(To.Y, Duration, Type));
        }

        public static JAction Sequence(params JAction[] Actions)
        {
            int Count = 0;
            double Time = 0;
            return new JAction((Node, Step) =>
            {
                double CurrentStep = Step - Time;
                if (Count < Actions.Length)
                    if (Actions[Count].Run(Node, CurrentStep))
                    {
                        Actions[Count].Reset(Node);
                        Count++;
                    }
                Time = Step;
                return Count == Actions.Length;
            }, n =>
            {
                Count = 0;
                Time = 0;
            });
        }

        public static JAction Group(params JAction[] Actions)
        {
            List<int> Done = new List<int>();
            double LastStep = 0;
            return new JAction((Node, Step) =>
            {
                double RealStep = Step - LastStep;
                LastStep = Step;
                for (int i = 0; i < Actions.Length; i++)
                    if (!Done.Contains(i))
                        if (Actions[i].Run(Node, RealStep))
                            Done.Add(i);
                return Done.Count == Actions.Length;
            }, Node =>
            {
                Done = new List<int>();
                LastStep = 0;
                for (int i = 0; i < Actions.Length; i++)
                    Actions[i].Reset(Node);
            });
        }

        public static JAction RepeatForever(JAction Action)
        {
            double Offset = 0;
            return new JAction((Node, Step) =>
            {
                if (Action.Run(Node, Step - Offset))
                    Action.Reset(Node);
                Offset = Step;
                return false;
            }, n =>
            {
                Offset = 0;
            });
        }

        public static JAction FadeIn(double Duration, StepType Type = StepType.Linear)
        {
            return new JAction((Node, Step) =>
            {
                Node.Alpha = (float)ProcessStep(Step / Duration, Type);
                return Node.Alpha >= 1;
            });
        }

        public static JAction FadeOut(double Duration, StepType Type = StepType.Linear)
        {
            return new JAction((Node, Step) =>
            {
                Node.Alpha = (float)ProcessStep(1 - (Step / Duration), Type);
                return Node.Alpha <= 0;
            });
        }

        public static JAction RotateBy(double Angle, double Duration, bool Clockwise)
        {
            double LastTime = 0;
            return new JAction((Node, Step) =>
            {
                double IncStep = Step - LastTime;
                double AnglePerSecond = Angle / Duration;
                LastTime = Step;
                if (Clockwise)
                    Node.Rotation += IncStep * AnglePerSecond;
                else
                    Node.Rotation -= IncStep * AnglePerSecond;
                return Step >= Duration;
            }, n =>
            {
                LastTime = 0;
            });
        }

        public static JAction Flash(Color Color, double Duration)
        {
            float rD = (255 - Color.R) / 255.0f;
            float gD = (255 - Color.G) / 255.0f;
            float bD = (255 - Color.B) / 255.0f;
            return new JAction((Node, Step) =>
            {
                double ScStep = Step / Duration;
                double RStep;
                if (ScStep < 0.5)
                    RStep = Step / (Duration / 2);
                else
                    RStep = 1 - (Step - (Duration / 2)) / (Duration / 2);
                Node.Color = new Color((float)(1 - rD * RStep), (float)(1 - gD * RStep), (float)(1 - bD * RStep));
                return ScStep >= 1;
            });
        }

        public static JAction Run(Action Block)
        {
            return new JAction((Node, Step) =>
            {
                DispatchQueue.DispatchMain(Block);
                return true;
            });
        }

        public static JAction Orbit(double AnglePerSecond, double Duration, bool Clockwise = false)
        {
            AnglePerSecond = Angle.ToRadians(AnglePerSecond);
            double LastAngle = 0;
            double LastStep = 0;
            double Radius = 0;
            bool First = true;

            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    Radius = Node.Position.Length;
                }
                LastAngle = Math.Atan2(Node.Position.Y, Node.Position.X);
                double ScStep = Step / Duration;
                double RStep = Step - LastStep;
                double CurA = LastAngle + (Clockwise ? -AnglePerSecond : AnglePerSecond) * RStep;
                Node.Position = new Location(Math.Cos(CurA) * Radius, Math.Sin(CurA) * Radius);
                LastStep = Step;
                return ScStep >= 1;
            }, n =>
            {
                First = true;
                LastStep = 0;
            });
        }

        public static JAction ScaleToX(double To, double Duration, StepType Type = StepType.Linear)
        {
            double StartScale = 1;
            bool First = true;

            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    StartScale = Node.Scale.X;
                }
                double RStep = ProcessStep(Step / Duration, Type);
                Node.Scale.X = StartScale + (To - StartScale) * RStep;
                return Step >= Duration;
            }, n =>
            {
                First = true;
            });
        }

        public static JAction ScaleToY(double To, double Duration, StepType Type = StepType.Linear)
        {
            double StartScale = 1;
            bool First = true;

            return new JAction((Node, Step) =>
            {
                if (First)
                {
                    First = false;
                    StartScale = Node.Scale.Y;
                }
                double RStep = ProcessStep(Step / Duration, Type);
                Node.Scale.Y = StartScale + (To - StartScale) * RStep;
                return Step >= Duration;
            }, n =>
            {
                First = true;
            });
        }

        public static JAction ScaleTo(double To, double Duration, StepType Type = StepType.Linear)
        {
            return Group(ScaleToX(To, Duration), ScaleToY(To, Duration));
        }

        #endregion

        #region Entrance Actions

        public static JAction FadeInFromAngle(Angle Angle, double Distance, double Duration, StepType Type = StepType.Linear)
        {
            return Group(FadeIn(Duration, Type), new JAction((Node, Step) =>
            {
                Node.Position += Angle.Location * Distance;
                return true;
            }), MoveBy((Angle + 180).Location * Distance, Duration, Type));
        }

        public static JAction FadeOutToAngle(Angle Angle, double Distance, double Duration, StepType Type = StepType.Linear)
        {
            return Group(FadeOut(Duration, Type), MoveBy(Angle.Location * Distance, Duration, Type));
        }

        #endregion

        #region Easing

        public static double ProcessStep(double Step, StepType Type)
        {
            if (Step < 0) Step = 0;
            if (Step > 1) Step = 1;
            switch (Type)
            {
                case StepType.Linear:
                    return Step;
                case StepType.EaseInSin:
                    return Math.Sin(Step * (Math.PI / 2));
                case StepType.EaseOutSin:
                    return Math.Sin(Step * (Math.PI / 2) + (Math.PI * 1.5)) + 1;
                case StepType.EaseInOutSin:
                    return Math.Sin(Step * Math.PI - (Math.PI / 2)) / 2 + 0.5f;
            }
            return Step;
        }

        #endregion
    }

    public enum StepType
    {
        Linear,
        EaseInSin,
        EaseOutSin,
        EaseInOutSin
    }
}
