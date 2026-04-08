namespace IssuingPayment.WorkerService.Application.Events;

public interface IAuthorizationEvent
{
    string CardId { get; init; }
    decimal Amount { get; init; }
    string Currency { get; init; }
    DateTime CreatedOn { get; init; }
}