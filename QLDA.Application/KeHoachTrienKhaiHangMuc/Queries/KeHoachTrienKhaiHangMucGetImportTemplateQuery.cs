using BuildingBlocks.CrossCutting.Offices;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucGetImportTemplateQuery
    : IRequest<List<List<ComboData>>>;

internal class KeHoachTrienKhaiHangMucGetImportTemplateQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucGetImportTemplateQuery, List<List<ComboData>>> {
    private readonly IRepository<DanhMucGiaiDoan, int> _giaiDoanRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<UserMaster, long> _userRepo =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IUserProvider _userProvider =
        serviceProvider.GetRequiredService<IUserProvider>();

    public async Task<List<List<ComboData>>> Handle(
        KeHoachTrienKhaiHangMucGetImportTemplateQuery request,
        CancellationToken cancellationToken = default) {
        var danhSachGiaiDoan = await _giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        var donViId = KeHoachTrienKhaiHangMucImportUserScope.TryGetCurrentDonViId(_userProvider);

        var danhSachCanBo = await _userRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.LaDonViChinh == true)
            .WhereIf(donViId > 0, e => e.DonViId == donViId)
            .Select(e => new ComboData {
                Name = e.HoTen ?? string.Empty,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        return [danhSachGiaiDoan, danhSachCanBo, danhSachCanBo];
    }
}
