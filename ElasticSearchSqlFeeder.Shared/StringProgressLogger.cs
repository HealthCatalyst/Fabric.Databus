using System.Text;

namespace ElasticSearchSqlFeeder.Shared
{
    public class StringProgressLogger : AbstractTextProgressLogger
    {
        readonly StringBuilder _logStringBuilder = new StringBuilder();

        public override void Reset()
        {
            _logStringBuilder.Clear();
        }

        public override void AppendLine(string formattableString)
        {
            _logStringBuilder.AppendLine(formattableString);
        }

        public override string GetLog()
        {
            return _logStringBuilder.ToString();
        }
    }
}
