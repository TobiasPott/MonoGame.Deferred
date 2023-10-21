using System.Diagnostics;


namespace DeferredEngine.Renderer
{

    public class PipelineProfiler
    {
        public const string FieldInfoPrefix = "Sample";

        public static long SampleDrawShadows;
        public static long SampleDrawCubeMap;
        public static long SampleUpdateViewProjection;
        public static long SampleDrawGBuffer;
        public static long SampleDrawScreenSpaceEffect;
        public static long SampleDrawBilateralBlur;
        public static long SampleDrawEnvironmentMap;
        public static long SampleDrawSSR;
        public static long SampleCompose;
        public static long SampleCombineTAA;
        public static long SampleDrawFinalRender;
        public static long SampleTotalRender;



        //Profiler
        public static bool IsProfilerEnabled = false;

        //Performance Profiler
        private readonly Stopwatch _timer = new Stopwatch();
        private long _prevTimestamp;

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// retrieves the current profiler time and sets the profiler timestamp 
        /// </summary>
        public void SampleTimestamp(ref long profilerSample)
        {
            //Performance Profiler
            if (PipelineProfiler.IsProfilerEnabled && IsEnabled)
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
            if (PipelineProfiler.IsProfilerEnabled && IsEnabled)
                _prevTimestamp = _timer.ElapsedTicks;
        }
        /// <summary>
        /// retrieves the current profiler time
        /// </summary>
        public void Sample(ref long profilerSample)
        {
            //Performance Profiler
            if (PipelineProfiler.IsProfilerEnabled && IsEnabled)
                profilerSample = _timer.ElapsedTicks;
        }
        /// <summary>
        /// resets or stops the profiler timer
        /// </summary>
        public void Reset()
        {
            //Profiler
            if (PipelineProfiler.IsProfilerEnabled && IsEnabled)
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