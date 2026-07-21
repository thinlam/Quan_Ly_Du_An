using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Commands;

public record HoSoDeXuatCapDoCnttUpdateCommand(HoSoDeXuatCapDoCnttUpdateModel Model)
    : IRequest<HoSoDeXuatCapDoCntt>;

internal class HoSoDeXuatCapDoCnttUpdateCommandHandler : IRequestHandler<HoSoDeXuatCapDoCnttUpdateCommand, HoSoDeXuatCapDoCntt> {
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> _duongDiRepo;
    private readonly IUserProvider _userProvider;

    public HoSoDeXuatCapDoCnttUpdateCommandHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _duongDiRepo = serviceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = HoSoDeXuatCapDoCntt.UnitOfWork;
    }

    public async Task<HoSoDeXuatCapDoCntt> Handle(HoSoDeXuatCapDoCnttUpdateCommand request, CancellationToken cancellationToken = default) {
        var trangThaiDaChuyen = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DaChuyen && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);
        var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.DuThao && s.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt, cancellationToken);

        var entity = await HoSoDeXuatCapDoCntt.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == request.Model.Id, cancellationToken);
        ManagedException.ThrowIfNull(entity, "Không tìm thấy hồ sơ đề xuất cấp độ CNTT");

        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)

        // get các trạng thái dc phép sửa trong dường đi tờ trình

        if (entity.TrangThaiId != null && entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiDaChuyen?.Id) {
            ManagedException.Throw( "Trạng thái không thể cập nhật.");
        }
        //long phongBanTaoId = 0;
        //long.TryParse(entity.CreatedBy, out phongBanTaoId);
        //get các trạng thái được phép xử lý
        //var duongDi = await _duongDiRepo.GetQueryableSet().AsNoTracking()
        //           .Where(x => x.Used && !(x.IsDeleted ?? false)
        //           && x.MaTrangThaiHienTai == entity.TrangThai.Ma
        //           && x.MaTrangThaiTiepTheo == TrangThaiPheDuyetCodes.HoSoDeXuatCapDoCntt.Sua
        //           && (x.RoleLevel == 0
        //           || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChuTri && _userProvider.Info.PhongBanID == entity.DuAn.DonViPhuTrachChinhId)
        //           || (x.RoleLevel == DuongDiToTrinhRoleLevel.NguoiPhuTrachChinh && _userProvider.Info.PhongBanID == phongBanTaoId)   // NGuoiPhuTrachChinh là Người tạo ra hồ sơ đề xuất cnntt này
        //           || (x.RoleLevel == DuongDiToTrinhRoleLevel.PhongBanChiDinh && _userProvider.Info.PhongBanID == x.RoleId) // chuyển chỉ định phòng hạ tầng nhận
        //           )).ToListAsync(cancellationToken);

        // TrangTHai có the Sua : DT / DC 
        //ManagedException.ThrowIf(duongDi == null, "Trạng thái không thể cập nhật.");

        entity.Update(request.Model);

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await HoSoDeXuatCapDoCntt.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return entity!;
    }
}