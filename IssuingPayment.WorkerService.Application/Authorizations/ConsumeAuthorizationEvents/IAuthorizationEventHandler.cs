using IssuingPayment.WorkerService.Application.Events;

namespace IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;

public interface IAuthorizationEventHandler
{
    Task HandleAsync(AuthorizationApprovedEvent e, CancellationToken cancellationToken);
    Task HandleAsync(AuthorizationDeclinedEvent e, CancellationToken cancellationToken);
}