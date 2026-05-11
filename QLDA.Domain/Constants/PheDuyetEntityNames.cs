namespace QLDA.Domain.Constants;

/// <summary>
/// Entity name constants for polymorphic PheDuyetHistory
/// </summary>
public static class PheDuyetEntityNames
{
    public const string Default = "Default";
    public const string PheDuyetDuToan = nameof(Entities.PheDuyetDuToan);
    public const string HoSoDeXuatCapDoCntt = nameof(Entities.HoSoDeXuatCapDoCntt);
    public const string HoSoMoiThauDienTu = nameof(Entities.HoSoMoiThauDienTu);
    // Future: KhaiToanKinhPhi, HoSo, etc.
}
