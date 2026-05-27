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

internal class QuyetDinhDieuChinhUpdateCommandHandler : IRequestHandler<QuyetDinhDieuChinhUpdateCommand, QuyetDinhDieuChinh> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<ThongTinDieuChinhChiPhi, Guid> _chiPhiRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhUpdateCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _chiPhiRepository = serviceProvider.GetRequiredService<IRepository<ThongTinDieuChinhChiPhi, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<QuyetDinhDieuChinh> Handle(QuyetDinhDieuChinhUpdateCommand request, CancellationToken cancellationToken) {
        var dto = request.Dto;

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.TraLai && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy dữ liệu cần cập nhật!");

        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
        }
        dto.TrangThaiId = entity.TrangThaiId;
        entity = new QuyetDinhDieuChinh
        {
            Id= entity.Id,
            DuAnId = request.Dto.DuAnId,
            BuocId = request.Dto.BuocId,
            LoaiDieuChinhId = request.Dto.LoaiDieuChinhId,
            LyDo = request.Dto.LyDo,
            Lan = request.Dto.Lan,
            NgayQuyetDinh = request.Dto.NgayQuyetDinh,
            SoQuyetDinh = request.Dto.SoQuyetDinh,
            TrichYeu = request.Dto.TrichYeu,
            TrangThaiId = trangThaiDuThao?.Id ?? 0,
            ThongTinDieuChinhChiPhi = request.Dto.ChiPhi == null
               ? null
               : new ThongTinDieuChinhChiPhi
               {
                   Id = Guid.NewGuid(),
                   TongMucDauTu = request.Dto.ChiPhi.TongMucDauTu,
                   ChiPhiXayLap = request.Dto.ChiPhi.ChiPhiXayLap,
                   ChiPhiThietBi = request.Dto.ChiPhi.ChiPhiThietBi,
                   ChiPhiKhac = request.Dto.ChiPhi.ChiPhiKhac,
                   ChiPhiDuPhong = request.Dto.ChiPhi.ChiPhiDuPhong
               }
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.UpdateAsync(entity, cancellationToken);
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