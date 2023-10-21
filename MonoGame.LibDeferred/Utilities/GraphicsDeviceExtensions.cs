using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace MonoGame.Ext
{
    public enum DepthStencilStateOption
    {
        KeepState = -1,
        Default,
        None,
        DepthRead,
    }
    public enum RasterizerStateOption
    {
        KeepState = -1,
        CullCounterClockwise,
        CullClockwise,
        CullNone,
    }
    public enum BlendStateOption
    {
        KeepState = -1,
        Opaque,
        Additive,
        AlphaBlend,
        NonPremultiplied,
    }

    public static class GraphicsDeviceExtensions
    {

        // ToDo: Extend to cover BlendState and wrap target state to map to an enum which includes a "keep" option to leave a state unchanged

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetStates(this GraphicsDevice graphicsDevice, 
            DepthStencilStateOption depthStencil = DepthStencilStateOption.KeepState, 
            RasterizerStateOption rasterizer = RasterizerStateOption.KeepState, 
            BlendStateOption blend = BlendStateOption.KeepState)
        {
            if (depthStencil != DepthStencilStateOption.KeepState) graphicsDevice.SetState(depthStencil);
            if (rasterizer != RasterizerStateOption.KeepState) graphicsDevice.SetState(rasterizer);
            if (blend != BlendStateOption.KeepState) graphicsDevice.SetState(blend);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetState(this GraphicsDevice graphicsDevice, DepthStencilStateOption state)
        {
            if (state == DepthStencilStateOption.KeepState)
                return;
            graphicsDevice.DepthStencilState = state switch
            {
                DepthStencilStateOption.Default => DepthStencilState.Default,
                DepthStencilStateOption.None => DepthStencilState.None,
                DepthStencilStateOption.DepthRead => DepthStencilState.DepthRead,
                _ => graphicsDevice.DepthStencilState
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetState(this GraphicsDevice graphicsDevice, RasterizerStateOption state)
        {
            if (state == RasterizerStateOption.KeepState)
                return;
            graphicsDevice.RasterizerState = state switch
            {
                RasterizerStateOption.CullCounterClockwise => RasterizerState.CullCounterClockwise,
                RasterizerStateOption.CullClockwise => RasterizerState.CullClockwise,
                RasterizerStateOption.CullNone => RasterizerState.CullNone,
                _ => graphicsDevice.RasterizerState
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetState(this GraphicsDevice graphicsDevice, BlendStateOption state)
        {
            if (state == BlendStateOption.KeepState)
                return;
            graphicsDevice.BlendState = state switch
            {
                BlendStateOption.Opaque => BlendState.Opaque,
                BlendStateOption.Additive => BlendState.Additive,
                BlendStateOption.AlphaBlend => BlendState.AlphaBlend,
                BlendStateOption.NonPremultiplied => BlendState.NonPremultiplied,
                _ => graphicsDevice.BlendState
            };
        }

    }
}