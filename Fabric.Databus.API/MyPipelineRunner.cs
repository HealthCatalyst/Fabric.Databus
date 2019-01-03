// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyPipelineRunner.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the MyPipelineRunner type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Fabric.Databus.API
{
    using System.Threading;

    using Fabric.Databus.Config;
    using Fabric.Databus.Domain.Importers;
    using Fabric.Databus.Domain.Jobs;
    using Fabric.Databus.PipelineRunner;

    using Unity;

    /// <inheritdoc />
    public class MyPipelineRunner : IImportRunner
    {
        /// <inheritdoc />
        public void RunPipeline(IJob job)
        {
            throw new System.NotImplementedException();
        }

        /// <inheritdoc />
        public void RunPipeline(IJob job, IJobStatusTracker jobStatusTracker)
        {
            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var container = new UnityContainer();
                var pipelineRunner = new PipelineRunner(container, cancellationTokenSource.Token);

                pipelineRunner.RunPipeline(job);
            }
        }
    }
}