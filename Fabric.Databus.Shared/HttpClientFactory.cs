// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpClientFactory.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the HttpClientFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.Shared
{
    using System.Net.Http;

    using Fabric.Databus.Interfaces.Http;

    /// <inheritdoc />
    /// <summary>
    /// The http client factory.
    /// </summary>
    public class HttpClientFactory : IHttpClientFactory
    {
        /// <inheritdoc />
        public HttpClient Create()
        {
            return new HttpClient();
        }
    }
}
