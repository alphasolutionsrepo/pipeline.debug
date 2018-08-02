namespace PipelineDebug.Model.Request
{
    public class MoveProcessorRequest : ProcessorIdRequest
    {
        public int NewIndex { get; set; }
    }
}
