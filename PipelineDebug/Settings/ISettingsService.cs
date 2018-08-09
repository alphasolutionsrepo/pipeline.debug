using PipelineDebug.Model;
using PipelineDebug.Settings.Constraints;
using System.Collections.Generic;

namespace PipelineDebug.Settings
{
    public interface ISettingsService
    {
        void SetSettings(Model.Settings settings);
        Model.Settings GetSettings();
        List<IConstraint> Constraints { get; }
        int MaxEnumerableIterations { get; }
        int MaxMemoryEntries { get; }
        bool LogToDiagnostics { get; }
        bool LogToMemory { get; }
        List<string> DefaultTaxonomies { get; }
    }
}
