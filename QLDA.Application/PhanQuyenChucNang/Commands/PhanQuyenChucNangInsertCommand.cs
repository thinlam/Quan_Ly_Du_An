using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLDA.Application.Authorization;
using QLDA.Application.PhanQuyenChucNangs.Commands;
using QLDA.Domain.Constants;
using QLDA.Domain.Entities;
using System.Data;

namespace QLDA.Application.PhanQuyenChucNangs.Commands;

public record PhanQuyenChucNangInsertUpdateCommand(PhanQuyenChucNang Entity) :  IRequest<int> {
}
    internal class PhanQuyenChucNangInsertUpdateCommandHandler
    : IRequestHandler<PhanQuyenChucNangInsertUpdateCommand, int>
{
    private readonly IRepository<PhanQuyenChucNang, int> _PhanQuyenChucNang;
  
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PhanQuyenChucNangInsertUpdateCommandHandler> _logger;

    public PhanQuyenChucNangInsertUpdateCommandHandler(IServiceProvider serviceProvider,
        ILogger<PhanQuyenChucNangInsertUpdateCommandHandler> logger) {
        _PhanQuyenChucNang = serviceProvider.GetRequiredService<IRepository<PhanQuyenChucNang, int>>();
      
        _logger = logger;
        _unitOfWork = _PhanQuyenChucNang.UnitOfWork;
    }

    public async Task<int> Handle(PhanQuyenChucNangInsertUpdateCommand request,
        CancellationToken cancellationToken = default) {
        try {
            using (await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken)) {
                if (request.Entity.Id>0) {
                    await _PhanQuyenChucNang.UpdateAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                } else {
                    await _PhanQuyenChucNang.AddAsync(request.Entity, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return request.Entity.Id;
            }
        } catch (Exception ex) {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

   
}