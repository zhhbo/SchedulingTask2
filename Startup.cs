using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Navigation;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Wkong.SchedulingTask.Services;
using Orchard.BackgroundTasks;
namespace Wkong.SchedulingTask
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddScoped<SetupEventHandler>();
            //services.AddScoped<ISetupEventHandler>(sp => sp.GetRequiredService<SetupEventHandler>());
           // services.AddScoped<IPermissionProvider, Permissions>();

           // services.AddRecipeExecutionStep<SettingsStep>();
            services.AddScoped<ISchedulingTaskManager, SchedulingTaskManager>();


            services.AddScoped<ISchedulingTaskProcessor, SchedulingTaskProcessor>();
            services.AddScoped<ISchedulingTaskService, SchedulingTaskService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IBackgroundTask, SchedulingTaskBackgroundTask>();
        }

        /*public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Admin
            routes.MapAreaRoute(
                name: "AdminSettings",
                areaName: "Orchard.Settings",
                template: "Admin/Settings/{groupId}",
                defaults: new { controller = "Admin", action = "Index" }
            );
        }*/
    }
}
