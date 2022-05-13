using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Demo.Funky.Deliveries.Shared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Demo.Funky.Deliveries.Features.SendSms
{
    public interface ISendSmsService
    {
        Task<Result> SendSmsAsync(SendSmsRequest request);
    }
    
    public class SendSmsService : ISendSmsService
    {
        private readonly SmsConfiguration _config;
        private readonly HttpClient _client;
        private readonly ILogger<SendSmsService> _logger;

        public SendSmsService(SmsConfiguration config, HttpClient client, ILogger<SendSmsService> logger)
        {
            _config = config;
            _client = client;
            _logger = logger;
        }

        public async Task<Result> SendSmsAsync(SendSmsRequest request)
        {
            try
            {
                var url = $"{_config.BaseUrl}/sms/send";
                var serializedData = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(serializedData, Encoding.UTF8, "application/json")
                };

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", _config.AuthToken);

                var httpResponse = await _client.SendAsync(httpRequest);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    return Result.Failure(ErrorCodes.CannotSendSms, ErrorMessages.CannotSendSms);
                }

                return Result.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "error occurred when sending SMS");
            }

            return Result.Failure(ErrorCodes.CannotSendSms, ErrorMessages.CannotSendSms);
        }
    }
}