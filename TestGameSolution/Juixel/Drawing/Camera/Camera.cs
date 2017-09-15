using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Juixel.Drawing
{
    public class Camera
    {
        public Node Target;

        public Location DrawOffset
        {
            get
            {
                if (Target == null || Target.Scene == null)
                    return 0;
                else
                    return (Target.Scene.Size / 2 - Target.SceneLocation).Round();
            }
        }
    }
}
