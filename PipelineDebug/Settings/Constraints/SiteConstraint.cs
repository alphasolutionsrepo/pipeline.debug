using Sitecore.Pipelines;

namespace PipelineDebug.Settings.Constraints
{
    public class SiteConstraint : IConstraint
    {
        public string SiteName { get; }
        public SiteConstraint(string siteName)
        {
            this.SiteName = siteName;
        }

        public bool IsSatisfied(PipelineArgs args)
        {
            return Sitecore.Context.Site?.Name == SiteName;
        }
    }
}