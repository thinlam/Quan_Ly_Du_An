using BuildingBlocks.CrossCutting.DateTimes;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.KySos.DTOs;


namespace QLDA.Application.KySos.Queries;

public record NoiDungDaKyGetDanhSachExportQuery(NoiDungDaKySearchDto SearchDto)
    : IRequest<List<NoiDungDaKyExportDto>>;

internal class NoiDungDaKyGetDanhSachExportQueryHandler(
    IServiceProvider serviceProvider)
    : IRequestHandler<NoiDungDaKyGetDanhSachExportQuery, List<NoiDungDaKyExportDto>>
{
    private readonly IRepository<Attachment, Guid> _tepDinhKemRepository =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    private readonly IRepository<UserMaster, long> _userRepository =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IDateTimeProvider _clock =
        serviceProvider.GetRequiredService<IDateTimeProvider>();

    public async Task<List<NoiDungDaKyExportDto>> Handle(
        NoiDungDaKyGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var search = request.SearchDto;
        var users = _userRepository.GetQueryableSet().AsNoTracking();

        var rows = await _tepDinhKemRepository
            .GetQueryableSet(OnlyNotDeleted: false, OrderByIndex: false)
            .AsNoTracking()
            .ApplyFiltersAsync(search, users, serviceProvider, _clock, cancellationToken);

        ManagedException.ThrowIf(rows.Count == 0, "Không có dữ liệu để xuất");

        return rows.Select((row, index) => new NoiDungDaKyExportDto
        {
            Stt = index + 1,
            TenFile = row.E.FileName,
            TenGoc = row.E.OriginalName,
            LoaiFile = row.E.Type,
            DungLuong = NoiDungDaKyQueryableExtensions.FormatDungLuong(row.E.Size),
            NguoiTao = row.User?.HoTen,
        }).ToList();
    }
}
