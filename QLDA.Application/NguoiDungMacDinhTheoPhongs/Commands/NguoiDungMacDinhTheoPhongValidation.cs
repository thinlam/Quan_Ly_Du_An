using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;

internal static class NguoiDungMacDinhTheoPhongValidation
{
    private const string DuplicateMessage =
        "Người dùng này đã được cấu hình mặc định cho phòng ban đã chọn.";

    public static void EnsureRequired(long phongBanId, long nguoiDungId)
    {
        ManagedException.ThrowIf(phongBanId <= 0, "Phòng ban là bắt buộc");
        ManagedException.ThrowIf(nguoiDungId <= 0, "Người dùng là bắt buộc");
    }

    public static async Task EnsureReferencesExistAsync(
        long phongBanId,
        long nguoiDungId,
        IRepository<DmDonVi, long> dmDonVi,
        IRepository<UserMaster, long> userMaster,
        CancellationToken cancellationToken)
    {
        var phongBanExists = await dmDonVi.GetQueryableSet()
            .AnyAsync(e => e.Id == phongBanId, cancellationToken);
        ManagedException.ThrowIf(!phongBanExists, "Không tìm thấy phòng ban");

        var nguoiDungExists = await userMaster.GetQueryableSet()
            .AnyAsync(e => e.Id == nguoiDungId, cancellationToken);
        ManagedException.ThrowIf(!nguoiDungExists, "Không tìm thấy người dùng");
    }

    public static async Task EnsureNotDuplicateAsync(
        long phongBanId,
        long nguoiDungId,
        IRepository<NguoiDungMacDinhTheoPhong, Guid> repository,
        CancellationToken cancellationToken,
        Guid? excludeId = null)
    {
        var exists = await repository.GetQueryableSet()
            .WhereIf(excludeId.HasValue, e => e.Id != excludeId)
            .AnyAsync(e => e.PhongBanId == phongBanId && e.NguoiDungId == nguoiDungId, cancellationToken);

        ManagedException.ThrowIf(exists, DuplicateMessage);
    }
}
