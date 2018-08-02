using Sitecore.Pipelines;

namespace PipelineDebug.Settings.Constraints
{
    public interface IConstraint
    {
        bool IsSatisfied(PipelineArgs args);
    }
}
