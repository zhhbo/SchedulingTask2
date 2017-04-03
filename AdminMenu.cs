using Orchard.Environment.Navigation;
using System;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Extensions.Features.Attributes;
namespace Wkong.SchedulingTask
{
    [OrchardFeature("Wkong.SchedulingTask.UI")]
    public class AdminMenu :  INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public string MenuName { get { return "admin"; } }
        public IStringLocalizer T { get; set; }
        public void BuildNavigation(string name, NavigationBuilder builder) {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))

                builder
                    .Add(T["定时任务"], "15.0", item =>
                {
                    item.Action("List", "Admin", new { area = "Wkong.SchedulingTask" });
                    item.LinkToFirstChild(false);
                });
        }
    }
}