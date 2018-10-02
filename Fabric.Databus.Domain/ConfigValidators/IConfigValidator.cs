using System.Threading.Tasks;

namespace Fabric.Databus.Domain.ConfigValidators
{
    using Fabric.Databus.Shared;

    public interface IConfigValidator
    {
        Task<ConfigValidationResult> ValidateFromText(string fileContents);
    }
}
