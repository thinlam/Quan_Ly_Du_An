using BuildingBlocks.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.TepDinhKems.Commands;

public record TepDinhKemBulkInsertOrUpdateCommand() : IRequest
{
    public required string GroupId { get; set; }
    public required List<TepDinhKem> Entities { get; set; }
    /// <summary>
    /// Phạm vi sync theo GroupType (cùng GroupId có thể có nhiều loại tệp).
    /// Bắt buộc khi <see cref="Entities"/> rỗng nhưng cần xóa mềm theo loại; nếu null sẽ suy ra từ Entities.
    /// </summary>
    public List<string>? GroupTypes { get; set; }
    public bool KySo { get; set; }
}

internal class TepDinhKemBulkInsertOrUpdateCommandHandler(IRepository<TepDinhKem, Guid> repository, IUnitOfWork unitOfWork,
    ILogger<TepDinhKemBulkInsertOrUpdateCommandHandler> logger) : IRequestHandler<TepDinhKemBulkInsertOrUpdateCommand>
{
    private readonly IRepository<TepDinhKem, Guid> _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<TepDinhKemBulkInsertOrUpdateCommandHandler> _logger = logger;

    public async Task Handle(TepDinhKemBulkInsertOrUpdateCommand request,
        CancellationToken cancellationToken = default)
    {

        if (_unitOfWork.HasTransaction)
        {
            await InsertOrUpdateAsync(request, cancellationToken);
        }
        else
        {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            await InsertOrUpdateAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
    }
    #region Private helper methods
    /// <summary>
    /// Xoá mềm
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task InsertOrUpdateAsync(TepDinhKemBulkInsertOrUpdateCommand request, CancellationToken cancellationToken = default)
    {
        request.Entities ??= [];
        var groupTypes = ResolveGroupTypes(request);
        if (request.Entities.Count == 0 && groupTypes.Count == 0)
            return;

        var filesQuery = _repository.GetQueryableSet().Where(e => e.GroupId == request.GroupId);
        if (groupTypes.Count > 0)
            filesQuery = filesQuery.Where(e => groupTypes.Contains(e.GroupType));

        var files = await filesQuery.ToListAsync(cancellationToken);

        await SyncHelper.SyncCollection(repository: _repository, existingEntities: files, requestEntities: request.Entities, (existing, request) =>
        {
            existing.Type = request.Type;
            existing.FileName = request.FileName;
            existing.OriginalName = request.OriginalName;
            existing.Path = request.Path;
            existing.Size = request.Size;
        }, cancellationToken: cancellationToken);
    }

    private static List<string> ResolveGroupTypes(TepDinhKemBulkInsertOrUpdateCommand request)
    {
        if (request.GroupTypes is { Count: > 0 })
            return request.GroupTypes;

        return request.Entities
            .Select(e => e.GroupType)
            .Where(t => !string.IsNullOrEmpty(t))
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Xoá cứng
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task InsertOrUpdateCascadeAsync(TepDinhKemBulkInsertOrUpdateCommand request, CancellationToken cancellationToken = default)
    {
        /*
         * Khi Update nếu ids trong request đã tồn tại trong db => toUpdate
         * nếu ids không tồn tại trong database => thêm mới
         * nếu ids tồn tại trong database => cập nhật
         * nếu ids trong database không tồn tại trong request => xóa
         *
         * Trường hợp ký số là file được tạo từ KySoController có GroupType.KySo
         * nên khi CRUD thì phải tách trường hợp của KySo ra khỏi logic
         */

        #region Không có file - xóa hết

        // Nếu danh sách trống và không phải api ký số thì xóa hết
        request.Entities ??= [];
        if (request.Entities.Count == 0)
        {
            await _repository.GetQueryableSet()
                .Where(e => e.GroupId == request.GroupId)
                .ExecuteDeleteAsync(cancellationToken);
            return;
        }

        #endregion

        #region Có file

        //danh sách id từ request
        var requestIds = request.Entities.Select(e => e.Id).ToList();

        //danh sách đã lưu từ trước - tồn tại trong db và request
        var existing = await _repository.GetOrderedSet()
            .Where(e => e.GroupId == request.GroupId)
            .ToListAsync(cancellationToken);
        ;
        var existingIds = existing.Select(e => e.Id).ToList();

        //chưa có trong db
        var toAdd = request.Entities
            .Where(e => !existingIds.Contains(e.Id)).ToList();

        //có trong db
        var toUpdate = request.Entities
            .Where(e => existingIds.Contains(e.Id)).ToList();

        //không có trong db và cả request
        var toRemove = existing
            .Where(e => !requestIds.Contains(e.Id))
            .ToList();

        if (toAdd.Count == 0 && toUpdate.Count == 0 && toRemove.Count == 0) return;
        if (toAdd.Count != 0)
        {
            _repository.BulkInsert(toAdd);
        }

        if (toUpdate.Count != 0)
        {
            _repository.BulkUpdate(toUpdate, x => new
            {
                x.Type,
                x.FileName,
                x.OriginalName,
                x.Path,
                x.Size
            });
        }

        //Trường hợp đang ký số thì request chỉ gửi file ký số thôi nếu không check !request.KySo thì nó sẽ xóa các file gốc
        if (toRemove.Count != 0 && !request.KySo)
            _repository.BulkDelete(toRemove);
        #endregion
    }
    #endregion
}
