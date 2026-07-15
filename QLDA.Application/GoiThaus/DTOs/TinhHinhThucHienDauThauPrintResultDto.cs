namespace QLDA.Application.GoiThaus.DTOs;

public class TinhHinhThucHienDauThauPrintResultDto
{
    public bool IsMultiSheet { get; set; }
    public List<TinhHinhThucHienDauThauSheetDto> Sheets { get; set; } = [];
    public List<TinhHinhThucHienDauThauExportDto> Items { get; set; } = [];
}

public class TinhHinhThucHienDauThauSheetDto
{
    public string Title { get; set; } = string.Empty;
    public List<TinhHinhThucHienDauThauExportDto> Items { get; set; } = [];
}
