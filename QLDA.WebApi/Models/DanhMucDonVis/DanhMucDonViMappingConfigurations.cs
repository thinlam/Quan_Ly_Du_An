using BuildingBlocks.Domain.Entities;

namespace QLDA.WebApi.Models.DanhMucDonVis;

public static class DanhMucDonViMappingConfigurations {
    public static DanhMucDonViModel ToModel(this DmDonVi entity)
        => new() {
            Id = entity.Id,
            Ma = entity.MaDonVi,
            Ten = entity.TenDonVi,
            MoTa = entity.MoTa,
            Used = entity.Used ?? false,
        };
}