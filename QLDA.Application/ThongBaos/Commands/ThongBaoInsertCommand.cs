using BuildingBlocks.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace QLDA.Application.ThongBaos.Commands;

/// <summary>
/// UC22 — Gọi dbo.CoreMessaging_CreateNotification (Dapper).
/// </summary>
public sealed record ThongBaoInsertCommand(
    long NguoiGuiId,
    long NguoiNhanId,
    string Body
) : IRequest<int>;

internal sealed class ThongBaoInsertCommandHandler
    : IRequestHandler<ThongBaoInsertCommand, int>
{
    private const string StoredProcedure = "dbo.CoreMessaging_CreateNotification";
    private const int NotifyTypesId = 24;
    private const string Subject = "Quản lý dự án";
    private const int PortalId = 0;

    private readonly IDapperRepository _dapper;

    public ThongBaoInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
    }

    public async Task<int> Handle(
        ThongBaoInsertCommand request,
        CancellationToken cancellationToken)
    {
        var parameters = new
        {
            NguoiNhanId = request.NguoiNhanId,
            NguoiGuiId = request.NguoiGuiId,
            NotifyTypesId,
            Subject,
            Body = request.Body,
            PortalId
        };

        // IDapperRepository.ExecuteStoredProcAsync chưa nhận CancellationToken.
        _ = cancellationToken;

        return await _dapper.ExecuteStoredProcAsync(StoredProcedure, parameters);
    }
}
