using DeferredEngine.Demo;
using DeferredEngine.Entities;
using DeferredEngine.Pipeline.Utilities;
using DeferredEngine.Recources;
using DeferredEngine.Rendering.Helper.HelperGeometry;
using DeferredEngine.Rendering.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Rendering
{

    public class EditableRenderingPipeline : DefaultRenderingPipeline
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Final output
        private RenderTarget2D _currentOutput;

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public ObjectHoverContext CurrentHoverContext => new ObjectHoverContext(_moduleStack.IdAndOutline.HoveredId, _matrices);


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  BASE FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize all our rendermodules and helpers. Done after the Load() function
        /// </summary>
        /// <param name="graphicsDevice"></param>
        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            base.Initialize(graphicsDevice);

            // update directional light module
            SetResolution(RenderingSettings.Screen.g_Resolution);

            RenderingSettings.Screen.g_FarClip.Changed += FarClip_OnChanged;
            RenderingSettings.Screen.g_FarClip.Set(512);
            SSReflectionFx.ModuleEnabled.Changed += SSR_Enabled_Changed;
            BloomFx.ModuleThreshold.Set(0.0f);
        }

        private void SSR_Enabled_Changed(bool enabled)
        {
            // clear SSReflection buffer if disabled/enabled
            if (!enabled)
            {
                _graphicsDevice.SetRenderTarget(_ssfxTargets.SSR_Main);
                _graphicsDevice.Clear(new Color(0, 0, 0, 0.0f));
            }
        }
        private void FarClip_OnChanged(float farClip)
        {
            _frustum.FarClip = farClip;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDERTARGET SETUP FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        public override void RequestRedraw(GameTime gameTime)
        {
            base.RequestRedraw(gameTime);

            _moduleStack.Lighting.UpdateGameTime(gameTime);
            if (SSReflectionFx.g_Noise)
                _fxStack.SSReflection.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
            _moduleStack.Environment.Time = (float)gameTime.TotalGameTime.TotalSeconds % 1000;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  RENDER FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Draw(DynamicMeshBatcher meshBatcher, EntityScene scene)
        {
            if (!this.Enabled)
                return;
            this.DrawEditorPrePass(meshBatcher, scene, EditorLogic.Instance.GetEditorData());
            base.Draw(meshBatcher, scene);
            this.DrawEditor(meshBatcher, scene, EditorLogic.Instance.GetEditorData());
        }

        private void DrawEditorPrePass(DynamicMeshBatcher meshBatcher, EntityScene scene, GizmoDrawContext gizmoContext)
        {
            if (!this.Enabled)
                return;
            // Step: -1
            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.e_EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, gizmoContext, EditorLogic.Instance.HasMouseMoved);

        }
        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        private void DrawEditor(DynamicMeshBatcher meshBatcher, EntityScene scene, GizmoDrawContext gizmoContext)
        {
            this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.SDFDistance);
            this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.SDFVolume);

            // Step: 15
            //Additional editor elements that overlay our screen
            if (RenderingSettings.e_EnableSelection)
            {
                this.DrawEditorPasses(scene, gizmoContext, IdAndOutlineRenderModule.e_DrawOutlines ? PipelineEditorPasses.IdAndOutline : 0);
                this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.Billboard | PipelineEditorPasses.TransformGizmo);
                //Draw debug/helper geometry
                this.DrawEditorPasses(scene, gizmoContext, PipelineEditorPasses.Helper);
            }
        }
        private void DrawEditorPasses(EntityScene scene, GizmoDrawContext gizmoContext, PipelineEditorPasses passes = PipelineEditorPasses.Billboard | PipelineEditorPasses.TransformGizmo)
        {
            if (passes == 0)
                return;

            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            // render directly to the output buffer
            if (passes.HasFlag(PipelineEditorPasses.Billboard))
            {
                _moduleStack.Billboard.DrawEditorBillboards(scene, gizmoContext);
            }
            if (passes.HasFlag(PipelineEditorPasses.IdAndOutline))
            {
                _moduleStack.IdAndOutline.Blit(_moduleStack.IdAndOutline.Target, null, BlendState.Additive);
            }
            if (passes.HasFlag(PipelineEditorPasses.TransformGizmo))
            {
                _moduleStack.IdAndOutline.DrawTransformGizmos(gizmoContext, IdAndOutlineRenderModule.Pass.Color);
            }
            if (passes.HasFlag(PipelineEditorPasses.Helper))
            {
                _moduleStack.Helper.Draw();
            }
            if (passes.HasFlag(PipelineEditorPasses.SDFDistance))
            {
                _graphicsDevice.SetStates(DepthStencilStateOption.None);
                _moduleStack.DistanceField.DrawDistance();
            }
            if (passes.HasFlag(PipelineEditorPasses.SDFVolume))
            {
                _graphicsDevice.SetStates(DepthStencilStateOption.None);
                _moduleStack.DistanceField.DrawVolume();
            }
            //if(passes.HasFlag(PipelineEditorPasses.SDFVolume))
            //{
            //    if (RenderingSettings.SDF.DrawDebug && _moduleStack.DistanceField.AtlasTarget != null)
            //    {
            //        _spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp);
            //        _spriteBatch.Draw(_moduleStack.DistanceField.AtlasTarget, new Rectangle(0, RenderingSettings.Screen.g_Height - 200, RenderingSettings.Screen.g_Width, 200), Color.White);
            //        _spriteBatch.End();
            //    }
            //}
        }


        public override void Dispose()
        {
            base.Dispose();
            _currentOutput?.Dispose();
        }

    }

}

