using IssuingPayment.WorkerService.Application.Events;
using Microsoft.Extensions.Logging;

namespace IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;

public class AuthorizationEventHandler : IAuthorizationEventHandler
{
    private readonly ILogger<AuthorizationEventHandler> _logger;
    private readonly IAuthorizationEventRepository _authorizationEventRepository;

    public AuthorizationEventHandler(ILogger<AuthorizationEventHandler> logger,  IAuthorizationEventRepository authorizationEventRepository)
    {
        _logger = logger;
        _authorizationEventRepository = authorizationEventRepository;
    }
    
    public async Task HandleAsync(AuthorizationApprovedEvent e, string messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Authorization approved event. Card: {CardId}, Amount: {Amount}, Currency: {Currency}, AuthorizationCode: {AuthorizationCode}, CreatedOn: {CreatedOn}",
            e.CardId, e.Amount, e.Currency, e.AuthorizationCode, e.CreatedOn);
        
        await _authorizationEventRepository.SaveApprovedAsync(e, messageId, DateTime.UtcNow, cancellationToken);
    }

    public async Task HandleAsync(AuthorizationDeclinedEvent e, string messageId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Declined approved event. Card: {CardId}, Amount: {Amount}, Currency: {Currency}, ReasonCode: {ReasonCode}, CreatedOn: {CreatedOn}",
            e.CardId, e.Amount, e.Currency, e.ReasonCode, e.CreatedOn);
        
        await _authorizationEventRepository.SaveDeclinedAsync(e, messageId, DateTime.UtcNow, cancellationToken);
    }
}