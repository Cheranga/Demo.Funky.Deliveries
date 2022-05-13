using AutoMapper;
using Demo.Funky.Deliveries.Features.AddOrder;
using Demo.Funky.Deliveries.Features.PickDelivery;
using Demo.Funky.Deliveries.Features.RegisterCustomer;
using Demo.Funky.Deliveries.Features.RegisterPicker;
using Demo.Funky.Deliveries.Models;

namespace Demo.Funky.Deliveries.Shared
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterCustomerRequest, Customer>();
            CreateMap<RegisterPickerRequest, Picker>();
            CreateMap<AddOrderRequest, Order>();
            CreateMap<PickDeliveryRequest, MatchMakingRequest>();
        }
    }
}