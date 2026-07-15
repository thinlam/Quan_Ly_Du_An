namespace QLDA.Application.DuAnCongViecs.DTOs;

public static class DuAnCongViecMappings {
    public static DuAnCongViec ToEntity(this DuAnCongViecInsertDto dto, long? nguoiTaoId) {
        return new DuAnCongViec {
            LeftId = dto.DuAnId,
            RightId = dto.CongViecId,
            IsDeleted = false,
            IsHoanThanh = false,
            NguoiPhuTrachChinhId = dto.NguoiPhuTrachChinhId,
            NguoiTaoId = nguoiTaoId
        };
    }

    public static void Update(this DuAnCongViec entity, DuAnCongViecUpdateDto dto) {
        entity.IsHoanThanh = dto.IsHoanThanh;
        entity.NguoiPhuTrachChinhId = dto.NguoiPhuTrachChinhId;
    }
}