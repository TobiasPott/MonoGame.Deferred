using DeferredEngine.Recources.Helper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DeferredEngine.Recources
{

    public class ModelDefinition
    {
        public BoundingBox BoundingBox;
        public Vector3 BoundingBoxOffset;
        public Model Model;

        public ModelDefinition(ContentManager content, string assetpath)
        {
            Model = content.Load<Model>(assetpath);


            // ToDo: Implement method without indices output to reduce garbage consumption
            GeometryDataExtractor.GetVerticesAndIndicesFromModel(Model, out Vector3[] vertices, out _);
            BoundingBox = BoundingBox.CreateFromPoints(vertices);

            //Find the middle
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }


        public ModelDefinition(Model model, BoundingBox box)
        {
            Model = model;
            BoundingBox = box;
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }

    }

}
