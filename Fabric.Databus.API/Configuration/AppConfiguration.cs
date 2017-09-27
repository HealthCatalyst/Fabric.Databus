using Fabric.Platform.Shared.Configuration;

namespace Fabric.Databus.API.Configuration
{
		public class AppConfiguration : IAppConfiguration
		{
				public IdentityServerConfidentialClientSettings IdentityServerConfidentialClientSettings { get; set; }

				public Domain.Configuration.ElasticSearchSettings ElasticSearchSettings { get; set; }
		}
}
