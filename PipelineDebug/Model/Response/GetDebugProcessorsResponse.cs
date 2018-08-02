using System.Collections.Generic;

namespace PipelineDebug.Model.Response
{
    public class GetDebugProcessorsResponse : BaseResponse
    {
        public List<SimpleProcessor> DebugProcessors { get; set; }
    }
}
