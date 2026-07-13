using QLDA.Application.QuanLyPheDuyet.DTOs;

namespace QLDA.Application.QuanLyPheDuyet;

/// <summary>Map list DTO → shape bind Excel (giữ <see cref="PheDuyetExportDto"/> cho template).</summary>
public static class PheDuyetExportMappings
{
    public static List<PheDuyetExportDto> ToExportDtos(IReadOnlyList<PheDuyetListItemDto> rows) =>
        rows.Select((row, index) => new PheDuyetExportDto
        {
            Stt = index + 1,
            TenDuAn = row.TenDuAn,
            TenGiaiDoan = row.TenGiaiDoan,
            TenBuoc = row.TenBuoc,
            NguoiTrinh = row.NguoiTrinh,
            NguoiDuyet = row.NguoiDuyet,
            TenTrangThai = row.TenTrangThai,
        }).ToList();
}
