
using Microsoft.Extensions.Localization;

namespace Wkong.SchedulingTask.Services
{
    public interface ISchedulingTask 
    {
        string Name { get; }
        LocalizedString Category { get; }
        LocalizedString Description { get; }

        string Form { get; }
        string MessageName { get; }
    }
}