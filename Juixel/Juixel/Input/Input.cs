using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Logging;
using Utilities.Threading;
using Win32;

namespace Juixel.Interaction
{
    public class Input
    {
        /// <summary>
        /// Refreshes Input values and dispatches handlers
        /// </summary>
        public static void StepInput()
        {
            KeyboardState NewKeyState = Keyboard.GetState();
            if (JuixelGame.Shared.Active)
            {
                CapsLock = (User.GetKeyState(VKey.CAPITAL) & 1) != 0;
                NumLock = (User.GetKeyState(VKey.NUMLOCK) & 1) != 0;
                ScrollLock = (User.GetKeyState(VKey.SCROLL) & 1) != 0;

                var KeyHandlers = _KeyHandlers.ToArray();
                for (int i = 0; i < KeyHandlers.Length; i++)
                {
                    var Pair = KeyHandlers[i];
                    var Handlers = Pair.Value.ToArray();
                    bool WasKeyDown = _LastKeyState.IsKeyDown(Pair.Key);
                    bool IsKeyDown = NewKeyState.IsKeyDown(Pair.Key);

                    if (IsKeyDown && !WasKeyDown)
                        for (int e = 0; e < Handlers.Length; e++)
                            Handlers[e].KeyDown(Pair.Key);
                    else if (!IsKeyDown && WasKeyDown)
                        for (int e = 0; e < Handlers.Length; e++)
                            Handlers[e].KeyUp(Pair.Key);
                }
            }
            _LastKeyState = NewKeyState;

            MouseState NewMouseState = Mouse.GetState();
            if (JuixelGame.Shared.Active)
            {
                var MouseHandlers = _MouseHandlers.ToArray();

                Point P_LastMousePosition = _LastMouseState.Position;
                Point P_NewMousePosition = NewMouseState.Position;

                Location LastMousePosition = new Location(P_LastMousePosition.X, P_LastMousePosition.Y);
                Location NewMousePosition = new Location(P_NewMousePosition.X, P_NewMousePosition.Y);

                if (_LastMouseState.LeftButton == ButtonState.Released && NewMouseState.LeftButton == ButtonState.Pressed)
                {
                    JuixelGame.Shared.CurrentScene.OnSelectDown(0, NewMousePosition);
                    for (int i = 0; i < MouseHandlers.Length; i++)
                        MouseHandlers[i].MouseDown(0, NewMousePosition);
                }

                if (_LastMouseState.RightButton == ButtonState.Pressed && NewMouseState.RightButton == ButtonState.Released)
                {
                    for (int i = 0; i < MouseHandlers.Length; i++)
                        MouseHandlers[i].MouseAltUp(0, NewMousePosition);
                }

                if (NewMousePosition != LastMousePosition)
                {
                    JuixelGame.Shared.CurrentScene.OnSelectMoved(0, NewMousePosition);
                    for (int i = 0; i < MouseHandlers.Length; i++)
                        MouseHandlers[i].MouseMoved(0, NewMousePosition);
                }

                if (_LastMouseState.LeftButton == ButtonState.Pressed && NewMouseState.LeftButton == ButtonState.Released)
                {
                    JuixelGame.Shared.CurrentScene.OnSelectUp(0, NewMousePosition);
                    for (int i = 0; i < MouseHandlers.Length; i++)
                        MouseHandlers[i].MouseUp(0, NewMousePosition);
                }

                if (_LastMouseState.RightButton == ButtonState.Released && NewMouseState.RightButton == ButtonState.Pressed)
                {
                    for (int i = 0; i < MouseHandlers.Length; i++)
                        MouseHandlers[i].MouseAltDown(0, NewMousePosition);
                }

                if (_LastMouseState.ScrollWheelValue != NewMouseState.ScrollWheelValue)
                {
                    int Amount = NewMouseState.ScrollWheelValue - _LastMouseState.ScrollWheelValue;
                    for (int i = 0; i < MouseHandlers.Length; i++)
                        MouseHandlers[i].MouseScroll(Amount);
                }
            }
            _LastMouseState = NewMouseState;
        }

        #region Key

        private static KeyboardState _LastKeyState;

        private static ConcurrentDictionary<Keys, HashSet<IKeyHandler>> _KeyHandlers
            = new ConcurrentDictionary<Keys, HashSet<IKeyHandler>>();

        public static bool CapsLock;
        public static bool NumLock;
        public static bool ScrollLock;

        /// <summary>
        /// Add a listener for key changes
        /// </summary>
        /// <param name="Key">The key to listen for</param>
        /// <param name="Handler"><see cref="IKeyHandler"/> used to receive key changes</param>
        public static void ListenForKey(Keys Key, IKeyHandler Handler)
        {
            HashSet<IKeyHandler> Handlers;
            if (!_KeyHandlers.TryGetValue(Key, out Handlers))
            {
                Handlers = new HashSet<IKeyHandler>();
                if (!_KeyHandlers.TryAdd(Key, Handlers))
                {
                    ListenForKey(Key, Handler);
                    return;
                }
            }
            Handlers.Add(Handler);
        }

        /// <summary>
        /// Removes a certain key listener
        /// </summary>
        /// <param name="Key">Key to remove from</param>
        /// <param name="Handler">The <see cref="IKeyHandler"/> to remove</param>
        public static void RemoveKeyListener(Keys Key, IKeyHandler Handler)
        {
            HashSet<IKeyHandler> Handlers;
            if (_KeyHandlers.TryGetValue(Key, out Handlers))
                Handlers.Remove(Handler);
        }

        /// <summary>
        /// Checks if a Key is pressed down
        /// </summary>
        /// <param name="Key">The Key to check</param>
        /// <returns></returns>
        public static bool KeyIsDown(Keys Key)
        {
            return _LastKeyState.IsKeyDown(Key);
        }

        #endregion

        #region Mouse

        private static MouseState _LastMouseState;

        private static LockingList<IMouseHandler> _MouseHandlers
            = new LockingList<IMouseHandler>();

        public static int MouseX => _LastMouseState.Position.X;

        public static int MouseY => _LastMouseState.Position.Y;

        public static int MouseScrollValue => _LastMouseState.ScrollWheelValue;

        public static bool MouseOnScreen => MouseX >= 0 && MouseX < JuixelGame.WindowWidth && MouseY >= 0 && MouseY < JuixelGame.WindowHeight;

        public static void ListenForMouse(IMouseHandler Handler)
        {
            _MouseHandlers.Add(Handler);
        }

        public static void RemoveMouseListener(IMouseHandler Handler)
        {
            _MouseHandlers.Remove(Handler);
        }

        #endregion

        #region Touch



        #endregion
    }
}

namespace Win32
{
    public enum VKey
    {
        // Tons of other VK_ codes removed.
        CAPITAL = 0x14,
        NUMLOCK = 0x90,
        SCROLL = 0x91,
    }

    public static class User
    {
        [DllImport("user32.dll")]
        public static extern short GetKeyState(VKey vkey);
    }
}