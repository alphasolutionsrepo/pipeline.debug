using PipelineDebug.Pipelines;

namespace PipelineDebug.Model
{
    public class SimpleProcessor
    {
        public SimpleProcessor(ProcessorWrapper processor)
        {
            if (processor.DebugProcessor != null)
            {
                Name = processor.DebugProcessor.ProcessorName;
                ProcessorId = processor.DebugProcessor.Id;
            }
            else
            {
                Name = processor.CoreProcessor.Name;
            }
        }
        public string Name { get; set; }
        public string ProcessorId { get; set; }
    }
}
