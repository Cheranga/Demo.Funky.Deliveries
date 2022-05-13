using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Features.AddOrder
{
    public class AddOrderRequest
    {
        [JsonIgnore]
        public string CorrelationId { get; set; }

        public string CustomerMobile { get; set; }
        public decimal Price { get; set; }
        public string ShopId { get; set; }
    }
}