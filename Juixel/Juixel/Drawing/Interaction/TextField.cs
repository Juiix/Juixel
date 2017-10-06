using Juixel.Drawing.Textures;
using Juixel.Interaction;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Microsoft.Xna.Framework.Input;
using Utilities.Logging;
using Utilities.JMath;
using Microsoft.Xna.Framework.Graphics;
using Juixel.Tools;
using Juixel.Drawing.Actions;

namespace Juixel.Drawing.Interaction
{
    public enum InteractiveState
    {
        Normal,
        Hovered,
        Selected
    }

    public class TextField : Node, IMouseHandler, IKeyHandler, IComparable<TextField>
    {
        private string _Text = "";
        /// <summary>
        /// The text displayed in the field
        /// </summary>
        public string Text
        {
            get => _Text;
            set
            {
                bool Changed = _Text != value;
                _Text = value;

                if (Changed)
                    OnTextChanged?.Invoke();

                if (_Text == "")
                {
                    Label.Text = PlaceholderText;
                    Label.Color = new Color((TextColor.R / 255.0f) * 0.7f, (TextColor.G / 255.0f) * 0.7f, (TextColor.B / 255.0f) * 0.7f);

                    if (UsePlaceholderAsTitle)
                    {
                        Label.Hidden = true;
                        TitleLabel.RunAction("move", JAction.Group(JAction.MoveToY(Label.Position.Y, 0.4, StepType.EaseInSin), JAction.ScaleTo(1, 0.4, StepType.EaseInSin)));
                    }
                }
                else
                {
                    Label.Text = _Text;
                    Label.Color = TextColor;
                    Label.Hidden = false;

                    if (UsePlaceholderAsTitle)
                        TitleLabel.RunAction("move", JAction.Group(JAction.MoveToY(Label.Position.Y - MinSize.Y * 0.6, 0.4, StepType.EaseInSin), JAction.ScaleTo(0.9, 0.4, StepType.EaseInSin)));
                    else
                        TitleLabel.Hidden = true;
                }
            }
        }

        /// <summary>
        /// The text to show when this 
        /// </summary>
        public string PlaceholderText = "";

        private Color _PlaceholderColor;
        private Color _TextColor;
        /// <summary>
        /// The color of the text while the placeholder is active
        /// </summary>
        public Color TextColor
        {
            get => _TextColor;
            set
            {
                _TextColor = value;
                _PlaceholderColor = GetSelected(_TextColor);
            }
        }

        /// <summary>
        /// All allowed input characters. Null = all characters
        /// </summary>
        public char[] CharFilter;

        /// <summary>
        /// The maximum allowed characters. -1 = No limit
        /// </summary>
        public int MaxCharacters = -1;

        /// <summary>
        /// The <see cref="LabelNode"/> used to display text
        /// </summary>
        protected LabelNode Label;

        /// <summary>
        /// <see langword="True"/> if the placeholder will become the title on the addition of text
        /// </summary>
        public bool UsePlaceholderAsTitle = false;

        private LabelNode TitleLabel;

        public override Location Size
        {
            get
            {
                Location LabelSize = Label.Size;
                return new Location(Math.Max(MinSize.X, LabelSize.X), Math.Max(MinSize.Y, LabelSize.Y));
            }
            set { }
        }

        /// <summary>
        /// The minimun size of this textfield
        /// </summary>
        private Location MinSize;

        /// <summary>
        /// The state of this Textfield
        /// </summary>
        private InteractiveState State = InteractiveState.Normal;

        /// <summary>
        /// All keys available for listening
        /// </summary>
        private Keys[] AllKeys = (Keys[])Enum.GetValues(typeof(Keys));

        public Color BackgroundColor = Color.White;

        private Node Background;
        private Action<Node, Location> BackgroundResize;

