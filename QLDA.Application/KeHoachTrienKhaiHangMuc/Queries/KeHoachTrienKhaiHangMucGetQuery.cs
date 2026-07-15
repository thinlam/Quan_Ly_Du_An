using Microsoft.EntityFrameworkCore;
using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs.Queries;

public record KeHoachTrienKhaiHangMucGetQuery : IRequest<KeHoachTrienKhaiHangMucDto> {
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class KeHoachTrienKhaiHangMucGetQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<KeHoachTrienKhaiHangMucGetQuery, KeHoachTrienKhaiHangMucDto> {
    private readonly IRepository<UserMaster, long> userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<KeHoachTrienKhaiHangMuc, Guid> KeHoachTrienKhaiHangMuc =
        serviceProvider.GetRequiredService<IRepository<KeHoachTrienKhaiHangMuc, Guid>>();
    private readonly IRepository<Attachment, Guid> TepDinhKem =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    public async Task<KeHoachTrienKhaiHangMucDto> Handle(KeHoachTrienKhaiHangMucGetQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = KeHoachTrienKhaiHangMuc.GetOrderedSet()
            .Include(e => e.DanhSachHangMuc)
            .Where(e => e.Id == request.Id);

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();


        var entity = await queryable.Select(e => new KeHoachTrienKhaiHangMucDto
        {
            Id = e.Id,
            TenTrangThai    = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : string.Empty,
            TrangThaiId = e.TrangThaiId,    
            DuAnId = e.DuAnId,
            TenDuAn = e.DuAn != null ? e.DuAn!.TenDuAn :string.Empty,    
            NgayTrinh = e.NgayToTrinh,    
            So = e.So,    
            TrichYeu = e.TrichYeu,    
            HangMucTrienKhai = e.DanhSachHangMuc !=null? e.DanhSachHangMuc.Where(x=> !x.IsDeleted).Select(h => new HangMucTrienKhaiDto
            {
                Id = h.Id,
                TenHangMuc = h.TenHangMuc,
                GiaiDoanId = h.GiaiDoanId,
                NgayBatDau = h.NgayBatDau,
                NgayKetThuc = h.NgayKetThuc,
                KinhPhi = h.KinhPhi,
                ThoiHan = h.ThoiHan,    
                CanBoChuTriId = h.CanBoChuTriId,
                CanBoPhoiHopIds = h.CanBoPhoiHopIds,
                DonViChuTriId = h.DonViChuTriId,
                DonViPhoiHops = h.DonViPhoiHopIds,
            }).ToList() : null ,
            DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
        })
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        return entity!;
    }
}