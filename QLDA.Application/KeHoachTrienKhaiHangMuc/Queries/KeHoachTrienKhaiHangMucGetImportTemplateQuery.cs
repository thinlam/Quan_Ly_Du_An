using BuildingBlocks.CrossCutting.Offices;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucGetImportTemplateQuery(Guid? DuAnId = null)
    : IRequest<List<List<ComboData>>>;

internal class KeHoachTrienKhaiHangMucGetImportTemplateQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucGetImportTemplateQuery, List<List<ComboData>>> {
    private readonly IRepository<DuAn, Guid> _duAnRepo =
        serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DanhMucGiaiDoan, int> _giaiDoanRepo =
        serviceProvider.GetRequiredService<IRepository<DanhMucGiaiDoan, int>>();
    private readonly IRepository<DmDonVi, long> _donViRepo =
        serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> _userRepo =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IAuthorizationManager _authManager =
        serviceProvider.GetRequiredService<IAuthorizationManager>();
    private readonly IUserProvider _userProvider =
        serviceProvider.GetRequiredService<IUserProvider>();

    public async Task<List<List<ComboData>>> Handle(
        KeHoachTrienKhaiHangMucGetImportTemplateQuery request,
        CancellationToken cancellationToken = default) {
        var danhSachDuAn = await _authManager
            .FilterVisible(_duAnRepo.GetQueryableSet(), AuthorizationResourceKeys.DuAn)
            .AsNoTracking()
            .WhereIf(request.DuAnId.HasValue, e => e.Id == request.DuAnId!.Value)
            .OrderBy(e => e.TenDuAn)
            .Select(e => new ComboData {
                Name = e.TenDuAn ?? string.Empty,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        var danhSachGiaiDoan = await _giaiDoanRepo.GetQueryableSet()
            .AsNoTracking()
            .Select(e => new ComboData {
                Name = e.Ten ?? string.Empty,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        var danhSachDonVi = await _donViRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.TenDonVi != null && e.TenDonVi != "")
            .OrderBy(e => e.TenDonVi)
            .Select(e => new ComboData {
                Name = e.TenDonVi!,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        var donViId = KeHoachTrienKhaiHangMucImportUserScope.TryGetCurrentDonViId(_userProvider);
        var danhSachCanBo = await _userRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.LaDonViChinh == true)
            .WhereIf(donViId > 0, e => e.DonViId == donViId)
            .Where(e => e.HoTen != null && e.HoTen != "")
            .OrderBy(e => e.HoTen)
            .Select(e => new ComboData {
                Name = e.HoTen!,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        return [danhSachDuAn, danhSachGiaiDoan, danhSachDonVi, danhSachCanBo];
    }
}
