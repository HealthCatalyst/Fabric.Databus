// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestConsoleProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestConsoleProgressLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System;

    /// <summary>
    /// The test console progress logger.
    /// </summary>
    public class TestConsoleProgressLogger : AbstractTextProgressLogger
    {
        /// <inheritdoc />
        public override void Reset()
        {
        }

        /// <inheritdoc />
        public override void AppendLine(string text)
        {
            Console.WriteLine(text);
        }

        /// <inheritdoc />
        public override string GetLog()
        {
            return string.Empty;
        }
    }
}