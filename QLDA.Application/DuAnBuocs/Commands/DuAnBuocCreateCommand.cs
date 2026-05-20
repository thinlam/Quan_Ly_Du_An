using Microsoft.EntityFrameworkCore;
using QLDA.Application.DuAnBuocs.DTOs;

namespace QLDA.Application.DuAnBuocs.Commands;

public record DuAnBuocCreateCommand(DuAnBuocCreateDto Dto) : IRequest<DuAnBuoc>;

public class DuAnBuocCreateCommandHandler(
    IRepository<DuAn, Guid> duAnRepository,
    IRepository<DuAnBuoc, int> duAnBuocRepository,
    IRepository<DanhMucBuoc, int> danhMucBuocRepository
) : IRequestHandler<DuAnBuocCreateCommand, DuAnBuoc> {
    private readonly IRepository<DuAn, Guid> _duAnRepository = duAnRepository;
    private readonly IRepository<DuAnBuoc, int> _duAnBuocRepository = duAnBuocRepository;
    private readonly IRepository<DanhMucBuoc, int> _danhMucBuocRepository = danhMucBuocRepository;

    public async Task<DuAnBuoc> Handle(DuAnBuocCreateCommand request, CancellationToken cancellationToken) {
        var (_, danhMucBuoc) = await ValidateAndGetAsync(request.Dto, cancellationToken);
        var entity = CreateEntity(request.Dto, danhMucBuoc);
        await SaveEntityAsync(entity, cancellationToken);
        return entity;
    }

    private async Task<(dynamic, DanhMucBuoc)> ValidateAndGetAsync(DuAnBuocCreateDto Dto, CancellationToken cancellationToken) {
        var duAnInfo = await _duAnRepository.GetQueryableSet()
            .Where(e => e.Id == Dto.DuAnId)
            .Select(e => new { e.Id, e.QuyTrinhId })
            .FirstOrDefaultAsync(cancellationToken);

        //Kiểm tra dự án tồn tại không
        ManagedException.ThrowIf(duAnInfo == null, "Dự án không tồn tại");

        //Kiểm tra bước đã tồn tại trong dự án chưa
        ManagedException.ThrowIf(await _duAnBuocRepository.GetQueryableSet().AnyAsync(x => x.DuAnId == Dto.DuAnId && x.BuocId == Dto.BuocId, cancellationToken), "Bước này đã tồn tại trong quy trình của dự án");

        var danhMucBuoc = await _danhMucBuocRepository.GetQueryableSet()
            .Include(x => x.BuocManHinhs)
            .FirstOrDefaultAsync(e => e.Id == Dto.BuocId, cancellationToken);

        //Kiểm tra bước có tồn tại không
        ManagedException.ThrowIf(danhMucBuoc == null, "Bước không tồn tại");

        //Kiểm tra bước có thuộc quy trình của dự án không
        ManagedException.ThrowIf(danhMucBuoc!.QuyTrinhId != duAnInfo.QuyTrinhId, "Bước không thuộc quy trình của dự án");

        ManagedException.ThrowIf(Dto.NgayDuKienBatDau.HasValue && Dto.NgayDuKienKetThuc.HasValue && Dto.NgayDuKienBatDau > Dto.NgayDuKienKetThuc,
            "Ngày dự kiến bắt đầu phải trước ngày dự kiến kết thúc");

        return (duAnInfo, danhMucBuoc);
    }

    private static DuAnBuoc CreateEntity(DuAnBuocCreateDto dto, DanhMucBuoc danhMucBuoc) {
        return new DuAnBuoc {
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            TenBuoc = dto.TenBuoc,
            NgayDuKienBatDau = dto.NgayDuKienBatDau,
            NgayDuKienKetThuc = dto.NgayDuKienKetThuc,
            GhiChu = dto.GhiChu,
            TrachNhiemThucHien = dto.TrachNhiemThucHien,
            Used = true,
            PhongPhuTrachChinhId = dto.PhongPhuTrachChinhId,
            DuAnBuocManHinhs = dto.DanhSachManHinh != null && dto.DanhSachManHinh.Count != 0
                ? [.. dto.DanhSachManHinh.Select((manHinhId, index) => new DuAnBuocManHinh { RightId = manHinhId, Stt = index + 1 })]
                : danhMucBuoc.BuocManHinhs?
                .Select(m => new DuAnBuocManHinh { RightId = m.RightId, Stt = m.Stt })
                .ToList() ?? [],
            DuAnBuocPhongBanPhoiHops = dto.DanhSachPhongBanPhoiHopIds != null && dto.DanhSachPhongBanPhoiHopIds.Count != 0
                ? [.. dto.DanhSachPhongBanPhoiHopIds.Select(id => new DuAnBuocPhongBanPhoiHop { LeftId = 0, RightId = id })]
                : []
        };
    }

    private async Task SaveEntityAsync(DuAnBuoc entity, CancellationToken cancellationToken) {
        await _duAnBuocRepository.AddAsync(entity, cancellationToken);
        await _duAnBuocRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
