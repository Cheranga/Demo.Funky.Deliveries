using System;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Funky.Deliveries.Extensions;
using Demo.Funky.Deliveries.Features.Customer;
using Demo.Funky.Deliveries.Models;
using Demo.Funky.Deliveries.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Demo.Funky.Deliveries.Features.AddOrder
{
    public class AddOrderFunction
    {
        private readonly IValidator<AddOrderRequest> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<AddOrderFunction> _logger;

        public AddOrderFunction(
            IValidator<AddOrderRequest> validator,
            IMapper mapper,
            ILogger<AddOrderFunction> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
        }

        [FunctionName(nameof(AddOrderFunction))]
        [OpenApiOperation("AddOrder", "orders")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(HttpConstants.CorrelationId, Description = "the unique correlation id", In = ParameterLocation.Header, Required = true)]
        [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(AddOrderRequest), Description = "the request to make an order by a customer", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(AddOrderResponse), Description = "The OK response")]
        public async Task<IActionResult> AddOrderAsync(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = "orders")]
            HttpRequest request,
            [DurableClient] IDurableEntityClient client)
        {
            var addOrderRequest = await GetRequest(request);
            var validationResult = await _validator.ValidateAsync(addOrderRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestResult();
            }

            var addOrderResponse = await AddOrderAsync(client, addOrderRequest);

            return new ObjectResult(addOrderResponse)
            {
                StatusCode = (int) (HttpStatusCode.Accepted)
            };
        }

        private async Task<AddOrderResponse> AddOrderAsync(IDurableEntityClient client, AddOrderRequest addOrderRequest)
        {
            var orderId = Guid.NewGuid().ToString("N").ToUpper();
            var entityId = EntityIdGenerator.GetEntityId<CustomerEntity>(addOrderRequest.CustomerMobile);
            var order = _mapper.Map<Order>(addOrderRequest);
            order.OrderId = orderId;

            await client.SignalEntityAsync<ICustomerEntity>(entityId, entity => entity.AddOrder(order));

            _logger.LogWarning("{CorrelationId} adding order to customer", addOrderRequest.CorrelationId);
            
            return new AddOrderResponse
            {
                CorrelationId = addOrderRequest.CorrelationId,
                OrderId = orderId
            };
        }

        private async Task<AddOrderRequest> GetRequest(HttpRequest request)
        {
            var addOrderRequest = await request.ToModel<AddOrderRequest>();
            if (request.Headers.TryGetValue(HttpConstants.CorrelationId, out var correlationId))
            {
                addOrderRequest.CorrelationId = correlationId;
            }

            return addOrderRequest;
        }
    }
}