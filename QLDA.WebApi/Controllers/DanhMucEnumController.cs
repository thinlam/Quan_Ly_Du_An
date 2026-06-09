using QLDA.Domain.Constants;
using System.ComponentModel;
using static QLDA.Domain.Constants.LoaiDeXuatLCNTonstants;

namespace QLDA.WebApi.Controllers {
    [Tags("Danh mục enum")]
    [Route("api/danh-muc-enum")]
    public class DanhMucEnumController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
        /// <summary>
        /// Lấy tên các danh mục Enum hiện có
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<List<string>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach-ten")]
        public async Task<ResultApi> GetEnumNames() {
            var result = new List<string> {
                nameof(EDanhMuc),
                nameof(EnumLoaiVanBanQuyetDinh),
                nameof(ETrangThaiMoiThau),
                nameof(EChiuTrachNhiemXuLy),
                nameof(ELoaiDeXuatKeHoachLCNT),
            };
            return await Task.FromResult(ResultApi.Ok(result));
        }

        /// <summary>
        /// Danh mục EDanhMuc là tên tất cả danh mục có thể sử dụng
        /// </summary>
        /// <param name="enumName"></param>
        /// <returns></returns>
        [ProducesResponseType<ResultApi<List<EnumsExtensions.EnumDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [HttpGet("danh-sach")]
        public async Task<ResultApi> ListEnum([DefaultValue("EDanhMuc")] string enumName) {
            List<EnumsExtensions.EnumDto> result = [];
            result = enumName switch {
                nameof(EDanhMuc) => EnumsExtensions.EnumAll<EDanhMuc>(),
                nameof(ELoaiVanBanQuyetDinh) => EnumsExtensions.EnumAll<EnumLoaiVanBanQuyetDinh>(), //nameof(ELoaiVanBanQuyetDinh) table name cho store procedure query - EnumLoaiVanBanQuyetDinh cho code first
                nameof(ETrangThaiMoiThau) => EnumsExtensions.EnumAll<ETrangThaiMoiThau>(),
                nameof(EChiuTrachNhiemXuLy) => EnumsExtensions.EnumAll<EChiuTrachNhiemXuLy>(),
                nameof(ELoaiDeXuatKeHoachLCNT) => EnumsExtensions.EnumAll<LoaiDeXuatLCNTonstants.ELoaiDeXuatKeHoachLCNT>(),
                _ => result
            };
            return await Task.FromResult(ResultApi.Ok(result));
        }
    }
}