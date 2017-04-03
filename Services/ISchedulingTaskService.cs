using Orchard.Events;
using Wkong.SchedulingTask.Models;
using System;
using Orchard;
using System.Threading.Tasks;
namespace Wkong.SchedulingTask.Services {
    public interface ISchedulingTaskService {
        Task EnqueueAsync(string taskName, string message, int priority, DateTime scheduledUtc, object parameters);
        Task<SchedulingTaskModel> EditTaskAsync(int id, object parameters);
        Task EditTaskAsync(Models.SchedulingTaskModel model);
        Task EnqueueAsync(string taskName, string message, int priority, DateTime scheduledUtc, int Frequency, int SpaceNum);
        Task EnqueueAsync(string taskName, string message, int priority, DateTime scheduledUtc, int Frequency, int SpaceNum, object parameters);
    }
}