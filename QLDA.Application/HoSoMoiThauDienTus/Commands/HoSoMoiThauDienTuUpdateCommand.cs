using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuUpdateCommand(HoSoMoiThauDienTuUpdateModel Model) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuUpdateCommandHandler : IRequestHandler<HoSoMoiThauDienTuUpdateCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<ToTrinhQuyetDinh, long> _toTrinhQuyetDinhRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationManager _authManager;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuUpdateCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _toTrinhQuyetDinhRepo = serviceProvider.GetRequiredService<IRepository<ToTrinhQuyetDinh, long>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuUpdateCommand request, CancellationToken cancellationToken = default) {
       
        var entity = await HoSoMoiThauDienTu.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        // ToTrinh/QuyetDinh (ToTrinhQuyetDinh) dùng chung bảng qua EntityId + Loai — load thủ công (Issue #179).
        entity.ToTrinh = await _toTrinhQuyetDinhRepo.GetQueryableSet()
            .FirstOrDefaultAsync(x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh, cancellationToken);
        entity.QuyetDinh = await _toTrinhQuyetDinhRepo.GetQueryableSet()
            .FirstOrDefaultAsync(x => x.EntityId == entity.Id && x.Loai == ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh, cancellationToken);

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);
        await _authManager.EnsureCanExecuteAsync(entity.BuocId, entity.DuAnId ?? Guid.Empty, _authContext, cancellationToken);


        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
        .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiTra = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TraLai && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiDaDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaDuyet && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiDaTrinh = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var model = request.Model;

        var currentStatus = entity.TrangThaiId;
        var allowEdit = currentStatus == trangThaiDuThao?.Id || currentStatus == trangThaiTra?.Id;

        if (allowEdit)
            entity.Update(model);
        else
            entity.TrangThaiDangTai = model.TrangThaiDangTai;
        //region thông tin thẩm định
        entity.NhaThauId = (model.ThamDinh ?? false) ? model.HoSoMoiThauThamDinh?.NhaThauId : null;
        entity.ThamDinh = model.ThamDinh;
         

        if (request.Model.ToTrinh != null)
        {
            ToTrinhQuyetDinhDto dto = request.Model.ToTrinh;

            if (entity.ToTrinh != null)
            {
                entity.ToTrinh.NguoiKy = dto.NguoiKy;
                entity.ToTrinh.So = dto.So;
                entity.ToTrinh.Ngay = dto.Ngay;
                entity.ToTrinh.ChucVu = dto.ChucVu;
                entity.ToTrinh.TrichYeu = dto.TrichYeu;
                await _toTrinhQuyetDinhRepo.UpdateAsync(entity.ToTrinh, cancellationToken);
            }
            else
            {
                entity.ToTrinh = new ToTrinhQuyetDinh()
                {
                    EntityId = entity.Id,
                    Loai = ToTrinhQuyetDinhLoai.HoSoMoiThauToTrinh,
                    NguoiKy = dto.NguoiKy,
                    So = dto.So,
                    Ngay = dto.Ngay,
                    ChucVu = dto.ChucVu,
                    TrichYeu = dto.TrichYeu,
                };
                await _toTrinhQuyetDinhRepo.AddAsync(entity.ToTrinh, cancellationToken);
            }
        }
        else if (entity.ToTrinh != null)
        {
            _toTrinhQuyetDinhRepo.Delete(entity.ToTrinh);
            entity.ToTrinh = null;
        }

        if (request.Model.QuyetDinh != null)
        {
            ToTrinhQuyetDinhDto dto = request.Model.QuyetDinh;

            if (entity.QuyetDinh != null)
            {
                entity.QuyetDinh.NguoiKy = dto.NguoiKy;
                entity.QuyetDinh.So = dto.So;
                entity.QuyetDinh.Ngay = dto.Ngay;
                entity.QuyetDinh.ChucVu = dto.ChucVu;
                entity.QuyetDinh.TrichYeu = dto.TrichYeu;
                await _toTrinhQuyetDinhRepo.UpdateAsync(entity.QuyetDinh, cancellationToken);
            }
            else
            {
                entity.QuyetDinh = new ToTrinhQuyetDinh()
                {
                    EntityId = entity.Id,
                    Loai = ToTrinhQuyetDinhLoai.HoSoMoiThauQuyetDinh,
                    NguoiKy = dto.NguoiKy,
                    So = dto.So,
                    Ngay = dto.Ngay,
                    ChucVu = dto.ChucVu,
                    TrichYeu = dto.TrichYeu
                };
                await _toTrinhQuyetDinhRepo.AddAsync(entity.QuyetDinh, cancellationToken);
            }
        }
        else if (entity.QuyetDinh != null)
        {
            _toTrinhQuyetDinhRepo.Delete(entity.QuyetDinh);
            entity.QuyetDinh = null;
        }

        await HoSoMoiThauDienTu.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity!;
    }
}
