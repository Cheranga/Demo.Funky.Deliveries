using System;

namespace Demo.Funky.Deliveries.Features.Customer
{
    public class OrderException : Exception
    {
        public string ErrorCode { get; }

        public OrderException(string errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}