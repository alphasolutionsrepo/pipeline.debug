using PipelineDebug.Pipelines.PipelineDebug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineDebug.Model
{
    public class ConfigurationDebugProcessor
    {
        //for deserialization
        public ConfigurationDebugProcessor()
        {

        }

        public ConfigurationDebugProcessor(DebugProcessor processor)
        {
            PipelineGroup = processor.PipelineGroup;
            PipelineName = processor.PipelineName;
            PipelineIndex = processor.PipelineIndex;
            Taxonomies = processor.Taxonomies;
        }

        public string PipelineGroup { get; set; }
        public string PipelineName { get; set; }
        public int PipelineIndex { get; set; }
        public List<string> Taxonomies { get; set; }
    }
}
