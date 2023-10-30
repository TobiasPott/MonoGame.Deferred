using DeferredEngine.Recources;
using DeferredEngine.Rendering;
using HelperSuite.GUIRenderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeferredEngine.Demo
{
    /// <summary>
    /// Manages our different screens and passes information accordingly
    /// </summary>
    public class ScreenManager : IDisposable
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private RenderingPipeline _renderer;
        private MainSceneLogic _sceneLogic;
        private GUIRenderer _guiRenderer;
        private GUILogic _guiLogic;
        private EditorLogic _editorLogic;

        private DebugScreen _debug;

        private DemoAssets _assets;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _renderer.Initialize(graphicsDevice);
            _sceneLogic.Initialize(_assets, graphicsDevice);
            _guiLogic.Initialize(_assets, _sceneLogic.Camera);
            _editorLogic.Initialize(graphicsDevice);
            _debug.Initialize(graphicsDevice);
            _guiRenderer.Initialize(graphicsDevice, RenderingSettings.Screen.g_Width, RenderingSettings.Screen.g_Height);
        }

        //Load content

        public void Load(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _renderer = new RenderingPipeline();
            _sceneLogic = new MainSceneLogic();
            _guiLogic = new GUILogic();
            _editorLogic = new EditorLogic();
            _assets = new DemoAssets();
            _debug = new DebugScreen();
            _guiRenderer = new GUIRenderer();

            Globals.content = content;

            _assets.Load(content, graphicsDevice);
            _renderer.Load(content);
            _sceneLogic.Load(content);
            _debug.LoadContent(content);
            _guiRenderer.Load(content);
        }

        public void Unload(ContentManager content)
        {
            content.Dispose();
        }

        //Update per frame
        public void Update(GameTime gameTime, bool isActive)
        {
            _guiLogic.Update(gameTime, isActive, _editorLogic.SelectedObject);
            _editorLogic.Update(gameTime, _sceneLogic.Scene, _renderer.CurrentHoverContext, _sceneLogic.MeshBatcher);
            _sceneLogic.Update(gameTime, isActive);
            _renderer.RequestRedraw(gameTime);
            _renderer.Update(_sceneLogic.Camera, _sceneLogic.MeshBatcher, _sceneLogic.Scene, _editorLogic.GetEditorData());

            _debug.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            //Our renderer gives us information on what id is currently hovered over so we can update / manipulate objects in the logic functions
            _renderer.Draw(_sceneLogic.MeshBatcher, _sceneLogic.Scene, _editorLogic.GetEditorData());
            _renderer.DrawEditor(_sceneLogic.MeshBatcher, _sceneLogic.Scene, _editorLogic.GetEditorData());

            if (RenderingSettings.e_IsEditorEnabled)
                _guiRenderer.Draw(_guiLogic.GuiCanvas);
            _debug.Draw(gameTime);
        }

        public void UpdateResolution()
        {
            if (_renderer != null)
            {
                _guiLogic.UpdateResolution();
                _renderer.UpdateResolution();
            }
        }

        public void Dispose()
        {
            _guiRenderer?.Dispose();
        }
    }
}
