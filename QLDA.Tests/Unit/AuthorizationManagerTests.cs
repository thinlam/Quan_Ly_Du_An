using System.Collections.Concurrent;
using BuildingBlocks.Domain.DTOs;
using BuildingBlocks.Domain.Providers;
using Microsoft.Extensions.DependencyInjection;
using QLDA.Application.Authorization;
using Xunit;

namespace QLDA.Tests.Unit;

public class AuthorizationManagerTests
{
    private static AuthorizationManager CreateManager(IAuthorizationContext? ctx = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizationContext>(ctx ?? new StubContext());
        var sp = services.BuildServiceProvider();
        return new AuthorizationManager(sp);
    }

    [Fact]
    public void RegisterProvider_ShouldAddProvider()
    {
        var manager = CreateManager();
        var provider = new StubProvider(canHandle: true, executeResult: true, viewResult: true);

        manager.RegisterProvider(AuthorizationResourceKeys.DuAn, provider);
    }

    [Fact]
    public void RegisterProvider_DuplicateKey_ShouldThrow()
    {
        var manager = CreateManager();
        var provider = new StubProvider(canHandle: true, executeResult: true, viewResult: true);
        manager.RegisterProvider(AuthorizationResourceKeys.DuAn, provider);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            manager.RegisterProvider(AuthorizationResourceKeys.DuAn, provider));

        Assert.Contains("already registered", ex.Message);
    }

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
        var manager = CreateManager();
        var provider = new StubProvider(canHandle: true, executeResult: true, viewResult: true);
        var entity = new { Id = 1 };

        manager.RegisterProvider(AuthorizationResourceKeys.DuAn, provider);

        var result = await manager.CanExecuteAsync(AuthorizationResourceKeys.DuAn, entity, CancellationToken.None);

        Assert.True(result);
        Assert.True(provider.CanExecuteCalled);
    }

    [Fact]
    public async Task CanViewAsync_RegisteredProvider_ShouldDelegate()
    {
        var manager = CreateManager();
        var provider = new StubProvider(canHandle: true, executeResult: true, viewResult: false);
        var entity = new { Id = 1 };

        manager.RegisterProvider(AuthorizationResourceKeys.DuAn, provider);

        var result = await manager.CanViewAsync(AuthorizationResourceKeys.DuAn, entity, CancellationToken.None);

        Assert.False(result);
        Assert.True(provider.CanViewCalled);
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
    public bool HasGlobalBypass => false;
    public long UserId => 1;
    public long? PhongBanId => 1;
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
