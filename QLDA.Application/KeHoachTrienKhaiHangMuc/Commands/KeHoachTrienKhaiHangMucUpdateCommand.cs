using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.KeHoachTrienKhaiHangMucMappings;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Constants;
using QLDA.Application.Common;
namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;

public record KeHoachTrienKhaiHangMucUpdateCommand(KeHoachTrienKhaiHangMucDto Dto) : IRequest<KeHoachTrienKhaiHangMuc>;

internal class KeHoachTrienKhaiHangMucUpdateCommandHandler : IRequestHandler<KeHoachTrienKhaiHangMucUpdateCommand, KeHoachTrienKhaiHangMuc>
{
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> _repo;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepo;
    private readonly IRepository<HangMucKeHoach, Guid> _hangMucKeHoach;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepo;
    private readonly IBuocAuthorizationProvider _auth;
    private readonly IUserProvider _user;
    private readonly IUnitOfWork _unitOfWork;

    public KeHoachTrienKhaiHangMucUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repo = serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
        _statusRepo = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _hangMucKeHoach = serviceProvider.GetRequiredService<IRepository<HangMucKeHoach, Guid>>();
        _duAnBuocRepo = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _auth = serviceProvider.GetRequiredService<IBuocAuthorizationProvider>();
        _user = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repo.UnitOfWork;
    }

    public async Task<KeHoachTrienKhaiHangMuc> Handle(KeHoachTrienKhaiHangMucUpdateCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {


            var trangThaiDuThao = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
            var trangThaiTraLai = await _statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
                                .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

            var entity = await _repo.GetQueryableSet()
                .Include(e => e.DanhSachHangMuc)
                .Include(e => e.TrangThai)
                .FirstOrDefaultAsync(e => e.Id == request.Dto.Id, cancellationToken);
            ManagedException.ThrowIf(entity == null, "Không tìm thấy dữ liệu.");

            if (request.Dto.BuocId.HasValue)
            {
                var buoc = await _duAnBuocRepo.GetQueryableSet()
                    .Include(e => e.DuAn)
                    .Include(e => e.DuAnBuocPhongBanPhoiHops)
                    .FirstOrDefaultAsync(e => e.Id == request.Dto.BuocId.Value, cancellationToken);
                if (buoc != null && !await _auth.CanExecuteStepAsync(buoc, _user, cancellationToken))
                    throw new ManagedException("Phòng ban không có quyền thao tác bước này");
            }

            if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
            {
                throw new ManagedException("Trạng thái không thể cập nhật!");
            }

            entity.DuAnId = request.Dto.DuAnId;
            entity.BuocId = request.Dto.BuocId;
            entity.So = request.Dto.So;
            entity.NgayToTrinh = request.Dto.NgayTrinh;
            entity.TrichYeu = request.Dto.TrichYeu;

          //  entity.SyncHangMuc(request.Dto.HangMucTrienKhai);
            await SyncHelper.SyncCollection(_hangMucKeHoach, entity.DanhSachHangMuc, [.. request.Dto.HangMucTrienKhai?.Select(e => e.ToEntity(entity.Id)) ?? []], (existing, request) =>
            {
                existing.TenHangMuc = request.TenHangMuc;
                existing.GiaiDoanId = request.GiaiDoanId;
                existing.TenHangMuc = request.TenHangMuc;
                existing.KinhPhi = request.KinhPhi;
                existing.NgayBatDau = request.NgayBatDau;
                existing.NgayKetThuc = request.NgayKetThuc;
                existing.ThoiHan = request.ThoiHan;
                existing.CanBoChuTriId = request.CanBoChuTriId;
                existing.CanBoPhoiHopIds = request.CanBoPhoiHopIds;
                existing.DonViChuTriId = request.DonViChuTriId;
                existing.DonViPhoiHopIds = request.DonViPhoiHopIds;
            }, cancellationToken);

            if (_unitOfWork.HasTransaction)
            {
                await UpdateAsync(entity, cancellationToken);
            }
            else
            {
                using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                await UpdateAsync(entity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }

            return entity;
        }
        catch (Exception)
        {

            throw;
        }
    }
    private async Task UpdateAsync(KeHoachTrienKhaiHangMuc entity, CancellationToken cancellationToken)
    {
        await _repo.UpdateAsync(entity, cancellationToken);
    }
}
