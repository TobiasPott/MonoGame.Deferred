using MonoGame.Ext;
using System.Diagnostics;


namespace DeferredEngine.Pipeline
{

    public class PipelineSamples
    {
        public const string FieldInfoPrefix = "S";

        public static double SDraw_Shadows => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_Shadows);
        //public static double SDraw_CubeMap => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_CubeMap);
        public static double SDraw_GBuffer => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_GBuffer);
        public static double SDraw_SSFx_SSAO => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_SSFx_SSAO);
        //public static double SDraw_SSFx_AO_BilateralBlur => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_SSFx_AO_BilateralBlur);
        public static double SDraw_EnvironmentMap => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_EnvironmentMap);
        public static double SDraw_SSFx_SSR => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_SSFx_SSR);
        public static double SDraw_DeferredCompose => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_DeferredCompose);
        public static double SDraw_CombineTAA => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_CombineTAA);
        public static double SDraw_FinalRender => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_FinalRender);
        public static double SDraw_TotalRender => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_Total);

        public static double SUpdate_ViewProjection => PipelineProfiler.GetTimestamp(TimestampIndices.Update_ViewProjection);
        public static double SUpdate_SDF => PipelineProfiler.GetTimestamp(TimestampIndices.Update_SDF);

        public static double SDraw_EditorPrePass => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_EditorPrePass);
        public static double SDraw_EditorPass => PipelineProfiler.GetTimestamp(TimestampIndices.Draw_EditorPass);
    }

    public class TimestampIndices
    {
        public const int Draw_Shadows = 1;
        //public const int Draw_CubeMap = 2;
        public const int Draw_GBuffer = 3;
        public const int Draw_SSFx_SSAO = 4;
        //public const int Draw_SSFx_AO_BilateralBlur = 5;
        public const int Draw_EnvironmentMap = 6;
        public const int Draw_SSFx_SSR = 7;
        public const int Draw_DeferredCompose = 8;
        public const int Draw_CombineTAA = 9;
        public const int Draw_Bloom = 10;
        public const int Draw_FinalRender = 11;
        public const int Draw_Total = 12;

        public const int Update_ViewProjection = 13;
        public const int Update_SDF = 14;

        public const int Draw_EditorPrePass = 15;
        public const int Draw_EditorPass = 16;
    }


    public class PipelineProfiler
    {
        public readonly static NotifiedProperty<bool> ModuleEnabled = new NotifiedProperty<bool>(true);
        //public static bool IsProfilerEnabled = false;


        public const int InitialTimestamps = 64;
        private static readonly double[] _timestamps = new double[InitialTimestamps];
        public static double GetTimestamp(int index) => _timestamps[index];


        //Profiler

        //Performance Profiler
        private readonly Stopwatch _timer = new Stopwatch();
        private double _lastTimestamp;
        private bool _enabled = true;
        public bool Enabled { get => _enabled && ModuleEnabled; set => _enabled = value; }


        /// <summary>
        /// retrieves the current profiler time and sets the profiler timestamp 
        /// </summary>
        public void SampleTimestamp(int timestampIndex)
        {
            //Performance Profiler
            if (Enabled)
            {
                double currentTime = _timer.Elapsed.TotalMilliseconds;
                _timestamps[timestampIndex] = currentTime - _lastTimestamp;
                _lastTimestamp = currentTime;
            }
        }
        /// <summary>
        /// retrieves the current profiler time
        /// </summary>
        public void Sample(int timestampIndex)
        {
            //Performance Profiler
            if (Enabled)
                _timestamps[timestampIndex] = _timer.Elapsed.TotalMilliseconds;
        }


        /// <summary>
        /// sets the current profiler timestamp 
        /// </summary>
        public void Timestamp()
        {
            //Performance Profiler
            if (Enabled)
                _lastTimestamp = _timer.ElapsedTicks;
        }
        /// <summary>
        /// resets or stops the profiler timer
        /// </summary>
        public void Reset()
        {
            //Profiler
            if (Enabled)
            {
                _timer.Restart();
                _lastTimestamp = 0;
                for (int i = 0; i < _timestamps.Length; i++)
                    _timestamps[i] = 0;
            }
            else if (_timer.IsRunning)
            {
                _timer.Stop();
            }
        }
    }

}