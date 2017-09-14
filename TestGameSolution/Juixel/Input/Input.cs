using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Threading;

namespace Juixel.Input
{
    public class Input
    {
        /// <summary>
        /// Refreshes Input values and dispatches handlers
        /// </summary>
        public static void StepInput()
        {
            KeyboardState NewKeyState = Keyboard.GetState();
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
            _LastKeyState = NewKeyState;

            MouseState NewMouseState = Mouse.GetState();
            var MouseHandlers = _MouseHandlers.ToArray();

            Point P_LastMousePosition = _LastMouseState.Position;
            Point P_NewMousePosition = NewMouseState.Position;

            Location LastMousePosition = new Location(P_LastMousePosition.X, P_LastMousePosition.Y);
            Location NewMousePosition = new Location(P_NewMousePosition.X, P_NewMousePosition.Y);

            if (NewMousePosition != LastMousePosition)
                for (int i = 0; i < MouseHandlers.Length; i++)
                    MouseHandlers[i].MouseMoved(0, NewMousePosition);

            if (_LastMouseState.LeftButton == ButtonState.Released && NewMouseState.LeftButton == ButtonState.Pressed)
                for (int i = 0; i < MouseHandlers.Length; i++)
                    MouseHandlers[i].MouseDown(0, NewMousePosition);

            if (_LastMouseState.LeftButton == ButtonState.Pressed && NewMouseState.LeftButton == ButtonState.Released)
                for (int i = 0; i < MouseHandlers.Length; i++)
                    MouseHandlers[i].MouseUp(0, NewMousePosition);

            _LastMouseState = NewMouseState;
        }

        #region Key

        private static KeyboardState _LastKeyState;

        private static ConcurrentDictionary<Keys, HashSet<IKeyHandler>> _KeyHandlers
            = new ConcurrentDictionary<Keys, HashSet<IKeyHandler>>();

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

        private static MouseState _LastMouseState;

        private static LockingList<IMouseHandler> _MouseHandlers
            = new LockingList<IMouseHandler>();

        #region Mouse

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