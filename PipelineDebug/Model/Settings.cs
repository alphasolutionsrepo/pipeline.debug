namespace PipelineDebug.Model
{
    public class Settings
    {
        public bool SessionOnly { get; set; }
        public string Site { get; set; }
        public string Language { get; set; }
        public string IncludeUrlPattern { get; set; }
        public string ExcludeUrlPattern { get; set; }

        public bool LogToDiagnostics { get; set; }
        public bool LogToMemory { get; set; }
        public int MaxEnumerableIterations { get; set; }
        public int MaxMemoryEntries { get; set; }
    }
}
