using Microsoft.Extensions.DependencyInjection;
using PipelineDebug.Model;
using PipelineDebug.Output;
using PipelineDebug.Settings;
using Sitecore.DependencyInjection;
using Sitecore.Pipelines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PipelineDebug.Pipelines.PipelineDebug
{
    public class DebugProcessor
    {
        protected ISettingsService SettingsService = ServiceLocator.ServiceProvider.GetRequiredService<ISettingsService>();
        protected IOutputService OutputService = ServiceLocator.ServiceProvider.GetRequiredService<IOutputService>();

        public DebugProcessor()
        {
            Id = Guid.NewGuid().ToString();
            Taxonomies = new List<string>();
        }

        public virtual string Id { get; }
        public virtual string PipelineGroup { get; set; }
        public virtual string PipelineName { get; set;  }
        public virtual int PipelineIndex { get; set; }
        public virtual string ProcessorName
        {
            get
            {
                var group = string.IsNullOrWhiteSpace(PipelineGroup) ? string.Empty : $"{PipelineGroup}\\";
                return $"DebugProcessor: {group}{PipelineName}[{PipelineIndex}]";
            }
        }

        public virtual List<string> Taxonomies { get; set; }
        
        public virtual void Process(PipelineArgs args)
        {
            if (SettingsService.Constraints.Any(c => !c.IsSatisfied(args)))
            {
                return;
            }

            var output = new OutputItem(this.Id, this.ProcessorName, args.GetType().FullName);

            try
            {
                foreach (var taxonomy in Taxonomies)
                {
                    if (string.IsNullOrWhiteSpace(taxonomy))
                        continue;

                    if (taxonomy.StartsWith(Constants.ContextName))
                    {
                        var removedName = taxonomy.Substring(Constants.ContextName.Length + 1);
                        var hierarchy = removedName.Split('.').ToList();
                        OutputTaxonomyValueRecursive(new Sitecore.Context(), hierarchy, Constants.ContextName, output);
                    }
                    else if (taxonomy.StartsWith(Constants.ArgsName))
                    {
                        string removedName;
                        if (taxonomy.Contains("]"))
                        {
                            removedName = taxonomy.Substring(taxonomy.IndexOf(']') + 2);
                        }
                        else
                        {
                            removedName = taxonomy.Substring(Constants.ArgsName.Length + 1);
                        }
                        var hierarchy = removedName.Split('.').ToList();
                        OutputTaxonomyValueRecursive(args, hierarchy, Constants.ArgsName, output);
                    }
                }
            }
            catch (Exception ex)
            {
                output.Entries.Add(new OutputMember("ERROR", ex.Message, null));
            }
            finally
            {
                OutputService.Output(output);
            }
        }

        protected virtual void OutputTaxonomyValueRecursive(object obj, List<string> hierarchy, string currentTaxonomy, OutputItem output)
        {
            //extract the next item in taxonomy to get from hierarchy and remove it.
            var name = hierarchy[0];
            hierarchy.RemoveAt(0);

            //check for members
            var members = GetMemberInfos(obj, obj.GetType(), name);
            
            //if no members existed, the type did not match the expected (can happen if a pipeline has different pipelineargs)
            if (members.Length == 0)
            {
                output.Entries.Add(new OutputMember(currentTaxonomy, $"Member {name} not found.", null));
                return;
            }

            //add the current member to the taxonomy
            currentTaxonomy += "." + name;

            foreach (var info in members)
            {
                object member = null;
                switch (info.MemberType)
                {
                    case MemberTypes.Field:
                        member = ((FieldInfo)info).GetValue(obj);
                        break;
                    case MemberTypes.Property:
                        member = ((PropertyInfo)info).GetValue(obj);
                        break;
                }

                if (member == null)
                {
                    output.Entries.Add(new OutputMember(currentTaxonomy, "null", info.MemberType));
                    continue;
                }

                //If the member is IEnumerable (and not a string), enumerate and treat each of them.
                if (!(member is string) && member is IEnumerable)
                {
                    int i = 0;
                    bool any = false;

                    foreach (var entry in ((IEnumerable)member))
                    {
                        if (i >= SettingsService.MaxEnumerableIterations)
                        {
                            break;
                        }

                        any = true;

                        var enumTaxonomy = currentTaxonomy + "[" + i + "]";
                        if (hierarchy.Count == 0)
                        {
                            output.Entries.Add(new OutputMember(currentTaxonomy, entry.ToString(), info.MemberType));
                        }
                        else
                        {
                            OutputTaxonomyValueRecursive(entry, hierarchy, enumTaxonomy, output);
                        }

                        i++;
                    }
                    if (!any)
                    {
                        output.Entries.Add(new OutputMember(currentTaxonomy, "IEnumerable empty", info.MemberType));
                    }
                }
                else
                {
                    if (hierarchy.Count == 0)
                    {
                        output.Entries.Add(new OutputMember(currentTaxonomy, member.ToString(), info.MemberType));
                    }
                    else
                    {
                        OutputTaxonomyValueRecursive(member, hierarchy, currentTaxonomy, output);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the members with the given name from the current type.
        /// In case no members are found, we check if they exist on basetype(s), and returns the first found.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <param name="type">Type of the object (basetype when called recursively)</param>
        /// <param name="name">Name of the member</param>
        /// <returns></returns>
        protected virtual MemberInfo[] GetMemberInfos(object obj, Type type, string name)
        {
            var members = type.GetMember(name, MemberTypes.Field | MemberTypes.Property, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (members.Length == 0 && type.BaseType != null)
            {
                return GetMemberInfos(obj, type.BaseType, name);
            }
            return members;
        }
    }
}
