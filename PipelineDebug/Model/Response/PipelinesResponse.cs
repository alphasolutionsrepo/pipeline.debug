using System.Collections.Generic;

namespace PipelineDebug.Model.Response
{
    public class PipelinesResponse : BaseResponse
    {
        public Dictionary<string, List<string>> PipelineGroups { get; set; }
    }
}
