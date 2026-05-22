using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Application.KySos.Queries;

public record NoiDungDaKyGetDanhSachQuery(NoiDungDaKySearchDto SearchDto)
    : AggregateRootPagination, IRequest<PaginatedList<TepDinhKemDto>>;

internal class NoiDungDaKyGetDanhSachQueryHandler
    : IRequestHandler<NoiDungDaKyGetDanhSachQuery, PaginatedList<TepDinhKemDto>> {
    private static readonly string[] SignedGroupTypes = [
        GroupTypeConstants.KySo,
        GroupTypeConstants.NoiDungDaKySo
    ];

    private readonly IRepository<TepDinhKem, Guid> _tepDinhKemRepository;
    private readonly IRepository<UserMaster, long> _userRepository;

    public NoiDungDaKyGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _tepDinhKemRepository = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _userRepository = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    }

    public async Task<PaginatedList<TepDinhKemDto>> Handle(
        NoiDungDaKyGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var search = request.SearchDto;
        var users = _userRepository.GetQueryableSet().AsNoTracking();

        var query = _tepDinhKemRepository.GetQueryableSet(OnlyNotDeleted: false)
            .AsNoTracking()
            .Where(e => e.ParentId != null)
            .Where(e => SignedGroupTypes.Contains(e.GroupType))
            .WhereIf(search.CreateUserId.HasValue,
                e => e.CreatedBy == search.CreateUserId!.Value.ToString())
            .WhereIf(search.TuNgay.HasValue,
                e => e.CreatedAt >= search.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(search.DenNgay.HasValue,
                e => e.CreatedAt <= search.DenNgay!.Value.ToEndOfDayUtc())
            .LeftOuterJoin(users, e => e.CreatedBy, u => u.UserPortalId.ToString(), (e, user) => new { e, user })
            .OrderByDescending(x => x.e.CreatedAt)
            .Select(x => new TepDinhKemDto {
                Id = x.e.Id,
                ParentId = x.e.ParentId,
                GroupId = x.e.GroupId,
                GroupType = x.e.GroupType,
                Type = x.e.Type,
                FileName = x.e.FileName,
                OriginalName = x.e.OriginalName,
                Path = x.e.Path,
                Size = x.e.Size,
                TenNguoiTao = x.user != null ? x.user.HoTen : null,
                CreatedBy = x.e.CreatedBy,
                CreatedAt = x.e.CreatedAt,
                UpdatedBy = x.e.UpdatedBy,
                UpdatedAt = x.e.UpdatedAt,
            });

        return await query.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
