using Microsoft.EntityFrameworkCore;
using QLDA.Application.CauHinhVaiTroQuyens.DTOs;

namespace QLDA.Application.CauHinhVaiTroQuyens.Queries;

public record CauHinhVaiTroQuyenGetDanhSachQuery : IRequest<List<CauHinhVaiTroQuyenDto>> {
    public string? VaiTro { get; set; }
    public string? NhomQuyen { get; set; }
}

internal class CauHinhVaiTroQuyenGetDanhSachQueryHandler : IRequestHandler<CauHinhVaiTroQuyenGetDanhSachQuery, List<CauHinhVaiTroQuyenDto>> {
    private readonly IRepository<CauHinhVaiTroQuyen, int> _repo;

    public CauHinhVaiTroQuyenGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _repo = serviceProvider.GetRequiredService<IRepository<CauHinhVaiTroQuyen, int>>();
    }

    public async Task<List<CauHinhVaiTroQuyenDto>> Handle(CauHinhVaiTroQuyenGetDanhSachQuery request, CancellationToken cancellationToken) {
        var query = _repo.GetQueryableSet(OnlyUsed: false, OnlyNotDeleted: false)
            .Include(c => c.Quyen)
            .Where(c => !c.IsDeleted)
            .WhereIf(!string.IsNullOrEmpty(request.VaiTro), c => c.VaiTro == request.VaiTro)
            .WhereIf(!string.IsNullOrEmpty(request.NhomQuyen), c => c.Quyen!.NhomQuyen == request.NhomQuyen);

        return await query
            .OrderBy(c => c.VaiTro)
            .ThenBy(c => c.QuyenId)
            .Select(c => new CauHinhVaiTroQuyenDto {
                Id = c.Id,
                VaiTro = c.VaiTro,
                QuyenId = c.QuyenId,
                QuyenMa = c.Quyen!.Ma ?? string.Empty,
                QuyenTen = c.Quyen.Ten ?? string.Empty,
                NhomQuyen = c.Quyen.NhomQuyen,
                KichHoat = c.KichHoat,
            })
            .ToListAsync(cancellationToken);
    }
}