        public TextField(string Placeholder, Font Font, Location MinSize, double TextSize)
        {
            this.MinSize = MinSize;
            PlaceholderText = Placeholder;
            TextColor = Color.White;

            var Back = new SpriteNode(TextureLibrary.Square);
            Back.Layer = -1;
            AddChild(Back);
            Background = Back;
            BackgroundResize = (Node, Size) => { Node.Size = Size; };

            TitleLabel = new LabelNode(Font, Placeholder, TextSize);
            TitleLabel.Color = _PlaceholderColor;
            AddChild(TitleLabel);

            Label = new LabelNode(Font, Placeholder, TextSize);
            AddChild(Label);
            
            Text = _Text;
            Interactive = true;

            BackgroundColor = new Color(0.12f, 0.12f, 0.12f);
            UpdateState(InteractiveState.Normal);
        }

        public void CustomizeBackground(Func<Node> BackgroundFactory, Action<Node, Location> Resize)
        {
            if (BackgroundFactory != null)
            {
                Background.Dispose();
                Background = BackgroundFactory();
                Background.Layer = -1;
                AddChild(Background);

                BackgroundResize = Resize;
            }
        }

        private Color GetSelected(Color Color)
        {
            float Scalar = ((Color.R + Color.G + Color.B) / 3f >= 127) ? 0.6f : 2f;
            return new Color((Color.R / 255.0f) * Scalar, (Color.G / 255.0f) * Scalar, (Color.B / 255.0f) * Scalar);
        }

        #region Events

        public event Action OnTextChanged;

        #endregion

        #region Drawing

        public override void Draw(JuixelTime Time, SpriteBatch SpriteBatch, Location Position, Angle Rotation, Location Scale, float Alpha)
        {
            var BackSize = new Location(Size.X + 20, Size.Y);
            Background.Position = -1 * BackSize * AnchorPoint + BackSize * Background.AnchorPoint;
            BackgroundResize?.Invoke(Background, BackSize);
            Background.Color = State == InteractiveState.Normal ? BackgroundColor : GetSelected(BackgroundColor);
            Label.AnchorPoint = AnchorPoint;
            TitleLabel.AnchorPoint = AnchorPoint;
            base.Draw(Time, SpriteBatch, Position, Rotation, Scale, Alpha);
        }

        protected virtual void UpdateState(InteractiveState State)
        {
            this.State = State;

            if (_Text == "")
                Label.Color = _PlaceholderColor;
            else
                Label.Color = _TextColor;
        }

        #endregion

        #region Interaction

        public override void OnSelectDown(int Id, Location Location)
        {
            base.OnSelectDown(Id, Location);

            UpdateState(InteractiveState.Selected);
        }

        public override void OnHoverEnter(Location Location)
        {
            base.OnHoverEnter(Location);
            
            if (State != InteractiveState.Selected)
                UpdateState(InteractiveState.Hovered);
        }

        public override void OnHoverMove(Location Location)
        {
            base.OnHoverMove(Location);

            if (JuixelGame.DeviceType == DeviceType.PC)
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;
        }

        public override void OnHoverLeave(Location Location)
        {
            base.OnHoverLeave(Location);
            
            if (State != InteractiveState.Selected)
                UpdateState(InteractiveState.Normal);
        }

        protected override void OnAddScene()
        {
            base.OnAddScene();
            
            Input.ListenForMouse(this);

            foreach (Keys Key in AllKeys)
                Input.ListenForKey(Key, this);
        }

        protected override void OnRemoveScene(Scene RemovedScene)
        {
            base.OnRemoveScene(RemovedScene);

            Input.RemoveMouseListener(this);

            foreach (Keys Key in AllKeys)
                Input.RemoveKeyListener(Key, this);
        }

        public void MouseDown(int Id, Location Location)
        {
            if (State == InteractiveState.Selected && !HitTest(Scene.FromScene(Location, Parent)))
                UpdateState(InteractiveState.Normal);
        }

        public void MouseMoved(int Id, Location Location)
        {
            
        }

        public void MouseUp(int Id, Location Location)
        {
            
        }

        public void MouseScroll(int Amount)
        {
            
        }

        public void MouseAltDown(int Id, Location Location)
        {
            
        }

