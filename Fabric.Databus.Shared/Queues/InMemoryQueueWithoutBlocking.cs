// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InMemoryQueueWithoutBlocking.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the InMemoryQueueWithoutBlocking type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Queues
{
    using System.Threading;
    using System.Threading.Tasks;

    using Fabric.Databus.Interfaces.Queues;

    /// <inheritdoc />
    public class InMemoryQueueWithoutBlocking<T> : InMemoryQueue<T>
        where T : class, IQueueItem
    {
        /// <inheritdoc />
        public InMemoryQueueWithoutBlocking(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        public override Task WaitTillEmptyAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
