using System.Collections.Generic;

namespace PipelineDebug.Model.Response
{
    public class PipelineDetailsResponse : BaseResponse
    {
        public string Group { get; set; }
        public string Pipeline { get; set; }
        public List<SimpleProcessor> Processors { get; set; }
    }
}
