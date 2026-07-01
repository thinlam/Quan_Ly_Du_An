using BuildingBlocks.Domain.Providers;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs;

internal static class KeHoachTrienKhaiHangMucImportUserScope {
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
}