        public void MouseAltUp(int Id, Location Location)
        {
            
        }

        public void KeyDown(Keys Key)
        {
            if (State == InteractiveState.Selected)
            {
                if (Key == Keys.Back)
                {
                    if (_Text.Length > 1)
                        Text = _Text.Substring(0, _Text.Length - 1);
                    else
                        Text = "";
                }
                else if (Key == Keys.Tab && Scene != null)
                {
                    TextField[] OtherFields = Scene.ChildrenWhere(_ => _ is TextField).Select(_ => (TextField)_).ToArray();
                    Array.Sort(OtherFields);
                    if (OtherFields.Length > 1)
                    {
                        if (Input.KeyIsDown(Keys.LeftShift))
                        {
                            for (int i = 0; i < OtherFields.Length; i++)
                                if (OtherFields[i] == this)
                                {
                                    TextField Other;
                                    if (i == 0)
                                        Other = OtherFields[OtherFields.Length - 1];
                                    else
                                        Other = OtherFields[i - 1];
                                    DispatchQueue.DispatchMain(() =>
                                    {
                                        UpdateState(InteractiveState.Normal);
                                        Other.UpdateState(InteractiveState.Selected);
                                    });
                                    return;
                                }
                        }
                        else
                        {
                            for (int i = 0; i < OtherFields.Length; i++)
                                if (OtherFields[i] == this)
                                {
                                    TextField Other;
                                    if (i == OtherFields.Length - 1)
                                        Other = OtherFields[0];
                                    else
                                        Other = OtherFields[i + 1];
                                    DispatchQueue.DispatchMain(() =>
                                    {
                                        UpdateState(InteractiveState.Normal);
                                        Other.UpdateState(InteractiveState.Selected);
                                    });
                                    return;
                                }
                        }
                    }
                }
                else
                {
                    char Char = CharForKey(Key, Input.KeyIsDown(Keys.LeftShift) || Input.KeyIsDown(Keys.RightShift));

                    if (Char != (char)0 && (CharFilter == null || CharFilter.Contains(Char)) && (MaxCharacters == -1 || _Text.Length < MaxCharacters))
                        Text += Char;
                }

                UpdateState(State);
            }
        }

        public void KeyUp(Keys Key)
        {
            
        }

        #endregion

        #region Key Conversions

