using System.Collections.Generic;

namespace ElasticSearchSqlFeeder.Shared
{
    public class ConfigValidationResult
    {
        public bool Success { get; set; }
        public List<string> Results { get; set; }
        public string ErrorText { get; set; }
    }
}