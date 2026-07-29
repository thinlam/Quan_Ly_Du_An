# Summary – API Lấy Toàn Bộ Tệp Đính Kèm Của Dự Án

> **Hoàn thành: 20/05/2026**
> **Trạng thái: ✅ Done**

---

## Những gì đã làm

### 1. Tạo mới `DuAnGetDanhSachTepDinhKemQuery`
**File:** `QLDA.Application/DuAns/Queries/DuAnGetDanhSachTepDinhKemQuery.cs`

Query nhận `DuAnId`, thu thập ID từ **19 bảng nghiệp vụ** liên kết, sau đó query `TepDinhKem` một lần với `GroupId IN (all_ids)`.

**Danh sách bảng được cover:**

| # | Entity | Ghi chú |
|---|--------|---------|
| 1 | `DuAn.Id` | File đính thẳng vào dự án |
| 2 | `GoiThau` | |
| 3 | `HopDong` | |
| 4 | `NghiemThu` | |
| 5 | `KetQuaTrungThau` | |
| 6 | `KeHoachVon` | |
| 7 | `DuToan` | |
| 8 | `DangTaiKeHoachLcntLenMang` | |
| 9 | `HoSoDeXuatCapDoCntt` | |
| 10 | `BaoCao` | Base class → cover BaoCaoTienDo, BaoCaoBaoHanhSanPham, BaoCaoBanGiaoSanPham |
| 11 | `PhuLucHopDong` | |
| 12 | `VanBanQuyetDinh` | Base class → cover tất cả loại quyết định |
| 13 | `ThanhToan` | |
| 14 | `TamUng` | |
| 15 | `HoSoMoiThauDienTu` | DuAnId nullable |
| 16 | `ToTrinhKeHoach` | |
| 17 | `BanGiaoHoSo` | DuAnId nullable |
| 18 | `PhanKhaiKinhPhi` | |
| 19 | `QuyetDinhDieuChinh` | |

**Điểm đáng chú ý:**
- Luôn filter `!e.IsDeleted` khi thu thập ID từ bảng liên kết
- Luôn filter `!f.IsDeleted` khi query TepDinhKem
- Dùng `files.ToDtos()` extension có sẵn tại `TepDinhKemMappingConfiguration.cs`

---

### 2. Thêm endpoint vào `DuAnController`
**File:** `QLDA.WebApi/Controllers/DuAnController.cs`

```csharp
[HttpGet("{id}/tat-ca-tep-dinh-kem")]
[ProducesResponseType<ResultApi<List<TepDinhKemDto>>>(StatusCodes.Status200OK)]
[ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
public async Task<ResultApi> GetTatCaTepDinhKem(Guid id) {
    var result = await Mediator.Send(new DuAnGetDanhSachTepDinhKemQuery { DuAnId = id });
    return ResultApi.Ok(result);
}
```

---

### 3. Response format

```
GET /api/du-an/{id}/tat-ca-tep-dinh-kem
```

```json
[
  {
    "id": "guid",
    "groupId": "guid",
    "groupType": "GoiThau",
    "type": "application/pdf",
    "fileName": "file.pdf",
    "originalName": "file.pdf",
    "path": "/uploads/...",
    "size": 1024,
    "parentId": null
  }
]
```

- `groupType` → biết file thuộc bảng nào
- `groupId` → ID của entity chứa file
- File đã xóa mềm (`IsDeleted = true`) **không** xuất hiện

---

## Checklist

- [x] Tạo `DuAnGetDanhSachTepDinhKemQuery.cs`
- [x] Thêm endpoint `GET /{id}/tat-ca-tep-dinh-kem` vào `DuAnController.cs`
- [x] Filter `!IsDeleted` cho cả entity lẫn TepDinhKem
- [x] Không có Migration (không thêm bảng/cột mới)

---

## Không cần làm

- ❌ Entity mới
- ❌ Migration
- ❌ DTO mới (tái sử dụng `TepDinhKemDto`)

---

