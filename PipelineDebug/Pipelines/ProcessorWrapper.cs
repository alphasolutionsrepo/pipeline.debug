using PipelineDebug.Pipelines.PipelineDebug;
using Sitecore.Pipelines;

namespace PipelineDebug.Pipelines
{
    public class ProcessorWrapper
    {
        public ProcessorWrapper(CoreProcessor coreProcessor, PipelineWrapper pipeline, DebugProcessor debugProcessor = null)
        {
            CoreProcessor = coreProcessor;
            Pipeline = pipeline;
            DebugProcessor = debugProcessor;
        }

        public CoreProcessor CoreProcessor { get; }
        public PipelineWrapper Pipeline { get; }
        public DebugProcessor DebugProcessor { get; }
    }
}
