using System;
using System.Net.Http;
using Demo.Funky.Deliveries;
using Demo.Funky.Deliveries.Features.SendSms;
using Demo.Funky.Deliveries.Shared;
using FluentValidation;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;

[assembly:FunctionsStartup(typeof(Startup))]
namespace Demo.Funky.Deliveries
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = GetConfiguration(builder);
            var services = builder.Services;

            RegisterLogging(services);
            RegisterValidators(services);
            RegisterMappers(services);
            RegisterServices(services);
            RegisterConfigurations(services, configuration);
        }

        private void RegisterLogging(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                var logger = new LoggerConfiguration()
                    .WriteTo.ColoredConsole(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.ApplicationInsights(TelemetryConfiguration.CreateDefault(), TelemetryConverter.Traces)
                    .CreateLogger();

                builder.AddSerilog(logger);
            });
        }

        private void RegisterConfigurations(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton(_ =>
            {
                var config = configuration.GetSection(nameof(SmsConfiguration)).Get<SmsConfiguration>();
                return config;
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            services.AddHttpClient<ISendSmsService, SendSmsService>();
            // services.AddHttpClient<ISendSmsService, SendSmsService>().AddPolicyHandler(GetRetryPolicy());
        }

        private void RegisterMappers(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
        }

        private void RegisterValidators(IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(ModelValidatorBase<>).Assembly);
        }
        
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var jitterer = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg =>
                {
                    var statusCode = msg.StatusCode;
                    var continueIf = RetryConstants.RetryStatusList.Contains(statusCode);

                    return continueIf;
                })
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(1000, 5000)));
        }
        
        protected virtual IConfigurationRoot GetConfiguration(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;
            var executionContextOptions = services.BuildServiceProvider().GetService<IOptions<ExecutionContextOptions>>().Value;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(executionContextOptions.AppDirectory)
                .AddJsonFile("local.settings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}