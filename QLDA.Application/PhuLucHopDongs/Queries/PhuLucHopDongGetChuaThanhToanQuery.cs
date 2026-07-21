using Microsoft.EntityFrameworkCore;
using QLDA.Application.PhuLucHopDongs.DTOs;

namespace QLDA.Application.PhuLucHopDongs.Queries;

public record PhuLucHopDongGetChuaThanhToanQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<List<PhuLucHopDongDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? Ten { get; set; }
    public string? SoPhuLucHopDong { get; set; }
    public Guid? HopDongId { get; set; }
    public Guid? ThanhToanId { get; set; }
    public Guid? PhuLucHopDongId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    PhuLucHopDongGetChuaThanhToanQueryHandler : IRequestHandler<PhuLucHopDongGetChuaThanhToanQuery,
    List<PhuLucHopDongDto>>
{
    private readonly IRepository<ThanhToan, Guid> ThanhToan;
    private readonly IRepository<PhuLucHopDong, Guid> PhuLucHopDong;

    public PhuLucHopDongGetChuaThanhToanQueryHandler(IServiceProvider serviceProvider)
    {
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
        PhuLucHopDong = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
    }

    public async Task<List<PhuLucHopDongDto>> Handle(PhuLucHopDongGetChuaThanhToanQuery request,
        CancellationToken cancellationToken = default)
    {
        // Giả định bạn đã lấy PhuLucHopDongQuery từ Repository của PhuLucHopDong
       // var ThanhToanQr = ThanhToan.GetQueryableSet().AsNoTracking();
        var thanhToans = await ThanhToan.GetQueryableSet()
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.Id,
                        x.IsDeleted,
                        x.PhuLucHopDongIds
                    })
                    .ToListAsync(cancellationToken);

        var queryable = PhuLucHopDong.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId);

        var data = await queryable
            .Select(e => new PhuLucHopDongDto
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                Ten = e.Ten,
                SoPhuLucHopDong = e.SoPhuLucHopDong,
                NoiDung = e.NoiDung,
                Ngay = e.Ngay,
                HopDongId = e.HopDongId,
                GiaTri = e.GiaTri
            })
            .ToListAsync(cancellationToken);

        if (request.ThanhToanId.HasValue)
        {
            // KỊCH BẢN 1: Truyền vào một ThanhToanId cụ thể
            // Điều kiện A: Phụ lục này chưa từng nằm trong bất kỳ Thanh toán HOẠT ĐỘNG nào khác
            // Điều kiện B: Phụ lục này đang nằm trong chính Thanh toán được truyền lên (Không cần biết Thanh toán đó đã xóa hay chưa)  data = data.Where(pl =>
            data = data.Where(pl => !thanhToans.Any(tt =>
                 !tt.IsDeleted &&
                 tt.Id != request.ThanhToanId.Value &&
                 tt.PhuLucHopDongIds != null &&
                 tt.PhuLucHopDongIds.Contains(pl.Id!.Value))
            || thanhToans.Any(tt =>
                 tt.Id == request.ThanhToanId.Value &&
                 tt.PhuLucHopDongIds != null &&
                 tt.PhuLucHopDongIds.Contains(pl.Id!.Value))

         ).ToList();
        

            //queryable = queryable.Where(pl =>
            //    !ThanhToanQr.Any(tt =>
            //        !tt.IsDeleted &&
            //        tt.Id != request.ThanhToanId.Value &&
            //        tt.PhuLucHopDongIds != null &&
            //        tt.PhuLucHopDongIds.Contains(pl.Id))

            //    || // HOẶC

            //    ThanhToanQr.Any(tt =>
            //        tt.Id == request.ThanhToanId.Value &&
            //        tt.PhuLucHopDongIds != null &&
            //        tt.PhuLucHopDongIds.Contains(pl.Id))
            //);


        }
        else
        {
            data = data.Where(pl =>

                !thanhToans.Any(tt =>
                    !tt.IsDeleted &&
                    tt.PhuLucHopDongIds != null &&
                    tt.PhuLucHopDongIds.Contains(pl.Id!.Value))

            ).ToList();
            // KỊCH BẢN 2: ThanhToanId = null (Tạo mới thanh toán)
            //    queryable = queryable.Where(pl =>
            //             CHỈ lấy những Phụ lục chưa nằm trong bất kỳ Thanh toán ACTIVE nào(!tt.IsDeleted)
            //             Nếu phụ lục nằm trong thanh toán đã bị xóa(tt.IsDeleted = true), Any() sẽ trả về false-> !false = true->Sẽ được lấy ra.
            //            !ThanhToanQr.Any(tt =>
            //                !tt.IsDeleted &&
            //                tt.PhuLucHopDongIds != null &&
            //                tt.PhuLucHopDongIds.Contains(pl.Id))
            //        );
        }


        //}
        return data.Select(e => new PhuLucHopDongDto {
                                     Id = e.Id,
                                     DuAnId = e.DuAnId,
                                     BuocId = e.BuocId,
                                     Ten = e.Ten,
                                     SoPhuLucHopDong = e.SoPhuLucHopDong,
                                     NoiDung = e.NoiDung,
                                     Ngay = e.Ngay,
                                     HopDongId = e.HopDongId,
                                     GiaTri = e.GiaTri
                                 })
                                 .ToList(); 
    }
}