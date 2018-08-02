using System.Collections.Generic;

namespace PipelineDebug.Model.Response
{
    public class GetOutputResponse : BaseResponse
    {
        public List<SimpleProcessor> DebugProcessors { get; set; }

        public List<OutputItem> Output { get; set; }
    }
}
