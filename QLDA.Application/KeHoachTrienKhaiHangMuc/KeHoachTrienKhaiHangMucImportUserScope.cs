using BuildingBlocks.Domain.Providers;
using QLDA.Application.DanhMucDonVis;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs;

internal static class KeHoachTrienKhaiHangMucImportUserScope {
    internal static long? TryGetCurrentDonViId(IUserProvider userProvider) =>
        DmDonViPhongBanScope.TryGetCurrentDonViId(userProvider);
}
