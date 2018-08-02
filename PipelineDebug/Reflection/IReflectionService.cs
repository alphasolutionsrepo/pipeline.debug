using PipelineDebug.Pipelines.PipelineDebug;
using Sitecore.Pipelines;
using System;

namespace PipelineDebug.Reflection
{
    public interface IReflectionService
    {
        DebugProcessor InitializeDebugProcessor(CoreProcessor processor);

        PipelineMethod GetCoreProcessorMethod(CoreProcessor processor);

        Type GetProcessorType(CoreProcessor processor);

        CoreProcessor[] GetProcessors(CorePipeline pipeline);

        void SetProcessors(CorePipeline pipeline, CoreProcessor[] processors);

    }
}
