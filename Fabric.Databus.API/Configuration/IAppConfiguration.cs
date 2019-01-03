// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAppConfiguration.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IAppConfiguration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API.Configuration
{
    /// <summary>
    /// The AppConfiguration interface.
    /// </summary>
    public interface IAppConfiguration
    {
        /// <summary>
        /// Gets the elastic search settings.
        /// </summary>
        Domain.Configuration.ElasticSearchSettings ElasticSearchSettings { get; }

        /// <summary>
        /// Gets or sets the authority.
        /// </summary>
        string Authority { get; set; }

        /// <summary>
        /// Gets or sets the client id.
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the scopes.
        /// </summary>
        string Scopes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable authorization.
        /// </summary>
        bool EnableAuthorization { get; set; }
    }
}
