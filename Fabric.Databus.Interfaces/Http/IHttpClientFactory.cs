// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpClientFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the IHttpClientFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Interfaces.Http
{
    using System.Net.Http;

    /// <summary>
    /// The HttpClientFactory interface.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpClient"/>.
        /// </returns>
        HttpClient Create();
    }
}
