using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Threading;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DeferredEngine.Demo
{
    public class Main : Game
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private readonly GraphicsDeviceManager _graphics;

        private readonly ScreenManager _screenManager;

        //Do not change, these are overwritten (Check GameSettings.cs in Resources
        private bool _vsync = true;
        private int _fixFPS = 0;
        private bool _isActive = true;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Main()
        {
            //Initialize graphics and content
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Initialize screen manager, which controls draw / logic for our screens
            _screenManager = new ScreenManager();

            //Size of our application / starting back buffer
            _graphics.PreferredBackBufferWidth = (int)RenderingSettings.Screen.g_Resolution.X;
            _graphics.PreferredBackBufferHeight = (int)RenderingSettings.Screen.g_Resolution.Y;

            //HiDef enables usable shaders
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            //_graphics.GraphicsDevice.DeviceLost += new EventHandler<EventArgs>(ClientLostDevice);

            //Mouse should not disappear
            IsMouseVisible = true;

            //Window settings
            Window.AllowUserResizing = true;
            Window.IsBorderless = false;

            //Update all our rendertargets when we resize
            Window.ClientSizeChanged += ClientChangedWindowSize;

            //Update framerate etc. when not the active window
            Activated += IsActivated;
            Deactivated += IsDeactivated;
        }


        private void CheckFPSLimitChange()
        {
            if (_vsync != RenderingSettings.Screen.g_VSync || _fixFPS != RenderingSettings.Screen.g_FixedFPS)
            {

                SetFPSLimit();
                _vsync = RenderingSettings.Screen.g_VSync;
                _fixFPS = RenderingSettings.Screen.g_FixedFPS;
            }
        }

        private void SetFPSLimit()
        {
            if (!RenderingSettings.Screen.g_VSync && RenderingSettings.Screen.g_FixedFPS <= 0)
            {
                _graphics.SynchronizeWithVerticalRetrace = false;
                IsFixedTimeStep = false;
                _graphics.ApplyChanges();
            }
            else
            {
                if (RenderingSettings.Screen.g_FixedFPS > 0)
                {
                    _graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = true;
                    TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / RenderingSettings.Screen.g_FixedFPS);
                }
                else //Vsync
                {
                    _graphics.SynchronizeWithVerticalRetrace = true;
                    IsFixedTimeStep = false;
                    _graphics.ApplyChanges();
                }
            }
        }

        private void IsActivated(object sender, EventArgs e)
        {
            _isActive = true;
        }

        private void IsDeactivated(object sender, EventArgs e)
        {
            _isActive = false;
        }

        /// <summary>
        /// Update rendertargets and backbuffer when resizing window size
        /// </summary>
        private void ClientChangedWindowSize(object sender, EventArgs e)
        {
            Debug.WriteLine($"{GraphicsDevice.Viewport} => {_graphics.PreferredBackBufferWidth} x {_graphics.PreferredBackBufferHeight}" +
                $"  >> {Window.ClientBounds}");
            if (RenderingSettings.Screen.g_TargetRect.Width != _graphics.PreferredBackBufferWidth ||
                RenderingSettings.Screen.g_TargetRect.Height != _graphics.PreferredBackBufferHeight)
            {

                Debug.WriteLine($"\t{GraphicsDevice.Viewport} => {_graphics.PreferredBackBufferWidth} x {_graphics.PreferredBackBufferHeight}" +
                    $"  >> {Window.ClientBounds}");
                if (Window.ClientBounds.Width == 0) return;
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                _graphics.ApplyChanges();

                //RenderingSettings.Screen.SetResolution(Window.ClientBounds.Width, Window.ClientBounds.Height);
                RenderingSettings.Screen.g_TargetRect = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                RenderingSettings.Screen.g_UIResolution = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height);

                _screenManager.UpdateResolution();

            }

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Add your initialization logic here
            GUIMouseInput.Initialize(RenderingSettings.Screen.g_Resolution);

            FullscreenTriangleBuffer.InitClass(GraphicsDevice);
            StaticAssets.InitClass(Content, GraphicsDevice);

            _screenManager.Load(Content, GraphicsDevice);
            _screenManager.Initialize(GraphicsDevice);

            // collect garbage upfront after everything is initialized
            GC.Collect();
            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            FullscreenTriangleBuffer.UnloadClass();
            StaticAssets.UnloadClass();
            base.Dispose(disposing);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _screenManager.Unload(Content);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (!_isActive) return;

            //Exit the game when pressing escape
            if (Input.WasKeyPressed(Keys.Escape))
                Exit();

            _screenManager.Update(gameTime, _isActive);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Don't draw when the game is not running
            if (!_isActive)
            {
                Thread.Sleep(20);
                return;
            }

            CheckFPSLimitChange();

            _screenManager.Draw(gameTime);
            //base.Draw(gameTime);
        }

    }
}
