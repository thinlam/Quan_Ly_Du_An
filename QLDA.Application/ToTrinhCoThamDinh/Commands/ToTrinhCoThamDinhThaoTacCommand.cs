using BuildingBlocks.Domain.Entities;
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
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;
    private readonly IUserProvider _userProvider;
    private readonly IRepository<UserMaster, long> _userMasterRepo;

    public ToTrinhCoThamDinhThaoTacCommandHandler(IServiceProvider serviceProvider) {
        _userMasterRepo =  serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.ToTrinhCoThamDinh, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duongDiRepo = serviceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
        _userProvider  = serviceProvider.GetRequiredService<IUserProvider>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<int> Handle(ToTrinhCoThamDinhThaoTacCommand request, CancellationToken cancellationToken) {
        try
        {


            var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.ToTrinhCoThamDinh, cancellationToken);
            var statusDict = statuses
                .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
                .ToDictionary(x => x.Ma!, x => x);


            var entity = await _repository.GetQueryableSet().Include(e => e.TrangThai).Include(e => e.DuAn)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            var userId = _userProvider.Info.UserID;
            var maTrangThai = entity.TrangThai.Ma;

            await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
            long createUserId = 0;
            long.TryParse(entity.CreatedBy, out createUserId);
            var userThucHien = _userMasterRepo.GetQueryableSet().AsNoTracking().Where(x => x.UserPortalId == createUserId).FirstOrDefault();

            // get các trạng thái được phép xử lý
            var duongDi = await _duongDiRepo.GetQueryableSet().AsNoTracking()
                       .Where(x => x.Used && !(x.IsDeleted ?? false)
                       && x.Loai == PheDuyetEntityNames.QuyetDinhKeHoachThueCNTT
                       && x.MaTrangThaiHienTai == entity.TrangThai.Ma
                       && x.MaTrangThaiTiepTheo == request.TrangThaiTiepTheo
                       && (x.RoleLevel == 0
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChuTri && (_userProvider.Info.PhongBanID == userThucHien.PhongBanId))//entity.DuAn.DonViPhuTrachChinhId)
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.NguoiPhuTrachChinh && _userProvider.Info.UserID == entity.DuAn.LanhDaoPhuTrachId)
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChiDinh && _userProvider.Info.PhongBanID == x.RoleId) // ví dụ phòng KHTC
                                                                                                                             
                       )).ToListAsync(cancellationToken);

            var trangThaiTiepTheoItems = statusDict.GetValueOrDefault(request.TrangThaiTiepTheo);
            ManagedException.ThrowIf(trangThaiTiepTheoItems == null, "Không tìm thấy trạng thái cần cập nhật!");

            if (duongDi == null || duongDi.Count == 0)
                ManagedException.Throw("Tài khoản không có quyền!");

            entity.TrangThaiId = trangThaiTiepTheoItems?.Id;
            var history = new PheDuyetHistory
            {
                Id = Guid.NewGuid(),
                EntityName = entity.Loai,
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
            Log.Information($"ToTrinhCoThamDinhThaoTacCommand error: {ex.Message}");
            throw;
        }
    }
}
