using Sitecore.Pipelines;
using System.Reflection;

namespace PipelineDebug.Reflection.Implementation
{
    public class Sitecore_9_0_2 : Base
    {
        protected override object GetPipelineMethodProcessor(PipelineMethod method)
        {
            return typeof(PipelineMethod).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(method);
        }
    }
}
