using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.NghiemThus.DTOs;

namespace QLDA.Application.NghiemThus.Queries;

public record NghiemThuGetComboboxQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<NghiemThuDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public Guid? ThanhToanId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    NghiemThuGetComboboxQueryHandler : IRequestHandler<NghiemThuGetComboboxQuery,
    PaginatedList<NghiemThuDto>> {
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<ThanhToan, Guid> ThanhToan;

    public NghiemThuGetComboboxQueryHandler(IServiceProvider serviceProvider) {
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        ThanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    }

    public async Task<PaginatedList<NghiemThuDto>> Handle(NghiemThuGetComboboxQuery request,
        CancellationToken cancellationToken = default) {
        // Giả định bạn đã lấy thanhToanQuery từ Repository của ThanhToan
        var thanhToanQuery = ThanhToan.GetQueryableSet().AsNoTracking();

        var queryable = NghiemThu.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted) // WHERE [n].[IsDeleted] = 0
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(request, e => e.SoBienBan, e => e.NoiDung);

        if (request.ThanhToanId.HasValue)
        {
            // Trường hợp SỬA: Lấy nghiệm thu thuộc chính thanh toán đang truyền lên HOẶC chưa có thanh toán nào dùng nó
            queryable = queryable.Where(e =>
            thanhToanQuery.Any(t => t.NghiemThuId == e.Id && t.Id == request.ThanhToanId));
        }
        else
        {
            // Trường hợp TẠO MỚI : KHÔNG TỒN TẠI bất kỳ thanh toán nào (chưa bị xóa) đang liên kết NghiemThuId này
            queryable = queryable.Where(e =>
                 !thanhToanQuery.Any(t => t.NghiemThuId == e.Id && !t.IsDeleted && t.DuAnId == request.DuAnId)  );
        }

        return await queryable
            .Select(e => new NghiemThuDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                HopDongId = e.HopDongId,
                PhuLucHopDongIds = e.NghiemThuPhuLucHopDongs!.Select(junction => junction.RightId).ToList(),
                Dot = e.Dot,
                Ngay = e.Ngay,
                NoiDung = e.NoiDung,
                SoBienBan = e.SoBienBan,
                ThanhToanId = !e.ThanhToan!.IsDeleted ? e.ThanhToan!.Id: null,
              
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}