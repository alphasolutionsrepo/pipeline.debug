using Sitecore.Pipelines;

namespace PipelineDebug.Settings.Constraints
{
    public class SessionConstraint : IConstraint
    {
        public string SessionId { get; }
        public SessionConstraint(string sessionId)
        {
            this.SessionId = sessionId;
        }

        public bool IsSatisfied(PipelineArgs args)
        {
            return System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Session != null && SessionId == System.Web.HttpContext.Current.Session.SessionID;
        }
    }
}
