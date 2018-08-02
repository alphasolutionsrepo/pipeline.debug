using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PipelineDebug.Discovery;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PipelineDebug.Model
{
    public class DiscoveryItem
    {
        public DiscoveryItem(string errorMessage)
        {
            TypeName = "Error";
            Name = errorMessage;
            IsPrimitive = true;
            HasToString = false;
            MemberType = MemberTypes.Custom;
            ProtectionLevel = "Unknown";
        }

        public DiscoveryItem(Type discoveryRootType)
        {
            Name = string.Empty;
            Type = discoveryRootType;
            Taxonomy = discoveryRootType.Name;
            TypeName = discoveryRootType.AsString();
            MemberType = MemberTypes.TypeInfo;
            ProtectionLevel = "public";
            IsPrimitive = false;
            HasToString = false;
        }

        public DiscoveryItem(PropertyInfo propertyInfo, string taxonomy)
        {
            Name = propertyInfo.Name;
            Taxonomy = taxonomy;
            Type = propertyInfo.PropertyType;
            TypeName = propertyInfo.PropertyType.AsString();
            MemberType = MemberTypes.Property;

            if (propertyInfo.GetMethod.IsPublic)
            {
                ProtectionLevel = "public";
            }
            else if (propertyInfo.GetMethod.IsAssembly)
            {
                ProtectionLevel = "internal";
            }
            else if (propertyInfo.GetMethod.IsFamily)
            {
                ProtectionLevel = "protected";
            }
            else if (propertyInfo.GetMethod.IsPrivate)
            {
                ProtectionLevel = "private";
            }
            else
            {
                ProtectionLevel = "unknown";
            }
            HasToString = propertyInfo.PropertyType.HasToString();
            IsPrimitive = propertyInfo.PropertyType.IsPrimitive();
        }

        public DiscoveryItem(FieldInfo fieldInfo, string taxonomy)
        {
            Name = fieldInfo.Name;
            Taxonomy = taxonomy;
            Type = fieldInfo.FieldType;
            TypeName = fieldInfo.FieldType.AsString();
            MemberType = MemberTypes.Field;
            
            if (fieldInfo.IsPublic)
            {
                ProtectionLevel = "public";
            }
            else if (fieldInfo.IsAssembly)
            {
                ProtectionLevel = "internal";
            }
            else if (fieldInfo.IsFamily)
            {
                ProtectionLevel = "protected";
            }
            else if (fieldInfo.IsPrivate)
            {
                ProtectionLevel = "private";
            }
            else
            {
                ProtectionLevel = "unknown";
            }
            HasToString = fieldInfo.FieldType.HasToString();
            IsPrimitive = fieldInfo.FieldType.IsPrimitive();
        }

        [JsonIgnore]
        public Type Type { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MemberTypes MemberType { get; }
        public string Taxonomy { get; }
        public string TypeName { get; }
        public string Name { get; }
        public string ProtectionLevel { get; }
        public bool IsPrimitive { get; }
        public bool HasToString { get; }
        public List<DiscoveryItem> Members { get; set; }
    }
}
