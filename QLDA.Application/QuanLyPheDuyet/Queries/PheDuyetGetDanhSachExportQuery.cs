using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Providers;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

public record PheDuyetGetDanhSachExportQuery : IRequest<List<PheDuyetExportDto>>
{
    public string? Type { get; set; }
    public string? TrangThai { get; set; }
}

internal class PheDuyetGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PheDuyetGetDanhSachExportQuery, List<PheDuyetExportDto>>
{
    private readonly IUserProvider _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
    private readonly IRepository<PheDuyet, Guid> _pheDuyetRepo =
        serviceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
    private readonly IRepository<DuAn, Guid> _duAnRepo =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IRepository<UserMaster, long> _userMasterRepo =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IAuthorizationContext _authContext =
        serviceProvider.GetRequiredService<IAuthorizationContext>();

    public async Task<List<PheDuyetExportDto>> Handle(
        PheDuyetGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var rows = PheDuyetQueryableExtensions.ApplyDanhSachFilters(
            new PheDuyetDanhSachFilter(request.Type, request.TrangThai),
            _pheDuyetRepo,
            _duAnRepo,
            _duAnBuocRepo,
            tepDinhKemRepo: null,
            _authContext,
            _userProvider.Info.UserID,
            includeAttachments: false);

        ManagedException.ThrowIf(rows.Count == 0, "Không có dữ liệu để xuất");

        var portalIds = rows
            .SelectMany(r => new long?[] {
                r.NguoiTrinhId,
                r.NguoiDuyetId is > 0 ? r.NguoiDuyetId : null,
            })
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var userMap = portalIds.Count == 0
            ? new Dictionary<long, string>()
            : await _userMasterRepo.GetQueryableSet().AsNoTracking()
                .Where(u => u.UserPortalId != null && portalIds.Contains(u.UserPortalId.Value))
                .ToDictionaryAsync(u => u.UserPortalId!.Value, u => u.HoTen ?? string.Empty, cancellationToken);

        return rows.Select((row, index) => new PheDuyetExportDto
        {
            Stt = index + 1,
            TenDuAn = row.TenDuAn,
            TenGiaiDoan = row.TenGiaiDoan,
            TenBuoc = row.TenBuoc,
            NguoiTrinh = ResolveUserName(userMap, row.NguoiTrinhId),
            NguoiDuyet = ResolveUserName(userMap, row.NguoiDuyetId is > 0 ? row.NguoiDuyetId : null),
            TenTrangThai = row.TenTrangThai,
        }).ToList();
    }

    private static string? ResolveUserName(IReadOnlyDictionary<long, string> userMap, long? portalId) =>
        portalId is > 0 && userMap.TryGetValue(portalId.Value, out var name) ? name : null;
}
