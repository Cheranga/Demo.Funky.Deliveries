using System.Collections.Generic;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Demo.Funky.Deliveries.Extensions;
using Demo.Funky.Deliveries.Features.Customer;
using Demo.Funky.Deliveries.Features.Picker;
using Demo.Funky.Deliveries.Features.SendSms;
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

namespace Demo.Funky.Deliveries.Features.CompleteDelivery
{
    public class CompleteDeliveryFunction
    {
        private readonly IValidator<CompleteDeliveryRequest> _validator;
        private readonly ISendSmsService _smsService;
        private readonly ILogger<CompleteDeliveryFunction> _logger;

        public CompleteDeliveryFunction(IValidator<CompleteDeliveryRequest> validator, ISendSmsService smsService, ILogger<CompleteDeliveryFunction> logger)
        {
            _validator = validator;
            _smsService = smsService;
            _logger = logger;
        }

        [FunctionName(nameof(CompleteDeliveryFunction))]
        [OpenApiOperation("CompleteDelivery", "Delivery")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(HttpConstants.CorrelationId, Description = "the unique correlation id", In = ParameterLocation.Header, Required = true)]
        [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(CompleteDeliveryRequest), Description = "the request to state that the delivery is completed", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(EmptyResult), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = "delivery/complete")]
            HttpRequest request,
            [DurableClient] IDurableEntityClient client)
        {
            var completeDeliveryRequest = await GetRequest(request);
            var validationResult = await _validator.ValidateAsync(completeDeliveryRequest);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("invalid complete order request received");
                return new BadRequestResult();
            }

            await Task.WhenAll(
                CompleteCustomerDeliveryAsync(completeDeliveryRequest, client),
                CompletePickerDeliveryAsync(completeDeliveryRequest, client));

            return new AcceptedResult();
        }

        private async Task CompletePickerDeliveryAsync(CompleteDeliveryRequest request, IDurableEntityClient client)
        {
            var entityId = EntityIdGenerator.GetEntityId<PickerEntity>(request.PickerMobile);
            await client.SignalEntityAsync<IPickerEntity>(entityId, entity => entity.CompleteDelivery(request.OrderId));

            var smsRequest = new SendSmsRequest
            {
                Messages = new List<SmsMessage>
                {
                    new SmsMessage
                    {
                        Body =
                            $"Your delivery {request.OrderId} is complete",
                        To = request.CustomerMobile
                    }
                }
            };

            _logger.LogWarning("{CorrelationId} delivery completed for customer", request.CorrelationId);

            await _smsService.SendSmsAsync(smsRequest);
        }

        private async Task CompleteCustomerDeliveryAsync(CompleteDeliveryRequest request, IDurableEntityClient client)
        {
            var entityId = EntityIdGenerator.GetEntityId<CustomerEntity>(request.CustomerMobile);
            await client.SignalEntityAsync<ICustomerEntity>(entityId, entity => entity.CompleteDelivery(request.OrderId));

            var smsRequest = new SendSmsRequest
            {
                Messages = new List<SmsMessage>
                {
                    new SmsMessage
                    {
                        Body =
                            $"Your order has been delivered by {request.PickerMobile}. Please rate the delivery to improve our services",
                        To = request.CustomerMobile
                    }
                }
            };

            _logger.LogWarning("{CorrelationId} delivery completed for picker", request.CorrelationId);

            await _smsService.SendSmsAsync(smsRequest);
        }

        private async Task<CompleteDeliveryRequest> GetRequest(HttpRequest request)
        {
            var completeDeliveryRequest = await request.ToModel<CompleteDeliveryRequest>();
            request.Headers.TryGetValue(HttpConstants.CorrelationId, out var correlationId);

            completeDeliveryRequest.CorrelationId = correlationId;

            return completeDeliveryRequest;
        }
    }
}