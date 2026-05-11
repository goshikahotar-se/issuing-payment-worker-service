using IssuingPayment.WorkerService.Application.Authorizations.ConsumeAuthorizationEvents;
using IssuingPayment.WorkerService.Application.Events;
using Microsoft.Extensions.Logging.Abstractions;
using IssuingPayment.WorkerService.Tests.Fakes;

namespace IssuingPayment.WorkerService.Tests;

public class AuthorizationEventHandlerTests
{
    [Fact]
    public async Task AuthorizationApprovedEventHandlerIsCalled()
    {
        //Arrange
        var approvedEvent = new AuthorizationApprovedEvent(Guid.NewGuid().ToString(), 
                                                           1000,
                                                           "GBP",
                                                           "03C0C0",
                                                           DateTime.UtcNow);

        string messageId = "msg_abdc";

        var repository = new FakeAuthorizationEventRepository();
        var logger = NullLogger<AuthorizationEventHandler>.Instance;
        var handler = new AuthorizationEventHandler(logger, repository);

        //Act
        await handler.HandleAsync(approvedEvent, messageId, CancellationToken.None);
        
        //Assert
        Assert.Equal(1, repository.ApprovedCallCount);
        Assert.Same(approvedEvent, repository.LastApprovedEvent);
        Assert.Equal(messageId, repository.LastMessageId);
        Assert.NotNull(repository.LastProcessedOn);
    }
    
    [Fact]
    public async Task AuthorizationDeclinedEventHandlerIsCalled()
    {
        //Arrange
        var declinedEvent = new AuthorizationDeclinedEvent(Guid.NewGuid().ToString(), 
            1000,
            "GBP",
            "InsufficientFunds",
            DateTime.UtcNow);

        string messageId = "msg_mnbij";

        var repository = new FakeAuthorizationEventRepository();
        var logger = NullLogger<AuthorizationEventHandler>.Instance;
        var handler = new AuthorizationEventHandler(logger, repository);

        //Act
        await handler.HandleAsync(declinedEvent, messageId, CancellationToken.None);
        
        //Assert
        Assert.Equal(1, repository.DeclinedCallCount);
        Assert.Same(declinedEvent, repository.LastDeclinedEvent);
        Assert.Equal(messageId, repository.LastMessageId);
        Assert.NotNull(repository.LastProcessedOn);
    }
}