using DeferredEngine.Recources;
using System.Diagnostics;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    MAIN RENDER FUNCTIONS, TheKosmonaut 2016

namespace DeferredEngine.Renderer
{

    public class PipelineProfiler
    {
        public static long d_profileDrawShadows;
        public static long d_profileDrawCubeMap;
        public static long d_profileUpdateViewProjection;
        public static long d_profileDrawGBuffer;
        public static long d_profileDrawScreenSpaceEffect;
        public static long d_profileDrawBilateralBlur;
        public static long d_profileDrawEnvironmentMap;
        public static long d_profileDrawSSR;
        public static long d_profileCompose;
        public static long d_profileCombineTAA;
        public static long d_profileDrawFinalRender;
        public static long d_profileTotalRender;





        //Performance Profiler
        private readonly Stopwatch _timer = new Stopwatch();
        private long _prevTimestamp;

        /// <summary>
        /// retrieves the current profiler time and sets the profiler timestamp 
        /// </summary>
        public void SampleTimestamp(ref long profilerSample)
        {
            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                long currentTime = _timer.ElapsedTicks;
                profilerSample = currentTime - _prevTimestamp;
                _prevTimestamp = currentTime;
            }
        }
        /// <summary>
        /// sets the current profiler timestamp 
        /// </summary>
        public void Timestamp()
        {
            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
                _prevTimestamp = _timer.ElapsedTicks;
        }
        /// <summary>
        /// retrieves the current profiler time
        /// </summary>
        public void Sample(ref long profilerSample)
        {
            //Performance Profiler
            if (RenderingSettings.d_IsProfileEnabled)
                profilerSample = _timer.ElapsedTicks;
        }
        /// <summary>
        /// resets or stops the profiler timer
        /// </summary>
        public void Reset()
        {
            //Profiler
            if (RenderingSettings.d_IsProfileEnabled)
            {
                _timer.Restart();
                _prevTimestamp = 0;
            }
            else if (_timer.IsRunning)
            {
                _timer.Stop();
            }
        }
    }

}