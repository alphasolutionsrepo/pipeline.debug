using Microsoft.Extensions.DependencyInjection;
using PipelineDebug.Discovery;
using PipelineDebug.Model;
using PipelineDebug.Model.Request;
using PipelineDebug.Model.Response;
using PipelineDebug.Output;
using PipelineDebug.Pipelines;
using PipelineDebug.Security;
using PipelineDebug.Settings;
using PipelineDebug.Settings.Constraints;
using Sitecore.DependencyInjection;
using Sitecore.Security.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace PipelineDebug.Controllers
{
    [AdministratorOnly]
    public class PipelineDebugController : ApiController
    {
        protected IPipelineService CorePipelineService;
        protected IDiscoveryService DiscoveryService;
        protected ISettingsService SettingsService;
        protected IOutputService OutputService;

        public PipelineDebugController()
        {
            CorePipelineService = ServiceLocator.ServiceProvider.GetRequiredService<IPipelineService>();
            DiscoveryService = ServiceLocator.ServiceProvider.GetRequiredService<IDiscoveryService>();
            SettingsService = ServiceLocator.ServiceProvider.GetRequiredService<ISettingsService>();
            OutputService = ServiceLocator.ServiceProvider.GetRequiredService<IOutputService>();
        }

        [HttpPost]
        [AllowAnonymous]
        public virtual BaseResponse Login(LoginRequest request)
        {
            if (AuthenticationManager.Login(request.UserName, request.Password, true) && Sitecore.Context.IsAdministrator)
            {
                return new BaseResponse()
                {
                    Status = ResponseStatus.Success
                };
            }
            return new BaseResponse()
            {
                Status = ResponseStatus.Unauthorized
            };
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual BaseResponse Logout()
        {
            AuthenticationManager.Logout();
            return new BaseResponse()
            {
                Status = ResponseStatus.Success
            };
        }

        [HttpGet]
        public virtual PipelinesResponse ListPipelines()
        {
            return new PipelinesResponse()
            {
                Status = ResponseStatus.Success,
                PipelineGroups = CorePipelineService.GetPipelines().GroupBy(p => p.Group).ToDictionary(group => group.Key, group => group.Select(p => p.Name).ToList())
            };
        }

        [HttpPost]
        public virtual PipelineDetailsResponse PipelineDetails(NamedPipelineRequest request)
        {
            var pipeline = CorePipelineService.GetPipeline(request.Group ?? string.Empty, request.Name);

            return new PipelineDetailsResponse()
            {
                Status = ResponseStatus.Success,
                Group = pipeline.Group,
                Pipeline = pipeline.Name,
                Processors = pipeline.Processors.Select(p => new SimpleProcessor(p)).ToList()
            };
        }

        [HttpPost]
        public virtual PipelineDetailsResponse AddProcessor(AddProcessorRequest request)
        {
            var pipeline = CorePipelineService.AddDebugProcessor(request.Group, request.Name, request.Index);
            return new PipelineDetailsResponse()
            {
                Status = ResponseStatus.Success,
                Group = pipeline.Group,
                Pipeline = pipeline.Name,
                Processors = pipeline.Processors.Select(p => new SimpleProcessor(p)).ToList()
            };
        }

        [HttpPost]
        public virtual BaseResponse RemoveProcessor(ProcessorIdRequest request)
        {
            var pipeline = CorePipelineService.RemoveDebugProcessor(request.ProcessorId);
            if (pipeline != null)
            {
                return new PipelineDetailsResponse()
                {
                    Status = ResponseStatus.Success,
                    Group = pipeline.Group,
                    Pipeline = pipeline.Name,
                    Processors = pipeline.Processors.Select(p => new SimpleProcessor(p)).ToList()
                };
            }
            return new BaseResponse()
            {
                Status = ResponseStatus.Error,
                ErrorMessage = "Unable to remove Processor"
            };
        }

        [HttpPost]
        public virtual BaseResponse MoveProcessor(MoveProcessorRequest request)
        {
            var pipeline = CorePipelineService.MoveDebugProcessor(request.ProcessorId, request.NewIndex);
            if (pipeline != null)
            {
                return new PipelineDetailsResponse()
                {
                    Status = ResponseStatus.Success,
                    Group = pipeline.Group,
                    Pipeline = pipeline.Name,
                    Processors = pipeline.Processors.Select(p => new SimpleProcessor(p)).ToList()
                };
            }
            return new BaseResponse()
            {
                Status = ResponseStatus.Error,
                ErrorMessage = "Unable to remove Processor"
            };
        }

        [HttpPost]
        public virtual GetProcessorSettingsResponse GetProcessorSettings(ProcessorIdRequest request)
        {
            var processor = CorePipelineService.GetProcessor(request.ProcessorId ?? string.Empty);

            return new GetProcessorSettingsResponse()
            {
                Status = ResponseStatus.Success,
                Id = processor?.DebugProcessor?.Id,
                Taxonomies = processor?.DebugProcessor?.Taxonomies,
                DiscoveryRoots = processor?.Pipeline?.DiscoveryRoots
            };
        }

        [HttpGet]
        public virtual GetDebugProcessorsResponse GetDebugProcessors()
        {
            return new GetDebugProcessorsResponse()
            {
                Status = ResponseStatus.Success,
                DebugProcessors = CorePipelineService.GetDebugProcessors().Select(p => new SimpleProcessor(p)).ToList()
            };
        }

        [HttpPost]
        public virtual GetOutputResponse GetOutput(OutputRequest request)
        {
            return new GetOutputResponse()
            {
                Status = ResponseStatus.Success,
                Output = OutputService.GetMemoryOutput(request?.FilterProcessorIds),
                DebugProcessors = CorePipelineService.GetDebugProcessors().Select(p => new SimpleProcessor(p)).ToList()
            };
        }

        [HttpGet]
        public virtual BaseResponse GetSettings()
        {
            return new GetSettingsResponse()
            {
                Status = ResponseStatus.Success,
                Settings = SettingsService.ToSettings()
            };
        }

        [HttpPost]
        public virtual BaseResponse SaveSettings(SaveSettingsRequest request)
        {
            SettingsService.FromSettings(request);

            return new BaseResponse()
            {
                Status = ResponseStatus.Success
            };
        }

        [HttpPost]
        public virtual BaseResponse SaveProcessorTaxonomies(SaveTaxonomiesRequest request)
        {
            var processor = CorePipelineService.GetProcessor(request.ProcessorId)?.DebugProcessor;
            processor.Taxonomies = request.Taxonomies ?? new List<string>();

            return new BaseResponse()
            {
                Status = ResponseStatus.Success
            };
        }

        [HttpPost]
        public virtual DiscoveryRootsResponse GetDiscoveryRoots(ProcessorIdRequest request)
        {
            var processor = CorePipelineService.GetProcessor(request.ProcessorId);

            return new DiscoveryRootsResponse()
            {
                Status = ResponseStatus.Success,
                DiscoveryRoots = processor.Pipeline.DiscoveryRoots,
                Taxonomies = processor.DebugProcessor.Taxonomies
            };
        }

        [HttpPost]
        public virtual DiscoveryResponse Discover(DiscoverRequest request)
        {
            var processor = CorePipelineService.GetProcessor(request.ProcessorId);
            var item = DiscoveryService.Discover(processor.Pipeline, request.Taxonomy);
            
            return new DiscoveryResponse()
            {
                Status = ResponseStatus.Success,
                DiscoveryItem = item
            };
        }

        [HttpPost]
        public virtual BaseResponse ImportConfiguration(ImportConfigurationRequest request)
        {
            //First we remove all current processors
            foreach (var processor in CorePipelineService.GetDebugProcessors())
            {
                CorePipelineService.RemoveDebugProcessor(processor.DebugProcessor.Id);
            }

            //Set settings
            SettingsService.FromSettings(request.Settings);

            //Add debug processors - make sure to take lowest indexes first, or they will get odd indexes
            foreach (var processor in request.DebugProcessors.OrderBy(p => p.PipelineIndex))
            {
                var pipeline = CorePipelineService.AddDebugProcessor(processor.PipelineGroup, processor.PipelineName, processor.PipelineIndex);
                pipeline.Processors[processor.PipelineIndex].DebugProcessor.Taxonomies = processor.Taxonomies;
            }

            return new BaseResponse()
            {
                Status = ResponseStatus.Success
            };
        }

        [HttpGet]
        public virtual ExportConfigurationResponse ExportConfiguration()
        {
            var config = new Configuration()
            {
                Settings = SettingsService.ToSettings(),
                DebugProcessors = CorePipelineService.GetDebugProcessors().Select(p => new ConfigurationDebugProcessor(p.DebugProcessor)).ToList()
            };

            return new ExportConfigurationResponse()
            {
                Status = ResponseStatus.Success,
                Configuration = config
            };
        }
    }
}

