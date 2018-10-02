namespace Fabric.Databus.Shared
{
    using System.Text;

    public class StringProgressLogger : AbstractTextProgressLogger
    {
        readonly StringBuilder _logStringBuilder = new StringBuilder();

        public override void Reset()
        {
            this._logStringBuilder.Clear();
        }

        public override void AppendLine(string text)
        {
            this._logStringBuilder.AppendLine(text);
        }

        public override string GetLog()
        {
            return this._logStringBuilder.ToString();
        }
    }
}
