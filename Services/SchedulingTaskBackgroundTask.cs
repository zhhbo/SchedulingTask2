using System;
using System.Threading;
using Orchard.BackgroundTasks;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
namespace Wkong.SchedulingTask.Services {
    public class SchedulingTaskBackgroundTask : IBackgroundTask
    {


        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken){
            var _schedulingTaskProcessor = serviceProvider.GetService<ISchedulingTaskProcessor>();
            return _schedulingTaskProcessor.ProcessTask();
        }
    }
}