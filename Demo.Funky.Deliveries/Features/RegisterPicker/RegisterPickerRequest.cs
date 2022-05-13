using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Features.RegisterPicker
{
    public class RegisterPickerRequest
    {
        [JsonIgnore]
        public string CorrelationId { get; set; }

        public string Name { get; set; }
        public string Mobile { get; set; }
    }
}