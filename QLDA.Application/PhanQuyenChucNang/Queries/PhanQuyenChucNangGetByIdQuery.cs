using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhanQuyenChucNangs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.PhanQuyenChucNangs.Queries;

public class PhanQuyenChucNangGetById : IRequest<PhanQuyenChucNangDto> {
    public int Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
    public bool IsNoTracking { get; set; }
}

internal class PhanQuyenChucNangGetByIdHandler(IServiceProvider serviceProvider)
    : IRequestHandler<PhanQuyenChucNangGetById, PhanQuyenChucNangDto> {
    private readonly IRepository<PhanQuyenChucNang, int> PhanQuyenChucNang = serviceProvider.GetRequiredService<IRepository<PhanQuyenChucNang, int>>();
    private readonly IRepository<UserMaster, long> _users = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IRepository<NguoiDungMacDinhTheoPhong, Guid> _PhongBanNguoiDungMacDinh = serviceProvider.GetRequiredService<IRepository<NguoiDungMacDinhTheoPhong, Guid>>();

    public async Task<PhanQuyenChucNangDto> Handle(PhanQuyenChucNangGetById request,
        CancellationToken cancellationToken = default) {
        try
        {
            var queryable = PhanQuyenChucNang.GetOrderedSet()
                        .Include(x => x.DanhSachChiTiet).Where(x => x.Id == request.Id);
        

        if (request.IsNoTracking)
            queryable = queryable.AsNoTracking();

        var entity = await queryable.FirstOrDefaultAsync(cancellationToken);
        ManagedException.ThrowIf(request.ThrowIfNull && entity == null, "Không tìm thấy dữ liệu");

        if (entity == null)
            return null!;

        return new PhanQuyenChucNangDto
        {
            Id = entity.Id,
            SuDung = entity.SuDung,
            MaChucNang = entity.MaChucNang,
            ChucNang = entity.ChucNang,
            Level = entity.Level,
            DanhSachChiTiet = entity.DanhSachChiTiet.Select(x => new PhanQuyenChucNangCapDoDto()
            {
                LevelId = x.LevelId,
                NguoiDungMacDinh = x.NguoiDungMacDinh,
                NguoiDungChiDinhs = x.NguoiDungChiDinhs,
                TenNguoiDungMacDinh = entity.Level == PhanQuyenChucNangLevel.PhongBan &&
                     x.NguoiDungMacDinh == true
                     ? (from md in _PhongBanNguoiDungMacDinh.GetQueryableSet()
                        join u in _users.GetQueryableSet() on md.NguoiDungId equals u.UserPortalId
                        where md.PhongBanId == x.LevelId
                        select u.HoTen
                     ).FirstOrDefault() : null
            }).ToList()
        };

        }
        catch (Exception ex)
        {

            throw;
        }
    }
}