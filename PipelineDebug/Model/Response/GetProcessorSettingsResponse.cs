using System.Collections.Generic;

namespace PipelineDebug.Model.Response
{
    public class GetProcessorSettingsResponse : BaseResponse
    {
        public string Id { get; set; }
        public List<string> Taxonomies { get; set; }
        public List<DiscoveryItem> DiscoveryRoots { get; set; }
    }
}
