using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticSearchSqlFeeder.Shared
{
    public static class SqlTypeToElasticSearchTypeConvertor
    {
        public static string GetElasticSearchType(Type type)
        {
            if (type == typeof(string)) return "keyword"; //TODO; use text or keyword

            if (type == typeof(int)) return "integer";

            if (type == typeof(DateTime)) return "date";

            if (type == typeof(Decimal)) return "double";

            if (type == typeof(double)) return "double";

            if (type == typeof(Int64)) return "integer";

            throw new NotImplementedException("No Elastic Search type found for type=" + type);
        }
    }
}
