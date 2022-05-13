using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Demo.Funky.Deliveries.Models;
using Demo.Funky.Deliveries.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Features.Customer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CustomerEntity : ICustomerEntity, IActor
    {
        [JsonProperty] public Models.Customer Data { get; set; }
        [JsonProperty] public Dictionary<string, Tuple<Order, DeliveryStatus>> OrderStatusMappedById { get; set; }

        public void Init(Models.Customer customer)
        {
            Data = customer;
        }

        public void AddOrder(Order order)
        {
            OrderStatusMappedById.TryAdd(order.OrderId, Tuple.Create(order, DeliveryStatus.WaitingToBePicked));
        }

        public void CompleteDelivery(string orderId)
        {
            if (OrderStatusMappedById.TryGetValue(orderId, out var foodOrder))
            {
                OrderStatusMappedById[orderId] = Tuple.Create(foodOrder.Item1, DeliveryStatus.Delivered);
            }
        }

        public Task<bool> IsReadyToBePickedAsync(string orderId)
        {
            if (OrderStatusMappedById.Any() && OrderStatusMappedById.TryGetValue(orderId, out var orderData))
            {
                var isReadyToPick = orderData.Item2 is DeliveryStatus.WaitingToBePicked or DeliveryStatus.Cancelled;
                return Task.FromResult(isReadyToPick);
            }

            throw new OrderException(ErrorCodes.OrderDoesNotExist);
        }

        public Task<Result> SetDeliveryAsync(string orderId)
        {
            if (OrderStatusMappedById.TryGetValue(orderId, out var orderData))
            {
                OrderStatusMappedById[orderId] = Tuple.Create(orderData.Item1, DeliveryStatus.Picked);
                return Task.FromResult(Result.Success());
            }

            return Task.FromResult(Result.Failure(ErrorCodes.OrderDoesNotExist, ErrorMessages.OrderDoesNotExist));
        }

        [FunctionName(nameof(CustomerEntity))]
        public static async Task HandleEntityOperation(
            [EntityTrigger] IDurableEntityContext context)
        {
            if (!context.HasState)
            {
                context.SetState(new CustomerEntity
                {
                    OrderStatusMappedById = new Dictionary<string, Tuple<Order, DeliveryStatus>>()
                });
            }

            await context.DispatchAsync<CustomerEntity>();
        }
    }
}