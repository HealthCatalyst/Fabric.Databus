// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the ConsoleProgressLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared.Loggers
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// The console progress logger.
    /// </summary>
    public class ConsoleProgressLogger : AbstractTextProgressLogger
    {
        /// <inheritdoc />
        public override void Reset()
        {
            Console.SetCursorPosition(0, 0);
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