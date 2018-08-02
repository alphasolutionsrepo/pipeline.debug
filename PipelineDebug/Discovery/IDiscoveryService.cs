using PipelineDebug.Model;
using PipelineDebug.Pipelines;

namespace PipelineDebug.Discovery
{
    public interface IDiscoveryService
    {
        DiscoveryItem Discover(PipelineWrapper pipelineWrapper, string taxonomy);
    }
}
