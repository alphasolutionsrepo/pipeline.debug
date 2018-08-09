using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PipelineDebug.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PipelineDebug.Model
{
    public class DiscoveryItem
    {
        public DiscoveryItem(string errorMessage, string declaringType)
        {
            Name = errorMessage;
            IsPrimitive = true;
            HasToString = false;
            MemberType = MemberTypes.Custom;
            ProtectionLevel = "Unknown";
            DeclaringType = declaringType;
        }

        public DiscoveryItem(string name, Type discoveryRootType)
        {
            Name = name;
            Type = discoveryRootType;
            Taxonomy = name;
            MemberType = MemberTypes.TypeInfo;
            ProtectionLevel = "public";
            IsPrimitive = false;
            HasToString = false;
            DeclaringType = discoveryRootType.FullName;
        }

        public DiscoveryItem(PropertyInfo propertyInfo, string taxonomy)
        {
            Name = propertyInfo.Name;
            Taxonomy = taxonomy;
            Type = propertyInfo.PropertyType;
            MemberType = MemberTypes.Property;
            DeclaringType = propertyInfo.DeclaringType.FullName;

            if (propertyInfo.GetMethod?.IsPublic ?? false)
            {
                ProtectionLevel = "public";
            }
            else if (propertyInfo.GetMethod?.IsAssembly ?? false)
            {
                ProtectionLevel = "internal";
            }
            else if (propertyInfo.GetMethod?.IsFamily ?? false)
            {
                ProtectionLevel = "protected";
            }
            else if (propertyInfo.GetMethod?.IsPrivate ?? false)
            {
                ProtectionLevel = "private";
            }
            else
            {
                ProtectionLevel = "no getter";
            }
            HasToString = propertyInfo.PropertyType.HasToString();
            IsPrimitive = propertyInfo.PropertyType.IsPrimitive();
        }

        public DiscoveryItem(FieldInfo fieldInfo, string taxonomy)
        {
            Name = fieldInfo.Name;
            Taxonomy = taxonomy;
            Type = fieldInfo.FieldType;
            MemberType = MemberTypes.Field;
            DeclaringType = fieldInfo.DeclaringType.FullName;

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
        public Type Type { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MemberTypes MemberType { get; private set; }
        public string Taxonomy { get; private set; }
        public string TypeName
        {
            get
            {
                return Type?.Name;
            }
        }
        public string TypeFullName
        {
            get
            {
                return Type?.FullName;
            }
        }
        public string Name { get; private set; }
        public string ProtectionLevel { get; private set; }
        public bool IsPrimitive { get; private set; }
        public bool HasToString { get; private set; }
        public List<DiscoveryItem> Members { get; set; }
        public string DeclaringType { get; private set; }
        
    }
}
