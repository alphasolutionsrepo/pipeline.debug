using log4net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PipelineDebug.Model;
using PipelineDebug.Settings;
using Sitecore.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace PipelineDebug.Output
{
    public class OutputService : IOutputService
    {
        protected ILog FileLog { get; }
        protected FixedSizeOutput<OutputItem> MemoryLog { get; }
        protected ISettingsService SettingsService;

        public OutputService()
        {
            SettingsService = ServiceLocator.ServiceProvider.GetRequiredService<ISettingsService>();
            FileLog = Sitecore.Diagnostics.LoggerFactory.GetLogger("PipelineDebug");
            MemoryLog = new FixedSizeOutput<OutputItem>(SettingsService.MaxMemoryEntries);
        }
        
        public virtual void Output(OutputItem output)
        {
            if (SettingsService.LogToDiagnostics)
            {
                FileLog.Info(ToLogString(output));
            }
            
            if (SettingsService.LogToMemory)
            {
                if (MemoryLog.Size != SettingsService.MaxMemoryEntries)
                {
                    MemoryLog.Size = SettingsService.MaxMemoryEntries;
                }
                MemoryLog.Enqueue(output);
            }
        }

        public virtual List<OutputItem> GetMemoryOutput(List<string> filterProcessorIds)
        {
            var output = MemoryLog.ToList();

            if (filterProcessorIds != null && filterProcessorIds.Count > 0)
            {
                output = output.Where(oi => !filterProcessorIds.Contains(oi.ProcessorId)).ToList();
            }
            output.Reverse();

            return output;
        }

        protected virtual string ToLogString(OutputItem output)
        {
            return JsonConvert.SerializeObject(output);
        }
    }
}
