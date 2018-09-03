using Sitecore.Pipelines;
using System.Reflection;

namespace PipelineDebug.Reflection.Implementation
{
    public class Sitecore_8_2_7 : Base
    {
        protected override object GetPipelineMethodProcessor(PipelineMethod method)
        {
            return typeof(PipelineMethod).GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(method);
        }
    }
}
