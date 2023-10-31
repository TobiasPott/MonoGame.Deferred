using DeferredEngine.Entities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System;


namespace DeferredEngine.Rendering
{
    public abstract class RenderingPipelineBase : IDisposable
    {
        public event Action<DrawEvents> EventTriggered;
        public virtual bool Enabled { get; set; } = true;

        //Graphics & Helpers
        protected GraphicsDevice _graphicsDevice;
        protected SpriteBatch _spriteBatch;

        /// <summary>
        /// Initialize variables
        /// </summary>
        /// <param name="content"></param>
        public virtual void Load(ContentManager content)
        { }
        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public virtual void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }


        /// <summary>
        /// Update our function
        /// </summary>
        public abstract void Update(Camera camera, DynamicMeshBatcher meshBatcher, EntityScene scene, GizmoDrawContext gizmoContext);

        // ! ! ! ! ! ! ! ! ! ! !
        // ToDo: PRIO I: Reduce Draw method to only call profiler and .Draw calls from modules
        //              Setup and processing should be internalized and done before any drawing takes place

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public abstract void Draw(DynamicMeshBatcher meshBatcher, EntityScene scene);




        public virtual void Dispose()
        {
            _spriteBatch?.Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  HELPER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        protected void BlitTo(Texture2D source, RenderTarget2D destRT = null, Rectangle? destRectangle = null)
        {
            _graphicsDevice.Blit(_spriteBatch, source, destRT, destRectangle: destRectangle);
        }

    }

}

