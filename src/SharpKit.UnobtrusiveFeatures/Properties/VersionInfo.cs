using System;

namespace SharpKit.UnobtrusiveFeatures
{
    public static class VersionInfo
    {
        internal const string Version = "1.0.0";
        internal const string BetaSuffix = null;

        public static Version GetVersion()
        {
            return new Version(Version);
        }
    }
}
