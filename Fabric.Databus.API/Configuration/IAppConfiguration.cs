namespace Fabric.Databus.API.Configuration
{
		public interface IAppConfiguration
		{
				Domain.Configuration.ElasticSearchSettings ElasticSearchSettings { get; }
				string Authority { get; set; }
				string ClientId { get; set; }
				string ClientSecret { get; set; }
				string Scopes { get; set; }
		}
}
