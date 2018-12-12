// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyThreadIdEnricher.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MyThreadIdEnricher type.
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
    public class MyThreadIdEnricher : ILogEventEnricher
    {
        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ThreadId", Thread.CurrentThread.ManagedThreadId));
        }
    }
}
