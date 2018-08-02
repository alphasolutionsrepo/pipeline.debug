using PipelineDebug.Model;
using PipelineDebug.Pipelines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PipelineDebug.Discovery
{
    public class DiscoveryService : IDiscoveryService
    {
        public virtual DiscoveryItem Discover(PipelineWrapper pipelineWrapper, string taxonomy)
        {
            if (string.IsNullOrWhiteSpace(taxonomy))
            {
                return null;
            }

            var path = taxonomy.Split('.').ToList();
            if (path.Count == 0)
            {
                return null;
            }
            var rootName = path[0];
            path.RemoveAt(0);

            var current = pipelineWrapper.DiscoveryRoots.FirstOrDefault(r => r.TypeName == rootName);
            if (current == null)
            {
                return null;
            }

            while (true)
            {
                if (current.Members == null)
                {
                    current.Members = new List<DiscoveryItem>();
                    GetMembersRecursive(current.Type, current);
                    return current;
                }

                if (path.Count == 0)
                {
                    break;
                }

                var name = path[0];
                path.RemoveAt(0);
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                current = current.Members.FirstOrDefault(f => f.Name == name);

                if (current == null)
                {
                    throw new Exception("Discovery could not find the member " + name);
                }
            }

            return null;
        }

        protected virtual void GetMembersRecursive(Type type, DiscoveryItem item)
        {
            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var genericType = type.EnumerableType();
                if (genericType != null)
                {
                    GetMembersRecursive(genericType, item);
                    return;
                }
                else
                {
                    //If it wasn't IEnumberable<T> or inherited from IEnumerable<T> then it's unhandled right now
                    item.Members.Add(new DiscoveryItem("Unhandled IEnumerable type"));

                    return;
                }
            }

            var members = type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var mem in members)
            {
                var newTaxonomy = $"{item.Taxonomy}.{mem.Name}";
                if (!item.Members.Any(di => mem.Name == di.Name))
                if (mem.MemberType == MemberTypes.Field)
                {
                    var fld = (FieldInfo)mem;
                    if (fld.IsDefined(typeof(CompilerGeneratedAttribute), true))
                        continue;
                    item.Members.Add(new DiscoveryItem(fld, newTaxonomy));
                }
                else if (mem.MemberType == MemberTypes.Property)
                {
                    var prop = (PropertyInfo)mem;
                    if (prop.IsDefined(typeof(CompilerGeneratedAttribute), true))
                        continue;
                        item.Members.Add(new DiscoveryItem(prop, newTaxonomy));
                }
            }
            if (type.BaseType != null)
            {
                GetMembersRecursive(type.BaseType, item);
            }
        }
    }
}
