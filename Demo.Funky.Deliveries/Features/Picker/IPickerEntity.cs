using System.Threading.Tasks;
using Demo.Funky.Deliveries.Shared;

namespace Demo.Funky.Deliveries.Features.Picker;

public interface IPickerEntity
{
    void Init(Models.Picker picker);
    Task<bool> IsAvailableAsync();
    void CompleteDelivery(string orderId);

    Task<Result> SetDeliveryAsync(string orderId);
}