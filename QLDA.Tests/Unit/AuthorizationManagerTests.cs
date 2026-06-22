using BuildingBlocks.Domain.DTOs;
using BuildingBlocks.Domain.Providers;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.Authorization;
using Xunit;

namespace QLDA.Tests.Unit;

public class AuthorizationManagerTests
{
    [Fact]
    public async Task CanExecuteAsync_UnregisteredResource_ShouldThrow()
    {
        var manager = CreateManager();
        var entity = new { Id = 1 };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => manager.CanExecuteAsync("UnknownResource", entity, CancellationToken.None));

        Assert.Contains("No authorization provider registered", ex.Message);
    }

    [Fact]
    public async Task CanExecuteAsync_RegisteredProvider_ShouldDelegate()
    {
        var provider = new StubProvider(canHandle: true, executeResult: true, viewResult: true);
        var manager = CreateManager(provider);
        var entity = new { Id = 1 };

        var result = await manager.CanExecuteAsync(AuthorizationResourceKeys.DuAn, entity, CancellationToken.None);

        Assert.True(result);
        Assert.True(provider.CanExecuteCalled);
    }

    [Fact]
    public async Task CanViewAsync_RegisteredProvider_ShouldDelegate()
    {
        var provider = new StubProvider(canHandle: true, executeResult: true, viewResult: false);
        var manager = CreateManager(provider);
        var entity = new { Id = 1 };

        var result = await manager.CanViewAsync(AuthorizationResourceKeys.DuAn, entity, CancellationToken.None);

        Assert.False(result);
        Assert.True(provider.CanViewCalled);
    }

    [Fact]
    public void Constructor_TwoProvidersSameKey_ShouldThrow()
    {
        var p1 = new StubProvider(canHandle: true, executeResult: true, viewResult: true);
        var p2 = new StubProvider(canHandle: true, executeResult: true, viewResult: true);

        var ex = Assert.Throws<InvalidOperationException>(() => CreateManager(p1, p2));

        Assert.Contains("Multiple IAuthorizationProvider instances", ex.Message);
    }

    private static AuthorizationManager CreateManager(
        IAuthorizationProvider? provider = null,
        IAuthorizationProvider? provider2 = null,
        IAuthorizationContext? ctx = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizationContext>(ctx ?? new StubContext());
        if (provider != null) services.AddSingleton<IAuthorizationProvider>(provider);
        if (provider2 != null) services.AddSingleton<IAuthorizationProvider>(provider2);
        var sp = services.BuildServiceProvider();
        return ActivatorUtilities.CreateInstance<AuthorizationManager>(sp);
    }
}

internal class StubUserProvider : IUserProvider
{
    public long Id => 1;
    public UserInfo Info { get; } = new UserInfo("1", "1", "1");
    public UserAuthInfo AuthInfo { get; } = new UserAuthInfo();
}

internal class StubContext : IAuthorizationContext
{
    public IUserProvider User => new StubUserProvider();
    public long UserId => 1;
    public long? PhongBanId => 1;
    public bool HasKhtcBypass => false;
    public bool IsAdminManager => false;
    public bool HasGlobalBypass => HasKhtcBypass || IsAdminManager;
    public bool HasReadAllBypass => false;
    public Task<long?> GetLanhDaoPhuTrachIdAsync(Guid duAnId, CancellationToken ct)
        => Task.FromResult<long?>(1);
}

internal class StubProvider : IAuthorizationProvider
{
    private readonly bool _canHandle;
    private readonly bool _executeResult;
    private readonly bool _viewResult;

    public bool CanExecuteCalled { get; private set; }
    public bool CanViewCalled { get; private set; }

    public StubProvider(bool canHandle, bool executeResult, bool viewResult)
    {
        _canHandle = canHandle;
        _executeResult = executeResult;
        _viewResult = viewResult;
    }

    public bool CanHandle(string resourceKey) => _canHandle;

    public Task<bool> CanExecuteAsync(object entity, IAuthorizationContext ctx, CancellationToken ct)
    {
        CanExecuteCalled = true;
        return Task.FromResult(_executeResult);
    }

    public Task<bool> CanViewAsync(object entity, IAuthorizationContext ctx, CancellationToken ct)
    {
        CanViewCalled = true;
        return Task.FromResult(_viewResult);
    }

    public IQueryable<T> Filter<T>(IQueryable<T> query, IAuthorizationContext ctx) where T : class => query;
}