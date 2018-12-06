// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThreadIdEnricher.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ThreadIdEnricher type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System.Threading;

    using Serilog.Core;
    using Serilog.Events;

    /// <inheritdoc />
    /// <summary>
    /// The thread id enricher.
    /// </summary>
    public class ThreadIdEnricher : ILogEventEnricher
    {
        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ThreadId", Thread.CurrentThread.ManagedThreadId));
        }
    }
}
