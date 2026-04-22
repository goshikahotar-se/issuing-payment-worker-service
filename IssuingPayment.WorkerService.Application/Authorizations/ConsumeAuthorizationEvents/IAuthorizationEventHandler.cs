using IssuingPayment.WorkerService.Application.Events;

namespace IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;

public interface IAuthorizationEventHandler
{
    Task HandleAsync(AuthorizationApprovedEvent e, string messageId, CancellationToken cancellationToken);
    Task HandleAsync(AuthorizationDeclinedEvent e, string messageId, CancellationToken cancellationToken);
}