using QLDA.Application.GoiThaus.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.GoiThaus.Queries;

public record GoiThauGetTinhHinhDauThauPrintQuery : IRequest<TinhHinhThucHienDauThauPrintResultDto>
{
    public int? Loai { get; set; }
}

internal class GoiThauGetTinhHinhDauThauPrintQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<GoiThauGetTinhHinhDauThauPrintQuery, TinhHinhThucHienDauThauPrintResultDto>
{
    private static readonly (TinhHinhThucHienDauThauLoai Loai, string Title)[] SheetTabs =
    [
        (TinhHinhThucHienDauThauLoai.ChuaCoKetQua, "Chưa có kết quả"),
        (TinhHinhThucHienDauThauLoai.CoKetQua, "Có kết quả"),
        (TinhHinhThucHienDauThauLoai.DaLenHopDong, "Đã lên hợp đồng"),
    ];

    private readonly IMediator _mediator = serviceProvider.GetRequiredService<IMediator>();

    public async Task<TinhHinhThucHienDauThauPrintResultDto> Handle(
        GoiThauGetTinhHinhDauThauPrintQuery request,
        CancellationToken cancellationToken = default)
    {
        var loai = ParseLoai(request.Loai);

        if (loai is null or TinhHinhThucHienDauThauLoai.TatCa)
        {
            var sheets = new List<TinhHinhThucHienDauThauSheetDto>(SheetTabs.Length);
            foreach (var tab in SheetTabs)
            {
                var items = await _mediator.Send(
                    new GoiThauGetTinhHinhDauThauExportQuery { Loai = tab.Loai },
                    cancellationToken);
                sheets.Add(new TinhHinhThucHienDauThauSheetDto
                {
                    Title = tab.Title,
                    Items = items,
                });
            }

            return new TinhHinhThucHienDauThauPrintResultDto
            {
                IsMultiSheet = true,
                Sheets = sheets,
            };
        }

        var data = await _mediator.Send(
            new GoiThauGetTinhHinhDauThauExportQuery { Loai = loai.Value },
            cancellationToken);

        return new TinhHinhThucHienDauThauPrintResultDto
        {
            IsMultiSheet = false,
            Items = data,
        };
    }

    private static TinhHinhThucHienDauThauLoai? ParseLoai(int? loai)
    {
        if (loai is null)
        {
            return null;
        }

        ManagedException.ThrowIf(
            !Enum.IsDefined(typeof(TinhHinhThucHienDauThauLoai), loai.Value),
            "Loại tab không hợp lệ. Chỉ chấp nhận giá trị 1 (Chưa có kết quả), 2 (Có kết quả), 3 (Đã lên hợp đồng), hoặc bỏ trống để xuất cả 3 tab.");

        return (TinhHinhThucHienDauThauLoai)loai.Value;
    }
}
