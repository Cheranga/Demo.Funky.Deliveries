using System.Threading.Tasks;
using Demo.Funky.Deliveries.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace Demo.Funky.Deliveries.Features.SendSms
{
    public class SendSmsFunction
    {
        private readonly ISendSmsService _smsService;

        public SendSmsFunction(ISendSmsService smsService)
        {
            _smsService = smsService;
        }

        [FunctionName(nameof(SendSmsFunction))]
        public async Task<Result> SendSmsAsync([ActivityTrigger] IDurableActivityContext context)
        {
            var sendSmsRequest = context.GetInput<SendSmsRequest>();
            var operation = await _smsService.SendSmsAsync(sendSmsRequest);
            if (!operation.Status)
            {
                throw new CannotSendSmsException(ErrorCodes.CannotSendSms, ErrorMessages.CannotSendSms);
            }

            return operation;
        }
    }
}