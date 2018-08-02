namespace PipelineDebug.Reflection
{
    public interface IReflectionServiceFactory
    {
        IReflectionService GetVersionSpecificService();
    }
}
