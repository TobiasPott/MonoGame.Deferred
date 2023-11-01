namespace DeferredEngine.Pipeline
{
    public class GlobalEvents
    {

        public static event Action FrameStarted;
        public static void OnFrameStarted() => FrameStarted?.Invoke();


    }
}
