using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using Orchard.Events;
using Microsoft.Extensions.Logging;
using Wkong.SchedulingTask.Models;
using Microsoft.AspNetCore.Modules;
using System.Threading.Tasks;
namespace Wkong.SchedulingTask.Services
{
    public class SchedulingTaskProcessor : ISchedulingTaskProcessor {
        private readonly ISchedulingTaskManager _schedulingTaskManager;
        private readonly IClock _clock;
        //private readonly Work<>
        private readonly IEventBus _eventBus;
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();
        private readonly ISchedulingTaskService _schedulingTaskService;

        public SchedulingTaskProcessor(
            IClock clock,
            ISchedulingTaskManager schedulingTaskManager,
            ILogger<SchedulingTaskProcessor> logger,
            ISchedulingTaskService schedulingTaskService,
            IEventBus eventBus) {
            _clock = clock;
            _schedulingTaskManager = schedulingTaskManager;
            _eventBus = eventBus;
            Logger =logger;
            _schedulingTaskService = schedulingTaskService;
        }

        public ILogger Logger { get; set; }
        public async Task ProcessTask() {
            // prevent two threads on the same machine to process the message queue
            if (_rwl.TryEnterWriteLock(0)) {
                try {
                    //_taskLeaseService.Value.Acquire("SchedulingTaskProcessor", _clock.Value.UtcNow.AddMinutes(10));
                    IEnumerable<Models.SchedulingTaskModel> messages = await _schedulingTaskManager.GetTasksAsysc(0, 10);

                    while ((messages = _schedulingTaskManager.GetTasks(0, 10).ToArray()).Any()) {
                        foreach (var message in messages)
                        {                            
                                ProcessMessage(message);                            
                        }
                    }
                }
                finally {
                    _rwl.ExitWriteLock();
                }
            }
        }

        private void ProcessMessage(Models.SchedulingTaskModel task) {

            Logger.LogDebug("Processing task {0}.", task.Id);

            try {
                if (task == null)
                    return;
                var payload = JObject.Parse(task.Parameters);
                var parameters = payload.ToDictionary();

                _eventBus.NotifyAsync(task.Message, parameters);

                Logger.LogDebug("Processed task Id {0}.", task.Id);
            }
            catch (Exception e) {
                Logger.LogError("An unexpected error while processing task {0}. Error message: {1}.", task.Id, e);
            }
            finally {

                    switch (task.Frequency)
                    {
                        case -2:
                            task.ScheduledUtc = task.ScheduledUtc.AddMinutes(task.SpaceNum);
                            _schedulingTaskService.EditTaskAsync(task);
                            break;
                        case -1:
                           task.ScheduledUtc= task.ScheduledUtc.AddHours(task.SpaceNum);
                            _schedulingTaskService.EditTaskAsync(task);
                            break;
                        case 1:
                            task.ScheduledUtc = task.ScheduledUtc.AddDays(task.SpaceNum);
                            _schedulingTaskService.EditTaskAsync(task);
                            break;
                        case 2:
                            task.ScheduledUtc = task.ScheduledUtc.AddDays(task.SpaceNum * 7);
                            _schedulingTaskService.EditTaskAsync(task);
                            break;
                        case 3:
                            task.ScheduledUtc = task.ScheduledUtc.AddMonths(task.SpaceNum);
                            _schedulingTaskService.EditTaskAsync(task);
                            break;
                        default:
                            _schedulingTaskManager.Delete(task);
                            break;
                    }

            }
        }
    }

    public static class JObjectExtensions
    {
        public static IDictionary<string, object> ToDic(this string paprs)
        {
            var payload = JObject.Parse(paprs);
            var parameters = payload.ToDictionary();
            return parameters;
        }

        public static IDictionary<string, object> ToDictionary(this JObject jObject) {
            return (IDictionary<string, object>)Convert(jObject);
        }

        private static object Convert(this JToken jToken) {
            if (jToken == null) {
                throw new ArgumentNullException();
            }

            switch (jToken.Type) {
                case JTokenType.Array:
                    var array = jToken as JArray;
                    return array.Values().Select(Convert).ToArray();
                case JTokenType.Object:
                    var obj = jToken as JObject;
                    return obj
                        .Properties()
                        .ToDictionary(property => property.Name, property => Convert(property.Value));
                default:
                    return jToken.ToObject<object>();
            }
        }
    }
}