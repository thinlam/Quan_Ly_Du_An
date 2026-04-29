namespace QLDA.FakeDataTool.Fakers;

/// <summary>
/// Static reference IDs for FK dependencies.
/// Matches IDs from DanhMuc entities and HasData seeding.
/// </summary>
public static class FKReferenceData
{
    // DanhMucLoaiDuAn IDs (seeded by CatalogSeeder)
    public const int LoaiDuAnCoSoHaTan = 1;
    public const int LoaiDuAnDeAn06 = 2;
    public const int LoaiDuAnKeHoachTPHCM = 3;

    // TrangThaiDuAn IDs (seeded via HasData in DbContext)
    public const int TrangThaiMoi = 1;
    public const int TrangThaiDangThucHien = 2;
    public const int TrangThaiHoanThanh = 3;
    public const int TrangThaiTamHoan = 4;
}