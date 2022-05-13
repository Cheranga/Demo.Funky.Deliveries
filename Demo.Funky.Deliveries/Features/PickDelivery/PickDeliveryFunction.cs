using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Funky.Deliveries.Extensions;
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

namespace Demo.Funky.Deliveries.Features.PickDelivery
{
    public class PickDeliveryFunction
    {
        private readonly IValidator<PickDeliveryRequest> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<PickDeliveryFunction> _logger;

        public PickDeliveryFunction(
            IValidator<PickDeliveryRequest> validator,
            IMapper mapper,
            ILogger<PickDeliveryFunction> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
        }

        [FunctionName(nameof(PickDeliveryFunction))]
        [OpenApiOperation("RegisterCustomer", "Delivery")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(HttpConstants.CorrelationId, Description = "the unique correlation id", In = ParameterLocation.Header, Required = true)]
        [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(MatchMakingRequest), Description = "the request to deliver the order", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(EmptyResult), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = "delivery/pick")]
            HttpRequest request,
            [DurableClient] IDurableOrchestrationClient client)
        {
            var pickOrderRequest = await GetPickOrderRequest(request);
            var validationResult = await _validator.ValidateAsync(pickOrderRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestResult();
            }

            await MatchMakeAsync(client, pickOrderRequest);

            return new AcceptedResult();
        }

        private async Task MatchMakeAsync(IDurableOrchestrationClient client, PickDeliveryRequest pickOrderRequest)
        {
            var matchMakerRequest = _mapper.Map<MatchMakingRequest>(pickOrderRequest);

            var instanceId = $"{pickOrderRequest.CustomerMobile}-{pickOrderRequest.PickerMobile}-{pickOrderRequest.OrderId}".ToUpper();

            _logger.LogWarning("{CorrelationId} starting delivery process for {OrderId}", pickOrderRequest.CorrelationId, pickOrderRequest.OrderId);
            await client.StartNewAsync(nameof(MatchMakerFunction), instanceId, matchMakerRequest);
        }

        private async Task<PickDeliveryRequest> GetPickOrderRequest(HttpRequest request)
        {
            var pickOrderRequest = await request.ToModel<PickDeliveryRequest>();
            if (request.Headers.TryGetValue(HttpConstants.CorrelationId, out var correlationId))
            {
                pickOrderRequest.CorrelationId = correlationId;
            }

            return pickOrderRequest;
        }
    }
}