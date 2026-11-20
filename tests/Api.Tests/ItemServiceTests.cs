using Api.Caching;
using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Api.Tests;

public class ItemServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_AddsItemToDatabase()
    {
        await using var db = CreateDbContext();
        var service = new ItemService(db, Mock.Of<ICacheService>());

        var created = await service.CreateAsync("widget");

        Assert.NotEqual(0, created.Id);
        Assert.Equal("widget", (await db.Items.FindAsync(created.Id))!.Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItemsInInsertionOrder()
    {
        await using var db = CreateDbContext();
        var service = new ItemService(db, Mock.Of<ICacheService>());

        await service.CreateAsync("first");
        await service.CreateAsync("second");

        var items = await service.GetAllAsync();

        Assert.Equal(["first", "second"], items.Select(i => i.Name));
    }

    [Fact]
    public async Task GetCountCachedAsync_ReturnsCachedValue_WithoutQueryingDatabase()
    {
        await using var db = CreateDbContext();
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync("items:count")).ReturnsAsync("42");
        var service = new ItemService(db, cache.Object);

        var count = await service.GetCountCachedAsync();

        Assert.Equal(42, count);
        cache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task GetCountCachedAsync_ComputesAndCaches_OnCacheMiss()
    {
        await using var db = CreateDbContext();
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync("items:count")).ReturnsAsync((string?)null);
        var service = new ItemService(db, cache.Object);
        await service.CreateAsync("widget");

        var count = await service.GetCountCachedAsync();

        Assert.Equal(1, count);
        cache.Verify(c => c.SetAsync("items:count", "1", It.IsAny<TimeSpan>()), Times.Once);
    }
}
