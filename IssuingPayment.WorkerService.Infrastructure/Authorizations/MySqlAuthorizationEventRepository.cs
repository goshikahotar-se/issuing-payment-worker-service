using Dapper;
using IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;
using IssuingPayment.WorkerService.Application.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace IssuingPayment.WorkerService.Infrastructure.Authorizations;

public class MySqlAuthorizationEventRepository : IAuthorizationEventRepository
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MySqlAuthorizationEventRepository> _logger;
    
    public MySqlAuthorizationEventRepository(IConfiguration configuration, ILogger<MySqlAuthorizationEventRepository> logger)
    {
        _configuration = configuration;
        _logger = logger;
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
        
        try
        {
            await conn.ExecuteAsync(command);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            _logger.LogWarning("Duplicate message detected. {MessageId}. Skipping insertion but treating as success.", messageId);
        }
    }

    public async Task SaveDeclinedAsync(AuthorizationDeclinedEvent declinedEvent, string messageId, DateTime processedOn, CancellationToken cancellationToken)
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
            EventType = "declined",
            CardId = declinedEvent.CardId,
            Amount = declinedEvent.Amount,
            Currency = declinedEvent.Currency,
            AuthorizationCode = (string?)null,
            ReasonCode = declinedEvent.ReasonCode,
            CreatedOn = declinedEvent.CreatedOn,
            ProcessedOn = processedOn
        };
        
        var command = new CommandDefinition(sql, eventParameters, cancellationToken: cancellationToken);

        try
        {
            await conn.ExecuteAsync(command);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            _logger.LogWarning("Duplicate message detected. {MessageId}. Skipping insertion but treating as success.", messageId);
        }
    }
}