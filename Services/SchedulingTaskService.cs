using Newtonsoft.Json;
using Wkong.SchedulingTask.Models;
using Microsoft.AspNetCore.Modules;
using System;
using System.Collections.Generic;
using Orchard.ContentManagement;
using YesSql.Core.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using Dapper;
namespace Wkong.SchedulingTask.Services
{
    public class SchedulingTaskService : ISchedulingTaskService {
        private readonly IClock _clock;
        private readonly IStore _store;
        private readonly string _tablePrefix;
        private readonly List<SchedulingTaskModel> _tasksQueue = new List<SchedulingTaskModel>();
        public SchedulingTaskService(
            IClock clock,
            IStore store,
            ILogger<SchedulingTaskService> logger) {
            _clock = clock;
            _store = store;
            Logger = logger;
            _tablePrefix = store.Configuration.TablePrefix;
        }
        public ILogger Logger { get; set; }
        public void Dispose()
        {
            FlushAsync().Wait();
        }

        private async Task FlushAsync()
        {
            List<SchedulingTaskModel> localQueue;

            lock (_tasksQueue)
            {
                localQueue = new List<SchedulingTaskModel>(_tasksQueue);
            }

            if (!localQueue.Any())
            {
                return;
            }

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix }{ nameof(SchedulingTaskModel)}";

                var insertCmd = $"insert into [{table}] ([Priority],[Message],[TaskName], [Parameters],[ScheduledUtc],[CreatedUtc], [CanExecute],[Frequency],[SpaceNum]) values (@Priority,@Message,@TaskName,@Parameters,@ScheduledUtc,@CreatedUtc,@CanExecute,@Frequency,@SpaceNum);";
                await connection.ExecuteAsync(insertCmd, _tasksQueue, transaction);
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while updating indexing tasks", e);
                throw;
            }
            finally
            {
                transaction.Commit();
                transaction.Dispose();

                if (_store.Configuration.ConnectionFactory.Disposable)
                {
                    connection.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }

            _tasksQueue.Clear();
        }


        public Task EnqueueAsync(string taskName,string message, int priority,DateTime scheduledUtc,object parameters ) {

            var schedulingTask = new Models.SchedulingTaskModel {
                Parameters = JsonConvert.SerializeObject(parameters),
                Message = message,
                CreatedUtc = _clock.UtcNow,
                Priority = priority,
                ScheduledUtc= scheduledUtc,
                CanExecute=true,
                TaskName= taskName
            };


            lock (_tasksQueue)
            {
                _tasksQueue.Add(schedulingTask);
            }

            return Task.CompletedTask;
        }
        public async Task<SchedulingTaskModel> EditTaskAsync(int id, object parameters)
        {
            await FlushAsync();
            SchedulingTaskModel schedulingTask ;
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                schedulingTask= await connection.QueryFirstAsync<SchedulingTaskModel>($"select top 1 * from [{table}] where Id = @Id", new { Id = id }, transaction);

                schedulingTask.Parameters = JsonConvert.SerializeObject(new { parameters = parameters }); ;//
                schedulingTask.CanExecute = true;


                var updateCmd = $" UPDATE [{table}] SET [Parameters]= @Parameters, [CanExecute]=@CanExecute where Id=@id";
                await connection.ExecuteAsync(updateCmd, new { Id=id,CanExecute=true,Parameters = schedulingTask.Parameters }, transaction);

                return schedulingTask;
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while reading indexing tasks", e);
                throw;
            }
            finally
            {
                transaction.Commit();
                transaction.Dispose();

                if (_store.Configuration.ConnectionFactory.Disposable)
                {
                    connection.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }
           


        }
        public async Task EditTaskAsync(SchedulingTaskModel model)
        {
            await FlushAsync();
            //SchedulingTaskModel schedulingTask;
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                var updateCmd = $" UPDATE [{table}] SET [Priority]=@Priority,[Message]=@Message,[TaskName]=@TaskName, [Parameters]= @Parameters,[ScheduledUtc]=@ScheduledUtc,[CreatedUtc]=@CreatedUtc, [CanExecute]=@CanExecute,[Frequency]=@Frequency,[SpaceNum]=@SpaceNum where Id=@Id";
                await connection.ExecuteAsync(updateCmd,model, transaction);
                //schedulingTask = await connection.QueryFirstAsync<SchedulingTaskModel>($"select top 1 * from [{table}] where Id = @Id", new { Id = model.Id }, transaction);

                return;
            }
            catch (Exception e)
            {
                Logger.LogError("An error occured while reading indexing tasks", e);
                throw;
            }
            finally
            {
                transaction.Commit();
                transaction.Dispose();

                if (_store.Configuration.ConnectionFactory.Disposable)
                {
                    connection.Dispose();
                }
                else
                {
                    connection.Close();
                }
            }



        }
        public Task EnqueueAsync(string taskName, string message, int priority, DateTime scheduledUtc, int Frequency, int SpaceNum)
        {

            var schedulingTask = new Models.SchedulingTaskModel
            {
                //Parameters = JsonConvert.SerializeObject(parameters),
                Message = message,
                CreatedUtc = _clock.UtcNow,
                Priority = priority,
                ScheduledUtc = scheduledUtc,
                TaskName = taskName,
                CanExecute = false,
                Frequency = Frequency,
                SpaceNum = SpaceNum
            };

            lock (_tasksQueue)
            {
                _tasksQueue.Add(schedulingTask);
            }

            return Task.CompletedTask;
        }
        public Task EnqueueAsync(string taskName, string message, int priority, DateTime scheduledUtc, int Frequency, int SpaceNum, object parameters)
        {

            var schedulingTask = new Models.SchedulingTaskModel
            {
                Parameters = JsonConvert.SerializeObject(new { parameters = parameters }),
                Message = message,
                CreatedUtc = _clock.UtcNow,
                Priority = priority,
                ScheduledUtc = scheduledUtc,
                TaskName = taskName,
                CanExecute = true,
                Frequency = Frequency,
                SpaceNum = SpaceNum
            };
            lock (_tasksQueue)
            {
                _tasksQueue.Add(schedulingTask);
            }

            return Task.CompletedTask;
        }
    }
}