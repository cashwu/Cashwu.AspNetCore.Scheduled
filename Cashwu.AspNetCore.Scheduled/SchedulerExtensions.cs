using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cashwu.AspNetCore.Scheduled
{
    public static class SchedulerExtensions
    {
        public static IServiceCollection AddScheduler(this IServiceCollection services)
        {
            return services.AddSingleton<IHostedService, SchedulerHostedService>(serviceProvider =>
            {
                var instance = new SchedulerHostedService(serviceProvider.GetServices<IScheduledTask>(),
                                                          serviceProvider.GetService<IHostApplicationLifetime>());

                instance.UnobservedTaskException += (sender, args) =>
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<IScheduledTask>>();
                    logger.LogError(args.Exception, args.Exception.ToString());
                    args.SetObserved();
                };

                return instance;
            });
        }

        public static IServiceCollection AddScheduler(this IServiceCollection services, EventHandler<UnobservedTaskExceptionEventArgs> unobservedTaskExceptionHandler)
        {
            return services.AddSingleton<IHostedService, SchedulerHostedService>(serviceProvider =>
            {
                var instance = new SchedulerHostedService(serviceProvider.GetServices<IScheduledTask>(),
                                                          serviceProvider.GetService<IHostApplicationLifetime>());

                instance.UnobservedTaskException += unobservedTaskExceptionHandler;

                return instance;
            });
        }
    }
}