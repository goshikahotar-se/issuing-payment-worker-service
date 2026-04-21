using Dapper;
using IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;
using IssuingPayment.WorkerService.Application.Events;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace IssuingPayment.WorkerService.Infrastructure.Authorizations;

public class MySqlAuthorizationEventRepository : IAuthorizationEventRepository
{
    private readonly IConfiguration _configuration;
    
    public MySqlAuthorizationEventRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public  async Task SaveApprovedAsync(AuthorizationApprovedEvent approvedEvent, string messageId, DateTime processedOn, CancellationToken cancellationToken)
    {
        var connectionString = _configuration["MySql:ConnectionString"];
        
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("MySql connection string is empty");
        
        await using var conn = new MySqlConnection(connectionString);

        var sql = @"INSERT INTO authorization_events (message_id, 
                                                      event_type, 
                                                      card_id, 
                                                      amount, 
                                                      currency, 
                                                      authorization_code, 
                                                      reason_code, 
                                                      created_on, 
                                                      processed_on) 
                                              VALUES (@MessageId,
                                                      @EventType,
                                                      @CardId,
                                                      @Amount,
                                                      @Currency,
                                                      @AuthorizationCode,
                                                      @ReasonCode,
                                                      @CreatedOn,
                                                      @ProcessedOn);";

        var eventParameters = new
        {
            MessageId = messageId,
            EventType = "approved",
            CardId = approvedEvent.CardId,
            Amount = approvedEvent.Amount,
            Currency = approvedEvent.Currency,
            AuthorizationCode = approvedEvent.AuthorizationCode,
            ReasonCode = (string?)null,
            CreatedOn = approvedEvent.CreatedOn,
            ProcessedOn = processedOn
        };
        
        var command = new CommandDefinition(sql, eventParameters, cancellationToken: cancellationToken);
        
        await conn.ExecuteAsync(command);
    }

    // public Task SaveDeclinedAsync(AuthorizationDeclinedEvent e, string messageId, DateTime processedOn,
    //     CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
}