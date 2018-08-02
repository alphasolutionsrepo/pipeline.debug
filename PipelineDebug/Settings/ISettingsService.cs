using PipelineDebug.Settings.Constraints;
using System.Collections.Generic;

namespace PipelineDebug.Settings
{
    public interface ISettingsService
    {
        void FromSettings(Model.Settings settings);
        Model.Settings ToSettings();
        List<IConstraint> Constraints { get; }
        int MaxEnumerableIterations { get; }
        int MaxMemoryEntries { get; }
        bool LogToDiagnostics { get; }
        bool LogToMemory { get; }
    }
}
