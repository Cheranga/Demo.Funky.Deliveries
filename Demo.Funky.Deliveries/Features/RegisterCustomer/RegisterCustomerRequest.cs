using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Features.RegisterCustomer
{
    public class RegisterCustomerRequest
    {
        [JsonIgnore]
        public string CorrelationId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
    }
}