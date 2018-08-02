using System.Collections.Generic;

namespace PipelineDebug.Model.Response
{
    public class DiscoveryRootsResponse : BaseResponse
    {
        public List<DiscoveryItem> DiscoveryRoots { get; set; }

        public List<string> Taxonomies { get; set; }
    }
}
