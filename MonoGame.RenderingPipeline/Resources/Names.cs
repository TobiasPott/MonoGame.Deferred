//#define FORWARDONLY

namespace DeferredEngine.Recources
{
    public static class Names
    {
        public const string Albedo = nameof(Albedo);
        public const string Normal = nameof(Normal);
        public const string Roughness = nameof(Roughness);
        public const string Metallic = nameof(Metallic);
        public const string Mask = nameof(Mask);
        public const string Displacement = nameof(Displacement);

        // ToDo: Add compiler flag to switch between GL and DX10 sampler naming
        private const string _samplerFormat = "{0}Sampler+{0}";
        public static string Sampler(string name) => string.Format(_samplerFormat, name);
    }
}
