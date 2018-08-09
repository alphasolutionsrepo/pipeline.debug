using Microsoft.Extensions.DependencyInjection;
using PipelineDebug.Model;
using PipelineDebug.Reflection;
using PipelineDebug.Settings;
using Sitecore.Abstractions;
using Sitecore.DependencyInjection;
using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace PipelineDebug.Pipelines
{
    public class PipelineService : IPipelineService
    {
        protected IReflectionService ReflectionService;
        protected ISettingsService SettingsService;
        protected BaseFactory BaseFactory;
        protected List<PipelineWrapper> ConfiguredPipelines = new List<PipelineWrapper>();
        protected Dictionary<string, ProcessorWrapper> ProcessorMap = new Dictionary<string, ProcessorWrapper>();

        public PipelineService()
        {
            var factory = ServiceLocator.ServiceProvider.GetRequiredService<IReflectionServiceFactory>();
            ReflectionService = factory.GetVersionSpecificService();
            SettingsService = ServiceLocator.ServiceProvider.GetRequiredService<ISettingsService>();
            BaseFactory = ServiceLocator.ServiceProvider.GetRequiredService<BaseFactory>();

            InitializePipelines();
        }
        
        protected virtual void InitializePipelines()
        {
            var nodes = BaseFactory.GetConfigNodes("pipelines/*");
            foreach (XmlNode node in nodes)
            {
                var groupName = node.Attributes["groupName"]?.Value;
                if (groupName != null)
                {
                    ConfiguredPipelines.AddRange(node.SelectNodes("pipelines/*").Cast<XmlNode>().Select(child => new PipelineWrapper(groupName, child.Name)));
                }
                else
                {
                    ConfiguredPipelines.Add(new PipelineWrapper(string.Empty, node.Name));
                }
            }
            ConfiguredPipelines = ConfiguredPipelines.OrderBy(p => p.Group).ThenBy(p => p.Name).ToList();
        }

        protected virtual void InitializePipeline(PipelineWrapper pipeline)
        {
            pipeline.CorePipeline = CorePipelineFactory.GetPipeline(pipeline.Name, pipeline.Group);

            var processors = ReflectionService.GetProcessors(pipeline.CorePipeline);

            var argTypes = GetAllProcessorArgTypes(pipeline.CorePipeline);
            if (argTypes.Count > 1)
            {
                for (var i = 0; i < argTypes.Count; i++)
                {
                    pipeline.DiscoveryRoots.Add(new DiscoveryItem($"{Constants.ArgsName}[{i}]", argTypes[i]));
                }
            }
            else
            {
                pipeline.DiscoveryRoots.Add(new DiscoveryItem(Constants.ArgsName, argTypes.First()));
            }

            pipeline.DiscoveryRoots.Add(new DiscoveryItem(Constants.ContextName, typeof(Sitecore.Context)));
            foreach (var processor in processors)
            {
                pipeline.Processors.Add(new ProcessorWrapper(processor, pipeline));
            }
            pipeline.Initialized = true;
        }
        
        public virtual List<PipelineWrapper> GetPipelines()
        {
            return ConfiguredPipelines;
        }

        public virtual PipelineWrapper GetPipeline(string group, string name)
        {
            var pipeline = GetPipelines().FirstOrDefault(p => p.Group == (group ?? string.Empty) && p.Name == name);
            if (!pipeline.Initialized)
            {
                InitializePipeline(pipeline);
            }
            return pipeline;
        }

        public virtual PipelineWrapper AddDebugProcessor(string group, string name, int index)
        {
            //Create a core processor
            var config = BaseFactory.GetConfigNode("pipelines/pipelineDebug/processor");
            var coreProcessor = new CoreProcessor();
            coreProcessor.Initialize(config);

            //Initialize it so we can add settings
            var debugProcessor = ReflectionService.InitializeDebugProcessor(coreProcessor);
            debugProcessor.PipelineGroup = group;
            debugProcessor.PipelineName = name;
            debugProcessor.PipelineIndex = index;
            debugProcessor.Taxonomies = SettingsService.DefaultTaxonomies;

            //Add it to our simple pipeline
            var pipeline = GetPipeline(group, name);
            var processorReference = new ProcessorWrapper(coreProcessor, pipeline, debugProcessor);
            pipeline.Processors.Insert(index, processorReference);
            ProcessorMap.Add(debugProcessor.Id, processorReference);

            //Set the result as the active processors
            ReflectionService.SetProcessors(pipeline.CorePipeline, pipeline.Processors.Select(wrapper => wrapper.CoreProcessor).ToArray());
            RefreshProcessorIndexes(pipeline);

            return pipeline;
        }

        public virtual PipelineWrapper RemoveDebugProcessor(string id)
        {
            if (ProcessorMap.ContainsKey(id))
            {
                var processorWrapper = ProcessorMap[id];
                var pipeline = processorWrapper.Pipeline;
                processorWrapper.Pipeline.Processors.Remove(processorWrapper);
                ProcessorMap.Remove(id);
                RefreshProcessorIndexes(pipeline);
                ReflectionService.SetProcessors(pipeline.CorePipeline, pipeline.Processors.Select(p => p.CoreProcessor).ToArray());
                return pipeline;
            }
            
            return null;
        }

        public virtual PipelineWrapper MoveDebugProcessor(string id, int newIndex)
        {
            if (ProcessorMap.ContainsKey(id))
            {
                var processorWrapper = ProcessorMap[id];
                var pipeline = processorWrapper.Pipeline;
                pipeline.Processors.Remove(processorWrapper);
                pipeline.Processors.Insert(newIndex, processorWrapper);
                RefreshProcessorIndexes(pipeline);
                ReflectionService.SetProcessors(pipeline.CorePipeline, pipeline.Processors.Select(p => p.CoreProcessor).ToArray());
                return pipeline;
            }

            return null;
        }

        protected virtual void RefreshProcessorIndexes(PipelineWrapper pipeline)
        {
            for (var i = 0; i < pipeline.Processors.Count; i++)
            {
                var processor = pipeline.Processors[i];
                if (processor.DebugProcessor != null)
                {
                    processor.DebugProcessor.PipelineIndex = i;
                }
            }
        }

        /// <summary>
        /// We dont include baseclasses if we find derived, but it's actually possible that we get multiple derived of the same baseclass, thus we return a list
        /// </summary>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        protected virtual List<Type> GetAllProcessorArgTypes(CorePipeline pipeline)
        {
            var types = new List<Type>();
            var processors = ReflectionService.GetProcessors(pipeline);

            foreach (var p in processors)
            {
                var processorType = ReflectionService.GetProcessorType(p);

                var methodInfo = GetMethodInfo(processorType, p.MethodName, new[] { typeof(PipelineArgs) });
                if (methodInfo != null)
                {
                    var type = methodInfo.GetParameters()[0].ParameterType;
                    if (!types.Contains(type) && !types.Any(t => type.IsAssignableFrom(t)))
                    {
                        types.RemoveAll(t => t.IsAssignableFrom(type));
                        types.Add(type);
                    }
                }
            }
            
            return types;
        }

        /// <summary>
        /// For some reason the type.GetMethod failed for me sometimes. Now I get them all and return the first matching (should also be the only one)
        /// </summary>
        /// <returns></returns>
        protected virtual MethodInfo GetMethodInfo(Type processorType, string name, Type[] checkParms)
        {
            var methods = processorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var m in methods)
            {
                if (m.Name != name)
                {
                    continue;
                }
                var methodParms = m.GetParameters();
                if (CheckParameters(methodParms, checkParms))
                {
                    return m;
                }
            }
            return null;
        }

        protected virtual bool CheckParameters(ParameterInfo[] methodParms, Type[] checkParms)
        {
            if (methodParms.Length != checkParms.Length)
            {
                return false;
            }

            for (int i = 0; i < methodParms.Length; i++)
            {
                if (!checkParms[i].IsAssignableFrom(methodParms[i].ParameterType))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual ProcessorWrapper GetProcessor(string id)
        {
            if (ProcessorMap.ContainsKey(id))
            {
                return ProcessorMap[id];
            }
            return null;
        }

        public virtual List<ProcessorWrapper> GetDebugProcessors()
        {
            return ProcessorMap.Select(p => p.Value).Where(p => p.DebugProcessor != null).ToList();
        }
    }
}
