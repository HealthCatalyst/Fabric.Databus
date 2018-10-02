// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestConsoleProgressLogger.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the TestConsoleProgressLogger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System;

    /// <summary>
    /// The test console progress logger.
    /// </summary>
    public class TestConsoleProgressLogger : AbstractTextProgressLogger
    {
        public override void Reset()
        {
        }

        public override void AppendLine(string text)
        {
            Console.WriteLine(text);
        }

        public override string GetLog()
        {
            return string.Empty;
        }
    }
}