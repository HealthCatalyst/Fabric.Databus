// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleBlockingCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the SimpleBlockingCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Collections.Concurrent;
    using System.Linq;

    using Fabric.Databus.Interfaces;

    using Serilog;
    using Serilog.Core;

    /// <summary>
    /// The simple blocking collection.
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public class SimpleBlockingCollection<T> : IMeteredBlockingCollection<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Logger Logger = new LoggerConfiguration().CreateLogger();

        private readonly BlockingCollection<T> _blockingCollection;

        private readonly string _name;

        public SimpleBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name)
        {
            this._blockingCollection = new BlockingCollection<T>(concurrentQueue);
            this._name = name;
        }

        public SimpleBlockingCollection(IProducerConsumerCollection<T> concurrentQueue, string name, int maxItems)
            : this(concurrentQueue, name)
        {
        }

        public T Take()
        {
            var item = this._blockingCollection.Take();

            return item;
        }

        public void Add(T item)
        {
            this._blockingCollection.Add(item);
        }


        public bool Any()
        {
            return this._blockingCollection.Any();
        }

        public bool TryTake(out T cacheItem)
        {
            var result = this._blockingCollection.TryTake(out cacheItem);

            return result;
        }

        public int Count => this._blockingCollection.Count;
        public bool IsCompleted => this._blockingCollection.IsCompleted;

        public void CompleteAdding()
        {
            this._blockingCollection.CompleteAdding();
        }

        // ReSharper disable once ConvertToAutoProperty
        public string Name => this._name;
    }
}
