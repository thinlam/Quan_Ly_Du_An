using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;
using QLDA.Application.ChuTruongLapKeHoachs.Commands;
using QLDA.Application.ChuTruongLapKeHoachs.Commands;
using QLDA.Application.Common;
using QLDA.Application.DeXuatChuTruongMois.Commands;
using QLDA.Application.DeXuatChuyenTieps.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;
using QLDA.Application.DeXuatNhuCauKinhPhis.Commands;
using QLDA.Application.DuToanDauTus.Commands;
using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;
using QLDA.Application.KeHoachTrienKhaiHangMucs.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PheDuyetDuToans.Commands;
using QLDA.Application.QuyetDinhDieuChinhs.Commands;
using QLDA.Application.ThoaThuanGiaoViecs.Commands;
using QLDA.Application.ThuyetMinhDuAns.Commands;
using QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;
using QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Dispatch duyet phe duyet theo type → den dung entity command
/// </summary>
public record PheDuyetDispatchChuyenCommand(string Type, Guid Id, string? NoiDung) : IRequest<int>;

internal class PheDuyetDispatchChuyenCommandHandler : IRequestHandler<PheDuyetDispatchChuyenCommand, int> {
    private readonly IMediator _mediator;

    public PheDuyetDispatchChuyenCommandHandler(IServiceProvider serviceProvider) {
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task<int> Handle(PheDuyetDispatchChuyenCommand request, CancellationToken cancellationToken) {
        IRequest<int> command = request.Type switch {
            
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon => new KeHoachLuaChonNhaThauRutGonChuyenCommand(request.Id),
            PheDuyetEntityNames.ThoaThuanGiaoViec => new ThoaThuanGiaoViecChuyenCommand(request.Id),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
