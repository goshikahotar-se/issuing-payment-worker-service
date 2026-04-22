using IssuingPayment.WorkerService.Application.Events;

namespace IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;

public interface IAuthorizationEventRepository
{
    Task SaveApprovedAsync(AuthorizationApprovedEvent e, string messageId, DateTime processedOn, CancellationToken cancellationToken);
    Task SaveDeclinedAsync(AuthorizationDeclinedEvent e, string messageId, DateTime processedOn, CancellationToken cancellationToken);
}