using QLDA.Application.Authorization;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.Commands;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;
using QLDA.Application.PhanKhaiKinhPhis.Commands;
using QLDA.Application.PheDuyetDuToans.Commands;
using QLDA.Application.QuyetDinhDieuChinhs.Commands;
using QLDA.Application.ThoaThuanGiaoViecs.Commands;
using QLDA.Application.ToTrinhPheDuyets.Commands;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Commands;

/// <summary>
/// Dispatch tu choi phe duyet theo type → den dung entity command.
/// Phân quyền ở tầng dispatch: chỉ Lãnh đạo phụ trách chính của DuAn hoặc role QLDA_LDDV mới được từ chối.
/// </summary>
public record PheDuyetDispatchTuChoiCommand(string Type, Guid Id, string NoiDung) : IRequest<int>;

internal class PheDuyetDispatchTuChoiCommandHandler(IServiceProvider serviceProvider) : IRequestHandler<PheDuyetDispatchTuChoiCommand, int> {
    private readonly IMediator _mediator = serviceProvider.GetRequiredService<IMediator>();
    private readonly IAuthorizationManager _auth = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<int> Handle(PheDuyetDispatchTuChoiCommand request, CancellationToken cancellationToken) {

        var duAnId = await PheDuyetDispatchHelper.GetDuAnIdAsync(serviceProvider, request.Type, request.Id, cancellationToken);
        await _auth.EnsureCanApproveDuAnAsync(duAnId ?? Guid.Empty, cancellationToken);

        IRequest<int> command = request.Type switch {
            PheDuyetEntityNames.PheDuyetDuToan => new PheDuyetDuToanTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.HoSoMoiThauDienTu => new HoSoMoiThauDienTuTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PhanKhaiKinhPhi => new PhanKhaiKinhPhiTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.QuyetDinhDieuChinh => new QuyetDinhDieuChinhTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.PheDuyetKhaoSat => new ToTrinhPheDuyetTrinhCommand(request.Id, PheDuyetEntityNames.PheDuyetKhaoSat, request.NoiDung),
            PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam => new DeXuatKinhPhiNamTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon => new KeHoachLuaChonNhaThauRutGonTuChoiCommand(request.Id, request.NoiDung),
            PheDuyetEntityNames.ThoaThuanGiaoViec => new ThoaThuanGiaoViecTuChoiCommand(request.Id, request.NoiDung),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
