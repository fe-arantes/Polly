using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConsoleApp.Resilience;
using ConsoleApp.Workers;
using Polly;

CreateHostBuilder(args).Build().Run();


static IHostBuilder CreateHostBuilder(string [] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddSingleton(CircuitBreaker.CreatePolicy());
                    //services.AddHostedService<CircuitBreakerWorker>();

                    services.AddSingleton<AsyncPolicy>(
                        WaitAndRetryExtensions.CreateWaitAndRetryPolicy(new []
                        {
                            TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(7)
                        }));

                    services.AddHostedService<WaitAndRetryExtensionsWorker>();
                });
