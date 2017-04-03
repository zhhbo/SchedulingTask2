using System;
using Orchard.Environment.Extensions.Features.Attributes;
using Orchard.Data.Migration;
using Wkong.SchedulingTask.Models;
namespace Wkong.SchedulingTask {
    [OrchardFeature("Wkong.SchedulingTask")]
    public class Migrations : DataMigration {
         
        public int Create() {
            SchemaBuilder.CreateTable(nameof(SchedulingTaskModel), table => table
                .Column<int>(nameof(SchedulingTaskModel.Id), c => c.Identity().PrimaryKey())
                .Column<string>(nameof(SchedulingTaskModel.Message), c => c.WithLength(64))
                .Column<string>(nameof(SchedulingTaskModel.TaskName), c => c.WithLength(64))
                .Column<string>(nameof(SchedulingTaskModel.Parameters), c => c.Unlimited())
                .Column<int>(nameof(SchedulingTaskModel.Priority), c => c.WithDefault(0))
                .Column<int>(nameof(SchedulingTaskModel.Frequency), c => c.WithDefault(0))
                .Column<int>(nameof(SchedulingTaskModel.SpaceNum), c => c.WithDefault(0))
                .Column<bool>(nameof(SchedulingTaskModel.CanExecute), c => c.WithDefault(false))
                .Column<DateTime>(nameof(SchedulingTaskModel.ScheduledUtc))
                .Column<DateTime>(nameof(SchedulingTaskModel.CreatedUtc))
                );

            return 1;
        }
    }
}