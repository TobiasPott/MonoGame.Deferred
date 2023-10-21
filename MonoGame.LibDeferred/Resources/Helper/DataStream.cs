using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace DeferredEngine.Recources.Helper
{
    public class DataStream
    {
        /*
         * I created this data stream to save SDF 32 bit files.
         * Format: int width, int height, int zdepth, float[] data
         * 
         * 
         * 
         * 
         */


        #region SDF Data

        public static void SaveImageData(float[] data, int width, int height, int zdepth, string path)
        {

            // create a byte array and copy the floats into it...

            if (data.Length != width * height * zdepth)
            {
                throw new Exception("Your output dimensions do not match!");
            }

            var byteArray = new byte[data.Length * 4];
            Buffer.BlockCopy(data, 0, byteArray, 0, byteArray.Length);

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);

                //Write resolution first
                BinaryWriter Writer = new BinaryWriter(fs);

                //
                Writer.Write(BitConverter.GetBytes(width));
                Writer.Write(BitConverter.GetBytes(height));
                Writer.Write(BitConverter.GetBytes(zdepth));

                Writer.Write(byteArray);

                Writer.Flush();
                Writer.Close();
                fs.Close();
            }
            finally
            {
                fs?.Dispose();
            }
        }

        /// <summary>
        /// Returns a true and a texture if the file is available. Otherwise false and nulls.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="path"></param>
        /// <param name="zdepth"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static bool LoadFromFile(GraphicsDevice graphics, string path, out int zdepth, out Texture2D output)
        {
            if (LoadFloatArray(path, out float[] data, out int width, out int height, out zdepth))
            {
                output = new Texture2D(graphics, width * zdepth, height, false, SurfaceFormat.Single);
                output.SetData(data);
                return true;
            }
            output = null;
            return false;
        }

        //Returns true if successful, else false
        public static bool LoadFloatArray(string path, out float[] floatArray, out int width, out int height, out int zdepth)
        {
            //Debug.WriteLine(path);  

            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.OpenOrCreate);
                BinaryReader Reader = new BinaryReader(fs);

                width = Reader.ReadInt32();
                height = Reader.ReadInt32();
                zdepth = Reader.ReadInt32();

                byte[] byteArray = Reader.ReadBytes(width * height * zdepth * 4);

                Reader.Close();
                fs.Close();

                floatArray = new float[byteArray.Length / 4];
                Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
            }
            catch (Exception e)
            {
                width = 0;
                height = 0;
                zdepth = 0;
                floatArray = null;

                Debug.WriteLine(e.Message);
                return false;


                //throw e;

            }
            finally
            {
                fs?.Dispose();
            }
            return true;
        }

        #endregion


    }
}
