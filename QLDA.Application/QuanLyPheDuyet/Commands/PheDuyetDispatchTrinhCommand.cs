using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;
using QLDA.Application.Common;
using QLDA.Application.DeXuatChuTruongMois.Commands;
using QLDA.Application.DeXuatChuyenTieps.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhis.Commands;
using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PheDuyetDuToans.Commands;
using QLDA.Application.QuyetDinhDieuChinhs.Commands;
using QLDA.Application.ThuyetMinhDuAns.Commands;
using QLDA.Application.ToTrinhKeHoachs.Commands;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Dispatch trinh phe duyet theo type → den dung entity command
/// </summary>
public record PheDuyetDispatchTrinhCommand(string Type, Guid Id, string? NoiDung = null) : IRequest<int>;

internal class PheDuyetDispatchTrinhCommandHandler : IRequestHandler<PheDuyetDispatchTrinhCommand, int> {
    private readonly IMediator _mediator;

    public PheDuyetDispatchTrinhCommandHandler(IServiceProvider serviceProvider) {
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task<int> Handle(PheDuyetDispatchTrinhCommand request, CancellationToken cancellationToken) {
        IRequest<int> command = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => new PheDuyetDuToanTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt => new HoSoDeXuatCapDoCnttTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhKeHoach => new ToTrinhKeHoachTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatChuTruongMoi => new DeXuatChuTruongMoiTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep => new DeXuatChuyenTiepTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi => new DeXuatNhuCauKinhPhiTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatNhuCauKinhPhiNamTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ThuyetMinhDuAn => new ThuyetMinhDuAnTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhPheDuyet => new ToTrinhPheDuyetTrinhCommand(request.Id, request.NoiDung),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
