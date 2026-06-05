using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Queries;

public record TheoDoiDeXuatNhuCauKinhPhiQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<TheoDoiDeXuatNhuCauKinhPhiDto>>
{
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiKeHoachId { get; set; }
    //public bool? DaDuyetTongHop { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public long? DonViDeXuatId { get; set; }
    public DateOnly? TuNgay { get; set; } // ngày phiếu chuyển của đề xuất
    public DateOnly? DenNgay { get; set; }
}

internal class
    TheoDoiDeXuatNhuCauKinhPhiQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<TheoDoiDeXuatNhuCauKinhPhiQuery, PaginatedList<TheoDoiDeXuatNhuCauKinhPhiDto>>
{
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> DeXuatNhuCauKinhPhi = ServiceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
   // private readonly IRepository<BuildingBlocks.Domain.Entities.UserMaster, long> _userMasterRepository =
     // ServiceProvider.GetRequiredService<IRepository<BuildingBlocks.Domain.Entities.UserMaster, long>>();
    private readonly IRepository<PheDuyet, Guid> pheDuyet = ServiceProvider.GetRequiredService<IRepository<PheDuyet, Guid>>();
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> keHoachNam = ServiceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();
    // private readonly IRepository<TepDinhKem, Guid> TepDinhKem =   ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository = ServiceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();
    private readonly IRepository<DmDonVi, long> DanhMucDonVi = ServiceProvider.GetRequiredService<IRepository<DmDonVi, long>>();


    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<TheoDoiDeXuatNhuCauKinhPhiDto>> Handle(TheoDoiDeXuatNhuCauKinhPhiQuery request,
        CancellationToken cancellationToken = default)
    {

        /*
         mục đích
        theo doi tình hình đề xuất nhu câu kinh phí qua 2 giai đoạn
        1 . đề xuất phòng KH-TC: tạo đề xuất, trình duyệt nội bộ phòng KH-TC, duyệt bởi trưởng phòng KH-TC
        2. Đề xuất vào kế hoạch năm: sau khi duyệt bởi trưởng phòng KH-TC, đề xuất sẽ được chuyển sang trạng thái "Đã duyệt" và hiển thị để đưa vào ke hoach năm
         dự án 1 - Phòng khtc đã duyêt - trạng thái 2: chờ Ban gd duyệt
        dự án 2 - phòng khtc đã từ chối - ---- ------
         */

        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
         .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
        var trangThaiDaTrinh = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
          .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DaTrinh && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
        var trangThaiDuThao = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
          .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.DuThao && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
        var trangThaiDaTra = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
          .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.TraLai && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);

        //  ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'"); 

        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null; // exclusive upper bound
        if (request.TuNgay.HasValue)
        {
            var dt = request.TuNgay.Value.ToDateTime(TimeOnly.MinValue);
            tuNgayDto = new DateTimeOffset(dt);
        }
        if (request.DenNgay.HasValue)
        {
            var dt = request.DenNgay.Value.ToDateTime(TimeOnly.MinValue);
            denNgayExclusiveDto = new DateTimeOffset(dt).AddDays(1);
        }
        // var users = _userMasterRepository.GetQueryableSet().AsNoTracking();
        // 1. Lấy IQueryable của PheDuyet để EF Core có thể dịch thành SQL Subquery
        var pheDuyetQuery = pheDuyet.GetQueryableSet().AsNoTracking();
        var keHoachNamQuery = keHoachNam.GetQueryableSet().AsNoTracking();

        var queryable = DeXuatNhuCauKinhPhi.GetQueryableSet().AsNoTracking()
           .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(      request.TrangThaiKeHoachId.HasValue,  e => e.DeXuatDaTrinhKeHoachNam!.Any(x =>
                    x.DeXuatNhuCauKinhPhiNam != null
                    && !x.DeXuatNhuCauKinhPhiNam.IsDeleted
                    && x.DeXuatNhuCauKinhPhiNam.TrangThaiId == request.TrangThaiKeHoachId ))
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.SoPhieuChuyen != null, e => e.SoPhieuChuyen.Contains(request.SoPhieuChuyen))
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayPhieuChuyen >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayPhieuChuyen < denNgayExclusiveDto)
            //.Join(users, e => long.Parse(e.CreatedBy!), u => u.UserPortalId, (e, user) => new { e, user })
            .WhereIf(request.DonViDeXuatId != null,x => x.DonViDeXuatId == request.DonViDeXuatId)
            .WhereIf(request.TrichYeu != null,x => x.TrichYeu == request.TrichYeu);

    return await queryable.Select(x => new TheoDoiDeXuatNhuCauKinhPhiDto(){
            Id = x.Id,
            DuAnId = x.DuAnId,
            BuocId = x.BuocId,
            TenDonViDeXuat = DanhMucDonVi.GetQueryableSet().Where(dv => dv.Id == x.DonViDeXuatId)
                .Select(dv => dv.TenDonVi ?? "Không rõ") .FirstOrDefault(),

            DonViDeXuatId = x.DonViDeXuatId,
            SoPhieuChuyen = x.SoPhieuChuyen,
            NgayPhieuChuyen = x.NgayPhieuChuyen,
            TrichYeu = x.TrichYeu,
            KinhPhiDeXuat = x.KinhPhiDeXuat,

            TrangThaiId = x.TrangThaiId,
            TenTrangThai = x.TrangThai != null  ? x.TrangThai.Ten  : "---",
            TrangThaiKeHoachNamId = x.DeXuatDaTrinhKeHoachNam!
                    .Where(t =>  t.DeXuatNhuCauKinhPhiNam != null && !t.DeXuatNhuCauKinhPhiNam.IsDeleted)
                    .Select(t => t.DeXuatNhuCauKinhPhiNam!.TrangThaiId).FirstOrDefault(),
            // text: đã trình nếu đã trình. còn lại '----'
            TenTrangThaiKeHoachNam = x.DeXuatDaTrinhKeHoachNam!
                    .Any(t =>
                        t.DeXuatNhuCauKinhPhiNam != null  && !t.DeXuatNhuCauKinhPhiNam.IsDeleted
                        && t.DeXuatNhuCauKinhPhiNam.TrangThaiId == trangThaiDaTrinh!.Id)? trangThaiDaTrinh!.Ten : "--",
            // text: Duyệt thì show ngày duyệt. còn lại '----'
            NgayDuyetKeHoach = x.DeXuatDaTrinhKeHoachNam!
                            .Where(t =>
                                t.DeXuatNhuCauKinhPhiNam != null && !t.DeXuatNhuCauKinhPhiNam.IsDeleted
                                && t.DeXuatNhuCauKinhPhiNam.NgayDuyet != null)
                            .Select(t =>  t.DeXuatNhuCauKinhPhiNam.NgayDuyet).FirstOrDefault() ,
            // text: Duyệt thì show 'Đã duyệt'. còn lại '----'
            TenTrangThaiBanGiamDoc = x.DeXuatDaTrinhKeHoachNam!
                            .Where(t => t.DeXuatNhuCauKinhPhiNam != null && !t.DeXuatNhuCauKinhPhiNam.IsDeleted)
                            .Select(t => t.DeXuatNhuCauKinhPhiNam!.TrangThaiId == trangThaiDaDuyet!.Id
                            ? (t.DeXuatNhuCauKinhPhiNam!.TrangThai != null ? t.DeXuatNhuCauKinhPhiNam.TrangThai!.Ten : "--")
                            : "--").FirstOrDefault() ?? "--"
        })
        .PaginatedListAsync( request.Skip(), request.Take(),  cancellationToken: cancellationToken);
    }
}