## Chi tiết kỹ thuật ban đầu (lưu để tham khảo)

### 1.1 Mô tả

| Mục | Nội dung |
|-----|---------|
| Endpoint | `GET /api/du-an/{id}/tat-ca-tep-dinh-kem` |
| Input | `id` (Guid – path param = `duAnId`) |
| Output | `List<TepDinhKemDto>` |
| Mô tả | Lấy toàn bộ tệp đính kèm thuộc dự án: bao gồm file của chính DuAn và tất cả bảng con liên kết qua `DuAnId` |

### 1.2 Response DTO (TepDinhKemDto – đã có sẵn)

```json
[
  {
    "id": null,
    "groupId": "Guid",
    "groupType": null,
    "type": null,
    "fileName": null,
    "originalName": null,
    "path": null,
    "size": 0,
    "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
]
```

> `groupType` cho biết file thuộc bảng nào (e.g. `"GoiThau"`, `"HopDong"`, `"NghiemThu"`…)
> `groupId` là `Id` của entity gốc (e.g. GoiThau.Id, HopDong.Id…)

---

## 2. Các bảng liên kết cần thu thập GroupId

Thu thập `Id` từ tất cả các bảng có `DuAnId = request.DuAnId`, sau đó query `TepDinhKem` với `GroupId IN (danh sách ids + duAnId)`.

| # | Entity | Ghi chú |
|---|--------|---------|
| 1 | `DuAn.Id` | File đính thẳng vào dự án (e.g. `EGroupType.QuyetDinhPheDuyetNhiemVu`) |
| 2 | `GoiThau` | `EGroupType.GoiThau` |
| 3 | `HopDong` | `EGroupType.HopDong` |
| 4 | `NghiemThu` | `EGroupType.NghiemThu` |
| 5 | `KetQuaTrungThau` | `EGroupType.KetQuaTrungThau` |
| 6 | `KeHoachVon` | `EGroupType.KeHoachVon` |
| 7 | `DuToan` | `EGroupType.DuToan`, `EGroupType.PheDuyetDuToan` |
| 8 | `DangTaiKeHoachLcntLenMang` | `EGroupType.DangTaiKeHoachLcntLenMang` |
| 9 | `HoSoDeXuatCapDoCntt` | `EGroupType.HoSoDeXuatCapDoCntt` |
| 10 | `BaoCao` (base) | Covers: `BaoCaoTienDo`, `BaoCaoBaoHanhSanPham`, `BaoCaoBanGiaoSanPham`, `BaoCaoKhoKhanVuongMac` |
| 11 | `PhuLucHopDong` | `EGroupType.PhuLucHopDong` |
| 12 | `VanBanQuyetDinh` (base) | Covers tất cả quyết định: `KeHoachLuaChonNhaThau`, `QuyetDinhDuyetDuAn`, `QuyetDinhDuyetKHLCNT`, `QuyetDinhDuyetQuyetToan`, `QuyetDinhLapBanQLDA`, `QuyetDinhLapBenMoiThau`, `QuyetDinhLapHoiDongThamDinh`, `VanBanPhapLy`, `VanBanChuTruong` |
| 13 | `ThanhToan` | `EGroupType.ThanhToan` |
| 14 | `TamUng` | `EGroupType.TamUng` |
| 15 | `HoSoMoiThauDienTu` | `EGroupType.HoSoMoiThauDienTu` (có `DuAnId?` nullable) |
| 16 | `ToTrinhKeHoach` | `EGroupType.ToTrinhKeHoach` |
| 17 | `BanGiaoHoSo` | `EGroupType.BanGiaoHoSo`, `EGroupType.BienBanBanGiao` (có `DuAnId?` nullable) |
| 18 | `PhanKhaiKinhPhi` | `EGroupType.PhanKhaiKinhPhi` |
| 19 | `QuyetDinhDieuChinh` | — |

> **Lưu ý:** `BaoCao` và `VanBanQuyetDinh` là base class của nhiều entity con → query 1 lần vào bảng gốc là đủ (EF TPH/TPC tùy configuration)

