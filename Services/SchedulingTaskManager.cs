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
using System.Collections;
namespace Wkong.SchedulingTask.Services
{
    public class SchedulingTaskManager : ISchedulingTaskManager {
        private readonly IClock _clock;
        private readonly IStore _store;
        private readonly string _tablePrefix;
        private IEnumerable<ISchedulingTask> _schedulingTasks;
        private readonly List<SchedulingTaskModel> _tasksQueue = new List<SchedulingTaskModel>();
        public SchedulingTaskManager(
            IClock clock,
            IStore store,
            IEnumerable<ISchedulingTask> tasks,
            ILogger<SchedulingTaskService> logger)
        {
            _clock = clock;
            _store = store;
            Logger = logger;
            _schedulingTasks = tasks;
            _tablePrefix = store.Configuration.TablePrefix;
        }
        public ILogger Logger { get; set; }

        public int GetTasksCount() {
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
             connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                var updateCmd = $" select count(*) From [{table}] ";
                return connection.ExecuteScalar<int>(updateCmd, transaction);

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

        public IEnumerable<Models.SchedulingTaskModel> GetTasks(int startIndex, int pageSize) {
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
             connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                return  connection.Query<SchedulingTaskModel>($"select top {pageSize} * from [{table}] where [ScheduledUtc] <= @Now  and [CanExecute]=@CanExecute and [Id] not in (select top { startIndex * (pageSize - 1)}  [Id] from[{ table}] where[ScheduledUtc] <= @Now  and[CanExecute] = @CanExecute Order By [Priority] DESC) Order By [Priority] DESC", new { Now = _clock.UtcNow, CanExecute = true }, transaction);
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
        public async Task<IEnumerable<Models.SchedulingTaskModel>> GetTasksAsysc(int startIndex, int pageSize)
        {

            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                return await connection.QueryAsync<SchedulingTaskModel>($"select top {pageSize} * from [{table}] where [ScheduledUtc] <= @Now  and [CanExecute]=@CanExecute and [Id] not in (select top { startIndex*(pageSize - 1)}  [Id] from[{ table}] where[ScheduledUtc] <= @Now  and[CanExecute] = @CanExecute Order By [Priority] DESC) Order By [Priority] DESC", new { Now=_clock.UtcNow ,CanExecute=true}, transaction);
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
        public IEnumerable<Models.SchedulingTaskModel> GetAllTask(int startIndex, int pageSize)
        {
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                return connection.Query<SchedulingTaskModel>($"select top {pageSize} * from [{table}] where  [Id] not in (select top { startIndex * (pageSize - 1)}  [Id] from[{ table}] Order By [Priority] DESC) Order By [Priority] DESC", transaction);
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
        public SchedulingTaskModel GetTask(int id) {
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";
                return  connection.QueryFirstOrDefault <SchedulingTaskModel>($"select top 1 * from [{table}] where Id = @Id", new { Id = id }, transaction);
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

        public void Delete(Models.SchedulingTaskModel job) {
            var connection = _store.Configuration.ConnectionFactory.CreateConnection();
            connection.Open();
            var transaction = connection.BeginTransaction(_store.Configuration.IsolationLevel);

            try
            {
                var table = $"{_tablePrefix}{nameof(SchedulingTaskModel)}";

                connection.Execute($"delete from [{table}] where  [Id]=@Id",new { Id=job.Id}, transaction);
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
        public IEnumerable<ISchedulingTask> GetSchedulingTasks()
        {
            return _schedulingTasks.OrderBy(x => x.MessageName);//.ToReadOnlyCollection();
        }
        public ISchedulingTask GetSchedulingTaskByMessageName(string name)
        {
            return _schedulingTasks.FirstOrDefault(x => x.MessageName == name);
        }
    }
}