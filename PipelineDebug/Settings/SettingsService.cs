using Newtonsoft.Json;
using PipelineDebug.Model;
using PipelineDebug.Settings.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PipelineDebug.Settings
{
    public class SettingsService : ISettingsService
    {
        public SettingsService()
        {
            Constraints = new List<IConstraint>();
            MaxEnumerableIterations = Sitecore.Configuration.Settings.GetIntSetting("PipelineDebug.Setting.MaxEnumerableIterations", 100);
            MaxMemoryEntries = Sitecore.Configuration.Settings.GetIntSetting("PipelineDebug.Setting.MaxMemoryEntries", 1000);
            LogToDiagnostics = Sitecore.Configuration.Settings.GetBoolSetting("PipelineDebug.Setting.LogToDiagnostics", true);
            LogToMemory = Sitecore.Configuration.Settings.GetBoolSetting("PipelineDebug.Setting.LogToMemory", true);
            var exclude = Sitecore.Configuration.Settings.GetSetting("PipelineDebug.Setting.ExcludeUrlConstraint");
            if (!string.IsNullOrWhiteSpace(exclude))
            {
                Constraints.Add(new ExcludeUrlConstraint(exclude));
            }

            SetDefaultTaxonomies(Sitecore.Configuration.Settings.GetSetting("PipelineDebug.Setting.DefaultTaxonomies"));
        }

        public virtual void SetSettings(Model.Settings settings)
        {
            Constraints.Clear();
            var sessionId = HttpContext.Current?.Request?.Cookies["ASP.Net_SessionId"]?.Value;
            if (settings.SessionOnly && !string.IsNullOrWhiteSpace(sessionId))
            {
                //Have to use the cookie since the session isn't initialized in this context.
                Constraints.Add(new SessionConstraint(sessionId));
            }
            if (!string.IsNullOrWhiteSpace(settings.Site))
            {
                Constraints.Add(new SiteConstraint(settings.Site));
            }
            if (!string.IsNullOrWhiteSpace(settings.IncludeUrlPattern))
            {
                Constraints.Add(new IncludeUrlConstraint(settings.IncludeUrlPattern));
            }
            if (!string.IsNullOrWhiteSpace(settings.ExcludeUrlPattern))
            {
                Constraints.Add(new ExcludeUrlConstraint(settings.ExcludeUrlPattern));
            }
            if (!string.IsNullOrWhiteSpace(settings.Language))
            {
                Constraints.Add(new LanguageConstraint(settings.Language));
            }

            MaxEnumerableIterations = settings.MaxEnumerableIterations;
            MaxMemoryEntries = settings.MaxMemoryEntries;
            LogToDiagnostics = settings.LogToDiagnostics;
            LogToMemory = settings.LogToMemory;
            SetDefaultTaxonomies(settings.DefaultTaxonomies);
        }

        public virtual Model.Settings GetSettings()
        {
            return new Model.Settings
            {
                SessionOnly = Constraints.OfType<SessionConstraint>().Any(),
                Site = Constraints.OfType<SiteConstraint>().FirstOrDefault()?.SiteName,
                Language = Constraints.OfType<LanguageConstraint>().FirstOrDefault()?.Language,
                IncludeUrlPattern = Constraints.OfType<IncludeUrlConstraint>().FirstOrDefault()?.Pattern,
                ExcludeUrlPattern = Constraints.OfType<ExcludeUrlConstraint>().FirstOrDefault()?.Pattern,
                LogToDiagnostics = LogToDiagnostics,
                LogToMemory = LogToMemory,
                MaxEnumerableIterations = MaxEnumerableIterations,
                MaxMemoryEntries = MaxMemoryEntries,
                DefaultTaxonomies = string.Join("|", DefaultTaxonomies)
            };
        }

        protected void SetDefaultTaxonomies(string taxonomies)
        {
            DefaultTaxonomies = taxonomies.Split('|').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        }

        public virtual List<IConstraint> Constraints { get; protected set; }
        public virtual int MaxEnumerableIterations { get; protected set; }
        public virtual int MaxMemoryEntries { get; protected set; }
        public virtual bool LogToDiagnostics { get; protected set; }
        public virtual bool LogToMemory { get; protected set; }
        public virtual List<string> DefaultTaxonomies { get; protected set; }
        
    }
}
