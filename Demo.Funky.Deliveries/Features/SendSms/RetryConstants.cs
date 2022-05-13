using System.Collections.Generic;
using System.Net;

namespace Demo.Funky.Deliveries.Features.SendSms
{
    public static class RetryConstants
    {
        public static readonly List<HttpStatusCode> RetryStatusList = new()
        {
            HttpStatusCode.TooManyRequests,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.GatewayTimeout
        };
    }
}