using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;

namespace QLDA.Application.DanhMucDonVis;

internal static class DmDonViPhongBanScope {
  internal static long? TryGetCurrentDonViId(IUserProvider userProvider) {
    if (userProvider.Id <= 0)
      return null;

    try {
      var donViId = userProvider.Info.DonViID;
      return donViId > 0 ? donViId : null;
    } catch (UnauthorizedAccessException) {
      return null;
    }
  }

  internal static IQueryable<DmDonVi> FilterPhongBanThuocDonVi(
      IQueryable<DmDonVi> query,
      long? donViId) =>
    query
      .Where(e => e.DonViCapChaId != null)
      .WhereIf(donViId > 0, e => e.DonViCapChaId == donViId);
}
