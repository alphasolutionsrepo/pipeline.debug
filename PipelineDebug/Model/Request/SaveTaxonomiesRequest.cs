using System.Collections.Generic;

namespace PipelineDebug.Model.Request
{
    public class SaveTaxonomiesRequest : ProcessorIdRequest
    {
        public List<string> Taxonomies { get; set; }
    }
}
