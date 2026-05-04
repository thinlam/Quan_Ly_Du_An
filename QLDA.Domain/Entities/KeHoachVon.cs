using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Kế hoạch vốn của dự án
/// </summary>
public class KeHoachVon : Entity<Guid>, IAggregateRoot {

	/// <summary>
	/// ID dự án (Foreign Key - Required)
	/// </summary>
	public Guid DuAnId { get; set; }

	/// <summary>
	/// ID nguồn vốn (Optional FK to funding source)
	/// </summary>
	public int? NguonVonId { get; set; }

	/// <summary>
	/// Năm kế hoạch
	/// </summary>
	public int Nam { get; set; }

	/// <summary>
	/// Số vốn (decimal 18,2)
	/// </summary>
	public decimal SoVon { get; set; }

	/// <summary>
	/// Số vốn điều chỉnh (decimal 18,2)
	/// </summary>
	public decimal? SoVonDieuChinh { get; set; }

	/// <summary>
	/// Số quyết định (max 100 chars)
	/// </summary>
	public string? SoQuyetDinh { get; set; }

	/// <summary>
	/// Ngày ký quyết định
	/// </summary>
	public DateTimeOffset? NgayKy { get; set; }

	/// <summary>
	/// Ghi chú
	/// </summary>
	public string? GhiChu { get; set; }

	#region Navigation Properties

	/// <summary>
	/// Dự án
	/// </summary>
	public DuAn? DuAn { get; set; }

		/// <summary>
		/// Nguồn vốn
		/// </summary>
		public DanhMucNguonVon? NguonVon { get; set; }

		#endregion
}