using System.Diagnostics;


namespace DeferredEngine.Rendering
{

    public class PipelineSamples
    {
        public const string FieldInfoPrefix = "S";

        public static long SDraw_Shadows;
        public static long SDraw_CubeMap;
        public static long SDraw_GBuffer;
        public static long SDraw_SSFx_SSAO;
        public static long SDraw_SSFx_AO_BilateralBlur;
        public static long SDraw_EnvironmentMap;
        public static long SDraw_SSFx_SSR;
        public static long SDraw_Compose;
        public static long SDraw_CombineTAA;
        public static long SDraw_FinalRender;
        public static long SDraw_TotalRender;

        public static long SUpdate_ViewProjection;
    }


    public class PipelineProfiler
    {


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