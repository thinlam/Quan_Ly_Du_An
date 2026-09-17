using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Domain.Entities;

namespace QLDA.Application.Dashboard.Queries;

/// <summary>
/// Query chi tiết giải ngân theo năm và nguồn vốn
/// </summary>
public record DashboardGetChiTietGiaiNganQuery(int Nam, int? NguonVonId = null) : IRequest<List<DashboardChiTietGiaiNganDto>>;

internal class DashboardGetChiTietGiaiNganQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DashboardGetChiTietGiaiNganQuery, List<DashboardChiTietGiaiNganDto>> {

    private readonly IRepository<DuAn, Guid> _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<KeHoachVon, Guid> _keHoachVon = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
    private readonly IRepository<ThanhToan, Guid> _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IRepository<HopDong, Guid> _hopDong = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<DashboardChiTietGiaiNganDto>> Handle(
        DashboardGetChiTietGiaiNganQuery request, CancellationToken cancellationToken) {
        try {
            return await DashboardGiaiNganHelper.GetChiTietGiaiNganAsync(
                _duAn,
                _keHoachVon,
                _thanhToan,
                _hopDong,
                _authManager,
                request.Nam,
                request.NguonVonId,
                cancellationToken);
        } catch (Exception exception) {
            Serilog.Log.Error(exception, "DashboardGetChiTietGiaiNganQuery năm {Nam}, nguồn vốn {NguonVonId} - error {Loi}", request.Nam, request.NguonVonId, exception.Message);
            throw;
        }
    }
}
