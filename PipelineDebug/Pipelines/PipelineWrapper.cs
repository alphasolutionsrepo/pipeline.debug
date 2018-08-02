using PipelineDebug.Model;
using Sitecore.Pipelines;
using System.Collections.Generic;

namespace PipelineDebug.Pipelines
{
    public class PipelineWrapper
    {
        public PipelineWrapper(string group, string name)
        {
            Group = group;
            Name = name;
            Processors = new List<ProcessorWrapper>();
            DiscoveryRoots = new List<DiscoveryItem>();
            Initialized = false;
        }

        public string Group { get; }

        public string Name { get; }

        public CorePipeline CorePipeline { get; set; }

        public List<ProcessorWrapper> Processors { get; }

        public List<DiscoveryItem> DiscoveryRoots { get; }

        public bool Initialized { get; set; }
    }
}
