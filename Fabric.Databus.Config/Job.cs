namespace Fabric.Databus.Config
{
    using ElasticSearchSqlFeeder.Interfaces;

    public class Job : IJob
    {
        public IQueryConfig Config { get; set; }

        public IJobData Data { get; set; }
    }
}