using System;

namespace Demo.Funky.Deliveries.Features.SendSms
{
    public class CannotSendSmsException : Exception
    {
        public string ErrorCode { get; }

        public CannotSendSmsException(string errorCode, string errorMessage): base(errorMessage)
        {
            ErrorCode = errorCode;
        }
    }
}