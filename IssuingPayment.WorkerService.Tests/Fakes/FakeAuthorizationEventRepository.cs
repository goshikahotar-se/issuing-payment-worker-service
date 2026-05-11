using IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;
using IssuingPayment.WorkerService.Application.Events;

namespace IssuingPayment.WorkerService.Tests.Fakes;

public class FakeAuthorizationEventRepository : IAuthorizationEventRepository
{
    public AuthorizationApprovedEvent? LastApprovedEvent { get; private set; }
    public AuthorizationDeclinedEvent? LastDeclinedEvent { get; private set; }
    public string? LastMessageId { get; private set; }
    public DateTime? LastProcessedOn { get; private set; }
    public int ApprovedCallCount { get; private set; }
    public int DeclinedCallCount { get; private set; }
    
    public Task SaveApprovedAsync(AuthorizationApprovedEvent e, string messageId, DateTime processedOn,
        CancellationToken cancellationToken)
    {
        LastApprovedEvent = e;
        LastMessageId = messageId;
        LastProcessedOn = processedOn;
        ApprovedCallCount++;
        
        return Task.CompletedTask;
    }

    public Task SaveDeclinedAsync(AuthorizationDeclinedEvent e, string messageId, DateTime processedOn,
        CancellationToken cancellationToken)
    {
        LastDeclinedEvent = e;
        LastMessageId = messageId;
        LastProcessedOn = processedOn;
        DeclinedCallCount++;
        
        return Task.CompletedTask;
    }
}