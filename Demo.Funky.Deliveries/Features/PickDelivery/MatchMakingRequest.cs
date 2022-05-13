namespace Demo.Funky.Deliveries.Features.PickDelivery
{
    public class MatchMakingRequest
    {
        public string CorrelationId { get; set; }
        public string CustomerMobile { get; set; }
        public string PickerMobile { get; set; }
        public string OrderId { get; set; }
    }
}