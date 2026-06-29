using QLDA.Application.Common.Interfaces;
using System.Text.Json.Serialization;
using QLDA.Application.Common.Constants;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;

public class TongHopVanBanQuyetDinhDto : IHasKey<Guid>, IMayHaveTepDinhKemDto {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    [JsonIgnore] public int? BuocId { get; set; }

    /// <summary>
    /// Có thể là số văn bản hoặc số quyết định
    /// </summary>
    public string? So { get; set; }

    public string? TrichYeu { get; set; }
    public string? Loai { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    [JsonIgnore] public string? TableName { get; set; }
    public string? PartialView { get; set; } 
    // public string? PartialView => LoaiVanBanQuyetDinhConst.Dictionary.TryGetValue(TableName ?? string.Empty, out var value) ? value : string.Empty;
    public DateTimeOffset? Ngay { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}

public class VanBanQuyetDinhDto : IHasKey<Guid>, IMayHaveTepDinhKemDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    [JsonIgnore] public int? BuocId { get; set; }

    /// <summary>
    /// Có thể là số văn bản hoặc số quyết định
    /// </summary>
    public string? So { get; set; }

    public string? TrichYeu { get; set; }
    public string? Loai { get; set; }
    public string? CoQuanQuyetDinh { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    [JsonIgnore] public string? TableName { get; set; }
    public string? PartialView { get; set; }// => LoaiVanBanQuyetDinhConst.Dictionary.TryGetValue(TableName ?? string.Empty, out var value) ? value : string.Empty;
    public DateTimeOffset? Ngay { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}