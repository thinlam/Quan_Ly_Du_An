using QLDA.Application.Common;
using QLDA.Application.HoSoDeXuatCapDoCntts.Commands;
using QLDA.Application.HoSoMoiThauDienTus.Commands;
using QLDA.Application.PheDuyetDuToans.Commands;
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
            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
