using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;
using QLDA.Application.Providers;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities.DanhMuc;
using Serilog;

namespace QLDA.Application.ToTrinhCoThamDinhs.Commands;

/// <summary>
/// Duyệt phân khai kinh phí - LDDV role
/// </summary>
public record ToTrinhCoThamDinhThaoTacCommand(Guid Id, string Loai, string TrangThaiTiepTheo, string? noiDung) : IRequest<int>;

internal class ToTrinhCoThamDinhThaoTacCommandHandler : IRequestHandler<ToTrinhCoThamDinhThaoTacCommand, int> {
    private readonly IRepository<Domain.Entities.ToTrinhCoThamDinh, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> _duongDiRepo;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;
    private readonly IUserProvider _userProvider;

    public ToTrinhCoThamDinhThaoTacCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhCoThamDinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duongDiRepo = serviceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
        _userProvider  = serviceProvider.GetRequiredService<IUserProvider>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
    }

    public async Task<int> Handle(ToTrinhCoThamDinhThaoTacCommand request, CancellationToken cancellationToken) {
        try
        {


            var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ToTrinhCoThamDinh, cancellationToken);
            var statusDict = statuses.ToDictionary(x => x.Ma);


            var entity = await _repository.GetQueryableSet().Include(e => e.TrangThai).Include(e => e.DuAn)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            var userId = _userProvider.Info.UserID;
            var maTrangThai = entity.TrangThai.Ma;

            if (entity.BuocId.HasValue) {
                var buoc = await _duAnBuocRepo.GetQueryableSet()
                    .Include(e => e.DuAn)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == entity.BuocId.Value, cancellationToken);
                if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _userProvider, cancellationToken))
                    throw new ManagedException("Phòng ban không có quyền thao tác bước này");
            }

            // get các trạng thái được phép xử lý
            var duongDi = await _duongDiRepo.GetQueryableSet().AsNoTracking()
                       .Where(x => x.Used && !(x.IsDeleted ?? false)
                       && x.MaTrangThaiHienTai == entity.TrangThai.Ma
                       && x.MaTrangThaiTiepTheo == request.TrangThaiTiepTheo
                       && (x.RoleLevel == 0
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChuTri && _userProvider.Info.PhongBanID == entity.DuAn.DonViPhuTrachChinhId)
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.NguoiPhuTrachChinh && _userProvider.Info.UserID == entity.DuAn.LanhDaoPhuTrachId)
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChiDinh && _userProvider.Info.PhongBanID == x.RoleId) // ví dụ phòng KHTC
                                                                                                                               // chưa xét cấp đơn vị && phòng ban chỉ định
                       )).ToListAsync(cancellationToken);

            var trangThaiTiepTheoItems = statusDict.GetValueOrDefault(request.TrangThaiTiepTheo);
            ManagedException.ThrowIf(trangThaiTiepTheoItems == null, "Không tìm thấy trạng thái cần cập nhật!");

            if (duongDi == null || duongDi.Count == 0)
                ManagedException.Throw("Tài khoản không có quyền!");

            entity.TrangThaiId = trangThaiTiepTheoItems?.Id;
            var history = new PheDuyetHistory
            {
                Id = Guid.NewGuid(),
                EntityName = PheDuyetEntityNames.QuyetDinhKeHoachThue,
                EntityId = entity.Id,
                DuAnId = entity.DuAnId,
                BuocId = entity.BuocId,
                NoiDung = request.noiDung,
                NguoiXuLyId = _userProvider.Info.UserID,
                TrangThaiId = trangThaiTiepTheoItems?.Id,
                NgayXuLy = DateTimeOffset.UtcNow
            };

            await _historyRepository.AddAsync(history, cancellationToken);

            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex )
        {
            Log.Information($"ToTrinhCoThamDinhInsertCommand error {ex.Message}");
            throw;
        }
    }
}