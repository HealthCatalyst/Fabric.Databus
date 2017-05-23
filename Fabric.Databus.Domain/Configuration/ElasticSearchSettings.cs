using System;

namespace Fabric.Databus.Domain.Configuration
{
    public class ElasticSearchSettings
    {
        public string Scheme { get; set; }
        public string Server { get; set; }

        public string Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Uri GetElasticSearchUri()
        {
            if (string.IsNullOrEmpty(Scheme) || string.IsNullOrEmpty(Server) ||
                string.IsNullOrEmpty(Port))
            {
                throw new ArgumentException("You must specify Scheme, Server and Port for elastic search.");
            }

            if (!string.IsNullOrEmpty(Username) &&
                !string.IsNullOrEmpty(Password))
            {
                return new Uri(
                    $"{Scheme}://{Username}:{Password}@{Server}:{Port}");
            }

            return new Uri($"{Scheme}://{Server}:{Port}");


        }

    }
}
