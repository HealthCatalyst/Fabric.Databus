using System;
using ElasticSearchSqlFeeder.Interfaces;

namespace ElasticSearchSqlFeeder.Shared
{
    public class ConsoleProgressLogger : AbstractTextProgressLogger
    {
        public override void Reset()
        {
            Console.SetCursorPosition(0, 0);
        }

        public override void AppendLine(string formattableString)
        {
            Console.WriteLine(formattableString);
        }

        public override string GetLog()
        {
            return string.Empty;
        }
    }
}