using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Application.QuyetDinhDieuChinhs.Commands;

/// <summary>
/// Tạo mới quyết định điều chỉnh
/// </summary>
public record QuyetDinhDieuChinhInsertCommand(QuyetDinhDieuChinhDto Dto) : IRequest<int>;

public record QuyetDinhDieuChinhDto {
    public Guid PheDuyetEntityId { get; set; }
    public string PheDuyetEntityName { get; set; } = default!;
    public Guid DuAnId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public string? LyDo { get; set; }
    public string? TepDinhKem { get; set; }
    public ThongTinDieuChinhChiPhiDto? ChiPhi { get; set; }
}

public record ThongTinDieuChinhChiPhiDto {
    public decimal? TongMucDauTu { get; set; }
    public decimal? ChiPhiXayLap { get; set; }
    public decimal? ChiPhiThietBi { get; set; }
    public decimal? ChiPhiKhac { get; set; }
    public decimal? ChiPhiDuPhong { get; set; }
}

internal class QuyetDinhDieuChinhInsertCommandHandler : IRequestHandler<QuyetDinhDieuChinhInsertCommand, int> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;
    private readonly IRepository<ThongTinDieuChinhChiPhi, Guid> _chiPhiRepository;
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository;
    private readonly IUserProvider _userProvider;
    private readonly IUnitOfWork _unitOfWork;

    public QuyetDinhDieuChinhInsertCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
        _chiPhiRepository = serviceProvider.GetRequiredService<IRepository<ThongTinDieuChinhChiPhi, Guid>>();
        _statusRepository = serviceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
        _userProvider = serviceProvider.GetRequiredService<IUserProvider>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task<int> Handle(QuyetDinhDieuChinhInsertCommand request, CancellationToken cancellationToken) {
        var dto = request.Dto;

        // Get status DT (Dự thảo) - default initial status
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.QuyetDinhDieuChinh.DuThao && s.Loai == PheDuyetEntityNames.QuyetDinhDieuChinh, cancellationToken);

        ManagedException.ThrowIfNull(trangThaiDuThao, "Không tìm thấy trạng thái 'Dự thảo'");

        // Auto-calculate Lan = COUNT + 1
        var lan = await _repository.GetQueryableSet()
            .CountAsync(e => e.PheDuyetEntityId == dto.PheDuyetEntityId
                             && e.PheDuyetEntityName == dto.PheDuyetEntityName
                             && !e.IsDeleted, cancellationToken) + 1;

        var entity = new QuyetDinhDieuChinh {
            Id = Guid.NewGuid(),
            PheDuyetEntityName = dto.PheDuyetEntityName,
            PheDuyetEntityId = dto.PheDuyetEntityId,
            DuAnId = dto.DuAnId,
            SoQuyetDinh = dto.SoQuyetDinh,
            NgayQuyetDinh = dto.NgayQuyetDinh,
            TrichYeu = dto.TrichYeu,
            LoaiDieuChinhId = dto.LoaiDieuChinhId,
            LyDo = dto.LyDo,
            TepDinhKem = dto.TepDinhKem,
            TrangThaiId = trangThaiDuThao.Id,
            Lan = lan
        };

        await _repository.AddAsync(entity, cancellationToken);

        // Add chi phí if provided (1-1 relationship)
        if (dto.ChiPhi != null) {
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

        return await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}