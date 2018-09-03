using PipelineDebug.Reflection.Implementation;
using PipelineDebug.Settings;

namespace PipelineDebug.Reflection
{
    public class ReflectionServiceFactory : IReflectionServiceFactory
    {
        protected IReflectionService _implementation;

        public ReflectionServiceFactory()
        {
			if (SitecoreVersion.Equals(SitecoreVersion.Versions.Sitecore8_2_7))
			{
				_implementation = new Sitecore_8_2_7();
			}
			else if (!SitecoreVersion.LesserThan(SitecoreVersion.Versions.Sitecore9_0_2))
			{
				_implementation = new Sitecore_9_0_2();
			}
			else
			{
				_implementation = new Base();
			}
        }

        public virtual IReflectionService GetVersionSpecificService()
        {
            return _implementation;
        }
    }
}
