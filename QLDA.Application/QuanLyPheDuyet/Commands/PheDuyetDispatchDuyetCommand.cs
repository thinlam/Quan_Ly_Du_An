using QLDA.Application.Authorization;
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
using QLDA.Application.QuyetDinhLapBanQLDAs.Commands;
using QLDA.Application.ThanhLyHopDongs.Commands;
using QLDA.Application.ThoaThuanGiaoViecs.Commands;
using QLDA.Application.ThuyetMinhDuAns.Commands;
using QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Application.ToTrinhThamDinhNhaThaus.Commands;
using QLDA.Application.TrienKhaiKeHoachLCNTs.Commands;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Dispatch duyet phe duyet theo type → den dung entity command.
/// Phân quyền ở tầng dispatch: chỉ Lãnh đạo phụ trách chính của DuAn hoặc role QLDA_LDDV mới được duyệt.
/// </summary>
public record PheDuyetDispatchDuyetCommand(string Type, Guid Id, string? NoiDung) : IRequest<int>;

internal class PheDuyetDispatchDuyetCommandHandler(IServiceProvider serviceProvider) : IRequestHandler<PheDuyetDispatchDuyetCommand, int> {
    private readonly IMediator _mediator = serviceProvider.GetRequiredService<IMediator>();
    private readonly IAuthorizationManager _auth = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<int> Handle(PheDuyetDispatchDuyetCommand request, CancellationToken cancellationToken) {
        var duAnId = await PheDuyetDispatchHelper.GetDuAnIdAsync(serviceProvider, request.Type, request.Id, cancellationToken);
        await _auth.EnsureCanApproveDuAnAsync(duAnId ?? Guid.Empty, cancellationToken);

        IRequest<int> command = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => new PheDuyetDuToanDuyetCommand(request.Id),
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuDuyetCommand(request.Id),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiDuyetCommand(request.Id),
            PheDuyetEntityNames.BaoCaoKetQuaKhaoSat => new BaoCaoKetQuaKhaoSatDuyetCommand(request.Id),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatChuTruongMoi => new DeXuatChuTruongMoiDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatChuTruongChuyenTiep => new DeXuatChuyenTiepDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhi => new DeXuatNhuCauKinhPhiDuyetCommand(request.Id),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatNhuCauKinhPhiNamDuyetCommand(request.Id),
            PheDuyetEntityNames.ThuyetMinhDuAn => new ThuyetMinhDuAnDuyetCommand(request.Id),
            PheDuyetEntityNames.ToTrinhKetQuaGoiThau => new ToTrinhKetQuaGoiThauDuyetCommand(request.Id),
            PheDuyetEntityNames.ToTrinhThamDinhNhaThau => new ToTrinhThamDinhNhaThauDuyetCommand(request.Id),
            PheDuyetEntityNames.TrienKhaiKeHoachLCNT => new TrienKhaiKeHoachLCNTDuyetCommand(request.Id),
            PheDuyetEntityNames.KeHoachTrienKhaiHangMuc => new KeHoachTrienKhaiHangMucDuyetCommand(request.Id),
            PheDuyetEntityNames.DuToanDauTu => new DuToanDauTuDuyetCommand(request.Id),

            PheDuyetEntityNames.PheDuyetKhaoSat => new ToTrinhPheDuyetDuyetCommand(request.Id, PheDuyetEntityNames.PheDuyetKhaoSat),
            PheDuyetEntityNames.ChuTruongLapKeHoach => new ChuTruongLapKeHoachDuyetCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon => new KeHoachLuaChonNhaThauRutGonDuyetCommand(request.Id),
            PheDuyetEntityNames.ThoaThuanGiaoViec => new ThoaThuanGiaoViecDuyetCommand(request.Id),

            PheDuyetEntityNames.QuyetDinhKeHoachThue => new ToTrinhPheDuyetDuyetCommand(request.Id, PheDuyetEntityNames.QuyetDinhKeHoachThue),
            PheDuyetEntityNames.ToTrinhKeHoach => new ToTrinhPheDuyetDuyetCommand(request.Id, PheDuyetEntityNames.ToTrinhKeHoach),
            PheDuyetEntityNames.QuyetDinhDuyetDuToan => new ToTrinhPheDuyetDuyetCommand(request.Id, PheDuyetEntityNames.QuyetDinhDuyetDuToan),
            PheDuyetEntityNames.QuyetDinhLapBanQLDA => new QuyetDinhLapBanQldaDuyetCommand(request.Id),

            PheDuyetEntityNames.ThanhLyHopDong => new ThanhLyHopDongDuyetCommand(request.Id),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}