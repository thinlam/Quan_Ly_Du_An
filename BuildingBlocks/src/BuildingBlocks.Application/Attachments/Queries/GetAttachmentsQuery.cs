using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Application.Attachments.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application.Attachments.Queries;

/// <summary>
/// Load attachments theo GroupIds.
/// BaseGroupTypes: base GroupType (có thể đã có prefix KySo_).
/// IncludeSigned=true (mặc định): tự thêm KySo_ variant nếu base chưa có prefix.
/// </summary>
public record GetAttachmentsQuery(
    List<string> GroupIds,
    List<string>? BaseGroupTypes = null,
    bool IncludeSigned = true) : IRequest<List<AttachmentDto>>;

public class GetAttachmentsQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<GetAttachmentsQuery, List<AttachmentDto>>
{
    private readonly IRepository<Attachment, Guid> _repository =
        serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    public async Task<List<AttachmentDto>> Handle(
        GetAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        var groupIds = NormalizeGroupIds(request.GroupIds);
        if (groupIds.Count == 0)
            throw new ManagedException("GroupId không được rỗng.");

        IQueryable<Attachment> query = _repository.GetQueryableSet()
            .Where(a => groupIds.Contains(a.GroupId));

        if (request.BaseGroupTypes is not null)
        {
            var groupTypesToFilter = AttachmentSubquery.ExpandGroupTypes(
                request.BaseGroupTypes,
                request.IncludeSigned);

            if (groupTypesToFilter.Count == 0)
                throw new ManagedException("BaseGroupTypes không được rỗng hoặc chỉ chứa giá trị trống.");

            query = query.Where(a => groupTypesToFilter.Contains(a.GroupType));
        }

        return await query.Select(a => a.ToDto()).ToListAsync(cancellationToken);
    }

    private static List<string> NormalizeGroupIds(List<string>? groupIds)
    {
        if (groupIds is null || groupIds.Count == 0)
            return [];

        return groupIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }
}
