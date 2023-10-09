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

            string bbxpath = content.RootDirectory + "/" + assetpath + ".bbox";
            //Look if there is a bounding box already created, otherwise create a new one
            if (!File.Exists(bbxpath) || !DataStream.LoadBoundingBox(bbxpath, out BoundingBox))
            {
                CreateBoundingBox(Model);

                //Optionally save that new one
                if (RenderingSettings.e_saveBoundingBoxes)
                {
                    DataStream.SaveBoundingBoxData(BoundingBox, bbxpath);
                }
            }

            //Find the middle
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }


        public ModelDefinition(Model model, BoundingBox box)
        {
            Model = model;
            BoundingBox = box;
            BoundingBoxOffset = (BoundingBox.Max + BoundingBox.Min) / 2.0f;
        }
        
        protected void CreateBoundingBox(Model model)
        {
            Vector3[] vertices;
            int[] indices;
            GeometryDataExtractor.GetVerticesAndIndicesFromModel(model, out vertices, out indices);

            BoundingBox = BoundingBox.CreateFromPoints(vertices);
        }

    }

}
