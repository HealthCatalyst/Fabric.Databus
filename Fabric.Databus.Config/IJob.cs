namespace Fabric.Databus.Config
{
    using ElasticSearchSqlFeeder.Interfaces;

    public interface IJob
    {
        IQueryConfig Config { get; set; }

        IJobData Data { get; set; }
    }
}