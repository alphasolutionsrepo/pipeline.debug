using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PipelineDebug.Model.Response
{
    public class BaseResponse
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResponseStatus Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}
