using AutoFixture;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using CashFlow.Api.Application.CommandHandlers;
using CashFlow.Api.Application.Commands;
using CashFlow.Api.Domain.Events;
using CashFlow.Api.Infrastructure.Messaging;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Interfaces;
using CashFlow.Api.Infrastructure.Settings;
using CashFlow.Api.Infrastructure.Persistence.NoSql.Models;
using CashFlow.Api.Domain.Aggregates.CashFlow;

namespace CashFlow.Tests.Application.CommandHandlers;

public class InflowCommandHandlerTests
{
    private readonly IFixture _fixture;

    public InflowCommandHandlerTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_ShouldProcessInflowAndReturnTransactionId()
    {
        // Arrange
        var command = _fixture.Build<InflowCommand>()
                              .With(x => x.CompanyAccountId, Guid.NewGuid())
                              .With(c => c.Amount, 100.0m)
                              .With(x => x.Description, "Deposit from Mary")
                              .Create();

        var eventStore = Substitute.For<IEventStore>();
        var snapshotStore = Substitute.For<ISnapshotStore>();
        var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());
        var publisher = Substitute.For<Publisher<InflowProcessedEvent>>(Substitute.For<ILogger<Publisher<InflowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
        var publisherFactory = Substitute.For<IPublisherFactory>();
        publisherFactory.CreatePublisher<InflowProcessedEvent>().Returns(publisher);
        var logger = Substitute.For<ILogger<InflowCommandHandler>>();

        snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        snapshotStore.GetLastSnapshotAsync(command.CompanyAccountId).Returns((CashFlowSnapshot?)null);

        var handler = new InflowCommandHandler(eventStore, snapshotStore, publisherFactory, logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();

        await eventStore.Received(1).AppendEventsAsync(Arg.Any<string>(), Arg.Is<IEnumerable<IDomainEvent>>(e => e.Any()));
        await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        await publisher.Received().PublishAsync(Arg.Any<InflowProcessedEvent>());
    }

    [Fact(Skip = "Refatorar publisher para testar")]
    public async Task Handle_ShouldProcessInflow_WhenSnapshotIsNull()
    {
        // Arrange
        var command = _fixture.Create<InflowCommand>();

        var eventStore = Substitute.For<IEventStore>();
        var snapshotStore = Substitute.For<ISnapshotStore>();
        var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());
        var publisher = Substitute.For<Publisher<InflowProcessedEvent>>(Substitute.For<ILogger<Publisher<InflowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
        var publisherFactory = Substitute.For<IPublisherFactory>();
        publisherFactory.CreatePublisher<InflowProcessedEvent>().Returns(publisher);
        var logger = Substitute.For<ILogger<InflowCommandHandler>>();

        snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        snapshotStore.GetLastSnapshotAsync(command.CompanyAccountId).Returns((CashFlowSnapshot?)null);

        InflowProcessedEvent? publishedEvent = null;
        publisher.PublishAsync(Arg.Do<InflowProcessedEvent>(e => publishedEvent = e)).Returns(Task.CompletedTask);

        var handler = new InflowCommandHandler(eventStore, snapshotStore, publisherFactory, logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);

        await eventStore.Received(1).AppendEventsAsync(Arg.Any<string>(), Arg.Any<IEnumerable<IDomainEvent>>());
        await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        await publisher.Received().PublishAsync(Arg.Any<InflowProcessedEvent>());

        publishedEvent.Should().NotBeNull();
        publishedEvent!.Amount.Should().Be(command.Amount);
    }

    [Fact(Skip = "Refatorar publisher para testar")]
    public async Task Handle_ShouldAppendEvents_AndPublishProcessedEvent_WhenSnapshotNotFound()
    {
        // Arrange
        var command = _fixture.Create<InflowCommand>();
        var aggregateId = $"{command.CompanyAccountId}_{DateTime.UtcNow:yyyy-MM-dd}";

        var eventStore = Substitute.For<IEventStore>();
        var snapshotStore = Substitute.For<ISnapshotStore>();
        var appSettings = new AppSettings(new AppSettingsFixture().Configuration, Substitute.For<ILogger<AppSettings>>());
        var publisher = Substitute.For<Publisher<InflowProcessedEvent>>(Substitute.For<ILogger<Publisher<InflowProcessedEvent>>>(), appSettings, Substitute.For<IPublishEndpoint>());
        var publisherFactory = Substitute.For<IPublisherFactory>();
        publisherFactory.CreatePublisher<InflowProcessedEvent>().Returns(publisher);
        var logger = Substitute.For<ILogger<InflowCommandHandler>>();

        snapshotStore.GetSnapshotAsync(Arg.Any<string>()).Returns((CashFlowSnapshot?)null);
        snapshotStore.GetLastSnapshotAsync(Arg.Any<Guid>()).Returns((CashFlowSnapshot?)null);

        var publishedEvents = new List<InflowProcessedEvent>();

        publisher.PublishAsync(Arg.Do<InflowProcessedEvent>(e =>
        {
            publishedEvents.Add(e);
        })).Returns(Task.CompletedTask);

        var handler = new InflowCommandHandler(eventStore, snapshotStore, publisherFactory, logger);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        await eventStore.Received(1).AppendEventsAsync(Arg.Is<string>(id => id.StartsWith(command.CompanyAccountId.ToString())), Arg.Any<IEnumerable<IDomainEvent>>());
        await snapshotStore.Received(1).SaveSnapshotAsync(Arg.Any<CashFlowAggregateRoot>());
        publisher.Received().PublishAsync(Arg.Any<InflowProcessedEvent>());

        result.Should().NotBe(Guid.Empty);
        publishedEvents.Should().ContainSingle();
    }
}