---

## 3. Thứ tự thực hiện

```
Bước 1: Application – DuAnGetDanhSachTepDinhKemQuery
Bước 2: WebApi – Thêm endpoint vào DuAnController
Bước 3: Build + Test
```

---

## 4. Chi tiết từng bước

---

### Bước 1 – Application: `DuAnGetDanhSachTepDinhKemQuery`

**File:** `QLDA.Application/DuAns/Queries/DuAnGetDanhSachTepDinhKemQuery.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.DuAns.Queries;

public class DuAnGetDanhSachTepDinhKemQuery : IRequest<List<TepDinhKemDto>> {
    public Guid DuAnId { get; set; }
}

internal class DuAnGetDanhSachTepDinhKemQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DuAnGetDanhSachTepDinhKemQuery, List<TepDinhKemDto>> {

    private readonly IRepository<TepDinhKem, Guid>          _tepDinhKemRepo          = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<GoiThau, Guid>             _goiThauRepo             = serviceProvider.GetRequiredService<IRepository<GoiThau, Guid>>();
    private readonly IRepository<HopDong, Guid>             _hopDongRepo             = serviceProvider.GetRequiredService<IRepository<HopDong, Guid>>();
    private readonly IRepository<NghiemThu, Guid>           _nghiemThuRepo           = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
    private readonly IRepository<KetQuaTrungThau, Guid>     _ketQuaTrungThauRepo     = serviceProvider.GetRequiredService<IRepository<KetQuaTrungThau, Guid>>();
    private readonly IRepository<KeHoachVon, Guid>          _keHoachVonRepo          = serviceProvider.GetRequiredService<IRepository<KeHoachVon, Guid>>();
    private readonly IRepository<DuToan, Guid>              _duToanRepo              = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
    private readonly IRepository<DangTaiKeHoachLcntLenMang, Guid> _dangTaiRepo       = serviceProvider.GetRequiredService<IRepository<DangTaiKeHoachLcntLenMang, Guid>>();
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> _hoSoDeXuatRepo          = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
    private readonly IRepository<BaoCao, Guid>              _baoCaoRepo              = serviceProvider.GetRequiredService<IRepository<BaoCao, Guid>>();
    private readonly IRepository<PhuLucHopDong, Guid>       _phuLucHopDongRepo       = serviceProvider.GetRequiredService<IRepository<PhuLucHopDong, Guid>>();
    private readonly IRepository<VanBanQuyetDinh, Guid>     _vanBanQuyetDinhRepo     = serviceProvider.GetRequiredService<IRepository<VanBanQuyetDinh, Guid>>();
    private readonly IRepository<ThanhToan, Guid>           _thanhToanRepo           = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    private readonly IRepository<TamUng, Guid>              _tamUngRepo              = serviceProvider.GetRequiredService<IRepository<TamUng, Guid>>();
    private readonly IRepository<HoSoMoiThauDienTu, Guid>  _hoSoMoiThauRepo         = serviceProvider.GetRequiredService<IRepository<HoSoMoiThauDienTu, Guid>>();
    private readonly IRepository<ToTrinhKeHoach, Guid>      _toTrinhKeHoachRepo      = serviceProvider.GetRequiredService<IRepository<ToTrinhKeHoach, Guid>>();
    private readonly IRepository<BanGiaoHoSo, Guid>         _banGiaoHoSoRepo         = serviceProvider.GetRequiredService<IRepository<BanGiaoHoSo, Guid>>();
    private readonly IRepository<PhanKhaiKinhPhi, Guid>     _phanKhaiKinhPhiRepo     = serviceProvider.GetRequiredService<IRepository<PhanKhaiKinhPhi, Guid>>();
    private readonly IRepository<QuyetDinhDieuChinh, Guid>  _quyetDinhDieuChinhRepo  = serviceProvider.GetRequiredService<IRepository<QuyetDinhDieuChinh, Guid>>();

    public async Task<List<TepDinhKemDto>> Handle(
        DuAnGetDanhSachTepDinhKemQuery request,
        CancellationToken cancellationToken = default) {

        var duAnId = request.DuAnId;
        var duAnIdStr = duAnId.ToString();

        // Thu thập tất cả GroupId từ các bảng liên kết
        var groupIds = new List<string> { duAnIdStr };

        // Helper: add list of guid strings
        void AddIds(IEnumerable<string> ids) => groupIds.AddRange(ids);

        AddIds(await _goiThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _hopDongRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _nghiemThuRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _ketQuaTrungThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _keHoachVonRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _duToanRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _dangTaiRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _hoSoDeXuatRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _baoCaoRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _phuLucHopDongRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _vanBanQuyetDinhRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _thanhToanRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _tamUngRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _hoSoMoiThauRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _toTrinhKeHoachRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _banGiaoHoSoRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _phanKhaiKinhPhiRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        AddIds(await _quyetDinhDieuChinhRepo.GetQueryableSet()
            .Where(e => e.DuAnId == duAnId && !e.IsDeleted)
            .Select(e => e.Id.ToString()).ToListAsync(cancellationToken));

        // Query TepDinhKem theo tất cả GroupIds
        var files = await _tepDinhKemRepo.GetQueryableSet()
            .Where(f => groupIds.Contains(f.GroupId))
            .ToListAsync(cancellationToken);

        return files.ToDtos();
    }
}
```

