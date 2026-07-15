using BuildingBlocks.CrossCutting.DateTimes;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.TepDinhKems.DTOs;


namespace QLDA.Application.KySos.Queries;

public record NoiDungDaKyGetDanhSachQuery(NoiDungDaKySearchDto SearchDto) : AggregateRootSearch, IRequest<PaginatedList<TepDinhKemDto>>;

internal class NoiDungDaKyGetDanhSachQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<NoiDungDaKyGetDanhSachQuery, PaginatedList<TepDinhKemDto>> {
    private readonly IRepository<Attachment, Guid> _tepDinhKemRepository =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    private readonly IRepository<UserMaster, long> _userRepository =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IDateTimeProvider _clock =
        serviceProvider.GetRequiredService<IDateTimeProvider>();

    public async Task<PaginatedList<TepDinhKemDto>> Handle(
        NoiDungDaKyGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var search = request.SearchDto;
        var users = _userRepository.GetQueryableSet().AsNoTracking();

        var rows = await _tepDinhKemRepository
            .GetQueryableSet(OnlyNotDeleted: false, OrderByIndex: false)
            .ApplyFiltersAsync(search, users, serviceProvider, _clock, cancellationToken);

        var dtos = rows.Select(x => new TepDinhKemDto {
            Id = x.E.Id,
            ParentId = x.E.ParentId,
            GroupId = x.E.GroupId,
            GroupType = x.E.GroupType,
            Type = x.E.Type,
            FileName = x.E.FileName,
            OriginalName = x.E.OriginalName,
            Path = x.E.Path,
            Size = x.E.Size,
            TenNguoiTao = x.User?.HoTen,
            CreatedBy = x.E.CreatedBy,
            CreatedAt = x.E.CreatedAt,
            UpdatedBy = x.E.UpdatedBy,
            UpdatedAt = x.E.UpdatedAt,
        }).ToList();

        return PaginatedList<TepDinhKemDto>.Create(
            dtos, request.Skip(), request.Take());
    }
}
