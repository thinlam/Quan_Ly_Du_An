using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;
using Serilog;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// UC22 — gọi dbo.CoreMessaging_CreateNotification sau khi lưu nghiệp vụ phê duyệt thành công.
/// </summary>
internal static class PheDuyetNotificationHelper
{
    private const string StoredProcedure = "dbo.CoreMessaging_CreateNotification";
    private const string Subject = "Quản lý dự án";
    private const int NotifyTypesId = 24;
    private const int PortalId = 0;
    private const int MaxBodyLength = 2000;

    internal static async Task<long> ResolveNguoiNhanIdAsync(
        IRepository<PheDuyetHistory, Guid> historyRepo,
        IRepository<PheDuyet, Guid> pheDuyetRepo,
        string entityName,
        Guid entityId,
        string? createdBy,
        string maTrangThaiTrinh = TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh,
        CancellationToken cancellationToken = default)
    {
        var fromHistory = await historyRepo.GetQueryableSet()
            .AsNoTracking()
            .Include(h => h.TrangThai)
            .Where(h => h.EntityId == entityId
                        && h.EntityName == entityName
                        && h.TrangThai != null
                        && h.TrangThai.Ma == maTrangThaiTrinh)
            .OrderByDescending(h => h.NgayXuLy)
            .Select(h => h.NguoiXuLyId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fromHistory is > 0)
            return fromHistory.Value;

        var pheDuyet = await pheDuyetRepo.GetQueryableSet()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.EntityId == entityId && p.EntityName == entityName, cancellationToken);

        if (pheDuyet?.NguoiTrinhId is > 0)
            return pheDuyet.NguoiTrinhId.Value;

        if (long.TryParse(createdBy, out var createdUserId) && createdUserId > 0)
            return createdUserId;

        return 0;
    }

    internal static string BuildBody(
        PheDuyetNotificationAction action,
        string? tenDuAn,
        string entityName,
        string? lyDo = null)
    {
        var ten = string.IsNullOrWhiteSpace(tenDuAn) ? "—" : tenDuAn.Trim();
        var loai = entityName.GetDescriptionFromName();

        var body = action switch {
            PheDuyetNotificationAction.Duyet =>
                $"Thông tin dự án {ten} ({loai}) đã được phê duyệt.",
            PheDuyetNotificationAction.TuChoi =>
                $"Thông tin dự án {ten} ({loai}) bị từ chối. Lý do: {lyDo?.Trim() ?? "—"}.",
            PheDuyetNotificationAction.TraLai =>
                $"Thông tin dự án {ten} ({loai}) cần cập nhật và gửi lại. Nội dung: {lyDo?.Trim() ?? "—"}.",
            PheDuyetNotificationAction.Chuyen =>
                $"Thông tin dự án {ten} ({loai}) đã được chuyển xử lý.",
            PheDuyetNotificationAction.PhatHanh =>
                $"Thông tin dự án {ten} ({loai}) đã được phát hành.",
            _ => $"Thông tin dự án {ten} ({loai}) đã được cập nhật."
        };

        return body.Length <= MaxBodyLength ? body : body[..MaxBodyLength];
    }

    internal static async Task TrySendAsync(
        IDapperRepository dapper,
        long nguoiGuiId,
        long nguoiNhanId,
        string body,
        Guid entityId,
        string entityName,
        PheDuyetNotificationAction action,
        CancellationToken cancellationToken)
    {
        if (nguoiNhanId <= 0) {
            Log.Warning(
                "UC22 skip notification: NguoiNhanId invalid. Entity={EntityName}, EntityId={EntityId}",
                entityName, entityId);
            return;
        }

        if (nguoiGuiId <= 0) {
            Log.Warning(
                "UC22 skip notification: NguoiGuiId invalid. Entity={EntityName}, EntityId={EntityId}",
                entityName, entityId);
            return;
        }

        var parameters = new DynamicParameters();
        parameters.Add("@NguoiNhanId", nguoiNhanId, DbType.Int64);
        parameters.Add("@NguoiGuiId", nguoiGuiId, DbType.Int64);
        parameters.Add("@NotifyTypesId", NotifyTypesId, DbType.Int32);
        parameters.Add("@Subject", Subject, DbType.String);
        parameters.Add("@Body", body ?? string.Empty, DbType.String);
        parameters.Add("@PortalId", PortalId, DbType.Int32);

        try {
            await dapper.ExecuteStoredProcAsync(
                StoredProcedure,
                parameters,
                cancellationToken: cancellationToken);
        } catch (Exception ex) {
            Log.Error(
                ex,
                "UC22 CoreMessaging_CreateNotification failed. Entity={EntityName}, EntityId={EntityId}, Action={Action}",
                entityName, entityId, action);
        }
    }

    internal static async Task NotifyAfterSaveAsync(
        IDapperRepository dapper,
        IRepository<PheDuyetHistory, Guid> historyRepo,
        IRepository<PheDuyet, Guid> pheDuyetRepo,
        long nguoiGuiId,
        string entityName,
        Guid entityId,
        string? tenDuAn,
        string? createdBy,
        PheDuyetNotificationAction action,
        string? lyDo = null,
        string maTrangThaiTrinh = TrangThaiPheDuyetCodes.DeXuatMacDinh.DaTrinh,
        CancellationToken cancellationToken = default)
    {
        var nguoiNhanId = await ResolveNguoiNhanIdAsync(
            historyRepo, pheDuyetRepo, entityName, entityId, createdBy, maTrangThaiTrinh, cancellationToken);

        var body = BuildBody(action, tenDuAn, entityName, lyDo);

        await TrySendAsync(
            dapper, nguoiGuiId, nguoiNhanId, body,
            entityId, entityName, action, cancellationToken);
    }
}
