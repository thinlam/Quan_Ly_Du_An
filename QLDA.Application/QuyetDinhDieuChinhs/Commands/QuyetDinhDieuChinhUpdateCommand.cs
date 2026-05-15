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

        var trangThaiDDC = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "DDC" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);
        var trangThaiTL = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == "TL" && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        var entity = await _repository.GetQueryableSet()
            .FirstOrDefaultAsync(e => e.Id == dto.PheDuyetEntityId, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        // Validate: only allow update when status is DDC or TL
        if (entity.TrangThaiId != trangThaiDDC?.Id && entity.TrangThaiId != trangThaiTL?.Id) {
            throw new ManagedException("Chỉ có thể cập nhật khi trạng thái là Nháp điều chỉnh hoặc Trả lại");
        }

        // Update fields
        entity.SoQuyetDinh = dto.SoQuyetDinh;
        entity.NgayQuyetDinh = dto.NgayQuyetDinh;
        entity.TrichYeu = dto.TrichYeu;
        entity.LoaiDieuChinhId = dto.LoaiDieuChinhId;
        entity.LyDo = dto.LyDo;
        entity.TepDinhKem = dto.TepDinhKem;

        // Update chi phí - remove old and add new
        var existingChiPhis = await _chiPhiRepository.GetQueryableSet()
            .Where(c => c.QuyetDinhDieuChinhId == entity.Id)
            .ToListAsync(cancellationToken);

        foreach (var cp in existingChiPhis) {
            cp.IsDeleted = true;
        }

        if (dto.ChiPhis?.Count > 0) {
            foreach (var chiPhiDto in dto.ChiPhis) {
                var chiPhi = new ThongTinDieuChinhChiPhi {
                    Id = Guid.NewGuid(),
                    QuyetDinhDieuChinhId = entity.Id,
                    TongMucDauTu = chiPhiDto.TongMucDauTu,
                    ChiPhiXayLap = chiPhiDto.ChiPhiXayLap,
                    ChiPhiThietBi = chiPhiDto.ChiPhiThietBi,
                    ChiPhiKhac = chiPhiDto.ChiPhiKhac,
                    ChiPhiDuPhong = chiPhiDto.ChiPhiDuPhong
                };
                await _chiPhiRepository.AddAsync(chiPhi, cancellationToken);
            }
        }

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}