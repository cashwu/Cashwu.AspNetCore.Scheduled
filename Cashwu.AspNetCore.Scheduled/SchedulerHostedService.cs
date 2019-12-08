using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace Cashwu.AspNetCore.Scheduled
{
    internal class SchedulerHostedService : BackgroundService
    {
        private readonly List<ScheduledTaskWrapper> _scheduledTasks = new List<ScheduledTaskWrapper>();
        private bool _applicationStarted;

        public event EventHandler<UnobservedTaskExceptionEventArgs> UnobservedTaskException;

        public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks, IHostApplicationLifetime hostApplicationLifetime)
        {
            hostApplicationLifetime.ApplicationStarted.Register(() =>
            {
                _applicationStarted = true;
            });
            
            var referenceTime = DateTime.UtcNow;

            foreach (var scheduledTask in scheduledTasks)
            {
                var scheduledTaskWrapper = new ScheduledTaskWrapper
                {
                    Schedule = CrontabSchedule.Parse(scheduledTask.Schedule),
                    Task = scheduledTask
                };

                scheduledTaskWrapper.NextRuntTime = scheduledTaskWrapper.Schedule.GetNextOccurrence(referenceTime);

                _scheduledTasks.Add(scheduledTaskWrapper);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_applicationStarted)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue; 
                }

                await ExecuteOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ExecuteOnceAsync(CancellationToken stoppingToken)
        {
            var taskFactory = new TaskFactory(TaskScheduler.Current);
            var referenceTime = DateTime.UtcNow;

            var tasksThatShouldRun = _scheduledTasks.Where(a => a.ShouldRun(referenceTime)).ToList();

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();

                await taskFactory.StartNew(async () =>
                {
                    try
                    {
                        await taskThatShouldRun.Task.ExecuteAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        var args = new UnobservedTaskExceptionEventArgs(ex as AggregateException ?? new AggregateException(ex));

                        UnobservedTaskException?.Invoke(this, args);

                        if (!args.Observed)
                        {
                            throw;
                        }
                    }
                }, stoppingToken);
            }
        }
    }
}