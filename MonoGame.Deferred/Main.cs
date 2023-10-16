﻿using DeferredEngine.Logic;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using HelperSuite.GUIHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace DeferredEngine
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
            _graphics.PreferredBackBufferWidth = (int)RenderingSettings.g_ScreenResolution.X;
            _graphics.PreferredBackBufferHeight = (int)RenderingSettings.g_ScreenResolution.Y;

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
            if (_vsync != RenderingSettings.g_ScreenVSync || _fixFPS != RenderingSettings.g_ScreenFixedFPS)
            {

                SetFPSLimit();
                _vsync = RenderingSettings.g_ScreenVSync;
                _fixFPS = RenderingSettings.g_ScreenFixedFPS;
            }
        }

        private void SetFPSLimit()
        {
            if (!RenderingSettings.g_ScreenVSync && RenderingSettings.g_ScreenFixedFPS <= 0)
            {
                _graphics.SynchronizeWithVerticalRetrace = false;
                IsFixedTimeStep = false;
                _graphics.ApplyChanges();
            }
            else
            {
                if (RenderingSettings.g_ScreenFixedFPS > 0)
                {
                    _graphics.SynchronizeWithVerticalRetrace = false;
                    IsFixedTimeStep = true;
                    TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0f / RenderingSettings.g_ScreenFixedFPS);
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
            if (GraphicsDevice.Viewport.Width != _graphics.PreferredBackBufferWidth ||
                GraphicsDevice.Viewport.Height != _graphics.PreferredBackBufferHeight)
            {
                if (Window.ClientBounds.Width == 0) return;
                _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                _graphics.ApplyChanges();

                RenderingSettings.SetResolution(Window.ClientBounds.Width, Window.ClientBounds.Height);

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
            GUIControl.Initialize(RenderingSettings.g_ScreenResolution);

            FullscreenTriangleBuffer.InitClass(GraphicsDevice);
            StaticAssets.InitClass(Content, GraphicsDevice);

            _screenManager.Load(Content, GraphicsDevice);
            // TODO: Add your initialization logic here
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

            //base.Update(gameTime);
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