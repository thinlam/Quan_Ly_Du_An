using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.KySos.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Application.KySos.Queries;

public record NoiDungDaKyGetDanhSachQuery(NoiDungDaKySearchDto SearchDto)
    : AggregateRootPagination, IRequest<PaginatedList<NoiDungDaKyDto>>;

internal class NoiDungDaKyGetDanhSachQueryHandler
    : IRequestHandler<NoiDungDaKyGetDanhSachQuery, PaginatedList<NoiDungDaKyDto>> {
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

    public async Task<PaginatedList<NoiDungDaKyDto>> Handle(
        NoiDungDaKyGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var search = request.SearchDto;
        var users = _userRepository.GetQueryableSet().AsNoTracking();

        var query = _tepDinhKemRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.ParentId != null)
            .Where(e => SignedGroupTypes.Contains(e.GroupType))
            .WhereIf(search.CreateUserId.HasValue,
                e => e.CreatedBy == search.CreateUserId!.Value.ToString())
            .WhereIf(search.TuNgay.HasValue,
                e => e.CreatedAt >= search.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(search.DenNgay.HasValue,
                e => e.CreatedAt <= search.DenNgay!.Value.ToEndOfDayUtc())
            .WhereIf(!string.IsNullOrWhiteSpace(search.GroupType),
                e => e.GroupType == search.GroupType)
            .LeftOuterJoin(users, e => e.CreatedBy, u => u.Id.ToString(), (e, user) => new { e, user })
            .OrderByDescending(x => x.e.CreatedAt)
            .Select(x => new NoiDungDaKyDto {
                Id = x.e.Id,
                ParentId = x.e.ParentId,
                FileName = x.e.FileName,
                FileOrginal = x.e.OriginalName,
                GroupId = x.e.GroupId,
                GroupName = x.e.GroupType,
                CreateUserId = x.user != null ? x.user.Id : null,
                CreateUserName = x.user != null ? x.user.HoTen : null,
                CreateDate = x.e.CreatedAt.ToDateOnlyVn(),
            });

        return await query.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
