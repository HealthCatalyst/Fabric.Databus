namespace Fabric.Shared.ReliableHttp.Events
{
    using System;

    public class TransientErrorEventArgs : EventArgs
    {
        public TransientErrorEventArgs(string resourceId, string method, Uri fullUri, string statusCode,
                                       string response, int retryCount, int maxRetryCount)
        {
            this.Method = method;
            this.FullUri = fullUri;
            this.StatusCode = statusCode;
            this.Response = response;
            this.RetryCount = retryCount;
            this.MaxRetryCount = maxRetryCount;
            this.ResourceId = resourceId;
        }

        public string Method { get; }
        public Uri FullUri { get; }
        public string StatusCode { get; }
        public string Response { get; }
        public int RetryCount { get; }
        public int MaxRetryCount { get; }
        public int ResourceId { get; }
    }
}