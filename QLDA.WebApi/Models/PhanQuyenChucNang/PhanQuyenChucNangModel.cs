namespace QLDA.WebApi.Models.PhanQuyenChucNangs
{
    public class PhanQuyenChucNangModel
    {
        public int? Id { get; set; }
       // public int QuyenId { get; set; }//chức năng : tạo mới/sửa/xóa
        public string? ChucNang { get; set; }//chức năng : tạo mới/sửa/xóa
        public string? MaChucNang { get; set; }//chức năng : tạo mới/sửa/xóa
        public bool SuDung { get; set; }
        public int Level { get; set; } // phương thức phòng ban/vai trò/ng dùng
       // public List<long>? DanhSachNguoiDung { get; set; } // đối tượng
        public List<PhanQuyenChucNangCapDoModel>? DanhSachChiTiet { get; set; } = new(); // đối tượng
        //public string? ChucNang { get; set; }
        //  public long? LevelId { get; set; }
        // public long? LevelId { get; set; } // phương thức phòng ban/vai trò/ng dùng
        // public bool? NguoiDungMacDinh { get; set; } // đối tượng
    }
}
public class PhanQuyenChucNangCapDoModel
{
    public long LevelId { get; set; }//PhongBanId,ChucVuid,User_porttalId
    public bool? NguoiDungMacDinh { get; set; } // nếu là phòng ban thì có thẻ chọn ng dùng măc định
    public List<long>? NguoiDungChiDinhs { get; set; } 
}
