using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Juixel.Interaction
{
    public interface IKeyHandler
    {
        void KeyDown(Keys Key);
        void KeyUp(Keys Key);
    }

    public interface IMouseHandler
    {
        void MouseDown(int Id, Location Location);
        void MouseMoved(int Id, Location Location);
        void MouseUp(int Id, Location Location);
        void MouseAltDown(int Id, Location Location);
        void MouseAltUp(int Id, Location Location);
        void MouseScroll(int Amount);
    }
}
