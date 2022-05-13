using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Funky.Deliveries.Extensions;
using Demo.Funky.Deliveries.Features.Customer;
using Demo.Funky.Deliveries.Features.Picker;
using Demo.Funky.Deliveries.Features.SendSms;
using Demo.Funky.Deliveries.Shared;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace Demo.Funky.Deliveries.Features.PickDelivery
{
    public class MatchMakerFunction
    {
        private readonly ILogger<MatchMakerFunction> _logger;

        public MatchMakerFunction(ILogger<MatchMakerFunction> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(MatchMakerFunction))]
        public async Task MatchAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var replaySafeLogger = context.CreateReplaySafeLogger(_logger);
            var request = context.GetInput<MatchMakingRequest>();

            var orderPickOperation = await AssignOrderToPickerAsync(request, context);

            if (orderPickOperation.Status)
            {
                replaySafeLogger.LogInformation("{CorrelationId} order picked successfully", request.CorrelationId);
                await Task.WhenAll(SendSmsToCustomerAsync(context, request), SendSmsToPickerAsync(context, request));
                return;
            }

            if (orderPickOperation.ErrorCode == ErrorCodes.OrderAlreadyPicked)
            {
                replaySafeLogger.LogInformation("{CorrelationId} {PickerId} has been rejected as this order has already been picked", request.CorrelationId, request.PickerMobile);
                await SendOrderAlreadyPickedMessageAsync(context, request);
            }

            if (orderPickOperation.ErrorCode == ErrorCodes.PickerUnavailable)
            {
                replaySafeLogger.LogInformation("{CorrelationId} {PickerId} have reached the maximum delivery count", request.CorrelationId, request.PickerMobile);
                await SendTooManyDeliveriesMessageAsync(context, request);
            }
        }

        private async Task SendTooManyDeliveriesMessageAsync(IDurableOrchestrationContext context, MatchMakingRequest request)
        {
            var orderAlreadyPickedMessage = new SendSmsRequest
            {
                Messages = new List<SmsMessage>
                {
                    new()
                    {
                        Body = "You already have some pending deliveries to do. Please complete them and check again. Thanks!",
                        To = request.PickerMobile
                    }
                }
            };

            await context.StartActivityWithRetry<Result, CannotSendSmsException>(nameof(SendSmsFunction), orderAlreadyPickedMessage);
        }

        private async Task SendOrderAlreadyPickedMessageAsync(IDurableOrchestrationContext context, MatchMakingRequest request)
        {
            var orderAlreadyPickedMessage = new SendSmsRequest
            {
                Messages = new List<SmsMessage>
                {
                    new()
                    {
                        Body = "Sorry, this order has already been picked",
                        To = request.PickerMobile
                    }
                }
            };

            await context.StartActivityWithRetry<Result, CannotSendSmsException>(nameof(SendSmsFunction), orderAlreadyPickedMessage);
        }

        private async Task SendSmsToPickerAsync(IDurableOrchestrationContext context, MatchMakingRequest request)
        {
            var sendSmsRequest = new SendSmsRequest
            {
                Messages = new List<SmsMessage>
                {
                    new()
                    {
                        Body = $"Congratulations! you picked the order {request.OrderId}. Please drive safe",
                        To = request.PickerMobile
                    }
                }
            };

            await context.StartActivityWithRetry<Result, CannotSendSmsException>(nameof(SendSmsFunction), sendSmsRequest);
        }

        private async Task SendSmsToCustomerAsync(IDurableOrchestrationContext context, MatchMakingRequest request)
        {
            var sendSmsRequest = new SendSmsRequest
            {
                Messages = new List<SmsMessage>
                {
                    new()
                    {
                        Body = $"Hi, your order {request.OrderId} has been picked up!",
                        To = request.CustomerMobile
                    }
                }
            };

            await context.StartActivityWithRetry<Result, CannotSendSmsException>(nameof(SendSmsFunction), sendSmsRequest);
        }

        private async Task<Result> AssignOrderToPickerAsync(MatchMakingRequest request, IDurableOrchestrationContext context)
        {
            var customerEntity = EntityIdGenerator.GetEntityId<CustomerEntity>(request.CustomerMobile);
            var pickerEntity = EntityIdGenerator.GetEntityId<PickerEntity>(request.PickerMobile);

            using (await context.LockAsync(customerEntity, pickerEntity))
            {
                var customerProxy = context.CreateEntityProxy<ICustomerEntity>(customerEntity);
                var pickerProxy = context.CreateEntityProxy<IPickerEntity>(pickerEntity);

                var isOrderReadyToBePicked = await customerProxy.IsReadyToBePickedAsync(request.OrderId);
                if (!isOrderReadyToBePicked) return Result.Failure(ErrorCodes.OrderAlreadyPicked, ErrorMessages.OrderAlreadyPicked);

                var isPickerAvailable = await pickerProxy.IsAvailableAsync();

                if (!isPickerAvailable) return Result.Failure(ErrorCodes.PickerUnavailable, ErrorMessages.PickerUnavailable);

                await Task.WhenAll(customerProxy.SetDeliveryAsync(request.OrderId), pickerProxy.SetDeliveryAsync(request.OrderId));
                return Result.Success();
            }
        }
    }
}