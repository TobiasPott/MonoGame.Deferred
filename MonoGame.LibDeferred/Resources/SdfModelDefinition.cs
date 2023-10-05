﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{
    public class SdfModelDefinition : ModelDefinition
    {
        public SignedDistanceField SDF;

        public SdfModelDefinition(ContentManager content, string assetpath, GraphicsDevice graphics, bool UseSDF = false) : this(content,
            assetpath, graphics, UseSDF, new Vector3(50, 50, 50))
        { }
        public SdfModelDefinition(ContentManager content, string assetpath, GraphicsDevice graphics, bool UseSDF, Vector3 sdfResolution /*default = 50^3*/)
            : base(content, assetpath, graphics, UseSDF, sdfResolution)
        {
            //SDF
            SDF = new SignedDistanceField(content.RootDirectory + "/" + assetpath + ".sdft", graphics, BoundingBox, BoundingBoxOffset, sdfResolution);
            SDF.IsUsed = UseSDF;
        }

        public SdfModelDefinition(Model model, BoundingBox box)
            :base(model, box)
        {
            Model = model;
            BoundingBox = box;
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }

    }

}
