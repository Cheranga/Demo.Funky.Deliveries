using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Funky.Deliveries.Extensions;
using Demo.Funky.Deliveries.Features.Customer;
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

namespace Demo.Funky.Deliveries.Features.RegisterCustomer
{
    public class RegisterCustomerFunction
    {
        private readonly IValidator<RegisterCustomerRequest> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterCustomerFunction> _logger;

        public RegisterCustomerFunction(
            IValidator<RegisterCustomerRequest> validator,
            IMapper mapper,
            ILogger<RegisterCustomerFunction> logger)
        {
            _validator = validator;
            _mapper = mapper;
            _logger = logger;
        }

        [FunctionName(nameof(RegisterCustomerFunction))]
        [OpenApiOperation("RegisterCustomer", "customers")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(HttpConstants.CorrelationId, Description = "the unique correlation id", In = ParameterLocation.Header, Required = true)]
        [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(RegisterCustomerRequest), Description = "the request to register a customer", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(EmptyResult), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = "customer")]
            HttpRequest request,
            [DurableClient]IDurableEntityClient client)
        {
            var registerCustomerRequest = await GetRequest(request);
            var validationResult = await _validator.ValidateAsync(registerCustomerRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestResult();
            }

            await InitializeCustomerAsync(registerCustomerRequest, client);

            return new AcceptedResult();
        }

        private async Task InitializeCustomerAsync(RegisterCustomerRequest registerCustomerRequest, IDurableEntityClient client)
        {
            var customer = _mapper.Map<Models.Customer>(registerCustomerRequest);
            
            var entityId = EntityIdGenerator.GetEntityId<CustomerEntity>(registerCustomerRequest.Mobile);
            await client.SignalEntityAsync<ICustomerEntity>(entityId, entity => entity.Init(customer));

            _logger.LogWarning("{CorrelationId} customer initialized", registerCustomerRequest.CorrelationId);
        }

        private async Task<RegisterCustomerRequest> GetRequest(HttpRequest request)
        {
            var registerCustomerRequest = await request.ToModel<RegisterCustomerRequest>();
            request.Headers.TryGetValue(HttpConstants.CorrelationId, out var correlationId);

            registerCustomerRequest.CorrelationId = correlationId;

            return registerCustomerRequest;
        }
    }
}