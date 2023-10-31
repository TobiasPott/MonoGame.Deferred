using DeferredEngine.Demo;
using DeferredEngine.Entities;
using DeferredEngine.Pipeline;
using DeferredEngine.Pipeline.Utilities;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Ext;
using System;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Rendering
{
    [Flags()]
    public enum PipelineEditorPasses
    {
        Billboard = 1,
        IdAndOutline = 2,
        Helper = 4,
        TransformGizmo = 8,
        SDFDistance = 16,
        SDFVolume = 32,
    }

    public class EditableRenderingPipeline : DemoRenderingPipeline
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  VARIABLES
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main Draw function of the game
        /// </summary>
        public ObjectHoverContext CurrentHoverContext => new ObjectHoverContext(_moduleStack.IdAndOutline.HoveredId, _matrices);
        private GizmoDrawContext _currentGizmoContext;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        //  MAIN DRAW FUNCTIONS
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Draw(DynamicMeshBatcher meshBatcher, EntityScene scene)
        {
            if (!this.Enabled)
                return;

            // retrieve curreentn gizmo draw data
            _currentGizmoContext = EditorLogic.Instance.GetEditorData();

            if (RenderingSettings.e_IsEditorEnabled)
                this.DrawEditorPrePass(meshBatcher, scene);

            base.Draw(meshBatcher, scene);

            if (RenderingSettings.e_IsEditorEnabled)
                this.DrawEditor(scene);
        }

        private void DrawEditorPrePass(DynamicMeshBatcher meshBatcher, EntityScene scene)
        {
            if (!this.Enabled)
                return;
            // Step: -1
            //Draw the elements that we are hovering over with outlines
            if (RenderingSettings.EnableSelection)
                _moduleStack.IdAndOutline.Draw(meshBatcher, scene, _currentGizmoContext, EditorLogic.Instance.HasMouseMoved);

            _profiler?.SampleTimestamp(ProfilerTimestamps.Draw_EditorPrePass);
        }
        private void DrawEditor(EntityScene scene)
        {
            this.DrawEditorPasses(scene, PipelineEditorPasses.SDFDistance);
            this.DrawEditorPasses(scene, PipelineEditorPasses.SDFVolume);

            // Step: 15
            //Additional editor elements that overlay our screen
            if (RenderingSettings.EnableSelection)
            {
                this.DrawEditorPasses(scene, IdAndOutlineRenderModule.e_DrawOutlines ? PipelineEditorPasses.IdAndOutline : 0);
                this.DrawEditorPasses(scene, PipelineEditorPasses.Billboard | PipelineEditorPasses.TransformGizmo);
                //Draw debug/helper geometry
                this.DrawEditorPasses(scene, PipelineEditorPasses.Helper);
            }

            _profiler?.SampleTimestamp(ProfilerTimestamps.Draw_EditorPass);
        }
        private void DrawEditorPasses(EntityScene scene, PipelineEditorPasses passes = PipelineEditorPasses.Billboard | PipelineEditorPasses.TransformGizmo)
        {
            if (passes == 0)
                return;

            _graphicsDevice.SetRenderTarget(null);
            _graphicsDevice.SetStates(DepthStencilStateOption.Default, RasterizerStateOption.CullCounterClockwise, BlendStateOption.Opaque);

            // render directly to the output buffer
            if (passes.HasFlag(PipelineEditorPasses.Billboard))
            {
                _moduleStack.Billboard.DrawEditorBillboards(scene, _currentGizmoContext);
            }
            if (passes.HasFlag(PipelineEditorPasses.IdAndOutline))
            {
                _moduleStack.IdAndOutline.Blit(_moduleStack.IdAndOutline.Target, null, BlendState.Additive);
            }
            if (passes.HasFlag(PipelineEditorPasses.TransformGizmo))
            {
                _moduleStack.IdAndOutline.DrawTransformGizmos(_currentGizmoContext, IdAndOutlineRenderModule.Pass.Color);
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

    }

}

