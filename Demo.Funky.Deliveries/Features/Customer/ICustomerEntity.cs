using System.Threading.Tasks;
using Demo.Funky.Deliveries.Models;
using Demo.Funky.Deliveries.Shared;

namespace Demo.Funky.Deliveries.Features.Customer;

public interface ICustomerEntity
{
    void Init(Models.Customer customer);
    void AddOrder(Order order);

    void CompleteDelivery(string orderId);

    Task<bool> IsReadyToBePickedAsync(string orderId);

    Task<Result> SetDeliveryAsync(string orderId);
}