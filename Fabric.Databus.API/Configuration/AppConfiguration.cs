namespace Fabric.Databus.API.Configuration
{
		public class AppConfiguration : IAppConfiguration
		{
				public Domain.Configuration.ElasticSearchSettings ElasticSearchSettings { get; set; }
				public string Authority { get; set; }
				public string ClientId { get; set; }
				public string ClientSecret { get; set; }
				public string Scopes { get; set; }
		}
}
