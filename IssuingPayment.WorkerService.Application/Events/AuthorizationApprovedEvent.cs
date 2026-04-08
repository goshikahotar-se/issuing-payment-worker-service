namespace IssuingPayment.WorkerService.Application.Events;

public record AuthorizationApprovedEvent(string CardId, 
                                         decimal Amount, 
                                         string Currency, 
                                         string AuthorizationCode, 
                                         DateTime CreatedOn): IAuthorizationEvent;