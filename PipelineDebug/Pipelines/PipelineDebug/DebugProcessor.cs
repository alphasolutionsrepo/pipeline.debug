using Microsoft.Extensions.DependencyInjection;
using PipelineDebug.Discovery;
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

        protected List<string> taxonomies;
        public virtual List<string> Taxonomies {
            get
            {
                return taxonomies;
            }
            set
            {
                taxonomies = value;
                hierarchy = new TaxonomyHierarchy(string.Empty, value);
            }
        }

        protected TaxonomyHierarchy hierarchy;
        
        public virtual void Process(PipelineArgs args)
        {
            if (SettingsService.Constraints.Any(c => !c.IsSatisfied(args)))
            {
                return;
            }

            var output = new OutputItem(this.Id, this.ProcessorName, args.GetType().FullName);

            try
            {
                foreach (var subhierarchy in hierarchy.Children)
                {
                    if (string.IsNullOrWhiteSpace(subhierarchy.Name))
                        continue;

                    if (subhierarchy.Name == Constants.ContextName)
                    {
                        foreach (var child in subhierarchy.Children)
                        {
                            OutputTaxonomyValueRecursive(new Sitecore.Context(), child, Constants.ContextName, output);
                        }
                    }
                    else if (subhierarchy.Name == Constants.ArgsName)
                    {
                        foreach (var child in subhierarchy.Children)
                        {
                            OutputTaxonomyValueRecursive(args, child, Constants.ArgsName, output);
                        }
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

        protected virtual void OutputTaxonomyValueRecursive(object obj, TaxonomyHierarchy hierarchy, string currentTaxonomy, OutputItem output)
        {
            //check for members
            var members = GetMemberInfos(obj, obj.GetType(), hierarchy.Name);
            
            //if no members existed, the type did not match the expected (can happen if a pipeline has different pipelineargs)
            if (members.Length == 0)
            {
                output.Entries.Add(new OutputMember(currentTaxonomy, $"Member {hierarchy.Name} not found.", null));
                return;
            }

            //add the current member to the taxonomy
            currentTaxonomy += "." + hierarchy.Name;

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

                    //We use stringindexing if it's not a generic IEnumerable and there's a string indexer
                    var stringIndexParam = member.GetType().StringIndexParameter();
                    var stringIndexed = member.GetType().EnumerableType() == null && stringIndexParam != null;

                    foreach (var entry in ((IEnumerable)member))
                    {
                        if (i >= SettingsService.MaxEnumerableIterations)
                        {
                            break;
                        }

                        any = true;
                        var enumTaxonomy = currentTaxonomy + "[" + i + "]";
                        
                        if (hierarchy.Selected)
                        {
                            output.Entries.Add(new OutputMember(currentTaxonomy, entry.ToString(), info.MemberType));
                        }

                        foreach (var child in hierarchy.Children)
                        {
                            if (stringIndexed)
                            {
                                var value = stringIndexParam.GetValue(member, new object[] { entry });
                                var kvp = new KeyValuePair<string, object>((string)entry, value);
                                OutputTaxonomyValueRecursive(kvp, child, enumTaxonomy, output);
                            }
                            else
                            {
                                OutputTaxonomyValueRecursive(entry, child, enumTaxonomy, output);
                            }
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
                    if (hierarchy.Selected)
                    {
                        output.Entries.Add(new OutputMember(currentTaxonomy, member.ToString(), info.MemberType));
                    }

                    foreach (var child in hierarchy.Children)
                    {
                        OutputTaxonomyValueRecursive(member, child, currentTaxonomy, output);
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
