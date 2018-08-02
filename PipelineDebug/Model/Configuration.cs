using System.Collections.Generic;

namespace PipelineDebug.Model
{
    public class Configuration
    {
        public Settings Settings { get; set; }

        public List<ConfigurationDebugProcessor> DebugProcessors { get; set; }
    }
}
