using DeferredEngine.Renderer.RenderModules.SDF;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class SdfModelDefinition : ModelDefinition
    {
        public static readonly Vector3 DefaultSdfResolution = new Vector3(50, 50, 50);

        public SignedDistanceField SDF;
        public SdfTriangle[] SdfTriangles = Array.Empty<SdfTriangle>();

        public SdfModelDefinition(ContentManager content, string assetpath, GraphicsDevice graphics, bool UseSDF = false) : this(content,
            assetpath, graphics, UseSDF, DefaultSdfResolution)
        { }
        public SdfModelDefinition(ContentManager content, string assetpath, GraphicsDevice graphics, bool UseSDF, Vector3 sdfResolution /*default = 50^3*/)
            : base(content, assetpath, graphics)
        {
            //SDF
            SDF = new SignedDistanceField(content.RootDirectory + "/" + assetpath + ".sdft", graphics, BoundingBox, BoundingBoxOffset, sdfResolution);
            SDF.IsUsed = UseSDF;
        }

        public SdfModelDefinition(Model model, BoundingBox box)
            : base(model, box)
        {
            Model = model;
            BoundingBox = box;
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }

    }

}
