using System.Collections.Generic;

namespace Demo.Funky.Deliveries.Features.SendSms
{
    public class SendSmsRequest
    {
        public List<SmsMessage> Messages { get; set; }
    }
}