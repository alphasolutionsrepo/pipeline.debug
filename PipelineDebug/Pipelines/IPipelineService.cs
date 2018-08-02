using System.Collections.Generic;

namespace PipelineDebug.Pipelines
{
    public interface IPipelineService
    {
        List<PipelineWrapper> GetPipelines();

        List<ProcessorWrapper> GetDebugProcessors();

        ProcessorWrapper GetProcessor(string id);
        
        PipelineWrapper GetPipeline(string group, string name);

        PipelineWrapper AddDebugProcessor(string group, string name, int index);

        PipelineWrapper RemoveDebugProcessor(string id);

        PipelineWrapper MoveDebugProcessor(string id, int newIndex);

        
    }
}
