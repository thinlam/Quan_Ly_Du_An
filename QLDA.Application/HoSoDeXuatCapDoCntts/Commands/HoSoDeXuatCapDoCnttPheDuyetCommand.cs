using BuildingBlocks.CrossCutting.ExtensionMethods;
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common;
using QLDA.Application.Providers;
using QLDA.Application.ToTrinhCoThamDinhs.Commands;
using QLDA.Domain.Constants;
using Serilog;
using System.Data;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;



public record HoSoDeXuatCapDoCnttPheDuyetCommand(Guid Id, string Loai, string TrangThaiTiepTheo, string? noiDung) : IRequest<int>;

//internal class HoSoDeXuatCapDoCnttPheDuyetCommanddHandler : IRequestHandler<HoSoDeXuatCapDoCnttPheDuyetCommand>
//{
//    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
//    private readonly IUnitOfWork _unitOfWork;

//    public HoSoDeXuatCapDoCnttPheDuyetCommanddHandler(IServiceProvider serviceProvider)
//    {
//        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
//        _unitOfWork = HoSoDeXuatCapDoCntt.UnitOfWork;
//    }

internal class HoSoDeXuatCapDoCnttPheDuyetCommandHandler : IRequestHandler<HoSoDeXuatCapDoCnttPheDuyetCommand, int>
{
    private readonly IRepository<Domain.Entities.HoSoDeXuatCapDoCntt, Guid> _repository;
    private readonly IRepository<PheDuyetHistory, Guid> _historyRepository;

    private readonly IRepository<UserMaster, long> _userMasterRepo; 
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> _duongDiRepo;
    private readonly IRepository<DuAnBuoc, int> _buocDuAnRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppSettingsProvider _settings;
    private readonly IUserProvider _userProvider;

