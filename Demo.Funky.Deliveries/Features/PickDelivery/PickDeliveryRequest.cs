using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Features.PickDelivery
{
    public class PickDeliveryRequest
    {
        [JsonIgnore]
        public string CorrelationId { get; set; }

        public string CustomerMobile { get; set; }
        public string PickerMobile { get; set; }
        public string OrderId { get; set; }
    }
}