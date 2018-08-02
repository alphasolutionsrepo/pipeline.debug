using System;
using System.Collections.Generic;

namespace PipelineDebug.Model
{
    public class OutputItem
    {
        public OutputItem(string processorId, string processorName, string argsType)
        {
            Time = DateTime.Now;
            ProcessorId = processorId;
            ProcessorName = processorName;
            Entries = new List<OutputMember>();
            ArgsType = argsType;
        }

        public DateTime Time { get; }
        public string ProcessorId { get; }
        public string ProcessorName { get; }
        public string ArgsType { get; }
        public string RequestId { get; }
        public List<OutputMember> Entries { get; }
    }
}
