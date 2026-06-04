using QLDA.Application.Common;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;
using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PheDuyetDuToans.Commands;
using QLDA.Application.QuyetDinhDieuChinhs.Commands;
using QLDA.Application.ToTrinhKeHoachs.Commands;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Dispatch tu choi phe duyet theo type → den dung entity command
/// </summary>
public record PheDuyetDispatchTuChoiCommand(string Type, Guid Id, string NoiDung) : IRequest<int>;

internal class PheDuyetDispatchTuChoiCommandHandler : IRequestHandler<PheDuyetDispatchTuChoiCommand, int> {
    private readonly IMediator _mediator;

    public PheDuyetDispatchTuChoiCommandHandler(IServiceProvider serviceProvider) {
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task<int> Handle(PheDuyetDispatchTuChoiCommand request, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do từ chối là bắt buộc");
        }

        IRequest<int> command = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => new PheDuyetDuToanTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt => new HoSoDeXuatCapDoCnttTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhKeHoach => new ToTrinhKeHoachTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PheDuyetKhaoSat => new ToTrinhPheDuyetTrinhCommand(request.Id, PheDuyetEntityNames.PheDuyetKhaoSat, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatKinhPhiNamTuChoiCommand(request.Id,  request.NoiDung),
            
            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
