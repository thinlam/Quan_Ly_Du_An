using BuildingBlocks.Domain.Interfaces;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ThongBaos.Commands;

public enum ThongBaoPheDuyetAction {
    Duyet,
    TraLai,
    TuChoi
}

/// <summary>
/// UC22 — Resolve người nhận + body, rồi gọi dbo.CoreMessaging_CreateNotification.
/// </summary>
public sealed record ThongBaoInsertCommand(
    string EntityName,
    Guid EntityId,
    ThongBaoPheDuyetAction Action,
    string? LyDo = null
) : IRequest<int>;

internal sealed class ThongBaoInsertCommandHandler
    : IRequestHandler<ThongBaoInsertCommand, int>
{
    private const string StoredProcedure = "dbo.CoreMessaging_CreateNotification";
    private const int NotifyTypesId = 24;
    private const string Subject = "Quản lý dự án";
    private const int PortalId = 0;

    private readonly IDapperRepository _dapper;
    private readonly IUserProvider _userProvider;
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<Domain.Entities.DeXuatChuyenTiep, Guid> _deXuatChuyenTiepRepository;
    private readonly ILogger<ThongBaoInsertCommandHandler> _logger;

    public ThongBaoInsertCommandHandler(IServiceProvider serviceProvider)
    {
        _dapper = serviceProvider.GetRequiredService<IDapperRepository>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _pheDuyetRepository = serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _deXuatChuyenTiepRepository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.DeXuatChuyenTiep, Guid>>();
        _logger = serviceProvider.GetRequiredService<ILogger<ThongBaoInsertCommandHandler>>();
    }

    public async Task<int> Handle(
        ThongBaoInsertCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var nguoiGuiId = _userProvider.Info.UserID;
            if (nguoiGuiId <= 0)
            {
                _logger.LogWarning(
                    "UC22 skip notification: NguoiGuiId invalid. EntityName={EntityName}, EntityId={EntityId}",
                    request.EntityName, request.EntityId);
                return 0;
            }

            var nguoiNhanId = await ResolveNguoiNhanIdAsync(request, cancellationToken);
            if (nguoiNhanId <= 0)
            {
                _logger.LogWarning(
                    "UC22 skip notification: NguoiNhanId invalid. EntityName={EntityName}, EntityId={EntityId}",
                    request.EntityName, request.EntityId);
                return 0;
            }

            var body = await BuildBodyAsync(request, cancellationToken);

            var parameters = new
            {
                NguoiNhanId = nguoiNhanId,
                NguoiGuiId = nguoiGuiId,
                NotifyTypesId,
                Subject,
                Body = body,
                PortalId
            };

            return await _dapper.ExecuteStoredProcAsync(StoredProcedure, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "UC22 CoreMessaging_CreateNotification failed. EntityName={EntityName}, EntityId={EntityId}, Action={Action}",
                request.EntityName, request.EntityId, request.Action);
            return 0;
        }
    }

    private async Task<long> ResolveNguoiNhanIdAsync(
        ThongBaoInsertCommand request,
        CancellationToken cancellationToken)
    {
        var pheDuyet = await _pheDuyetRepository.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.EntityId == request.EntityId && e.EntityName == request.EntityName,
                cancellationToken);

        long nguoiNhanId = pheDuyet?.NguoiTrinhId ?? 0;
        if (nguoiNhanId > 0)
            return nguoiNhanId;

        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(
                OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(
                s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh
                     && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt,
                cancellationToken);

        if (trangThaiDaTrinh == null)
            return 0;

        var nguoiTrinhTuHistory = await _historyRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(h => h.EntityId == request.EntityId
                        && h.EntityName == request.EntityName
                        && h.TrangThaiId == trangThaiDaTrinh.Id
                        && h.NguoiXuLyId != null
                        && h.NguoiXuLyId > 0)
            .OrderByDescending(h => h.NgayXuLy)
            .Select(h => h.NguoiXuLyId)
            .FirstOrDefaultAsync(cancellationToken);

        return nguoiTrinhTuHistory ?? 0;
    }

    private async Task<string> BuildBodyAsync(
        ThongBaoInsertCommand request,
        CancellationToken cancellationToken)
    {
        var tenLoai = request.EntityName.GetDescriptionFromName();
        var ketQua = request.Action switch {
            ThongBaoPheDuyetAction.Duyet => "đã được duyệt",
            ThongBaoPheDuyetAction.TraLai =>
                $"đã bị trả lại{(string.IsNullOrWhiteSpace(request.LyDo) ? string.Empty : $". Lý do: {request.LyDo}")}",
            ThongBaoPheDuyetAction.TuChoi =>
                $"đã bị từ chối{(string.IsNullOrWhiteSpace(request.LyDo) ? string.Empty : $". Lý do: {request.LyDo}")}",
            _ => "đã được xử lý"
        };

        if (request.EntityName == PheDuyetEntityNames.DeXuatChuTruongChuyenTiep)
        {
            var entity = await _deXuatChuyenTiepRepository.GetQueryableSet()
                .AsNoTracking()
                .Include(x => x.DuAn)
                .ThenInclude(x => x!.BuocHienTai)
                .FirstOrDefaultAsync(e => e.Id == request.EntityId, cancellationToken);

            return $"Tờ trình/phê duyệt <b>{tenLoai}</b>" +
                   $" giá trị giải ngân <b>{entity?.SoLieuGiaiNgan}</b>" +
                   $" của dự án <b>{entity?.DuAn?.TenDuAn}</b> - " +
                   $"<b>{entity?.DuAn?.BuocHienTai?.TenBuoc}</b> {ketQua}";
        }

        return $"Tờ trình/phê duyệt <b>{tenLoai}</b> {ketQua}";
    }
}
