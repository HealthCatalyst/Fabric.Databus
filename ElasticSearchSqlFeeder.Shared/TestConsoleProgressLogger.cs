namespace ElasticSearchSqlFeeder.Shared
{
    using System;

    public class TestConsoleProgressLogger : AbstractTextProgressLogger
    {
        public override void Reset()
        {
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