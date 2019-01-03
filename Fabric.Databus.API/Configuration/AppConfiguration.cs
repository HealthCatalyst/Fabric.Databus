// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppConfiguration.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AppConfiguration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Configuration
{
    /// <inheritdoc />
    public class AppConfiguration : IAppConfiguration
    {
        /// <inheritdoc />
        public Domain.Configuration.ElasticSearchSettings ElasticSearchSettings { get; set; }

        /// <inheritdoc />
        public string Authority { get; set; }

        /// <inheritdoc />
        public string ClientId { get; set; }

        /// <inheritdoc />
        public string ClientSecret { get; set; }

        /// <inheritdoc />
        public string Scopes { get; set; }

        /// <inheritdoc />
        public bool EnableAuthorization { get; set; }
    }
}
