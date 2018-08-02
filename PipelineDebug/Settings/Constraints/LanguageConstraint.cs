using Sitecore.Pipelines;

namespace PipelineDebug.Settings.Constraints
{
    public class LanguageConstraint : IConstraint
    {
        public string Language { get; }
        public LanguageConstraint(string languageName)
        {
            this.Language = languageName;
        }

        public bool IsSatisfied(PipelineArgs args)
        {
            return Sitecore.Context.Language?.Name == Language;
        }
    }
}