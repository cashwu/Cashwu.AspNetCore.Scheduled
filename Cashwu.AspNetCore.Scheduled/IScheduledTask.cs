using System.Threading;
using System.Threading.Tasks;

namespace Cashwu.AspNetCore.Scheduled
{
    public interface IScheduledTask
    {
        string Schedule { get; }

        bool IsApplicationStartedRun { get; }

        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}