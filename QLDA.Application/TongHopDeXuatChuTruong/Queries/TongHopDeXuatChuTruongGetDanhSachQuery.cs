using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TongHopDeXuatChuTruongs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.TongHopDeXuatChuTruongs.Queries;

public record TongHopDeXuatChuTruongGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter,  IRequest<TongHopDeXuatChuTruongResponseDto> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }

    public string? Loai { get; set; }
    public long? DonViPhuTrachId { get; set; }
    public int? Nam { get; set; } 
}

internal class
    TongHopDeXuatChuTruongGetDanhSachQueryHandler(IServiceProvider ServiceProvider)    : IRequestHandler<TongHopDeXuatChuTruongGetDanhSachQuery, TongHopDeXuatChuTruongResponseDto> {
    private readonly IRepository<DeXuatChuTruongMoi, Guid> DeXuatChuTruongMoi =  ServiceProvider.GetRequiredService<IRepository<DeXuatChuTruongMoi, Guid>>();
    private readonly IRepository<DeXuatChuyenTiep, Guid> DeXuatChuyenTiep =  ServiceProvider.GetRequiredService<IRepository<DeXuatChuyenTiep, Guid>>();
    private readonly IRepository<DmDonVi, long> DmDonVi = ServiceProvider.GetRequiredService<IRepository<DmDonVi, long>>()   ;
    private readonly IRepository<UserMaster, long> userMaster = ServiceProvider.GetRequiredService<IRepository<UserMaster, long>>()   ;
    //private readonly IRepository<DmDonVi, Guid> PheDuyet = ServiceProvider.GetRequiredService<IRepository<DmDonVi, Guid>>()   ;
    private readonly IRepository<QLDA.Domain.Entities.TepDinhKem, Guid> TepDinhKem =  ServiceProvider.GetRequiredService<IRepository<QLDA.Domain.Entities.TepDinhKem, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();
    public async Task<TongHopDeXuatChuTruongResponseDto> Handle(TongHopDeXuatChuTruongGetDanhSachQuery request,  CancellationToken cancellationToken = default) {
        var userQuery = userMaster.GetQueryableSet().AsNoTracking();
        var dmDonViQuery = DmDonVi.GetQueryableSet().AsNoTracking();
     
        // 1. Khởi tạo Quereryable gốc cho Đề xuất mới
        var queryableMoi = DeXuatChuTruongMoi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.DonViPhuTrachId != null, e => e.DonViPhuTrachChinhId == request.DonViPhuTrachId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            
            .WhereIf(request.Nam != null, e => e.NamDeXuat == request.Nam)
            // Map về cấu trúc chung của TongHopDeXuatChuTruongDto (Loai = "DeXuatMoi")
            .Select(e => new TongHopDeXuatChuTruongDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TenDuAn =e.DuAn != null ? e.DuAn.TenDuAn : "Không rõ",
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,

                TenPhongBanPhuTrach = e.CreatedBy != null
                ? dmDonViQuery.Where(dv => dv.Id == userQuery.Where(us => us.UserPortalId == Convert.ToInt64(e.CreatedBy)).Select(us => us.PhongBanId).FirstOrDefault())
                              .Select(dv => dv.TenDonVi)
                              .FirstOrDefault() ?? "Không rõ"
                : "Không rõ",
                Loai = "DeXuatMoi", 
                ////DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                ////    .Where(i => i.GroupId == e.Id.ToString())
                ////    .Select(i => i.ToDto()).ToList()
            });

        // 2. Khởi tạo Queryable gốc cho Đề xuất chuyển tiếp
        var queryableCT = DeXuatChuyenTiep.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.Nam != null, e => e.NamDeXuat == request.Nam)
            .Select(e => new TongHopDeXuatChuTruongDto()
            {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TenDuAn =e.DuAn != null ? e.DuAn.TenDuAn : "Không rõ",
                TenPhongBanPhuTrach = e.CreatedBy != null
            ? dmDonViQuery.Where(dv => dv.Id == userQuery.Where(us => us.UserPortalId == Convert.ToInt64(e.CreatedBy))
                .Select(us => us.PhongBanId).FirstOrDefault())
                          .Select(dv => dv.TenDonVi)
                          .FirstOrDefault() ?? "Không rõ"
            : "Không rõ",
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                Loai = "ChuyenTiep", 
            });
        var finalQueryable = queryableMoi.Concat(queryableCT);
        try
        {
            // return      await finalQueryable.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
            // Sử dụng Task.WhenAll để kích hoạt đếm song song dưới DB nhằm tối ưu hiệu năng tốc độ
            var tongDeXuatMoiTask = queryableMoi.Count();
            var tongChuyenTiepTask = queryableCT.Count();
            var dataTask = new PaginatedList<TongHopDeXuatChuTruongDto>();
            if(request.Loai == "DeXuatMoi")
                dataTask = await queryableMoi.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
            else if(request.Loai == "ChuyenTiep") 
                dataTask = await queryableCT.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
            else dataTask = await finalQueryable.PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);

          //  await Task.WhenAll(tongDeXuatMoiTask, tongChuyenTiepTask, pagedDataTask);

            // Trả về Object theo đúng định dạng bạn yêu cầu
            return new TongHopDeXuatChuTruongResponseDto
            {
                TongDeXuatMoi =  tongDeXuatMoiTask,
                TongDeXuatChuyenTiep =  tongChuyenTiepTask,
                Data = dataTask
            };
        }
        catch (Exception ex)
        {

            throw;
        }
    }
}