namespace Demo.Funky.Deliveries.Shared
{
    public class ErrorCodes
    {
        public const string OrderDoesNotExist = nameof(OrderDoesNotExist);
        public const string DeliveryInProgress = nameof(DeliveryInProgress);
        public const string CannotSendSms = nameof(CannotSendSms);
        public const string OrderAlreadyPicked = nameof(OrderAlreadyPicked);
        public const string PickerUnavailable = nameof(PickerUnavailable);
    }

    public class ErrorMessages
    {
        public const string OrderDoesNotExist = "order does not exists";
        public const string DeliveryInProgress = "delivery is inprogress";
        public const string CannotSendSms = "cannot send SMS";
        public const string OrderAlreadyPicked = "order already picked";
        public const string PickerUnavailable = "picker unavailable";
    }
}