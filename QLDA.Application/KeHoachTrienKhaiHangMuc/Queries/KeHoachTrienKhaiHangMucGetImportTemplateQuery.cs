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
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
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
        var visibleDuAn = _authManager
            .FilterVisible(_duAnRepo.GetQueryableSet(), AuthorizationResourceKeys.DuAn);

        var danhSachDuAn = await visibleDuAn
            .AsNoTracking()
            .WhereIf(request.DuAnId.HasValue, e => e.Id == request.DuAnId!.Value)
            .OrderBy(e => e.TenDuAn)
            .Select(e => new ComboData {
                Name = e.TenDuAn ?? string.Empty,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);

        var danhSachGiaiDoan = await KeHoachTrienKhaiHangMucImportGiaiDoanHelper.LoadGiaiDoanComboAsync(
            _duAnRepo,
            _duAnBuocRepo,
            _giaiDoanRepo,
            visibleDuAn,
            request.DuAnId,
            cancellationToken);

        var donViId = TryGetCurrentDonViId(_userProvider);
        var danhSachDonVi = await _donViRepo.GetQueryableSet()
            .AsNoTracking()
            .WhereIf(donViId > 0, e => e.DonViCapChaId == donViId)
            .Where(e => e.TenDonVi != null && e.TenDonVi != "")
            .OrderBy(e => e.TenDonVi)
            .Select(e => new ComboData {
                Name = e.TenDonVi!,
                Id = e.Id.ToString(),
            })
            .ToListAsync(cancellationToken);
        var danhSachCanBo = await _userRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(e => e.LaDonViChinh == true)
            .WhereIf(donViId > 0, e => e.DonViId == donViId)
            .Where(e => e.HoTen != null && e.HoTen != "")
            .Where(e => e.UserPortalId != null)
            .OrderBy(e => e.HoTen)
            .Select(e => new ComboData {
                Name = e.HoTen!,
                Id = e.UserPortalId!.Value.ToString(),
            })
            .ToListAsync(cancellationToken);

        return [danhSachDuAn, danhSachGiaiDoan, danhSachDonVi, danhSachCanBo, danhSachDonVi, danhSachCanBo];
    }

    private static long? TryGetCurrentDonViId(IUserProvider userProvider)
    {
        if (userProvider.Id <= 0)
            return null;

        try
        {
            var donViId = userProvider.Info.DonViID;
            return donViId > 0 ? donViId : null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
