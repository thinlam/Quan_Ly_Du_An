namespace QLDA.Application.HoSoMoiThauDienTus.DTOs
{
    public class HoSoMoiThauDienTuSearchDto
    {
        public string? GlobalFilter { get; set; }

        public Guid? DuAnId { get; set; }

        public Guid? GoiThauId { get; set; }
    }
}