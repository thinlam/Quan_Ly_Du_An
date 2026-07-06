using QLDA.Application.BaoCaoKetQuaKhaoSats.Commands;
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
using QLDA.Application.QuyetDinhLapBanQLDAs.Commands;
using QLDA.Application.ThanhLyHopDongs.Commands;
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
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatChuTruongMoi => new DeXuatChuTruongMoiTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep => new DeXuatChuyenTiepTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi => new DeXuatNhuCauKinhPhiTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatNhuCauKinhPhiNamTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ThuyetMinhDuAn => new ThuyetMinhDuAnTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau => new ToTrinhKetQuaGoiThauTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau => new ToTrinhThamDinhNhaThauTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.TrienKhaiKeHoachLCNT => new TrienKhaiKeHoachLCNTTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc => new KeHoachTrienKhaiHangMucTrinhCommand(request.Id),
            PheDuyetEntityNames.DuToanDauTu => new DuToanDauTuTrinhCommand(request.Id),
            PheDuyetEntityNames.ChuTruongLapKeHoach => new ChuTruongLapKeHoachTrinhCommand(request.Id, PheDuyetEntityNames.ChuTruongLapKeHoach),

            //simple ToTrinhPheDuyet
            PheDuyetEntityNames.ToTrinhKeHoach => new ToTrinhPheDuyetTrinhCommand(request.Id, PheDuyetEntityNames.ToTrinhKeHoach),
            PheDuyetEntityNames.PheDuyetKhaoSat => new ToTrinhPheDuyetTrinhCommand(request.Id, PheDuyetEntityNames.PheDuyetKhaoSat, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDuyetDuToan => new ToTrinhPheDuyetTrinhCommand(request.Id, PheDuyetEntityNames.QuyetDinhDuyetDuToan, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhKeHoachThue => new ToTrinhPheDuyetTrinhCommand(request.Id, PheDuyetEntityNames.QuyetDinhKeHoachThue, request.NoiDung),
           // chỉ trình k cần duyệt
            PheDuyetEntityNames.KHLCNTDuToanSanCo => new ToTrinhKhongDuyetCommand(request.Id, PheDuyetEntityNames.KHLCNTDuToanSanCo, request.NoiDung),
            PheDuyetEntityNames.KHLCNTDuToanYeuCauRieng => new ToTrinhKhongDuyetCommand(request.Id, PheDuyetEntityNames.KHLCNTDuToanYeuCauRieng, request.NoiDung),
            PheDuyetEntityNames.KeHoachTongTheLCNT => new ToTrinhKhongDuyetCommand(request.Id, PheDuyetEntityNames.KeHoachTongTheLCNT, request.NoiDung),
            PheDuyetEntityNames.KeHoachLCNTChuanBiDauTu => new ToTrinhKhongDuyetCommand(request.Id, PheDuyetEntityNames.KeHoachLCNTChuanBiDauTu, request.NoiDung),
          
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon => new KeHoachLuaChonNhaThauRutGonTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ThoaThuanGiaoViec => new ThoaThuanGiaoViecTrinhCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhLapBanQLDA => new QuyetDinhLapBanQldaTrinhCommand(request.Id, request.NoiDung),

            PheDuyetEntityNames.ThanhLyHopDong => new ThanhLyHopDongTrinhCommand(request.Id, request.NoiDung),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
