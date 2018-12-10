namespace Fabric.Shared.ReliableHttp.Events
{
    using System;
    using System.ComponentModel;

    public class NavigatingEventArgs : CancelEventArgs
    {
        public NavigatingEventArgs(string resourceId, string method, Uri fullUri)
        {
            this.FullUri = fullUri;
            this.Method = method;
            this.ResourceId = resourceId;
        }

        public string Method { get; set; }
        public int ResourceId { get; }
        public Uri FullUri { get; set; }
    }
}