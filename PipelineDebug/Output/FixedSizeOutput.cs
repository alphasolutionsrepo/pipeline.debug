using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PipelineDebug.Output
{
    public class FixedSizeOutput<T> : ConcurrentQueue<T>, IReadOnlyCollection<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; set; }

        public FixedSizeOutput(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                if (Size < 1)
                {
                    return;
                }

                while (base.Count > Size)
                {
                    T discard;
                    base.TryDequeue(out discard);
                }
            }
        }
    }
}
