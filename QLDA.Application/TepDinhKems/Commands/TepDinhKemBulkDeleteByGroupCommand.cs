namespace QLDA.Application.TepDinhKems.Commands;

public class TepDinhKemBulkDeleteByGroupCommand : IRequest {
    public required List<string> GroupId { get; set; }
    public List<string> EGroupTypes { get; set; } = [];
}

internal class TepDinhKemBulkDeleteByGroupCommandHandler(
    IRepository<Attachment, Guid> tepDinhKemRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<TepDinhKemBulkDeleteByGroupCommand> {
    public async Task Handle(TepDinhKemBulkDeleteByGroupCommand request,
        CancellationToken cancellationToken = default) {
        using (await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted,
                   cancellationToken)) {
            //Lấy các file theo groupId và groupType hoặc các file bị lỗi
            var danhSachTepDinhKem = tepDinhKemRepository.GetQueryableSet()
                    .WhereIf(request.EGroupTypes.Any(),
                        o => request.GroupId.Contains(o.GroupId) && request.EGroupTypes.Contains(o.GroupType),
                        o => request.GroupId.Contains(o.GroupId))
                ;

            tepDinhKemRepository.BulkDelete(danhSachTepDinhKem);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
}