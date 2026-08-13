using System.Data;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.DanhMucBuocs.DTOs;
using QLDA.Application.DuAnBuocs;
using QLDA.Application.DuAnBuocs.Extensions;

namespace QLDA.Application.DuAns.Commands;

/// <summary>
/// Command cập nhật bước hiện tại của dự án theo cấu trúc Materialized Path.
/// Chỉ cập nhật nếu bước mới nằm sau bước hiện tại trong cây thứ tự.
/// </summary>
/// <remarks>
/// Cập nhật bước hiện tại của dự án
/// 1.Lấy tất các bước của dự án <br/>
/// 2.Tổ chức lại cây dựa vào ParentId và Stt (làm phẳng - Flatten) <br/>
/// 3. So sánh vị trí bước hiện tại và bước mới trong thứ tự đã flatten <br/>
/// </remarks>
public record DuAnUpdateStepCommand(Guid DuAnId, int? BuocId) : IRequest<DuAnBuoc?>;

internal class DuAnUpdateStepCommandHandler
    : IRequestHandler<DuAnUpdateStepCommand, DuAnBuoc?> {
    private readonly IRepository<DuAn, Guid> DuAn;
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc;
    private readonly IUnitOfWork _unitOfWork;

    public DuAnUpdateStepCommandHandler(IServiceProvider serviceProvider) {
        DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        DuAnBuoc = serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
        _unitOfWork = DuAn.UnitOfWork;
    }

    public async Task<DuAnBuoc?> Handle(DuAnUpdateStepCommand request, CancellationToken cancellationToken) {
        if (request.BuocId is null or 0)
            return null;

        // Validate tồn tại dự án và bước
        await ValidateAsync(request, cancellationToken);

        if (_unitOfWork.HasTransaction) {
            return await UpdateCurrentStepAsync(request, cancellationToken);
        } else {
            using var tx = await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken); await UpdateCurrentStepAsync(request, cancellationToken);
            var buocMoi = await UpdateCurrentStepAsync(request, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return buocMoi;
        }


    }
    #region Private helper methods
    private async Task ValidateAsync(DuAnUpdateStepCommand request, CancellationToken cancellationToken) {
        ManagedException.ThrowIf(
            when: !await DuAn.GetQueryableSet().AnyAsync(e => e.Id == request.DuAnId, cancellationToken),
            message: "Không tồn tại dự án");
        ManagedException.ThrowIf(
           when: !await DuAnBuoc.GetQueryableSet().AnyAsync(e => e.Id == request.BuocId, cancellationToken),
            message: "Không tồn tại bước");
    }
    private async Task<DuAnBuoc?> UpdateCurrentStepAsync(DuAnUpdateStepCommand request, CancellationToken cancellationToken) {
        var buocHienTai = await DuAn.GetQueryableSet()
                .Include(e => e.BuocHienTai!.Buoc!.GiaiDoan)
                .Where(e => e.Id == request.DuAnId)
                .Select(e => e.BuocHienTai)
                .FirstOrDefaultAsync(cancellationToken);

        // Nếu dự án chưa có bước hiện tại => gán luôn bước mới
        if (buocHienTai == null) {
            await SetStep(request, cancellationToken);
            return null;
        } else {
            var buocHienTaiMapping = buocHienTai.ToDto();
            // Lấy tất cả các bước của dự án để build lại cây
            var all = await DuAnBuoc.GetQueryableSet().AsNoTracking()
                .Include(e => e.Buoc!.GiaiDoan)
                .Where(e => e.DuAnId == request.DuAnId)
                .ToListAsync(cancellationToken);

            var orderedSteps = all.ToSteps().ToTreeList();// 28 rows

            var latestStep = orderedSteps.First(e => e.Id == request.BuocId);
          
            // Duyệt cây theo pre-order dựa vào ParentId và Stt
            var lookup = orderedSteps.GroupBy(e => e.ParentId)
                .ToDictionary(g => g.Key, g => g.OrderBy(e => e.Stt).ToList());

            var ordered = new List<(int Id,int? BuocId, int Level, int Stt, string? Path)>();

            void Traverse(int parentId) {
                if (!lookup.TryGetValue(parentId, out var children))
                    return;
                foreach (var node in children) {
                    ordered.Add((node.Id, node.BuocId,node.Level, node.Stt, node.Path));

                    Traverse(node.BuocId??0); // ✅
                }
            }

            Traverse(0);// bắt đầu từ gốc (ParentId = 0)


            // So sánh vị trí bước hiện tại và bước mới trong thứ tự đã flatten
            var currentIndex = ordered.FindIndex( x => x.Id == buocHienTaiMapping.Id);

            var nextIndex = ordered.FindIndex(  x => x.Id == latestStep.Id);

           if (nextIndex > currentIndex) {
              await SetStep(request, cancellationToken);
            }
            return all.First(e => e.Id == request.BuocId);
        }
    }

    /// <summary>
    /// Gán bước hiện tại cho dự án.
    /// </summary>
    private async Task SetStep(DuAnUpdateStepCommand request, CancellationToken cancellationToken = default) {
        await DuAn.GetQueryableSet()
            .Where(e => e.Id == request.DuAnId)
            .ExecuteUpdateAsync(setCall => setCall.SetProperty(e => e.BuocHienTaiId, request.BuocId),
                cancellationToken);
    }

    #endregion

}
