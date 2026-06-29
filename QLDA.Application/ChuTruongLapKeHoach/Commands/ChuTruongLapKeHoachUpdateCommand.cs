using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.ChuTruongLapKeHoachs;
using QLDA.Application.Authorization;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.ChuTruongLapKeHoachs.Commands;

public record ChuTruongLapKeHoachUpdateCommand(ChuTruongLapKeHoach Dto) : IRequest<ChuTruongLapKeHoach>;

internal class ChuTruongLapKeHoachUpdateCommandHandler : IRequestHandler<ChuTruongLapKeHoachUpdateCommand, ChuTruongLapKeHoach>
{
    private readonly IRepository<ChuTruongLapKeHoach, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IAuthorizationContext _authContext;
    private readonly IUnitOfWork _unitOfWork;

    public ChuTruongLapKeHoachUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _authContext = serviceProvider.GetRequiredService<IAuthorizationContext>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<ChuTruongLapKeHoach> Handle(ChuTruongLapKeHoachUpdateCommand request, CancellationToken cancellationToken = default)
    {
        // Auto-assign Dự thảo status
        var trangThaiDuyet = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiDaTrinh = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repo.GetQueryableSet()
            .Include(e => e.TrangThai)
            .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);// Không tìm thấy dữ liệu
        ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

        await _auth.EnsureCanExecuteStepAsync(request.Dto.BuocId, _authContext, cancellationToken);


        // Validate current status must be null (legacy), Dự thảo, or Migrated (LEG)
        //   throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là dự thảo");
        if ( entity.TrangThaiId == trangThaiDaTrinh.Id  || entity.TrangThaiId == trangThaiDuyet.Id ) //chưa duyệt cứ cho upd
        {
            entity.ButPhe = request.Dto.ButPhe;
        }
        else
        {
            entity.DuAnId = request.Dto.DuAnId;
            entity.BuocId = request.Dto?.BuocId;
            entity.SoToTrinh = request.Dto?.SoToTrinh;
            entity.NgayToTrinh = request.Dto?.NgayToTrinh;
            entity.TrichYeu = request.Dto?.TrichYeu;
            entity.LoaiDeXuat = request.Dto?.LoaiDeXuat;
        }
          
        await _repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
