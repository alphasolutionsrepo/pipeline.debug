using PipelineDebug.Pipelines.PipelineDebug;
using Sitecore.Pipelines;
using System;
using System.Reflection;
using System.Xml;

namespace PipelineDebug.Reflection.Implementation
{
    public class Base : IReflectionService
    {
        public virtual Type GetProcessorType(CoreProcessor processor)
        {
            if (processor == null)
            {
                return null;
            }

            //if it's already initialized we can get it from method
            var method = GetCoreProcessorMethod(processor);
            if (method != null)
            {
                return GetPipelineMethodProcessor(method).GetType();
            }
            //otherwise we create an object from config
            else
            {
                var config = GetProcessorConfig(processor);
                var processorObject = GetProcessorObject(config);
                return processorObject.Object.GetType();
            }
        }

        public virtual PipelineMethod GetCoreProcessorMethod(CoreProcessor processor)
        {
            return typeof(CoreProcessor).GetField("_method", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(processor) as PipelineMethod;
        }

        /// <summary>
        /// First initializes the method on the object and then findes the processor from the object
        /// </summary>
        public virtual DebugProcessor InitializeDebugProcessor(CoreProcessor processor)
        {
            var getMethod = typeof(CoreProcessor).GetMethod("GetMethod", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null);
            var pipelineMethod = getMethod.Invoke(processor, new object[] { new PipelineArgs() }) as PipelineMethod;
            return GetPipelineMethodProcessor(pipelineMethod) as DebugProcessor;
        }

        protected virtual XmlNode GetProcessorConfig(CoreProcessor processor)
        {
            return typeof(CoreProcessor).GetField("_configNode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(processor) as XmlNode;
        }

        protected virtual ProcessorObject GetProcessorObject(XmlNode config)
        {
            return typeof(CorePipelineFactory).GetMethod("GetProcessorObject", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { config }) as ProcessorObject;
        }

        protected virtual object GetPipelineMethodProcessor(PipelineMethod method)
        {
            return typeof(PipelineMethod).GetField("_object", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(method);
        }

        public virtual CoreProcessor[] GetProcessors(CorePipeline pipeline)
        {
            return typeof(CorePipeline).GetField("_processors", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(pipeline) as CoreProcessor[];
        }

        public virtual void SetProcessors(CorePipeline pipeline, CoreProcessor[] processors)
        {
            typeof(CorePipeline).GetField("_processors", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(pipeline, processors);
        }
    }
}
