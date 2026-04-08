using IssuingPayment.WorkerService.Application.Events;
using Microsoft.Extensions.Logging;

namespace IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;

public class AuthorizationEventHandler : IAuthorizationEventHandler
{
    private readonly ILogger<AuthorizationEventHandler> _logger;

    public AuthorizationEventHandler(ILogger<AuthorizationEventHandler> logger)
    {
        _logger = logger;
    }
    
    public Task HandleAsync(AuthorizationApprovedEvent e, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Authorization approved event. Card: {CardId}, Amount: {Amount}, Currency: {Currency}, AuthorizationCode: {AuthorizationCode}, CreatedOn: {CreatedOn}",
            e.CardId, e.Amount, e.Currency, e.AuthorizationCode, e.CreatedOn);
        
        return Task.CompletedTask;
    }

    public Task HandleAsync(AuthorizationDeclinedEvent e, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Declined approved event. Card: {CardId}, Amount: {Amount}, Currency: {Currency}, ReasonCode: {ReasonCode}, CreatedOn: {CreatedOn}",
            e.CardId, e.Amount, e.Currency, e.ReasonCode, e.CreatedOn);
        
        return Task.CompletedTask;
    }
}