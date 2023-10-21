using DeferredEngine.Recources;
using Microsoft.Xna.Framework;

namespace DeferredEngine.Rendering
{
    public class HaltonSequence
    {
        private const int HaltonSequenceLength = 16;

        private Vector3[] _haltonSequence;
        private int _haltonSequenceIndex = -1;

        /// <summary>
        /// The halton sequence is a good way to create good distribution
        /// I use a 2,3 sequence
        /// https://en.wikipedia.org/wiki/Halton_sequence
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNext(bool rebuild = false)
        {
            Vector2 invResolution = RenderingSettings.g_ScreenInverseResolution * 2;
            //First time? Create the sequence
            if (_haltonSequence == null)
            {
                _haltonSequence = new Vector3[HaltonSequenceLength];
                rebuild = true;
            }
            if (rebuild)
            {
                for (int index = 0; index < HaltonSequenceLength; index++)
                {
                    for (int baseValue = 2; baseValue <= 3; baseValue++)
                    {
                        float result = 0;
                        float f = 1;
                        int i = index + 1;

                        while (i > 0)
                        {
                            f /= baseValue;
                            result += f * (i % baseValue);
                            i /= baseValue; //floor / int()
                        }

                        if (baseValue == 2)
                            _haltonSequence[index].X = (result - 0.5f) * invResolution.X;
                        else
                            _haltonSequence[index].Y = (result - 0.5f) * invResolution.Y;
                    }
                }
            }
            _haltonSequenceIndex++;
            if (_haltonSequenceIndex >= HaltonSequenceLength) _haltonSequenceIndex = 0;
            return _haltonSequence[_haltonSequenceIndex];
        }

    }

}

