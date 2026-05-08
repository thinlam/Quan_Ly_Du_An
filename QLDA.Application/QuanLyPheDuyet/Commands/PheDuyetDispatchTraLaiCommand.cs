using QLDA.Application.Common;
using QLDA.Application.PheDuyetDuToans.Commands;
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
            _ => throw new ManagedException($"Loại phê duyệt '{request.Type}' không hợp lệ")
        };
        return await _mediator.Send(command, cancellationToken);
    }
}
