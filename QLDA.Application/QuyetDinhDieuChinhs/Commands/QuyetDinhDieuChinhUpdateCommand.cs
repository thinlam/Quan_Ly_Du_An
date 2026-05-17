using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Cập nhật quyết định điều chỉnh - chỉ khi ở trạng thái Nháp hoặc Trả lại
/// </summary>
public record QuyetDinhDieuChinhUpdateCommand(QuyetDinhDieuChinhDto Dto) : IRequest<int>;

internal class QuyetDinhDieuChinhUpdateCommandHandler : IRequestHandler<QuyetDinhDieuChinhUpdateCommand, int> {
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

    public async Task<int> Handle(QuyetDinhDieuChinhUpdateCommand request, CancellationToken cancellationToken) {
        var dto = request.Dto;

        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiTraLai = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.TraLai && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == dto.PheDuyetEntityId, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        // Validate: only allow update when status is DT (Dự thảo) or TL (Trả lại)
        if (entity.TrangThaiId != trangThaiDuThao?.Id && entity.TrangThaiId != trangThaiTraLai?.Id) {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Dự thảo");
        }

        // Update fields
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.NgayQuyetDinh = dto.NgayQuyetDinh;
        entity.TrichYeu = dto.TrichYeu;
        entity.LoaiDieuChinhId = dto.LoaiDieuChinhId;
        entity.LyDo = dto.LyDo;
        entity.TepDinhKem = dto.TepDinhKem;

        // Update chi phí (1-1 relationship) - upsert pattern
        var existingChiPhi = await _chiPhiRepository.GetQueryableSet()
            .FirstOrDefaultAsync(c => c.QuyetDinhDieuChinhId == entity.Id, cancellationToken);

        if (dto.ChiPhi != null) {
            if (existingChiPhi != null) {
                existingChiPhi.TongMucDauTu = dto.ChiPhi.TongMucDauTu;
                existingChiPhi.ChiPhiXayLap = dto.ChiPhi.ChiPhiXayLap;
                existingChiPhi.ChiPhiThietBi = dto.ChiPhi.ChiPhiThietBi;
                existingChiPhi.ChiPhiKhac = dto.ChiPhi.ChiPhiKhac;
                existingChiPhi.ChiPhiDuPhong = dto.ChiPhi.ChiPhiDuPhong;
            }
            else {
                var chiPhi = new ThongTinDieuChinhChiPhi {
                    Id = Guid.NewGuid(),
                    QuyetDinhDieuChinhId = entity.Id,
                    TongMucDauTu = dto.ChiPhi.TongMucDauTu,
                    ChiPhiXayLap = dto.ChiPhi.ChiPhiXayLap,
                    ChiPhiThietBi = dto.ChiPhi.ChiPhiThietBi,
                    ChiPhiKhac = dto.ChiPhi.ChiPhiKhac,
                    ChiPhiDuPhong = dto.ChiPhi.ChiPhiDuPhong
                };
                await _chiPhiRepository.AddAsync(chiPhi, cancellationToken);
            }
        }
        else if (existingChiPhi != null) {
            existingChiPhi.IsDeleted = true;
        }

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}