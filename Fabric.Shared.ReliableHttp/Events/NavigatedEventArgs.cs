namespace Fabric.Shared.ReliableHttp.Events
{
    using System;

    public class NavigatedEventArgs : EventArgs
    {
        public NavigatedEventArgs(string resourceId, string method, Uri fullUri, string statusCode, string response)
        {
            this.Method = method;
            this.FullUri = fullUri;
            this.StatusCode = statusCode;
            this.Response = response;
            this.ResourceId = resourceId;
        }

        public string Method { get; set; }

        public string StatusCode { get; set; }
        public string Response { get; }
        public int ResourceId { get; }
        public Uri FullUri { get; set; }
    }
}