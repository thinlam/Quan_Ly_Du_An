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
using QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;
using QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Dispatch duyet phe duyet theo type → den dung entity command
/// </summary>
public record PheDuyetDispatchDuyetCommand(string Type, Guid Id) : IRequest<int>;

internal class PheDuyetDispatchDuyetCommandHandler : IRequestHandler<PheDuyetDispatchDuyetCommand, int> {
    private readonly IMediator _mediator;

    public PheDuyetDispatchDuyetCommandHandler(IServiceProvider serviceProvider) {
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task<int> Handle(PheDuyetDispatchDuyetCommand request, CancellationToken cancellationToken) {
        IRequest<int> command = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => new PheDuyetDuToanDuyetCommand(request.Id),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt => new HoSoDeXuatCapDoCnttDuyetCommand(request.Id),
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuDuyetCommand(request.Id),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiDuyetCommand(request.Id),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatDuyetCommand(request.Id),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhDuyetCommand(request.Id),
            PheDuyetEntityNames.ToTrinhKeHoach => new ToTrinhKeHoachDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatChuTruongMoi => new DeXuatChuTruongMoiDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep => new DeXuatChuyenTiepDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi => new DeXuatNhuCauKinhPhiDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatNhuCauKinhPhiNamDuyetCommand(request.Id),
            PheDuyetEntityNames.PheDuyetKhaoSat => new ToTrinhPheDuyetDuyetCommand(request.Id, PheDuyetEntityNames.PheDuyetKhaoSat),
            PheDuyetEntityNames.ThuyetMinhDuAn => new ThuyetMinhDuAnDuyetCommand(request.Id),
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau => new ToTrinhKetQuaGoiThauDuyetCommand(request.Id),
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau => new ToTrinhThamDinhNhaThauDuyetCommand(request.Id),
            PheDuyetEntityNames.TrienKhaiKeHoachLCNT => new TrienKhaiKeHoachLCNTDuyetCommand(request.Id),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
