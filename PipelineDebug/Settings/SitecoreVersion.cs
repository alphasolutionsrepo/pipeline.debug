using System;
using System.Linq;
using System.Reflection;

namespace PipelineDebug.Settings
{
    public static class SitecoreVersion
    {
        //We'll allow it to fail if it can't find the SitecoreVersion - in that case I need to figure out what else to do.
        private static string _sitecoreVersion = typeof(Sitecore.Context).Assembly.CustomAttributes.First(a => a.AttributeType == typeof(AssemblyFileVersionAttribute)).ConstructorArguments.First().Value as string;
        public static bool Equals(string version)
        {
            return _sitecoreVersion == version;
        }

        public static bool LesserThan(string version)
        {
            return Compare(version) == -1;
        }

        public static bool GreaterThan(string version)
        {
            return Compare(version) == 1;
        }

        private static int Compare(string version)
        {
            var a = _sitecoreVersion.Split('.');
            var b = version.Split('.');
            for (int i=0; i < Math.Max(a.Length, b.Length); i++)
            {
                if (i > a.Length)
                    return -1;
                if (i > b.Length)
                    return 1;
                var compare = a[i].CompareTo(b[i]);
                if (compare != 0)
                    return compare;
            }
            return 0;
        }

        public struct Versions
        {
            public static string Sitecore9_0_2 = "11.1.2.00461";
        }
    }
}
