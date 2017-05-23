using System.Threading.Tasks;
using ElasticSearchSqlFeeder.Shared;

namespace Fabric.Databus.Domain.ConfigValidators
{
    public interface IConfigValidator
    {
        Task<ConfigValidationResult> ValidateFromText(string fileContents);
    }
}
