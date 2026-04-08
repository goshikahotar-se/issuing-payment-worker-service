namespace IssuingPayment.WorkerService.Infrastructure.Authorizations;

public record SnsNotificationEnvelope(string Type, string Message);