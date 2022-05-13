namespace Demo.Funky.Deliveries.Features.CompleteDelivery
{
    public class CompleteDeliveryRequest
    {
        public string CorrelationId { get; set; }
        public string CustomerMobile { get; set; }
        public string PickerMobile { get; set; }
        public string OrderId { get; set; }
    }
}