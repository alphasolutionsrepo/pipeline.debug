using PipelineDebug.Model;
using PipelineDebug.Pipelines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            var current = pipelineWrapper.DiscoveryRoots.FirstOrDefault(r => taxonomy.StartsWith(r.Name));
            if (current == null)
            {
                return null;
            }

            var path = taxonomy.Substring(current.Name.Length).Split('.').ToList();

            while (true)
            {
                if (current.Members == null)
                {
                    current.Members = new List<DiscoveryItem>();
                    GetMembers(current.Type, current);
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

        protected virtual void GetMembers(Type type, DiscoveryItem item)
        {
            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var genericType = type.EnumerableType();
                if (genericType != null)
                {
                    GetMembers(genericType, item);
                    return;
                }

                var indexer = type.StringIndexParameter();
                if (indexer != null)
                {
                    var valueType = indexer.PropertyType;
                    var generic = typeof(KeyValuePair<,>);
                    var constructed = generic.MakeGenericType(new Type[] { typeof(string), valueType });
                    GetMembers(constructed, item);
                    return;
                    //item.Members.Add(new DiscoveryItem(Constants.KeyValuePairKey, $"{item.Taxonomy}.{Constants.KeyValuePairKey}", typeof(string), indexer.DeclaringType));
                    //item.Members.Add(new DiscoveryItem(Constants.KeyValuePairValue, $"{item.Taxonomy}.{Constants.KeyValuePairValue}", indexer.PropertyType, indexer.DeclaringType));
                    //return;
                }

                //If it wasn't IEnumberable<T> or inherited from IEnumerable<T> 
                //Or had a string indexer
                //then it's unhandled right now
                item.Members.Add(new DiscoveryItem("Unhandled IEnumerable type", type.FullName));
                return;
            }

            var members = type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            foreach (var mem in members)
            {
                //We don't support explicit interface implementations as of now. That would require casting on the other end which doesn't make it it that easy
                if (mem.Name.Contains("."))
                {
                    continue;
                }
                if (!item.Members.Any(di => mem.Name == di.Name))
                {
                    var newTaxonomy = $"{item.Taxonomy}.{mem.Name}";

                    if (mem.MemberType == MemberTypes.Field)
                    {
                        var fld = (FieldInfo)mem;
                        if (fld.IsDefined(typeof(CompilerGeneratedAttribute), true))
                        {
                            continue;
                        }
                        item.Members.Add(new DiscoveryItem(fld, newTaxonomy));
                    }
                    else if (mem.MemberType == MemberTypes.Property)
                    {
                        var prop = (PropertyInfo)mem;
                        if (prop.IsDefined(typeof(CompilerGeneratedAttribute), true))
                        {
                            continue;
                        }
                        item.Members.Add(new DiscoveryItem(prop, newTaxonomy));
                    }
                }
            }
            if (type.BaseType != null)
            {
                GetMembers(type.BaseType, item);
            }
        }
    }
}
