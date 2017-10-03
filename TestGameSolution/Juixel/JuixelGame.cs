using Juixel.Drawing;
using Juixel.Drawing.Assets;
using Juixel.Drawing.Textures;
using Juixel.Interaction;
using Juixel.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Logging;

namespace Juixel
{
    /// <summary>
    /// The main class of the engine. To implement, connect it to the Monogame hooks
    /// </summary>
    public class JuixelGame
    {
        #region Statics

        /// <summary>
        /// The type of device the game is being run on
        /// </summary>
        public static DeviceType DeviceType;

        /// <summary>
        /// The width of the window
        /// </summary>
        public static int WindowWidth = 0;

        /// <summary>
        /// The height of the window
        /// </summary>
        public static int WindowHeight = 0;

        public static JuixelGame Shared;

        public static bool Desktop => DeviceType.Desktop.HasFlag(DeviceType);

        #endregion

        #region Properties

        /// <summary>
        /// The main Game of Monogame
        /// </summary>
        private Game _Game;

        public bool Active => _Game.IsActive;

        public SpriteBatch SpriteBatch;
        public GraphicsDeviceManager Graphics;

        public ContentManager Content => _Game.Content;
        public GraphicsDevice GraphicsDevice => _Game.GraphicsDevice;

        /// <summary>
        /// Set this to change Mouse visibility. <see langword="False"/> is invisible
        /// </summary>
        public bool MouseVisible
        {
            get => _Game.IsMouseVisible;
            set => _Game.IsMouseVisible = value;
        }

        /// <summary>
        /// <see langword="True"/> if the game locks to 60 FPS
        /// </summary>
        public bool LockFrameRate
        {
            get => _Game.IsFixedTimeStep;
            set => _Game.IsFixedTimeStep = value;
        }

        /// <summary>
        /// The <see cref="Color"/> present when nothing is drawn
        /// </summary>
        public Color BackgroundColor = Color.Black;

        /// <summary>
        /// The tint <see cref="Color"/> of the screen
        /// </summary>
        public static Color TintColor = Color.White;

        /// <summary>
        /// The intensity of the <see cref="TintColor"/>. 1.0 will block the screen with the color
        /// </summary>
        public static float TintIntensity = 0;

        /// <summary>
        /// The current <see cref="Scene"/> being presented
        /// </summary>
        public Scene CurrentScene;

        public Font DebugFont;

        #endregion

        #region Hooks

        public virtual void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            TextureLibrary.Setup(GraphicsDevice, Content);
            LoadFonts();
            DebugFont = Font.GetFont("");

            Effects.TileBlendEffect = Content.Load<Effect>("Effects/TileBlend");

            Masks.TileBlendMask = Content.Load<Texture2D>("Masks/TileBlendMask");

            Effects.TileBlendEffect.Parameters["MaskTexture"].SetValue(Masks.TileBlendMask);

            CurrentScene = MakeFirstScene();
        }

        public virtual void LoadFonts()
        {

        }

        public virtual void UnloadContent()
        {

        }

        public virtual void Initialize()
        {

        }

        #endregion

        #region Init

        public JuixelGame(Game Game, DeviceType Device)
        {
            _Game = Game;
            DeviceType = Device;
            Graphics = new GraphicsDeviceManager(Game);
            Content.RootDirectory = "Content";

            WindowWidth = Graphics.PreferredBackBufferWidth;
            WindowHeight = Graphics.PreferredBackBufferHeight;

            Graphics.PreferMultiSampling = false;

            Logger.SaveLogs = false;

            Shared = this;
        }

        protected virtual Scene MakeFirstScene()
        {
            return null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the Window size. Should be used for <see cref="DeviceType.PC"/> and <see cref="DeviceType.Mac"/>
        /// </summary>
        /// <param name="Width">The width of the window</param>
        /// <param name="Height">The height of the window</param>
        public void SetWindowSize(int Width, int Height)
        {
            Graphics.PreferredBackBufferWidth = Width;
            Graphics.PreferredBackBufferHeight = Height;
            UpdateWindowSize();
            Graphics.ApplyChanges();
        }

        public void UpdateWindowSize()
        {
            WindowWidth = Graphics.PreferredBackBufferWidth;
            WindowHeight = Graphics.PreferredBackBufferHeight;

            if (CurrentScene != null)
                CurrentScene.Size = new Location(WindowWidth, WindowHeight);
        }

        public void ChangeScene(Scene To)
        {
            CurrentScene.Dispose();
            CurrentScene = To;
        }

        private JuixelTime Time = new JuixelTime();

        public virtual void Update(GameTime Time)
        {
#if DEBUG
            Logger.StepLog();
#endif
            Input.StepInput();
            DispatchQueue.Main.Step();

            double ElapsedMS = Time.ElapsedGameTime.TotalMilliseconds;
            this.Time.ElapsedMS = ElapsedMS;
            this.Time.ElapsedSec = ElapsedMS / 1000;
            this.Time.TotalMS += this.Time.ElapsedMS;
            this.Time.TotalSec += this.Time.ElapsedSec;

            CurrentScene.Update(this.Time);
        }

        private static double DebugTextTime = 0;
        private static string DebugText = "";

        public static void ShowDebugText(string Text, double Time)
        {
            DebugText = Text;
            DebugTextTime = Time;
        }

        public virtual void Draw(GameTime Time)
        {
            GraphicsDevice.Clear(BackgroundColor);
            SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            CurrentScene.Draw(this.Time, SpriteBatch);

            if (TintColor != Color.White && TintIntensity > 0)
                SpriteBatch.Draw(TextureLibrary.Square, new Rectangle(0, 0, WindowWidth, WindowHeight), TintColor * TintIntensity);

            if (DebugFont != null)
            {
                string Text = "FPS: " + GetFPS(Time);
                if (DebugTextTime > 0 && DebugText != null)
                {
                    DebugTextTime -= this.Time.ElapsedSec;
                    Text += " D: " + DebugText;
                }
                FontInfo Info = DebugFont.GetSized(0);
                Vector2 Size = Info.BMFont.MeasureString(Text);

                SpriteBatch.Draw(TextureLibrary.Square, new Rectangle(0, 0, (int)Size.X + 8, (int)Size.Y), Color.Black * 0.5f);
                SpriteBatch.DrawString(Info.BMFont, Text, new Vector2(4, 0), Color.White);
            }

            SpriteBatch.End();
        }

        private const int FPS_Sample_Size = 20;
        private List<double> _FPSList = new List<double>();

        public double GetFPS(GameTime Time)
        {
            double NewFPS = 1000 / Time.ElapsedGameTime.TotalMilliseconds;
            _FPSList.Add(NewFPS);
            if (_FPSList.Count > FPS_Sample_Size)
                _FPSList.RemoveAt(0);

            double Total = 0;
            for (int i = 0; i < _FPSList.Count; i++)
                Total += _FPSList[i];

            return (int)((Total / _FPSList.Count + 0.005) * 100) / 100.0;
        }

        #endregion
    }
}
