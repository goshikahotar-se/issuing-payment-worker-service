using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;
using IssuingPayment.WorkerService.Application.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IssuingPayment.WorkerService.Infrastructure.Authorizations;

public class AuthorizationEventSqsListener : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly IAuthorizationEventHandler _authorizationEventHandler;
    private readonly ILogger<AuthorizationEventSqsListener> _logger;

    public AuthorizationEventSqsListener(IAmazonSQS sqsClient,
                                         IConfiguration configuration,
                                         IAuthorizationEventHandler authorizationEventHandler,
                                         ILogger<AuthorizationEventSqsListener> logger)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _authorizationEventHandler = authorizationEventHandler;
        _logger = logger;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting SQS Listener...");
        var queueUrl = _configuration["LocalStack:QueueUrl"];
            
        if(string.IsNullOrWhiteSpace(queueUrl))
            throw new InvalidOperationException("Queue URL is empty");

        var request = new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 20
        };
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(request, cancellationToken);

                var messages = response.Messages;

                if (messages is null || messages.Count == 0)
                    continue;

                foreach (var message in messages)
                {
                    await ProcessMessageAsync(queueUrl, message, cancellationToken);
                }
            }
            catch (AmazonSQSException e)
            {
                _logger.LogError(e, "SQS exception");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "An unhandled exception occurred");
            }
        }
    }

    private async Task ProcessMessageAsync(string queueUrl, Message message, CancellationToken cancellationToken)
    {
        try
        {
            if (await TryProcessMessageAsync(queueUrl, message, cancellationToken)) return;

            await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Invalid JSON; deleting message {MessageId}", message.MessageId);
            await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
        }
    }

    private async Task<bool> TryProcessMessageAsync(string queueUrl, Message message, CancellationToken cancellationToken)
    {
        var envelope = JsonSerializer.Deserialize<SnsNotificationEnvelope>(message.Body);

        if (IsInvalidEnvelope(envelope))
        {
            await HandleInvalidEnvelopeAsync(queueUrl, message, cancellationToken);
            return true;
        }

        var eventMessageBody = envelope!.Message;

        using var doc = JsonDocument.Parse(eventMessageBody);
        var root = doc.RootElement;
        
        if (IsAuthorizationCodePresent(root))
        {
            var approvedEvent = JsonSerializer.Deserialize<AuthorizationApprovedEvent>(eventMessageBody);
            if (approvedEvent is null) return true;

            await HandleApprovedAuthorizationEvent(message, cancellationToken, approvedEvent);
        }
        else if (IsReasonCodePresent(root))
        {
            var declinedEvent = JsonSerializer.Deserialize<AuthorizationDeclinedEvent>(eventMessageBody);
            if (declinedEvent is null) return true;

            await HandleDeclinedAuthorizationEvent(message, cancellationToken, declinedEvent);
        }
        else
        {
            HandleUnknownAuthorizationPayload(message, eventMessageBody);
        }

        return false;
    }

    private void HandleUnknownAuthorizationPayload(Message message, string eventMessageBody)
    {
        var previewMessageLength = 300;
        var previewMessage = eventMessageBody.Length <= previewMessageLength
            ? eventMessageBody
            : eventMessageBody.Substring(0, previewMessageLength);

        _logger.LogWarning("Unknown authorization payload. MessageId={MessageId} Preview={Preview}",
            message.MessageId, previewMessage);
    }

    private async Task HandleDeclinedAuthorizationEvent(Message message, CancellationToken cancellationToken,
        AuthorizationDeclinedEvent declinedEvent)
    {
        _logger.LogInformation("Declined authorization received. MessageId={MessageId}", message.MessageId);
        await _authorizationEventHandler.HandleAsync(declinedEvent, message.MessageId, cancellationToken);
    }

    private async Task HandleInvalidEnvelopeAsync(string queueUrl, Message message, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Invalid SNS envelope; deleting message {MessageId}", message.MessageId);
        await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, cancellationToken);
    }    

    private async Task HandleApprovedAuthorizationEvent(Message message, CancellationToken cancellationToken,
        AuthorizationApprovedEvent approvedEvent)
    {
        _logger.LogInformation("Approved authorization received. MessageId={MessageId}", message.MessageId);
        await _authorizationEventHandler.HandleAsync(approvedEvent, message.MessageId, cancellationToken);
    }

    private bool IsInvalidEnvelope(SnsNotificationEnvelope? envelope)
    {
        return envelope is null || string.IsNullOrWhiteSpace(envelope.Message);
    }
    
    private static bool IsAuthorizationCodePresent(JsonElement root)
    {
        return root.TryGetProperty("AuthorizationCode", out _);
    }
    
    private static bool IsReasonCodePresent(JsonElement root)
    {
        return root.TryGetProperty("ReasonCode", out _);
    }
}