using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Juixel.Drawing.Textures;
using Juixel;
using Juixel.Tools;

namespace Clans.Drawing.GameObjects
{
    public class GameObject : Node, IUpdatable
    {
        /// <summary>
        /// The internal <see cref="SpriteNode"/>
        /// </summary>
        protected SpriteNode Sprite;

        /// <summary>
        /// The current <see cref="AnimatedSprite"/>, if it has one
        /// </summary>
        protected AnimatedSprite Animation;

        /// <summary>
        /// The time spent on each animation frame
        /// </summary>
        protected double FrameRate = 0.4;

        /// <summary>
        /// Determines if <see cref="Update(JuixelTime)"/> will be called each update
        /// </summary>
        protected virtual bool Updates => false;

        /// <summary>
        /// The base object of all objects in the map
        /// </summary>
        public GameObject()
        {

        }

        protected override void OnAddScene()
        {
            base.OnAddScene();

            if (Updates)
                Scene.AddUpdatable(this);
        }

        protected override void OnRemoveScene(Scene RemovedScene)
        {
            base.OnRemoveScene(RemovedScene);

            if (Updates)
                RemovedScene.AddUpdatable(this);
        }

        public virtual void Update(JuixelTime Time)
        {

        }
    }
}
