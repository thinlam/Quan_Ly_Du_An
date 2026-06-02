using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Application.QuyetDinhDieuChinhs.DTOs;
using QLDA.Domain.Constants;
using System.Data;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Cập nhật quyết định điều chỉnh - chỉ khi ở trạng thái Nháp hoặc Trả lại
/// </summary>
public record QuyetDinhDieuChinhUpdateCommand(QuyetDinhDieuChinhUpdateDto Dto) : IRequest<QuyetDinhDieuChinh>;

internal class QuyetDinhDieuChinhUpdateCommandHandler : IRequestHandler<QuyetDinhDieuChinhUpdateCommand, QuyetDinhDieuChinh>
{
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<ThongTinDieuChinhChiPhi, Guid> _chiPhiRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhUpdateCommandHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _chiPhiRepository = serviceProvider.GetRequiredService<IRepository<ThongTinDieuChinhChiPhi, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<QuyetDinhDieuChinh> Handle(QuyetDinhDieuChinhUpdateCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .Include(e => e.ThongTinDieuChinhChiPhi)
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu cần cập nhật!");
      
        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id)
        {
            throw new ManagedException("Trạng thái không thể cập nhật!");
        }
        entity.DuAnId = dto.DuAnId;
        entity.BuocId = dto.BuocId;
        entity.LoaiDieuChinhId = dto.LoaiDieuChinhId;
        entity.LyDo = dto.LyDo;
        entity.Lan = dto.Lan;
        entity.NgayQuyetDinh = dto.NgayQuyetDinh;
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.TrichYeu = dto.TrichYeu;

        if (dto.ChiPhi != null)
        {
            if (entity.ThongTinDieuChinhChiPhi != null)
            {
                entity.ThongTinDieuChinhChiPhi.TongMucDauTu = dto.ChiPhi.TongMucDauTu;
                entity.ThongTinDieuChinhChiPhi.ChiPhiXayLap = dto.ChiPhi.ChiPhiXayLap;
                entity.ThongTinDieuChinhChiPhi.ChiPhiThietBi = dto.ChiPhi.ChiPhiThietBi;
                entity.ThongTinDieuChinhChiPhi.ChiPhiKhac = dto.ChiPhi.ChiPhiKhac;
                entity.ThongTinDieuChinhChiPhi.ChiPhiDuPhong = dto.ChiPhi.ChiPhiDuPhong;
            }
            else
            {
                var chiPhi = new ThongTinDieuChinhChiPhi
                {
                    Id = Guid.NewGuid(),
                    QuyetDinhDieuChinhId = entity.Id,
                    TongMucDauTu = dto.ChiPhi.TongMucDauTu,
                    ChiPhiXayLap = dto.ChiPhi.ChiPhiXayLap,
                    ChiPhiThietBi = dto.ChiPhi.ChiPhiThietBi,
                    ChiPhiKhac = dto.ChiPhi.ChiPhiKhac,
                    ChiPhiDuPhong = dto.ChiPhi.ChiPhiDuPhong
                };
                await _chiPhiRepository.AddAsync(chiPhi, cancellationToken);
                entity.ThongTinDieuChinhChiPhi = chiPhi;
            }
        }
        else if (entity.ThongTinDieuChinhChiPhi != null)
        {
            _chiPhiRepository.Delete(entity.ThongTinDieuChinhChiPhi);
            entity.ThongTinDieuChinhChiPhi = null;
        }

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
        return entity;

        // entity.Update(dto);

        // var existingChiPhi = await _chiPhiRepository.GetQueryableSet()
        //     .FirstOrDefaultAsync(c => c.QuyetDinhDieuChinhId == entity.Id, cancellationToken);

        // if (dto.ChiPhi != null) {
        //     if (existingChiPhi != null) {
        //         existingChiPhi.Update(dto.ChiPhi);
        //     }
        //     else {
        //         var chiPhi = dto.ChiPhi.ToEntity(entity.Id);
        //         await _chiPhiRepository.AddAsync(chiPhi, cancellationToken);
        //     }
        // }
        // else if (existingChiPhi != null) {
        //     existingChiPhi.IsDeleted = true;
        // }

    }
}