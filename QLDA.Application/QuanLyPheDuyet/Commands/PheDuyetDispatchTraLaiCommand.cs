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
/// Dispatch tra lai phe duyet theo type → den dung entity command
/// </summary>
public record PheDuyetDispatchTraLaiCommand(string Type, Guid Id, string NoiDung) : IRequest<int>;

internal class PheDuyetDispatchTraLaiCommandHandler : IRequestHandler<PheDuyetDispatchTraLaiCommand, int> {
    private readonly IMediator _mediator;

    public PheDuyetDispatchTraLaiCommandHandler(IServiceProvider serviceProvider) {
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public async Task<int> Handle(PheDuyetDispatchTraLaiCommand request, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(request.NoiDung)) {
            throw new ManagedException("Lý do trả lại là bắt buộc");
        }

        IRequest<int> command = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => new PheDuyetDuToanTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoDeXuatCapDoCntt => new HoSoDeXuatCapDoCnttTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatChuTruongMoi => new DeXuatChuTruongMoiTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep => new DeXuatChuyenTiepTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi => new DeXuatNhuCauKinhPhiTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatNhuCauKinhPhiNamTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ThuyetMinhDuAn => new ThuyetMinhDuAnTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau => new ToTrinhKetQuaGoiThauTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau => new ToTrinhThamDinhNhaThauTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.TrienKhaiKeHoachLCNT => new TrienKhaiKeHoachLCNTTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc => new KeHoachTrienKhaiHangMucTraLaiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.DuToanDauTu => new DuToanDauTuTraLaiCommand(request.Id, request.NoiDung),
            //simple ToTrinhPheDuyet
            PheDuyetEntityNames.PheDuyetKhaoSat => new ToTrinhPheDuyetTraLaiCommand(request.Id, PheDuyetEntityNames.PheDuyetKhaoSat, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhKeHoachThue => new ToTrinhPheDuyetTraLaiCommand(request.Id, PheDuyetEntityNames.QuyetDinhKeHoachThue, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDuyetDuToan => new ToTrinhPheDuyetTraLaiCommand(request.Id, PheDuyetEntityNames.QuyetDinhDuyetDuToan, request.NoiDung),
            PheDuyetEntityNames.ToTrinhKeHoach => new ToTrinhPheDuyetTraLaiCommand(request.Id, PheDuyetEntityNames.ToTrinhKeHoach, request.NoiDung),

            PheDuyetEntityNames.ChuTruongLapKeHoach => new ChuTruongLapKeHoachTraLaiCommand(request.Id, PheDuyetEntityNames.ChuTruongLapKeHoach),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
