using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Reflection;

namespace PipelineDebug.Model
{
    public class OutputMember
    {
        public OutputMember(string taxonomy, string value, MemberTypes? memberType)
        {
            Taxonomy = taxonomy;
            Value = value;
            MemberType = memberType;
        }

        public string Taxonomy { get; }
        public string Value { get; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MemberTypes? MemberType { get; }
    }
}
