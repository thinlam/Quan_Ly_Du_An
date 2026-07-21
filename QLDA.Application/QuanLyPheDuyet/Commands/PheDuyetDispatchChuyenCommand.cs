using QLDA.Application.KeHoachLuaChonNhaThauRutGons.Commands;
using QLDA.Application.ThoaThuanGiaoViecs.Commands;
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
            
            PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon => new KeHoachLuaChonNhaThauRutGonChuyenCommand(request.Id, PheDuyetEntityNames.KeHoachLuaChonNhaThauRutGon),
            PheDuyetEntityNames.ThoaThuanGiaoViec => new ThoaThuanGiaoViecChuyenCommand(request.Id),

            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
