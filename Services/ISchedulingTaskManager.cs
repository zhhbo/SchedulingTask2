using System.Collections.Generic;
using Wkong.SchedulingTask.Models;
using Orchard;
using System.Threading.Tasks;
namespace Wkong.SchedulingTask.Services {
    public interface ISchedulingTaskManager  {
        SchedulingTaskModel GetTask(int id);
        void Delete(Models.SchedulingTaskModel job);
        IEnumerable<Models.SchedulingTaskModel> GetTasks(int startIndex, int count);
        Task<IEnumerable<Models.SchedulingTaskModel>> GetTasksAsysc(int startIndex, int count);
        IEnumerable<Models.SchedulingTaskModel> GetAllTask(int startIndex, int pageSize);
        int GetTasksCount();
        IEnumerable<ISchedulingTask> GetSchedulingTasks();
        ISchedulingTask GetSchedulingTaskByMessageName(string name);
    }
}