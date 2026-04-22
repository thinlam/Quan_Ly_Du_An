using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.Queries;

public record DuAnGetDanhSachQuery(DuAnSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<DuAnDto>> {
    public bool IsNoTracking { get; set; }
}

internal class DuAnGetDanhSachQueryHandler : IRequestHandler<DuAnGetDanhSachQuery, PaginatedList<DuAnDto>> {
    private readonly IRepository<DuAn, Guid> DuAn;

    public DuAnGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    }


    public async Task<PaginatedList<DuAnDto>> Handle(DuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = DuAn.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.DuToanHienTai)
            .Include(e => e.DuToans)
            .WhereIf(request.SearchDto.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower().Contains(request.SearchDto.TenDuAn!.ToLower()))
            .WhereIf(request.SearchDto.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == request.SearchDto.ThoiGianKhoiCong)
            .WhereIf(request.SearchDto.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == request.SearchDto.ThoiGianHoanThanh)
            .WhereIf(request.SearchDto.LoaiDuAnId > 0, e => e.LoaiDuAnId == request.SearchDto.LoaiDuAnId)
            .WhereFunc(request.SearchDto.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId)
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null)
            )
            .WhereIf(request.SearchDto.HinhThucDauTuId > 0, e => e.HinhThucDauTuId == request.SearchDto.HinhThucDauTuId)
            .WhereIf(request.SearchDto.LoaiDuAnTheoNamId > 0, e => e.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId);

        return await queryable
            .Select(e => new DuAnDto() {
                Id = e.Id,
                TenDuAn = e.TenDuAn,
                QuyTrinhId = e.QuyTrinhId,
                DiaDiem = e.DiaDiem,
                ChuDauTuId = e.ChuDauTuId,
                ThoiGianKhoiCong = e.ThoiGianKhoiCong,
                ThoiGianHoanThanh = e.ThoiGianHoanThanh,
                MaDuAn = e.MaDuAn,
                MaNganSach = e.MaNganSach,
                DuAnTrongDiem = e.DuAnTrongDiem,
                GiaiDoanId = e.GiaiDoanHienTaiId != null ? e.GiaiDoanHienTaiId : e.BuocHienTai != null && e.BuocHienTai.Buoc != null ? e.BuocHienTai.Buoc.GiaiDoanId : null,
                LinhVucId = e.LinhVucId,
                NhomDuAnId = e.NhomDuAnId,
                NangLucThietKe = e.NangLucThietKe,
                QuyMoDuAn = e.QuyMoDuAn,
                HinhThucQuanLyDuAnId = e.HinhThucQuanLyDuAnId,
                LoaiDuAnId = e.LoaiDuAnId,
                TongMucDauTu = e.TongMucDauTu,
                TrangThaiDuAnId = e.TrangThaiDuAnId,
                GhiChu = e.GhiChu,
                NgayBatDau = e.NgayBatDau!.Value.Date,
                LanhDaoPhuTrachId = e.LanhDaoPhuTrachId,
                DonViPhuTrachChinhId = e.DonViPhuTrachChinhId,
                DonViPhoiHopIds = e.DuAnChiuTrachNhiemXuLys!
                    .Where(i => i.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                    .Select(i => i.ChiuTrachNhiemXuLyId).ToList(),

                #region Task #9121
                HinhThucDauTuId = e.HinhThucDauTuId,
                LoaiDuAnTheoNamId = e.LoaiDuAnTheoNamId,
                TenBuoc = e.BuocHienTai != null ? e.BuocHienTai.TenBuoc : null,
                BuocId = e.BuocHienTai != null ? e.BuocHienTai.BuocId : null,
                #endregion
            })
            .PaginatedListAsync(request.SearchDto.Skip(), request.SearchDto.Take(), cancellationToken: cancellationToken);
    }
}