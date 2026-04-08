namespace IssuingPayment.WorkerService.Application.Events;

public record AuthorizationDeclinedEvent(string CardId,
                                         decimal Amount,
                                         string Currency,
                                         string ReasonCode,
                                         DateTime CreatedOn) : IAuthorizationEvent;