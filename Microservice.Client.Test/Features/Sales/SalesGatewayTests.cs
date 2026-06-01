using FluentAssertions;
using Microservice.Client.Features.Sales.Models;
using Microservice.Client.Features.Sales.Services;
using Microservice.Client.Infrastructure.Http;
using Microservice.Client.Infrastructure.Offline;
using Microservice.Client.Infrastructure.Offline.IndexedDb;
using Microservice.Client.Infrastructure.Offline.Sync;
using Moq;
using Xunit;

namespace Microservice.Client.Test.Features.Sales;

public class SalesGatewayTests
{
    private readonly Mock<IConnectivity> _connectivity = new();
    private readonly Mock<ISyncQueue> _queue = new();
    private readonly Mock<IIndexedDb> _cache = new();
    private readonly ApiOptions _options = new() { BaseUrl = "https://localhost/", Version = "v1" };

    private SalesGateway Build() =>
        new(new ApiClient(new HttpClient { BaseAddress = new Uri("https://localhost/") }),
            _cache.Object, _connectivity.Object, _queue.Object, _options);

    private static CreateSaleRequest SampleSale() =>
        new(Guid.NewGuid(), [new CreateSaleItemRequest(Guid.NewGuid(), 2)]);

    [Fact]
    public async Task CreateAsync_offline_enqueues_and_reports_queued()
    {
        _connectivity.SetupGet(c => c.IsOnline).Returns(false);
        var gateway = Build();

        var result = await gateway.CreateAsync(SampleSale());

        result.IsSuccess.Should().BeTrue();
        result.Value.Queued.Should().BeTrue();
        result.Value.ResourceId.Should().BeNull();
        _queue.Verify(q => q.EnqueueAsync(It.Is<SyncOperation>(o =>
            o.EntityType == "sale" && o.Method == "POST")), Times.Once);
    }

    [Fact]
    public async Task Queued_operation_carries_a_stable_idempotency_key()
    {
        _connectivity.SetupGet(c => c.IsOnline).Returns(false);
        SyncOperation? captured = null;
        _queue.Setup(q => q.EnqueueAsync(It.IsAny<SyncOperation>()))
            .Callback<SyncOperation>(o => captured = o)
            .Returns(ValueTask.CompletedTask);
        var gateway = Build();

        await gateway.CreateAsync(SampleSale());

        captured.Should().NotBeNull();
        captured!.IdempotencyKey.Should().Be(captured.Id); // op id == idempotency key (exactly-once on replay)
        captured.JsonBody.Should().NotBeNullOrEmpty();
    }
}
