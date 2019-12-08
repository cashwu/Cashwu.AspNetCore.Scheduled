# Asp.Net Core hosting scheduled

---

[![Nuget](https://img.shields.io/badge/Nuget-Cashwu.AspnetCore.Scheduled-blue.svg)](https://www.nuget.org/packages/Cashwu.AspnetCore.Scheduled)

---

## Implement your scheduled task from IScheduledTask

- `Schedule` is cron schedule, could referrence [crontab](https://crontab.guru/)
  - minimum 1 minute (* * * * *)
- `IsApplicationStartedRun` is application started run once immediately
- `ExecuteAsync` is your scheduled job

```csharp
public class YourTask : IScheduledTask
{
    public string Schedule { get; }

    public bool IsApplicationStartedRun { get; }
    
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
```

## Startup.cs

- add implement scheduled task for singleton
- add Scheduler extension

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IScheduledTask, YourTask>()
    services.AddScheduler();
}
```
