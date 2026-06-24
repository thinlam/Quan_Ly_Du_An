namespace QLDA.WebApi.Models.PhanQuyenChucNangs
{
    public class PhanQuyenChucNangModel
    {
        public int? Id { get; set; }
       // public int QuyenId { get; set; }//chức năng : tạo mới/sửa/xóa
        public string ChucNang { get; set; }//chức năng : tạo mới/sửa/xóa
        public string MaChucNang { get; set; }//chức năng : tạo mới/sửa/xóa
        public bool SuDung { get; set; }
        //public string? ChucNang { get; set; }
        //  public long? LevelId { get; set; }
        public int? Level { get; set; } // phương thức phòng ban/vai trò/ng dùng
        public long? LevelId { get; set; } // phương thức phòng ban/vai trò/ng dùng
        public List<long>? DanhSachNguoiDung { get; set; } // đối tượng
        public bool? NguoiDungMacDinh { get; set; } // đối tượng
    }
}
