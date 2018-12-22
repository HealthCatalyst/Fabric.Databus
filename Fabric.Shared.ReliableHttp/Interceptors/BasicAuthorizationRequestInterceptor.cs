// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BasicAuthorizationRequestInterceptor.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the BasicAuthorizationRequestInterceptor type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Shared.ReliableHttp.Interceptors
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Fabric.Shared.ReliableHttp.Interfaces;

    /// <inheritdoc />
    /// <summary>
    /// The basic authorization request interceptor.
    /// </summary>
    public class BasicAuthorizationRequestInterceptor : IHttpRequestInterceptor
    {
        /// <summary>
        /// The username.
        /// </summary>
        private readonly string username;

        /// <summary>
        /// The password.
        /// </summary>
        private readonly string password;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicAuthorizationRequestInterceptor"/> class.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        public BasicAuthorizationRequestInterceptor(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        /// <inheritdoc />
        public Task InterceptRequestAsync(string requestId, HttpMethod method, HttpRequestMessage request)
        {
            this.AddAuthorizationToken(request);
            return Task.CompletedTask;
        }

        /// <summary>
        /// The add authorization token.
        /// </summary>
        /// <param name="requestMessage">
        /// The request Message.
        /// </param>
        private void AddAuthorizationToken(HttpRequestMessage requestMessage)
        {
            if (!string.IsNullOrEmpty(this.username) && !string.IsNullOrEmpty(this.password))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{this.username}:{this.password}");
                requestMessage.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }
    }
}
