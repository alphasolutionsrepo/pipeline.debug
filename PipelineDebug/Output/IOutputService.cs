using PipelineDebug.Model;
using System.Collections.Generic;

namespace PipelineDebug.Output
{
    public interface IOutputService
    {
        
        void Output(OutputItem item);

        List<OutputItem> GetMemoryOutput(List<string> filterProcessorIds);
    }
}
