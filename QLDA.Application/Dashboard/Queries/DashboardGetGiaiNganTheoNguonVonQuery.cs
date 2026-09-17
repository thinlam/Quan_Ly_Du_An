using Aspose.Cells;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// Query thống kê giải ngân theo nguồn vốn theo năm
/// </summary>
public record DashboardGetGiaiNganTheoNguonVonQuery(int Nam)
    : IRequest<List<DashboardGiaiNganTheoNguonVonDto>>;


internal class DashboardGetGiaiNganTheoNguonVonQueryHandler(
    IServiceProvider serviceProvider)
    : IRequestHandler<
          DashboardGetGiaiNganTheoNguonVonQuery,
          List<DashboardGiaiNganTheoNguonVonDto>> {

    private readonly IRepository<ThanhToan, Guid> _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IRepository<KeHoachVon, Guid> _keHoachVon = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
    private readonly IRepository<DuAn, Guid> _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<HopDong, Guid> _hopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
    private readonly IRepository<DanhMucNguonVon, int> _dmNguonVon = serviceProvider.GetRequiredService<IRepository<DanhMucNguonVon, int>>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<DashboardGiaiNganTheoNguonVonDto>> Handle(
        DashboardGetGiaiNganTheoNguonVonQuery request,
        CancellationToken cancellationToken) {
        try {
            return await DashboardGiaiNganHelper.GetGiaiNganTheoNguonVonAsync(
                _duAn,
                _dmNguonVon,
                _keHoachVon,
                _thanhToan,
                _hopDong,
                _authManager,
                request.Nam,
                cancellationToken);
        } catch (Exception exception) {
            Serilog.Log.Error("DashboardGetGiaiNganTheoNguonVonQuery năm {Nam} - error {Loi}", request.Nam, exception.Message);
            throw;
        }
    }
}