        private char CharForKey(Keys Key, bool Shift)
        {
            switch (Key)
            {
                case Keys.A: return TranslateAlphabetic('a', Shift, Input.CapsLock);
                case Keys.B: return TranslateAlphabetic('b', Shift, Input.CapsLock);
                case Keys.C: return TranslateAlphabetic('c', Shift, Input.CapsLock);
                case Keys.D: return TranslateAlphabetic('d', Shift, Input.CapsLock);
                case Keys.E: return TranslateAlphabetic('e', Shift, Input.CapsLock);
                case Keys.F: return TranslateAlphabetic('f', Shift, Input.CapsLock);
                case Keys.G: return TranslateAlphabetic('g', Shift, Input.CapsLock);
                case Keys.H: return TranslateAlphabetic('h', Shift, Input.CapsLock);
                case Keys.I: return TranslateAlphabetic('i', Shift, Input.CapsLock);
                case Keys.J: return TranslateAlphabetic('j', Shift, Input.CapsLock);
                case Keys.K: return TranslateAlphabetic('k', Shift, Input.CapsLock);
                case Keys.L: return TranslateAlphabetic('l', Shift, Input.CapsLock);
                case Keys.M: return TranslateAlphabetic('m', Shift, Input.CapsLock);
                case Keys.N: return TranslateAlphabetic('n', Shift, Input.CapsLock);
                case Keys.O: return TranslateAlphabetic('o', Shift, Input.CapsLock);
                case Keys.P: return TranslateAlphabetic('p', Shift, Input.CapsLock);
                case Keys.Q: return TranslateAlphabetic('q', Shift, Input.CapsLock);
                case Keys.R: return TranslateAlphabetic('r', Shift, Input.CapsLock);
                case Keys.S: return TranslateAlphabetic('s', Shift, Input.CapsLock);
                case Keys.T: return TranslateAlphabetic('t', Shift, Input.CapsLock);
                case Keys.U: return TranslateAlphabetic('u', Shift, Input.CapsLock);
                case Keys.V: return TranslateAlphabetic('v', Shift, Input.CapsLock);
                case Keys.W: return TranslateAlphabetic('w', Shift, Input.CapsLock);
                case Keys.X: return TranslateAlphabetic('x', Shift, Input.CapsLock);
                case Keys.Y: return TranslateAlphabetic('y', Shift, Input.CapsLock);
                case Keys.Z: return TranslateAlphabetic('z', Shift, Input.CapsLock);

                case Keys.D0: return (Shift) ? ')' : '0';
                case Keys.D1: return (Shift) ? '!' : '1';
                case Keys.D2: return (Shift) ? '@' : '2';
                case Keys.D3: return (Shift) ? '#' : '3';
                case Keys.D4: return (Shift) ? '$' : '4';
                case Keys.D5: return (Shift) ? '%' : '5';
                case Keys.D6: return (Shift) ? '^' : '6';
                case Keys.D7: return (Shift) ? '&' : '7';
                case Keys.D8: return (Shift) ? '*' : '8';
                case Keys.D9: return (Shift) ? '(' : '9';

                case Keys.Add: return '+';
                case Keys.Divide: return '/';
                case Keys.Multiply: return '*';
                case Keys.Subtract: return '-';

                case Keys.Space: return ' ';
                //case Keys.Tab: return '\t';

                case Keys.Decimal: if (Input.NumLock && !Shift) return '.'; break;
                case Keys.NumPad0: if (Input.NumLock && !Shift) return '0'; break;
                case Keys.NumPad1: if (Input.NumLock && !Shift) return '1'; break;
                case Keys.NumPad2: if (Input.NumLock && !Shift) return '2'; break;
                case Keys.NumPad3: if (Input.NumLock && !Shift) return '3'; break;
                case Keys.NumPad4: if (Input.NumLock && !Shift) return '4'; break;
                case Keys.NumPad5: if (Input.NumLock && !Shift) return '5'; break;
                case Keys.NumPad6: if (Input.NumLock && !Shift) return '6'; break;
                case Keys.NumPad7: if (Input.NumLock && !Shift) return '7'; break;
                case Keys.NumPad8: if (Input.NumLock && !Shift) return '8'; break;
                case Keys.NumPad9: if (Input.NumLock && !Shift) return '9'; break;

                case Keys.OemBackslash: return Shift ? '|' : '\\';
                case Keys.OemCloseBrackets: return Shift ? '}' : ']';
                case Keys.OemComma: return Shift ? '<' : ',';
                case Keys.OemMinus: return Shift ? '_' : '-';
                case Keys.OemOpenBrackets: return Shift ? '{' : '[';
                case Keys.OemPeriod: return Shift ? '>' : '.';
                case Keys.OemPipe: return Shift ? '|' : '\\';
                case Keys.OemPlus: return Shift ? '+' : '=';
                case Keys.OemQuestion: return Shift ? '?' : '/';
                case Keys.OemQuotes: return Shift ? '"' : '\'';
                case Keys.OemSemicolon: return Shift ? ':' : ';';
                case Keys.OemTilde: return Shift ? '~' : '`';
            }

            return (char)0;
        }

        private char TranslateAlphabetic(char Char, bool Shift, bool CapsLock)
        {
            return (CapsLock ^ Shift) ? char.ToUpper(Char) : Char;
        }

        public int CompareTo(TextField other)
        {
            Location Mine = SceneLocation;
            Location Other = other.SceneLocation;

            Location Difference = Other - Mine;
            if (Difference.Y == 0)
                return -(int)(Difference.X / Math.Abs(Difference.X));
            else
                return -(int)(Difference.Y / Math.Abs(Difference.Y));
        }

        #endregion
    }
}
