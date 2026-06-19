using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.HoSoMoiThauDienTus.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.HoSoMoiThauDienTus.Commands;

public record HoSoMoiThauDienTuUpdateCommand(HoSoMoiThauDienTuUpdateModel Model) : IRequest<HoSoMoiThauDienTu>;

internal class HoSoMoiThauDienTuUpdateCommandHandler : IRequestHandler<HoSoMoiThauDienTuUpdateCommand, HoSoMoiThauDienTu> {
    private readonly IRepository<HoSoMoiThauDienTu, Guid> HoSoMoiThauDienTu;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public HoSoMoiThauDienTuUpdateCommandHandler(IServiceProvider serviceProvider) {
        HoSoMoiThauDienTu = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = HoSoMoiThauDienTu.UnitOfWork;
    }

    public async Task<HoSoMoiThauDienTu> Handle(HoSoMoiThauDienTuUpdateCommand request, CancellationToken cancellationToken = default) {
       
        var entity = await HoSoMoiThauDienTu.GetQueryableSet().Include( e => e.ChiDinhThau)
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ mời thầu điện tử");

        await _auth.EnsureCanExecuteStepAsync(entity.BuocId, _authContext, cancellationToken);

        //// Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        //if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiPheDuyet?.Ma != "LEG") {
        //    throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
        //}
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
        .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DuThao && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiTra = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.TraLai && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiDaDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaDuyet && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);
        var trangThaiDaTrinh = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoMoiThauDienTu.DaTrinh && s.Loai == PheDuyetEntityNames.HoSoMoiThauDienTu, cancellationToken);

        
        var currentStatus = entity.TrangThaiId;
        var allowEdit = currentStatus == trangThaiDuThao?.Id || currentStatus == trangThaiTra?.Id;

        if (allowEdit)
            entity.Update(request.Model);
        else
            entity.TrangThaiDangTai = request.Model.TrangThaiDangTai;
        if (request.Model.ChiDinhThau != null)
        {
            ChiDinhThauDto dto = request.Model.ChiDinhThau;

            if (entity.ChiDinhThau != null)
            {
                entity.ChiDinhThau.NguoiKy = dto.NguoiKy;
                entity.ChiDinhThau.So = dto.So;
                entity.ChiDinhThau.Ngay = dto.Ngay;
                entity.ChiDinhThau.ChucVu = dto.ChucVu;
                entity.ChiDinhThau.TrichYeu = dto.TrichYeu;

                // await _chiDinhThau.UpdateAsync(entity.ChiDinhThau, cancellationToken);
            }
            else
            {
                entity.ChiDinhThau = new ChiDinhThau()
                {
                    NguoiKy = dto.NguoiKy,
                    So = dto.So,
                    Ngay = dto.Ngay,
                    ChucVu = dto.ChucVu,
                    TrichYeu = dto.TrichYeu
                };
                // await _chiDinhThau.AddAsync(entity.ChiDinhThau, cancellationToken);
            }
        }
        else
        {
            if (entity.ChiDinhThau != null)
                entity.ChiDinhThau = null;
        }

        await HoSoMoiThauDienTu.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}