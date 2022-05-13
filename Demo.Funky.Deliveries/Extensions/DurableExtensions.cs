using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Demo.Funky.Deliveries.Extensions
{
    public static class DurableExtensions
    {
        public static Task<TResult> StartActivityWithRetry<TResult, TException>(this IDurableOrchestrationContext context, string functionName, object input,
            int firstRetryInSeconds = 5, int maxNumberOfRetries = 5) where TException:Exception
        {
            var retryOptions = new RetryOptions(TimeSpan.FromSeconds(firstRetryInSeconds), maxNumberOfRetries)
            {
                Handle = exception => exception.InnerException != null && exception.InnerException.GetType() == typeof(TException)
            };

            return context.CallActivityWithRetryAsync<TResult>(functionName, retryOptions, input);
        }
    }
}