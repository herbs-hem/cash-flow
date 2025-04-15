using AutoFixture;
using CashFlow.Api.Application.CommandHandlers;
using CashFlow.Api.Application.Commands;
using CashFlow.Api.Domain.Aggregates.CashFlow;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Infrastructure.Messaging;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;
using CashFlow.Api.Infrastructure.Settings;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using NSubstitute;

namespace CashFlow.Tests.Application.CommandHandlers;

public class OutflowCommandHandlerTests
{
    private readonly IFixture _fixture;

    public OutflowCommandHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_ShouldProcessOutflow_AndPublishEvents()
    {
        // Arrange
        var command = _fixture.Create<OutflowCommand>();

        var aggregate = new CashFlowAggregateRoot(
            $"{command.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}",
            DateTime.UtcNow.Date)
        {
            CompanyAccountId = command.CompanyAccountId,
            BalanceStartDay = 1000,
            BalanceEndDay = 1000
        };

        var eventStore = Substitute.For<IEventStore>();
        var snapshotStore = Substitute.For<ISnapshotStore>();
        var publisherFactory = Substitute.For<IPublisherFactory>();
        var logger = Substitute.For<ILogger<OutflowCommandHandler>>();
        var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());
        var publisher = Substitute.For<Publisher<OutflowProcessedEvent>>(Substitute.For<ILogger<Publisher<OutflowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
        publisherFactory.CreatePublisher<OutflowProcessedEvent>().Returns(publisher);

        // simula snapshot null, mas tem o último snapshot (sem serialização)
        snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        snapshotStore.GetLastSnapshotAsync(Arg.Any<Guid>()).Returns(new CashFlowSnapshot
        {
            AggregateId = aggregate.AggregateId,
            AggregateData = aggregate.ToBsonDocument(aggregate.GetType()),
            CompanyAccountId = aggregate.CompanyAccountId,
            BalanceEnd = 1000
        });

        var handler = new OutflowCommandHandler(eventStore, snapshotStore, publisherFactory, logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await eventStore.Received(1).AppendEventsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<IDomainEvent>>());
        await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        await publisher.Received().PublishAsync(Arg.Any<OutflowProcessedEvent>());
        result.Should().NotBe(Guid.Empty);
    }
}