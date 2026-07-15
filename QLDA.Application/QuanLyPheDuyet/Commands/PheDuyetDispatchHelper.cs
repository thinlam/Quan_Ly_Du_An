using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Helpers dùng chung cho các PheDuyetDispatch*Command (Duyet/TraLai/TuChoi).
/// Tập trung resolve DuAnId từ một entity theo type (string) để áp phân quyền
/// "Lãnh đạo phụ trách chính hoặc QLDA_LDDV" ngay tại tầng dispatch, trước
/// khi route sang inner command xử lý nghiệp vụ.
/// </summary>
internal static class PheDuyetDispatchHelper
{
    /// <summary>
    /// Resolve DuAnId (Guid?) cho entity ứng với PheDuyetEntityNames.* từ request.
    /// Trả về null nếu type không thuộc dispatch scope HOẶC entity không có DuAnId / không tồn tại.
    /// </summary>
    internal static async Task<Guid?> GetDuAnIdAsync(
        IServiceProvider serviceProvider,
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var repo = serviceProvider.GetRequiredService<DbContext>();

        // Mỗi nhánh project thẳng sang Guid? để có cùng kiểu trả về — tránh generic
        // constraint ràng buộc property DuAnId (không phải entity nào cũng expose).
        return entityType switch {
            PheDuyetEntityNames.PheDuyetDuToan =>
                await repo.Set<PheDuyetDuToan>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt =>
                await repo.Set<HoSoDeXuatCapDoCntt>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.HoSoMoiThauDienTu =>
                await repo.Set<HoSoMoiThauDienTu>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.PhanKhaiKinhPhi =>
                await repo.Set<PhanKhaiKinhPhi>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.QuyetDinhDieuChinh =>
                await repo.Set<QuyetDinhDieuChinh>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.DeXuatChuTruongMoi =>
                await repo.Set<DeXuatChuTruongMoi>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep =>
                await repo.Set<DeXuatChuyenTiep>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi =>
                await repo.Set<DeXuatNhuCauKinhPhi>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            // DeXuatNhuCauKinhPhiNam không gắn DuAn cụ thể (kinh phí năm) → no-op (trả về null → no-op auth check).
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => null,
            PheDuyetEntityNames.ThuyetMinhDuAn =>
                await repo.Set<ThuyetMinhDuAn>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau =>
                await repo.Set<ToTrinhKetQuaGoiThau>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau =>
                await repo.Set<ToTrinhThamDinhNhaThau>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.TrienKhaiKeHoachLCNT =>
                await repo.Set<TrienKhaiKeHoachLCNT>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc =>
                await repo.Set<KeHoachTrienKhaiHangMuc>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.DuToanDauTu =>
                await repo.Set<DuToanDauTu>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.ChuTruongLapKeHoach =>
                await repo.Set<ChuTruongLapKeHoach>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat =>
                await repo.Set<BaoCaoKetQuaKhaoSat>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.PheDuyetKhaoSat or
            PheDuyetEntityNames.QuyetDinhKeHoachThue or
            PheDuyetEntityNames.ToTrinhKeHoach or
            PheDuyetEntityNames.QuyetDinhDuyetDuToan =>
                await repo.Set<ToTrinhPheDuyet>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon =>
                await repo.Set<KeHoachLuaChonNhaThauRutGon>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.ThoaThuanGiaoViec =>
                await repo.Set<ThoaThuanGiaoViec>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.QuyetDinhLapBanQLDA =>
                await repo.Set<QuyetDinhLapBanQLDA>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            PheDuyetEntityNames.ThanhLyHopDong =>
                await repo.Set<ThanhLyHopDong>().Where(e => e.Id == entityId).Select(e => (Guid?)e.DuAnId).FirstOrDefaultAsync(cancellationToken),
            _ => null
        };
    }
}
