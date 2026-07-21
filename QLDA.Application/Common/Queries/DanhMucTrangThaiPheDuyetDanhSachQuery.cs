using Microsoft.EntityFrameworkCore;

namespace QLDA.Application.Common;

public static class DanhMucTrangThaiPheDuyetExtensions
{
    /// <summary>
    /// Hàm dùng chung để lấy thông tin trạng thái theo Loai và Ma
    /// </summary>
   

    /// <summary>
    /// Hàm lấy danh sách nhiều trạng thái theo Loai (Phục vụ cho file TrinhCommand cần lấy List/Dict)
    /// </summary>
    public static async Task<List<DanhMucTrangThaiPheDuyet>> GetByLoaiAsync(
        this IRepository<DanhMucTrangThaiPheDuyet, int> statusRepo,
        string loai,
        CancellationToken cancellationToken = default)
    {
        return await statusRepo.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .Where(s => s.Loai == loai)
            .ToListAsync(cancellationToken);
    }
}