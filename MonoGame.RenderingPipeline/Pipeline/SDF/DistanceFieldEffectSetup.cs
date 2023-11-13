using DeferredEngine.Pipeline;
using DeferredEngine.Recources;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Rendering.SDF
{
    public class DistanceFieldEffectSetup : BaseFxSetup
    {
        public Effect Effect { get; protected set; }

        public EffectPass Pass_Distance { get; protected set; }
        public EffectPass Pass_Volume { get; protected set; }
        public EffectPass Pass_GenerateSDF { get; protected set; }

        public EffectParameter Param_FrustumCorners { get; protected set; }
        public EffectParameter Param_CameraPositon { get; protected set; }
        public EffectParameter Param_DepthMap { get; protected set; }

        public EffectParameter Param_VolumeTex { get; protected set; }
        public EffectParameter Param_VolumeTexSize { get; protected set; }
        public EffectParameter Param_VolumeTexResolution { get; protected set; }

        public EffectParameter Param_InstanceInverseMatrix { get; protected set; }
        public EffectParameter Param_InstanceScale { get; protected set; }
        public EffectParameter Param_InstanceSDFIndex { get; protected set; }
        public EffectParameter Param_InstancesCount { get; protected set; }

        public EffectParameter Param_MeshOffset { get; protected set; }
        public EffectParameter Param_TriangleTexResolution { get; protected set; }
        public EffectParameter Param_TriangleAmount { get; protected set; }

        public DistanceFieldEffectSetup(string shaderPath = "Shaders/SDF/VolumeProjection")
            : base()
        {
            Effect = Globals.content.Load<Effect>(shaderPath);

            Pass_Distance = Effect.Techniques["Distance"].Passes[0];
            Pass_Volume = Effect.Techniques["Volume"].Passes[0];
            Pass_GenerateSDF = Effect.Techniques["GenerateSDF"].Passes[0];

            Param_FrustumCorners = Effect.Parameters["FrustumCorners"];
            Param_CameraPositon = Effect.Parameters["CameraPosition"];

            Param_DepthMap = Effect.Parameters[Names.Sampler("DepthMap")];

            Param_VolumeTex = Effect.Parameters[Names.Sampler("VolumeTex")];
            Param_VolumeTexSize = Effect.Parameters["VolumeTexSize"];
            Param_VolumeTexResolution = Effect.Parameters["VolumeTexResolution"];

            Param_InstanceInverseMatrix = Effect.Parameters["InstanceInverseMatrix"];
            Param_InstanceScale = Effect.Parameters["InstanceScale"];
            Param_InstanceSDFIndex = Effect.Parameters["InstanceSDFIndex"];
            Param_InstancesCount = Effect.Parameters["InstancesCount"];

            Param_MeshOffset = Effect.Parameters["MeshOffset"];
            Param_TriangleTexResolution = Effect.Parameters["TriangleTexResolution"];
            Param_TriangleAmount = Effect.Parameters["TriangleAmount"];
        }

        public override void Dispose()
        {
            Effect?.Dispose();
        }
    }

}
