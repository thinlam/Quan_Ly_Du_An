using QLDA.Application.KeHoachTrienKhaiHangMucs.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.KeHoachTrienKhaiHangMucs;

internal static class KeHoachTrienKhaiHangMucExportMapper
{
    private const string KhacGiaiDoanTen = "Khác";

    public static List<KeHoachTrienKhaiHangMucExportItemDto> ToExportRows(
        IEnumerable<HangMucKeHoach> hangMucs,
        IReadOnlyDictionary<int, string> giaiDoanTenById,
        IReadOnlyDictionary<int, int> giaiDoanSortById,
        IReadOnlyDictionary<long, string> donViTenById,
        IReadOnlyDictionary<long, string> userTenById)
    {
        var items = hangMucs.ToList();
        if (items.Count == 0)
            return [];

        var itemIndexById = items
            .Select((h, i) => (h.Id, i))
            .ToDictionary(x => x.Id, x => x.i);

        var firstGroupIndexByGiaiDoanId = items
            .Select((h, i) => (h.GiaiDoanId, i))
            .GroupBy(x => x.GiaiDoanId)
            .ToDictionary(g => g.Key, g => g.Min(x => x.i));

        var groups = items
            .GroupBy(h => h.GiaiDoanId)
            .Select(g =>
            {
                if (g.Key is int id && giaiDoanTenById.TryGetValue(id, out var ten))
                {
                    return new KeHoachTrienKhaiGroupByGiaiDoanDto
                    {
                        GiaiDoanId = id,
                        TenGiaiDoan = ten,
                        SortOrder = giaiDoanSortById.GetValueOrDefault(id, int.MaxValue - 1),
                        Items = g.OrderBy(x => itemIndexById.GetValueOrDefault(x.Id, int.MaxValue)).ToList(),
                    };
                }

                return new KeHoachTrienKhaiGroupByGiaiDoanDto
                {
                    GiaiDoanId = null,
                    TenGiaiDoan = KhacGiaiDoanTen,
                    SortOrder = int.MaxValue,
                    Items = g.OrderBy(x => itemIndexById.GetValueOrDefault(x.Id, int.MaxValue)).ToList(),
                };
            })
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => firstGroupIndexByGiaiDoanId.GetValueOrDefault(g.GiaiDoanId, int.MaxValue))
            .ToList();

        var rows = new List<KeHoachTrienKhaiHangMucExportItemDto>();
        for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            var group = groups[groupIndex];
            rows.Add(new KeHoachTrienKhaiHangMucExportItemDto
            {
                Stt = ToGroupLetter(groupIndex),
                GiaiDoan = group.TenGiaiDoan,
                IsGroupHeader = true,
            });

            var itemStt = 1;
            foreach (var hangMuc in group.Items)
                rows.Add(MapItem(hangMuc, itemStt++, donViTenById, userTenById));
        }

        return rows;
    }

    private static KeHoachTrienKhaiHangMucExportItemDto MapItem(
        HangMucKeHoach hangMuc,
        int stt,
        IReadOnlyDictionary<long, string> donViTenById,
        IReadOnlyDictionary<long, string> userTenById) =>
        new()
        {
            Stt = stt.ToString(),
            TenHangMuc = hangMuc.TenHangMuc,
            DonViChuTri = ResolveName(hangMuc.DonViChuTriId, donViTenById),
            DonViPhoiHop = JoinNames(hangMuc.DonViPhoiHopIds, donViTenById),
            NgayBatDau = ToDateTime(hangMuc.NgayBatDau),
            NgayKetThuc = ToDateTime(hangMuc.NgayKetThuc),
            ThoiHan = CalcThoiHan(hangMuc),
            CanBoChuTri = ResolveName(hangMuc.CanBoChuTriId, userTenById),
            CanBoPhoiHop = JoinNames(hangMuc.CanBoPhoiHopIds, userTenById),
            KinhPhi = hangMuc.KinhPhi,
        };

    private static string ToGroupLetter(int index) =>
        ((char)('A' + index)).ToString();

    private static DateTime? ToDateTime(DateOnly? date) =>
        date?.ToDateTime(TimeOnly.MinValue);

    private static int? CalcThoiHan(HangMucKeHoach hangMuc)
    {
        if (hangMuc.NgayBatDau.HasValue && hangMuc.NgayKetThuc.HasValue)
            return hangMuc.NgayKetThuc.Value.DayNumber - hangMuc.NgayBatDau.Value.DayNumber + 1;

        return null;
    }

    private static string ResolveName(long? id, IReadOnlyDictionary<long, string> lookup) =>
        id is long key && lookup.TryGetValue(key, out var name) ? name : string.Empty;

    private static string JoinNames(IEnumerable<long>? ids, IReadOnlyDictionary<long, string> lookup)
    {
        if (ids == null)
            return string.Empty;

        return string.Join(", ",
            ids.Select(id => lookup.GetValueOrDefault(id, string.Empty))
                .Where(name => !string.IsNullOrWhiteSpace(name)));
    }
}
