using DeferredEngine.Recources.Helper;
using DeferredEngine.Recources;
using DeferredEngine.Renderer.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Renderer.RenderModules.Default
{
    public interface IRenderModule
    {
        void Apply(Matrix localWorldMatrix, Matrix? view, Matrix viewProjection);
    }

    public static class RenderModule
    {

        public static bool ApplyShaders(GraphicsDevice graphicsDevice, RenderType renderType, IRenderModule renderModule,
            Matrix localToWorldMatrix, Matrix? view, Matrix viewProjection,
            int transformId, int outlineId, bool outlined)
        {
            switch (renderType)
            {
                case RenderType.Opaque:
                case RenderType.ShadowLinear:
                case RenderType.ShadowOmnidirectional:
                case RenderType.Forward:
                    renderModule.Apply(localToWorldMatrix, view, viewProjection);
                    break;
                case RenderType.Hologram:
                    ApplyHologramShaders(localToWorldMatrix, viewProjection);
                    break;
                case RenderType.IdRender:
                case RenderType.IdOutline:
                    if (!ApplyIdAndOutlineShaders(graphicsDevice, renderType, localToWorldMatrix, viewProjection, transformId, outlineId, outlined))
                        return false;
                    break;
            }
            return true;
        }
        private static void ApplyHologramShaders(Matrix localToWorldMatrix, Matrix viewProjection)
        {
            HologramEffectSetup.Instance.Param_World.SetValue(localToWorldMatrix);
            HologramEffectSetup.Instance.Param_WorldViewProj.SetValue(localToWorldMatrix * viewProjection);
            HologramEffectSetup.Instance.Effect.CurrentTechnique.Passes[0].Apply();
        }
        private static bool ApplyIdAndOutlineShaders(GraphicsDevice graphicsDevice, RenderType renderType, Matrix localToWorldMatrix, Matrix viewProjection,
            int transformId, int outlineId, bool outlined)
        {
            // ToDo: @tpott: Extract IdRender and Bilboard Shaders members
            IdAndOutlineEffectSetup.Instance.Param_WorldViewProj.SetValue(localToWorldMatrix * viewProjection);

            if (renderType == RenderType.IdRender)
            {
                IdAndOutlineEffectSetup.Instance.Param_ColorId.SetValue(IdGenerator.GetColorFromId(transformId).ToVector4());
                IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
            }
            if (renderType == RenderType.IdOutline)
            {

                //Is this the Id we want to outline?
                if (transformId == outlineId)
                {
                    graphicsDevice.RasterizerState = RasterizerState.CullNone;

                    IdAndOutlineEffectSetup.Instance.Param_World.SetValue(localToWorldMatrix);

                    if (outlined)
                        IdAndOutlineEffectSetup.Instance.Pass_Outline.Apply();
                    else
                        IdAndOutlineEffectSetup.Instance.Pass_Id.Apply();
                }
                else
                    return false;
            }
            return true;
        }

    }
}