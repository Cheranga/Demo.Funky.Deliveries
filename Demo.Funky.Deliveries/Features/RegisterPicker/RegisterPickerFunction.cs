using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using Demo.Funky.Deliveries.Extensions;
using Demo.Funky.Deliveries.Features.Picker;
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

namespace Demo.Funky.Deliveries.Features.RegisterPicker
{
    public class RegisterPickerFunction
    {
        private readonly IValidator<RegisterPickerRequest> _validator;
        private readonly IMapper _mapper;
        private readonly ILogger<RegisterPickerFunction> _logger;

        public RegisterPickerFunction(
            IValidator<RegisterPickerRequest> validator,
            IMapper mapper,
            ILogger<RegisterPickerFunction> log)
        {
            _validator = validator;
            _mapper = mapper;
            _logger = log;
        }

        [FunctionName(nameof(RegisterPickerFunction))]
        [OpenApiOperation("RegisterPicker", "pickers")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(HttpConstants.CorrelationId, Description = "the unique correlation id", In = ParameterLocation.Header, Required = true)]
        [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(RegisterPickerRequest), Description = "the request to register a picker", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(EmptyResult), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, nameof(HttpMethod.Post), Route = "picker")]
            HttpRequest request,
            [DurableClient]IDurableEntityClient client)
        {
            var registerPickerRequest = await GetRequest(request);
            var validationResult = await _validator.ValidateAsync(registerPickerRequest);
            if (!validationResult.IsValid)
            {
                return new BadRequestResult();
            }

            await InitializePickerAsync(registerPickerRequest, client);

            return new AcceptedResult();
        }

        private async Task InitializePickerAsync(RegisterPickerRequest request, IDurableEntityClient client)
        {
            var picker = _mapper.Map<Models.Picker>(request);

            var entityId = EntityIdGenerator.GetEntityId<PickerEntity>(request.Mobile);
            await client.SignalEntityAsync<IPickerEntity>(entityId, entity => entity.Init(picker));
        }
        
        private async Task<RegisterPickerRequest> GetRequest(HttpRequest request)
        {
            var registerPickerRequest = await request.ToModel<RegisterPickerRequest>();
            request.Headers.TryGetValue(HttpConstants.CorrelationId, out var correlationId);

            registerPickerRequest.CorrelationId = correlationId;

            return registerPickerRequest;
        }
    }
}

