using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

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
            TepDinhKem = FormatTepDinhKem(row.DanhSachTepDinhKem),
        }).ToList();

    /// <summary>Ghép tên file để bind cột <c>$TepDinhKem</c> (wrap text trên template).</summary>
    internal static string FormatTepDinhKem(IEnumerable<TepDinhKemDto>? files)
    {
        if (files == null)
            return string.Empty;

        var names = files
            .Select(f => !string.IsNullOrWhiteSpace(f.OriginalName) ? f.OriginalName!.Trim()
                : !string.IsNullOrWhiteSpace(f.FileName) ? f.FileName!.Trim()
                : null)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        return names.Count == 0 ? string.Empty : string.Join(Environment.NewLine, names);
    }
}
