namespace QLDA.Application.HoSoMoiThauDienTus.DTOs
{
    public class HoSoMoiThauDienTuSearchDto
    {
        public string? GlobalFilter { get; set; }

        public Guid? DuAnId { get; set; }

        public Guid? GoiThauId { get; set; }
        /// <summary>
        /// Loại dự án theo năm - tài chính
        /// </summary>
        /// <remarks>PMIS #9609</remarks>
        public int? LoaiDuAnTheoNamId { get; set; }
    }
}