    public HoSoDeXuatCapDoCnttPheDuyetCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<Domain.Entities.HoSoDeXuatCapDoCntt, Guid>>();
        _historyRepository = serviceProvider.GetRequiredService<IRepository<PheDuyetHistory, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userMasterRepo =  serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
        _duongDiRepo = serviceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
        _buocDuAnRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _settings = serviceProvider.GetRequiredService<IAppSettingsProvider>();
        _unitOfWork = _repository.UnitOfWork;
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
    }

    public async Task<int> Handle(HoSoDeXuatCapDoCnttPheDuyetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            /*
             Hiện tại đang dùng cho các màn hình sau 
            QUyết định kế hoạch thuê
            Hồ sơ đề xuất cấp độ cntt
             */
            //         Validate transition (Khởi tạo → Chuyển phòng hạ tầng → Trả/Trình → Trả/Duyệt/Từ chối)// mới issue 9488

            var statuses = await _statusRepository.GetByLoaiAsync(PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);
            var statusDict = statuses
                .Where(x => !string.IsNullOrWhiteSpace(x.Ma))
                .Where(x => x.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt)
                .ToDictionary(x => x.Ma!, x => x);
            ManagedException.ThrowIfNull(statusDict, "Trạng thái tiếp theo ko hợp lệ hoặc không tồn tại");

            var entity = await _repository.GetQueryableSet().Include(e => e.TrangThai).Include(e => e.DuAn)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            var userId = _userProvider.Info.UserID;
            var maTrangThai = entity.TrangThai.Ma;

            await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
            // Get PhongBanPhuTrachChinh/PhoiHop ở bước này 
            // var buoc = _buocDuAnRepo.GetQueryableSet().
            long createUserId = 0;
            long.TryParse(entity.CreatedBy, out createUserId);
            var userChuTri = _userMasterRepo.GetQueryableSet().AsNoTracking().Where(x => x.UserPortalId == createUserId).FirstOrDefault();
            
            
            // get các trạng thái được phép xử lý
            var duongDi = await _duongDiRepo.GetQueryableSet().AsNoTracking()
                       .Where(x => x.Used && !(x.IsDeleted ?? false)
                       && x.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt
                       && x.MaTrangThaiHienTai == entity.TrangThai.Ma
                       && x.MaTrangThaiTiepTheo == request.TrangThaiTiepTheo
                       && (x.RoleLevel == 0
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChuTri && _userProvider.Info.PhongBanID == userChuTri.PhongBanId)
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.NguoiPhuTrachChinh &&  _userProvider.Info.UserID == entity.DuAn.LanhDaoPhuTrachId )
                       || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChiDinh && _userProvider.Info.PhongBanID == x.RoleId) // chuyển chỉ định phòng hạ tầng nhận
                       )).ToListAsync(cancellationToken);

            var trangThaiTiepTheoItems = statusDict.GetValueOrDefault(request.TrangThaiTiepTheo);
            ManagedException.ThrowIf(trangThaiTiepTheoItems == null, "Không tìm thấy trạng thái cần cập nhật!");

            if (duongDi == null || duongDi.Count == 0)
                ManagedException.Throw("Không tìm thấy trạng thái hợp lệ với tài khoản này!");

            entity.TrangThaiId = trangThaiTiepTheoItems?.Id;
            var history = new PheDuyetHistory
            {
                Id = Guid.NewGuid(),
                EntityName = PheDuyetEntityNames.HoSoDeXuatCapDoCntt,
                EntityId = entity.Id,
                DuAnId = entity.DuAnId,
                BuocId = entity.BuocId,
                NoiDung = !string.IsNullOrEmpty(request.noiDung) ? request.noiDung
                        : $"{PheDuyetEntityNames.HoSoDeXuatCapDoCntt.GetDescriptionFromName()} đã {trangThaiTiepTheoItems?.Ten}",
                NguoiXuLyId = _userProvider.Info.UserID,
                TrangThaiId = trangThaiTiepTheoItems?.Id,
                NgayXuLy = DateTimeOffset.UtcNow
            };

            await _historyRepository.AddAsync(history, cancellationToken);

            return await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Information($"HoSoDeXuatCapDoCnttPheDuyetCommand error: {ex.Message}");
            throw;
        }
    }
}

    
    //internal class HoSoDeXuatCapDoCnttThayDoiTrangThaiCommandHandler 
    //    : IRequestHandler<HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand> {

    //    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    //    private readonly IUnitOfWork _unitOfWork;

    //    public HoSoDeXuatCapDoCnttThayDoiTrangThaiCommandHandler(IServiceProvider serviceProvider) {
    //        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
    //        _unitOfWork = HoSoDeXuatCapDoCntt.UnitOfWork;
    //    }

    //    public async Task Handle(HoSoDeXuatCapDoCnttThayDoiTrangThaiCommand request, CancellationToken cancellationToken = default) {
    //        var entity = await HoSoDeXuatCapDoCntt.GetQueryableSet()
    //            .FirstOrDefaultAsync(e => e.Id == request.Dto.HoSoId && !e.IsDeleted, cancellationToken);
    //        ManagedException.ThrowIfNull(entity);

    //        // Validate transition (Khởi tạo → Trình → Duyệt/Từ chối) // cũ
    //        // Validate transition (Khởi tạo → Chuyển phòng hạ tầng → Trả/Trình → Trả/Duyệt/Từ chối)// mới issue 9488
    //        ValidateStatusTransition(entity.TrangThaiId, request.Dto.TrangThaiId);

    //        entity.TrangThaiId = request.Dto.TrangThaiId;
    //        entity.NgayTrinh = DateOnly.FromDateTime(DateTime.UtcNow).ToStartOfDayUtc();

    //        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    //        await HoSoDeXuatCapDoCntt.UpdateAsync(entity, cancellationToken);
    //        await _unitOfWork.SaveChangesAsync(cancellationToken);
    //        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    //    }

    //    private static void ValidateStatusTransition(int? currentStatus, int newStatus) {
    //        // Khởi tạo → Trình ✓
    //        // Trình → Duyệt ✓
    //        // Trình → Từ chối ✓
    //        // Các chuyển đổi khác → ✗
    //        if (currentStatus == newStatus) {
    //            throw new InvalidOperationException("Trạng thái mới phải khác trạng thái hiện tại");
    //        }
    //    }
//}
