using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.Funky.Deliveries.Models;
using Demo.Funky.Deliveries.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Features.Picker
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PickerEntity : IPickerEntity, IActor
    {
        [JsonProperty] public Models.Picker Data { get; set; }
        [JsonProperty] public Dictionary<string, DeliveryStatus> OrdersMappedById { get; set; }

        public void Init(Models.Picker picker)
        {
            Data = picker;
        }

        public Task<bool> IsAvailableAsync()
        {
            var isAvailable = OrdersMappedById.Count(x => x.Value == DeliveryStatus.Picked) < 3;
            return Task.FromResult(isAvailable);
        }

        public void CompleteDelivery(string orderId)
        {
            if (OrdersMappedById.ContainsKey(orderId))
            {
                OrdersMappedById[orderId] = DeliveryStatus.Delivered;
            }
        }

        public Task<Result> SetDeliveryAsync(string orderId)
        {
            OrdersMappedById.TryAdd(orderId, DeliveryStatus.Picked);
            return Task.FromResult(Result.Success());
        }

        [FunctionName(nameof(PickerEntity))]
        public static async Task HandleEntityOperation(
            [EntityTrigger] IDurableEntityContext context)
        {
            if (!context.HasState)
            {
                context.SetState(new PickerEntity
                {
                    OrdersMappedById = new Dictionary<string, DeliveryStatus>()
                });
            }
            
            await context.DispatchAsync<PickerEntity>();
        }
    }
}