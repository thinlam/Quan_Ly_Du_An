using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;

namespace QLDA.Application.QuyetDinhDieuChinhs.Queries;

public record QuyetDinhDieuChinhGetChiTietQuery(Guid Id) : IRequest<QuyetDinhDieuChinhChiTietDto>;

public class QuyetDinhDieuChinhChiTietDto {
    public Guid Id { get; set; }
    public string PheDuyetEntityName { get; set; } = default!;
    public Guid PheDuyetEntityId { get; set; }
    public Guid DuAnId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public int LoaiDieuChinhId { get; set; }
    public string? TenLoaiDieuChinh { get; set; }
    public string? LyDo { get; set; }
    public string? TepDinhKem { get; set; }
    public int TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public int Lan { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public List<ThongTinDieuChinhChiPhiItemDto> ChiPhis { get; set; } = [];
}

public class ThongTinDieuChinhChiPhiItemDto {
    public Guid Id { get; set; }
    public decimal? TongMucDauTu { get; set; }
    public decimal? ChiPhiXayLap { get; set; }
    public decimal? ChiPhiThietBi { get; set; }
    public decimal? ChiPhiKhac { get; set; }
    public decimal? ChiPhiDuPhong { get; set; }
}

internal class QuyetDinhDieuChinhGetChiTietQueryHandler : IRequestHandler<QuyetDinhDieuChinhGetChiTietQuery, QuyetDinhDieuChinhChiTietDto> {
    private readonly IRepository<QuyetDinhDieuChinh, Guid> _repository;

    public QuyetDinhDieuChinhGetChiTietQueryHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();
    }

    public async Task<QuyetDinhDieuChinhChiTietDto> Handle(QuyetDinhDieuChinhGetChiTietQuery request, CancellationToken cancellationToken) {
        var entity = await _repository.GetQueryableSet()
            .Include(e => e.LoaiDieuChinh)
            .Include(e => e.TrangThai)
            .Include(e => e.ThongTinDieuChinhChiPhis.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        ManagedException.ThrowIfNull(entity, "Không tìm thấy quyết định điều chỉnh");

        return new QuyetDinhDieuChinhChiTietDto {
            Id = entity.Id,
            PheDuyetEntityName = entity.PheDuyetEntityName,
            PheDuyetEntityId = entity.PheDuyetEntityId,
            DuAnId = entity.DuAnId,
            SoQuyetDinh = entity.SoQuyetDinh,
            NgayQuyetDinh = entity.NgayQuyetDinh,
            TrichYeu = entity.TrichYeu,
            LoaiDieuChinhId = entity.LoaiDieuChinhId,
            TenLoaiDieuChinh = entity.LoaiDieuChinh?.Ten,
            LyDo = entity.LyDo,
            TepDinhKem = entity.TepDinhKem,
            TrangThaiId = entity.TrangThaiId,
            MaTrangThai = entity.TrangThai?.Ma,
            TenTrangThai = entity.TrangThai?.Ten,
            Lan = entity.Lan,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy,
            ChiPhis = entity.ThongTinDieuChinhChiPhis.Select(c => new ThongTinDieuChinhChiPhiItemDto {
                Id = c.Id,
                TongMucDauTu = c.TongMucDauTu,
                ChiPhiXayLap = c.ChiPhiXayLap,
                ChiPhiThietBi = c.ChiPhiThietBi,
                ChiPhiKhac = c.ChiPhiKhac,
                ChiPhiDuPhong = c.ChiPhiDuPhong
            }).ToList()
        };
    }
}