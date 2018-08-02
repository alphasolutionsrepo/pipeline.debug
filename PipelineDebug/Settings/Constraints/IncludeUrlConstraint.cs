using Sitecore.Pipelines;
using System.Text.RegularExpressions;

namespace PipelineDebug.Settings.Constraints
{
    public class IncludeUrlConstraint : IConstraint
    {
        private Regex _regex;

        public string Pattern { get; }

        public IncludeUrlConstraint(string regexPattern)
        {
            Pattern = regexPattern;
            _regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        }

        public bool IsSatisfied(PipelineArgs args)
        {
            return System.Web.HttpContext.Current?.Request?.Url?.AbsoluteUri != null && _regex.IsMatch(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);
        }
    }
}
