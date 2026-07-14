using System.Data;
using System.Net.Mime;
using QLDA.Application.Common;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.DuAnBuocs.Commands;
using QLDA.Application.DuAns.Commands;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.DuAns.Queries;
using QLDA.Application.DuToans.Commands;
using QLDA.Application.DuToans.DTOs;
using QLDA.Application.KeHoachVons.DTOs;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DuAns;

namespace QLDA.WebApi.Controllers
{
    [Tags("Dự án")]
    public class DuAnController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
    {
        /// <summary>
        /// Chi tiết
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("api/du-an/{id}/chi-tiet")]
        [ProducesResponseType<ResultApi<DuAnDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> Get(Guid id)
        {
            return ResultApi.Ok(await GetDuAnWithFiles(id, default));
        }

        /// <summary>
        /// Xóa tạm
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpDelete("api/du-an/{id}/xoa-tam")]
        [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> SoftDelete(Guid id)
        {
            await Mediator.Send(new DuAnDeleteCommand(id));

            return ResultApi.Ok(1);
        }

        /// <summary>
        /// Danh sách phân trang dự án
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="searchDto"></param>
        /// <returns></returns>
        [HttpGet("api/du-an/danh-sach")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType<ResultApi<PaginatedList<DuAnDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> Get([FromQuery] DuAnSearchDto searchDto)
        {
            var res = await Mediator.Send(new DuAnGetDanhSachQuery(searchDto));
            return ResultApi.Ok(res);
        }

        /// <summary>
        /// Báo cáo dự toán dự án
        /// </summary>
        /// TongHopVonGiaiNganQuery(int Nam, int LoaiDuAnId)
        /// 
        [HttpGet("api/du-an/von-giai-ngan")]
        [ProducesResponseType<ResultApi<List<BaoCaoDuAnDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetTongHopVonGiaiNgan([FromQuery] int Nam)
        {
            var res = await Mediator.Send(new TongHopVonGiaiNganQuery(Nam, 0));
            return ResultApi.Ok(res);
        }

        [HttpGet("api/du-an/bao-cao-du-toan")]
        [ProducesResponseType<ResultApi<PaginatedList<BaoCaoDuAnDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetBaoCaoDuToan([FromQuery] BaoCaoDuAnSearchDto searchDto)
        {
            var res = await Mediator.Send(new BaoCaoDuAnGetDanhSachQuery(searchDto));
            return ResultApi.Ok(res);
        }

        [ResponseCache(CacheProfileName = "Combobox")]
        [HttpGet("api/du-an/danh-sach-combobox")]
        [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Get(Guid? duAnId,
            string? filter,
            int pageIndex = 0,
            int pageSize = 0
        )
        {
            var res = await Mediator.Send(new DuAnGetDanhSachComboboxQuery()
            {
                DuAnId = duAnId,
                GlobalFilter = filter,
                PageIndex = pageIndex,
                PageSize = pageSize
            });
            return ResultApi.Ok(res);
        }
        /// <summary>
        /// Theo dõi dự án theo phòng được phân công — 4 panel thống kê + danh sách phân trang
        /// </summary>
        [HttpGet("api/du-an/theo-doi-du-an-phong-phan-cong")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType<ResultApi<TheoDoiDuAnPhongPhanCongResultDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetTheoDoiDuAnPhongPhanCong([FromQuery] TheoDoiDuAnPhongPhanCongSearchDto searchDto)
        {
            var res = await Mediator.Send(new TheoDoiDuAnPhongPhanCongQuery(searchDto));
            return ResultApi.Ok(res);
        }

        /// <summary>
        /// Thống kê theo dõi dự án theo giai đoạn — 4 panel thống kê + danh sách phân trang
        /// </summary>
        [HttpGet("api/du-an/theo-doi-du-an-theo-giai-doan")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType<ResultApi<TheoDoiDuAnTheoGiaiDoanResultDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetTheoDoiDuAnTheoGiaiDoan([FromQuery] TheoDoiDuAnTheoGiaiDoanSearchDto searchDto)
        {
            var res = await Mediator.Send(new TheoDoiDuAnTheoGiaiDoanQuery(searchDto));
            return ResultApi.Ok(res);
        }

        [HttpGet("api/du-an/danh-sach-theo-phong-ban")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType<ResultApi<PaginatedList<DuAn>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetDuAnPhongBanDangThucHien([FromQuery] int? PageNo, [FromQuery] int? PageSize)
        {
            var res = await Mediator.Send(
                new DuAnGetTheoPhongBanGetQuery
                {
                    PageIndex = PageNo ?? 1,
                    PageSize = PageSize ?? 1000000,

                    ThrowIfNull = true,
                    IsNoTracking = true
                });

            return ResultApi.Ok(res);
        }

        /// <summary>
        /// Lấy danh sách dự án đang trễ hạn thực hiện tại  bước có thời hạn
        /// </summary>
        /// <remarks>
        /// Trả về danh sách dự án thoả điều kiện.
        /// </remarks>
        /// <response code="200">Danh sách dự án</response>
        /// <response code="400">Request không hợp lệ</response>
        /// <response code="500">Lỗi hệ thống</response>
        /// <returns></returns>
        [HttpGet("api/du-an/danh-sach-tre-han")]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType<ResultApi<PaginatedList<DuAnTreHanDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetProjectOverdue([FromQuery] DuAnSearchOverdueDto searchDto)
        {
            var res = await Mediator.Send(new DuAnGetDanhSachTreHanQuery(searchDto));
            return ResultApi.Ok(res);
        }

        /// <summary>
        /// Thêm mới dự án
        /// </summary>
        /// <remarks>
        /// Quy trình id là bắt buộc
        /// </remarks>
        /// <param name="model"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpPost("api/du-an/them-moi")]
        [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Create(
            [FromBody] DuAnInsertDto model,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken = default

        )
        {
            using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var entity = await Mediator.Send(new DuAnInsertCommand(model), cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await Mediator.Send(new DuAnBuocCloneCommand(entity), cancellationToken);

            // Map phòng ban phụ trách chính + danh sách phòng ban phối hợp từ DuAn
            // sang tất cả các DuAnBuoc vừa được clone (chỉ áp dụng khi thêm mới)
            var phongBanPhoiHopIds = entity.DuAnChiuTrachNhiemXuLys?
                .Where(x => x.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                .Select(x => x.RightId)
                .ToList() ?? [];

            await Mediator.Send(new DuAnBuocMapPhongBanCommand(
                entity.Id,
                entity.DonViPhuTrachChinhId,
                phongBanPhoiHopIds
            ), cancellationToken);

            // Handle DuToan files
            List<(DuToan, List<Attachment>)> duToans = [.. model.DuToans?.Select(e => e.ToEntity(entity.Id)) ?? []];
            if (duToans.Count != 0)
            {

                //Thêm dự toán
                await Mediator.Send(new DuToanInsertRangeCommand([.. duToans.Select(e => e.Item1)]), cancellationToken);

                //Thêm files
                foreach (var (duToan, files) in duToans)
                {
                    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                    {
                        GroupId = duToan.Id.ToString(),
                        Entities = files,
                    }, cancellationToken);
                }

            }

            // Handle KeHoachVon files (KeHoachVons already created via DuAnMappings.ToEntity)
            if (model.KeHoachVons != null && model.KeHoachVons.Count != 0 && entity.KeHoachVons != null)
            {
                var khvList = entity.KeHoachVons.ToList();
                for (int i = 0; i < model.KeHoachVons.Count && i < khvList.Count; i++)
                {
                    var khvModel = model.KeHoachVons[i];
                    if (khvModel.DanhSachTepDinhKem?.Count > 0)
                    {
                        var khvFiles = khvModel.DanhSachTepDinhKem.ToEntities(
                            groupId: khvList[i].Id,
                            groupType: EGroupType.KeHoachVon
                        );
                        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                        {
                            GroupId = khvList[i].Id.ToString(),
                            Entities = [.. khvFiles],
                        }, cancellationToken);
                    }
                }
            }

            // Handle DuAn decision files
            if (model.DanhSachTepQuyetDinh?.Count > 0)
            {
                var decisionFiles = model.DanhSachTepQuyetDinh.ToEntities(
                    groupId: entity.Id,
                    groupType: EGroupType.QuyetDinhPheDuyetNhiemVu
                );
                await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                {
                    GroupId = entity.Id.ToString(),
                    Entities = [.. decisionFiles],
                }, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return ResultApi.Ok(await GetDuAnWithFiles(entity.Id, cancellationToken));
        }
        /// <summary>
        /// Cập nhật dự án
        /// </summary>
        /// <remarks>
        /// DanhSachNguonVon có thể null
        /// </remarks>
        /// <param name="updateDto"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Authorize(Roles = RoleConstants.GroupAdminOrManager)]
        [HttpPut("api/du-an/cap-nhat")]
        [ProducesResponseType<ResultApi<DuAnDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<ResultApi> Update(
            [FromBody] DuAnUpdateModel updateDto,
            [FromServices] IUnitOfWork unitOfWork,
            CancellationToken cancellationToken = default
        )
        {
            using var tx = await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var entity = await Mediator.Send(new DuAnUpdateCommand(updateDto), cancellationToken);

            await Mediator.Send(new DuAnBuocCloneCommand(entity), cancellationToken);

            // Xử lý DuToan và TepDinhKem tương tự như trong hàm tạo mới
            List<(DuToan, List<Attachment>)> duToans = [.. updateDto.DuToans?.Select(e => e.ToEntityWithFiles(entity.Id)) ?? []];

            // Cập nhật dự toán và files
            foreach (var (duToan, files) in duToans)
            {
                // Thêm hoặc cập nhật files cho mỗi dự toán
                await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                {
                    GroupId = duToan.Id.ToString(),
                    Entities = files,
                }, cancellationToken);
            }

            // Handle KeHoachVon files
            if (updateDto.KeHoachVons != null)
            {
                foreach (var khvModel in updateDto.KeHoachVons)
                {
                    var (khv, khvFiles) = khvModel.ToEntityWithFiles(entity.Id);
                    await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
                    {
                        GroupId = khv.Id.ToString(),
                        Entities = khvFiles,
                    }, cancellationToken);
                }
            }

            await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand
            {
                GroupId = entity.Id.ToString(),
                Entities = [.. updateDto.DanhSachTepQuyetDinh?.ToEntities(entity.Id, EGroupType.QuyetDinhPheDuyetNhiemVu) ?? []],
            }, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return ResultApi.Ok(await GetDuAnWithFiles(entity.Id, cancellationToken));
        }

        /// <summary>
        /// Lấy toàn bộ tệp đính kèm của dự án (bao gồm tất cả bảng liên kết)
        /// </summary>
        /// <param name="id">Id dự án</param>
        /// <returns>Danh sách tệp đính kèm</returns>
        [HttpGet("api/du-an/{id}/tat-ca-tep-dinh-kem")]
        [ProducesResponseType<ResultApi<List<TepDinhKemDto>>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
        public async Task<ResultApi> GetTatCaTepDinhKem(Guid id)
        {
            var result = await Mediator.Send(new DuAnGetDanhSachTepDinhKemQuery { DuAnId = id });
            return ResultApi.Ok(result);
        }

        private async Task<DuAnDto> GetDuAnWithFiles(Guid duAnId, CancellationToken cancellationToken)
        {
            var entity = await Mediator.Send(new DuAnGetQuery()
            {
                Id = duAnId,
                ThrowIfNull = true,
                IsNoTracking = true,
                IncludeNguonVon = true,
                IncludeChiuTrachNhiemXuLy = true,
                IncludeDuToan = true,
                IncludeKeHoachVon = true,
            }, cancellationToken);

            var groupIds = new List<string>();
            if (entity.DuToans != null)
            {
                groupIds.AddRange(entity.DuToans.Select(dt => dt.Id.ToString()));
            }
            if (entity.KeHoachVons != null)
            {
                groupIds.AddRange(entity.KeHoachVons.Select(kh => kh.Id.ToString()));
            }
            groupIds.Add(entity.Id.ToString());

            List<Attachment>? files = null;
            if (groupIds.Count != 0)
            {
                files = await Mediator.Send(new GetDanhSachTepDinhKemQuery()
                {
                    GroupId = [.. groupIds]
                }, cancellationToken);
            }
            return entity.ToDto(files);
        }
    }
}