> **Lưu ý:**
> - Dùng `!e.IsDeleted` để bỏ qua các entity đã xóa mềm
> - `VanBanQuyetDinh` là base class của `KeHoachLuaChonNhaThau`, `VanBanPhapLy`, `VanBanChuTruong`, v.v. → query 1 lần là đủ
> - `BaoCao` là base class của `BaoCaoTienDo`, `BaoCaoBaoHanhSanPham`, `BaoCaoBanGiaoSanPham`, `BaoCaoKhoKhanVuongMac` → query 1 lần là đủ
> - `ToDtos()` extension đã có tại `TepDinhKemMappingConfiguration.cs`

---

### Bước 2 – WebApi: Thêm endpoint vào `DuAnController`

**File:** `QLDA.WebApi/Controllers/DuAnController.cs`

**Thêm using:**
```csharp
using QLDA.Application.DuAns.Queries;
```
*(đã có sẵn trong file)*

**Thêm action:**
```csharp
/// <summary>
/// Lấy toàn bộ tệp đính kèm của dự án (bao gồm tất cả bảng liên kết)
/// </summary>
/// <param name="id">Id dự án</param>
/// <returns>Danh sách tệp đính kèm</returns>
[HttpGet("{id}/tat-ca-tep-dinh-kem")]
[ProducesResponseType<ResultApi<List<TepDinhKemDto>>>(StatusCodes.Status200OK)]
[ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
public async Task<ResultApi> GetTatCaTepDinhKem(Guid id) {
    var result = await Mediator.Send(new DuAnGetDanhSachTepDinhKemQuery { DuAnId = id });
    return ResultApi.Ok(result);
}
```

---

### Bước 3 – Build + Test

```bash
dotnet build
```

Kiểm tra:
- `GET /api/du-an/{id}/tat-ca-tep-dinh-kem` trả về `200 OK` với `List<TepDinhKemDto>`
- `groupType` phân biệt được file thuộc bảng nào
- Các entity đã xóa mềm (`IsDeleted = true`) không xuất hiện trong kết quả
- Nếu dự án không có file nào → trả về mảng rỗng `[]`

---

## 5. Checklist

- [x] Bước 1: Tạo file `DuAnGetDanhSachTepDinhKemQuery.cs`
- [x] Bước 2: Thêm action `GetTatCaTepDinhKem` vào `DuAnController.cs`
- [x] Bước 3: Build không lỗi
- [x] Bước 4: Test API trả đúng response format
