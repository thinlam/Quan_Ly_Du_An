namespace QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;

internal static class NguoiDungMacDinhTheoPhongMappings
{
    public static IQueryable<NguoiDungMacDinhTheoPhongDto> SelectDto(
        this IQueryable<NguoiDungMacDinhTheoPhong> query,
        IQueryable<DmDonVi> dmDonVi,
        IQueryable<UserMaster> userMaster)
    {
        return from cfg in query
            join pb in dmDonVi on cfg.PhongBanId equals pb.Id into pbJoin
            from pb in pbJoin.DefaultIfEmpty()
            join nd in userMaster on cfg.NguoiDungId equals nd.Id into ndJoin
            from nd in ndJoin.DefaultIfEmpty()
            select new NguoiDungMacDinhTheoPhongDto
            {
                Id = cfg.Id,
                PhongBanId = cfg.PhongBanId,
                TenPhongBan = pb.TenDonVi,
                NguoiDungId = cfg.NguoiDungId,
                TenNguoiDung = nd.HoTen
            };
    }
}
