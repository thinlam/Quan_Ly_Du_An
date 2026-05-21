using System.Data;

namespace QLDA.Application.KySos.Commands;

public record NoiDungDaKyInsertCommand : IRequest {
    public required Guid TepDinhKemId { get; set; }
    public string? FileName { get; set; }
    public string? FileOrginal { get; set; }
    public string? GroupId { get; set; }
    public string? GroupName { get; set; }
}

internal class NoiDungDaKyInsertCommandHandler : IRequestHandler<NoiDungDaKyInsertCommand> {
    private readonly IRepository<NoiDungDaKySo, Guid> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NoiDungDaKyInsertCommandHandler(IServiceProvider serviceProvider) {
        _repository = serviceProvider.GetRequiredService<IRepository<NoiDungDaKySo, Guid>>();
        _unitOfWork = _repository.UnitOfWork;
    }

    public async Task Handle(NoiDungDaKyInsertCommand request, CancellationToken cancellationToken = default) {
        var entity = new NoiDungDaKySo {
            TepDinhKemId = request.TepDinhKemId,
            FileName     = request.FileName,
            FileOrginal  = request.FileOrginal,
            GroupId      = request.GroupId,
            GroupName    = request.GroupName,
        };

        using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);
    }
}
