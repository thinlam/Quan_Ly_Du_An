# Issue #9609 — Bổ sung tiêu chí tìm kiếm "Loại dự án theo năm"

> Cập nhật: 18/06/2026
> Trạng thái: ✅ Hoàn thành (BE) — 0 errors build

## 1. Bối cảnh

FE đã bổ sung UI combobox search "Loại dự án theo năm" ở các màn hình. BE cần tiếp nhận param `LoaiDuAnTheoNamId` và filter danh sách theo navigation `e.DuAn.LoaiDuAnTheoNamId`.

**Yêu cầu gốc:**
> Bổ sung thêm tiêu chí tìm kiếm CBB "Loại dự án theo năm" ở tất cả màn hình (với các entities có liên kết DuAn)
> `.WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)`

**6 màn hình FE yêu cầu (đợt 1):** Gói thầu, Hợp đồng, Phụ lục hợp đồng, Báo cáo tiến độ, Khó khăn vướng mắc, Tổng hợp văn bản quyết định.

**Mở rộng (đợt 2):** Tất cả entity còn lại có `DuAn? DuAn` navigation property đều được áp dụng đồng nhất.

---

## 2. Pattern chuẩn

### 2.1. Thêm property vào Search DTO / Query record
```csharp
/// <summary>
/// Loại dự án theo năm - tài chính
/// </summary>
/// <remarks>PMIS #9609</remarks>
public int? LoaiDuAnTheoNamId { get; set; }
```

### 2.2. Filter trong QueryHandler
Đặt **sau** filter `DuAnId`, **trước** `WhereGlobalFilter`:
```csharp
.WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
```

**Biến thể theo cấu trúc Query:**
| Cấu trúc Query | Cú pháp truy cập |
|----------------|------------------|
| Query wrap SearchDto (GoiThau, HopDong...) | `request.SearchDto.LoaiDuAnTheoNamId` |
| Inline record (PhuLucHopDong, BaoCaoTienDo...) | `request.LoaiDuAnTheoNamId` |

### 2.3. Controller tiếp nhận + truyền param
```csharp
// Inline-param controller
public async Task<ResultApi> Get(..., int? loaiDuAnTheoNamId = null) {
    var res = await Mediator.Send(new XxxGetDanhSachQuery() {
        ...
        LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
    });
}

// SearchModel controller
LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
```

### 2.4. Nguyên tắc tuân thủ
- ✅ Filter đặt **sau** `DuAnId`/`LoaiDuAnId`, **trước** `WhereGlobalFilter`
- ✅ Dùng `e.DuAn!.LoaiDuAnTheoNamId` (null-forgiving — đã filter `!e.DuAn!.IsDeleted` trước đó)
- ❌ KHÔNG thêm `!e.IsDeleted` (`GetQueryableSet()` đã filter sẵn)
- ❌ KHÔNG thêm `FilterVisible` (query đã có `_authManager.FilterVisible`/`_buocAuth.FilterVisibleChildEntities` ở đầu)
- ❌ KHÔNG nullable-warn vì `DuAn` navigation đã được bảo đảm bởi filter IsDeleted

---

## 3. Entities được cập nhật (28 nhóm)

### 3.1. Đợt 1 — 6 màn hình FE (API danh sách + Print/Export)

| # | Entity | Query | Search DTO/Model | Controller | Print |
|---|--------|-------|------------------|------------|-------|
| 1 | Gói thầu (`GoiThau`) | `GoiThauGetDanhSachQuery` | `GoiThauSearchDto` + `GoiThauPrintSearchDto` | (DTO trực tiếp) | ✅ `InGoiThau` |
| 2 | Hợp đồng (`HopDong`) | `HopDongGetDanhSachQuery` | `HopDongSearchDto` + `HopDongPrintSearchDto` | (DTO trực tiếp) | ✅ `InHopDong` |
| 3 | Phụ lục hợp đồng (`PhuLucHopDong`) | `PhuLucHopDongGetDanhSachQuery` | `PhuLucHopDongSearchModel` + `PhuLucHopDongPrintSearchModel` | ✅ | ✅ `InPhuLucHopDong` |
| 4 | Báo cáo tiến độ (`BaoCaoTienDo`) | `BaoCaoTienDoGetDanhSachQuery` | `BaoCaoTienDoSearchModel` + `BaoCaoTienDoPrintSearchModel` | ✅ | ✅ `InBaoCaoTienDo` |
| 5 | Khó khăn vướng mắc (`BaoCaoKhoKhanVuongMac`) | `KhoKhanVuongMacGetDanhSachQuery` | `KhoKhanVuongMacSearchModel` + `KhoKhanVuongMacPrintSearchModel` | ✅ | ✅ `InKhoKhanVuongMac` |
| 6 | Tổng hợp văn bản quyết định (`VanBanQuyetDinh`) | `TongHopVanBanQuyetDinhGetListQuery` | `TongHopVanBanQuyetDinhSearchModel` + `TongHopVanBanQuyetDinhPrintSearchModel` | ✅ | ✅ `InTongHopVanBanQuyetDinh` |

### 3.2. Đợt 2 — Entities bổ sung (API danh sách)

| Nhóm | Entities |
|------|----------|
| **Quyết định (7)** | `QuyetDinhDuyetDuToan`, `QuyetDinhLapBanQLDA`, `QuyetDinhLapBenMoiThau`, `QuyetDinhLapHoiDongThamDinh`, `QuyetDinhDuyetKHLCNT`, `QuyetDinhDuyetQuyetToan`, `QuyetDinhDieuChinh` |
| **Văn bản (2)** | `VanBanChuTruong`, `VanBanPhapLy` |
| **Báo cáo (3)** | `BaoCaoBanGiaoSanPham`, `BaoCaoBaoHanhSanPham`, `BaoCaoKetQuaKhaoSat` |
| **Đề xuất (4)** | `DeXuatChuTruongMoi`, `DeXuatChuyenTiep`, `DeXuatNhuCauKinhPhi`, `ChuTruongLapKeHoach` |
| **Tờ trình (4)** | `ToTrinhPheDuyet`, `ToTrinhCoThamDinh`, `ToTrinhKetQuaGoiThau`, `ToTrinhThamDinhNhaThau` |
| **Tài chính (3)** | `TamUng`, `ThanhToan`, `NghiemThu` |
| **Hồ sơ (2)** | `HoSoMoiThauDienTu`, `HoSoDeXuatCapDoCntt` |
| **Kế hoạch/Kết quả (4)** | `KetQuaTrungThau`, `KeHoachLuaChonNhaThauRutGon`, `KeHoachTrienKhaiHangMuc`, `KeHoachTrienKhaiChiTietDuAn` |

> Lưu ý: `BaoCaoBanGiaoSanPham` và `BaoCaoBaoHanhSanPham` cũng có Print/Export (đợt 1 mở rộng) → cập nhật cả `InBaoCaoBanGiaoSanPham` và `InBaoCaoBaoHanhSanPham` trong PrintController.

---

## 4. Cấu trúc Controller — 2 pattern gặp

### Pattern A — SearchModel/DTO parameter (object mapping)
```csharp
public async Task<ResultApi> Get([FromQuery] XxxSearchModel searchModel) {
    var res = await Mediator.Send(new XxxGetDanhSachQuery() {
        DuAnId = searchModel.DuAnId,
        ...
        LoaiDuAnTheoNamId = searchModel.LoaiDuAnTheoNamId,
    });
}
```
**Áp dụng:** PhuLucHopDong, BaoCaoTienDo, KhoKhanVuongMac, TongHopVanBanQuyetDinh, QuyetDinhLapBanQLDA, QuyetDinhLapBenMoiThau, QuyetDinhLapHoiDongThamDinh, BaoCaoBanGiaoSanPham, BaoCaoBaoHanhSanPham, BaoCaoKetQuaKhaoSat, DeXuatChuTruongMoi, DeXuatNhuCauKinhPhi, ChuTruongLapKeHoach, ToTrinhPheDuyet, ToTrinhCoThamDinh, ToTrinhKetQuaGoiThau, ToTrinhThamDinhNhaThau, KeHoachTrienKhaiHangMuc, KeHoachTrienKhaiChiTietDuAn.

### Pattern B — Inline query params (flat params)
```csharp
public async Task<ResultApi> Get([FromQuery] Guid? duAnId, int? buocId,
    string? globalFilter = null, int pageIndex = 0, int pageSize = 0,
    int? loaiDuAnTheoNamId = null) {
    var res = await Mediator.Send(new XxxGetDanhSachQuery() {
        ...
        LoaiDuAnTheoNamId = loaiDuAnTheoNamId,
    });
}
```
**Áp dụng:** QuyetDinhDuyetDuToan, QuyetDinhDuyetKHLCNT, QuyetDinhDuyetQuyetToan, QuyetDinhDieuChinh, VanBanChuTruong, VanBanPhapLy, TamUng, ThanhToan, NghiemThu, KetQuaTrungThau, KeHoachLuaChonNhaThauRutGon, DeXuatChuyenTiep.

### Pattern C — DTO truyền trực tiếp (không cần sửa Controller)
```csharp
public async Task<ResultApi> Get([FromQuery] GoiThauSearchDto searchDto) {
    var res = await Mediator.Send(new GoiThauGetDanhSachQuery(searchDto) { ... });
}
```
**Áp dụng:** GoiThau, HopDong, HoSoMoiThauDienTu, HoSoDeXuatCapDoCntt (chỉ cần thêm property vào SearchDto).

---

## 5. Tổng hợp files chỉnh sửa (96 files)

### 5.1. Application Layer

#### Query Handlers (35 files)
| File |
|------|
| `QLDA.Application/GoiThaus/Queries/GoiThauGetDanhSachQuery.cs` |
| `QLDA.Application/HopDongs/Queries/HopDongGetDanhSachQuery.cs` |
| `QLDA.Application/PhuLucHopDongs/Queries/PhuLucHopDongGetDanhSachQuery.cs` |
| `QLDA.Application/BaoCaoTienDos/Queries/BaoCaoTienDoGetDanhSachQuery.cs` |
| `QLDA.Application/KhoKhanVuongMacs/Queries/KhoKhanVuongMacGetDanhSachQuery.cs` |
| `QLDA.Application/TongHopVanBanQuyetDinhs/Queries/TongHopVanBanQuyetDinhGetListQuery.cs` |
| `QLDA.Application/QuyetDinhDuyetDuToan/Queries/QuyetDinhDuyetDuToanGetDanhSachQuery.cs` |
| `QLDA.Application/QuyetDinhLapBanQLDAs/Queries/QuyetDinhLapBanQldaGetDanhSachQuery.cs` |
| `QLDA.Application/QuyetDinhLapBenMoiThaus/Queries/QuyetDinhLapBenMoiThauGetDanhSachQuery.cs` |
| `QLDA.Application/QuyetDinhLapHoiDongThamDinhs/Queries/QuyetDinhLapHoiDongThamDinhGetDanhSachQuery.cs` |
| `QLDA.Application/QuyetDinhDuyetKHLCNTs/Queries/QuyetDinhDuyetKHLCNTGetDanhSachQuery.cs` |
| `QLDA.Application/QuyetDinhDuyetQuyetToans/Queries/QuyetDinhDuyetQuyetToanGetDanhSachQuery.cs` |
| `QLDA.Application/QuyetDinhDieuChinhs/Queries/QuyetDinhDieuChinhGetDanhSachQuery.cs` |
| `QLDA.Application/VanBanChuTruongs/Queries/VanBanChuTruongGetDanhSachQuery.cs` |
| `QLDA.Application/VanBanPhapLys/Queries/VanBanPhapLyGetDanhSachQuery.cs` |
| `QLDA.Application/BaoCaoBanGiaoSanPhams/Queries/BaoCaoBanGiaoSanPhamGetDanhSachQuery.cs` |
| `QLDA.Application/BaoCaoBaoHanhSanPhams/Queries/BaoCaoBaoHanhSanPhamGetDanhSachQuery.cs` |
| `QLDA.Application/BaoCaoKetQuaKhaoSats/Queries/BaoCaoKetQuaKhaoSatGetDanhSachQuery.cs` |
| `QLDA.Application/DeXuatChuTruongMoi/Queries/DeXuatChuTruongMoiGetDanhSachQuery.cs` |
| `QLDA.Application/DeXuatChuyenTiep/Queries/DeXuatChuyenTiepGetDanhSachQuery.cs` |
| `QLDA.Application/DeXuatNhuCauKinhPhi/Queries/DeXuatNhuCauKinhPhiGetDanhSachQuery.cs` |
| `QLDA.Application/ChuTruongLapKeHoach/Queries/ChuTruongLapKeHoachGetDanhSachQuery.cs` |
| `QLDA.Application/ToTrinhPheDuyet/Queries/ToTrinhPheDuyetGetPaginatedQuery.cs` |
| `QLDA.Application/ToTrinhCoThamDinh/Queries/ToTrinhCoThamDinhGetPaginatedQuery.cs` |
| `QLDA.Application/ToTrinhKetQuaGoiThau/Queries/ToTrinhKetQuaGoiThauGetDanhSachQuery.cs` |
| `QLDA.Application/ToTrinhThamDinhNhaThau/Queries/ToTrinhThamDinhNhaThauGetDanhSachQuery.cs` |
| `QLDA.Application/TamUngs/Queries/TamUngGetDanhSachQuery.cs` |
| `QLDA.Application/ThanhToans/Queries/ThanhToanGetDanhSachQuery.cs` |
| `QLDA.Application/NghiemThus/Queries/NghiemThuGetDanhSachQuery.cs` |
| `QLDA.Application/HoSoMoiThauDienTus/Queries/HoSoMoiThauDienTuGetDanhSachQuery.cs` |
| `QLDA.Application/HoSoDeXuatCapDoCntts/Queries/HoSoDeXuatCapDoCnttGetDanhSachQuery.cs` |
| `QLDA.Application/KetQuaTrungThaus/Queries/KetQuaTrungThauGetDanhSachQuery.cs` |
| `QLDA.Application/KeHoachLuaChonNhaThauRutGon/Queries/KeHoachLuaChonNhaThauRutGonGetDanhSachQuery.cs` |
| `QLDA.Application/KeHoachTrienKhaiHangMuc/Queries/KeHoachTrienKhaiHangMucGetDanhSachQuery.cs` |
| `QLDA.Application/KeHoachTrienKhaiChiTietDuAn/Queries/KeHoachTrienKhaiChiTietDuAnDanhSachQuery.cs` |

#### Search DTOs (13 files)
| File |
|------|
| `QLDA.Application/GoiThaus/DTOs/GoiThauSearchDto.cs` |
| `QLDA.Application/GoiThaus/DTOs/GoiThauPrintSearchDto.cs` |
| `QLDA.Application/HopDongs/DTOs/HopDongSearchDto.cs` |
| `QLDA.Application/HopDongs/DTOs/HopDongPrintSearchDto.cs` |
| `QLDA.Application/BaoCaoKetQuaKhaoSats/DTOs/BaoCaoKetQuaKhaoSatSearchDto.cs` |
| `QLDA.Application/DeXuatChuTruongMoi/DTOs/DeXuatChuTruongMoiSearchDto.cs` |
| `QLDA.Application/DeXuatNhuCauKinhPhi/DTOs/DeXuatNhuCauKinhPhiSearchDto.cs` |
| `QLDA.Application/ChuTruongLapKeHoach/DTOs/ChuTruongLapKeHoachSearchDto.cs` |
| `QLDA.Application/HoSoMoiThauDienTus/DTOs/HoSoMoiThauDienTuSearchDto.cs` |
| `QLDA.Application/HoSoDeXuatCapDoCntts/DTOs/HoSoDeXuatCapDoCnttSearchDto.cs` |
| `QLDA.Application/ToTrinhPheDuyet/DTOs/ToTrinhPheDuyetSearchDto.cs` |
| `QLDA.Application/ToTrinhKetQuaGoiThau/DTOs/ToTrinhKetQuaGoiThauSearchDto.cs` |
| `QLDA.Application/ToTrinhThamDinhNhaThau/DTOs/ToTrinhThamDinhNhaThauSearchDto.cs` |

### 5.2. WebApi Layer

#### Controllers (32 files)
| File |
|------|
| `QLDA.WebApi/Controllers/GoiThauController.cs` *(không sửa — DTO trực tiếp)* |
| `QLDA.WebApi/Controllers/HopDongController.cs` *(không sửa — DTO trực tiếp)* |
| `QLDA.WebApi/Controllers/PhuLucHopDongController.cs` |
| `QLDA.WebApi/Controllers/BaoCaoTienDoController.cs` |
| `QLDA.WebApi/Controllers/KhoKhanVuongMacController.cs` |
| `QLDA.WebApi/Controllers/TongHopVanBanQuyetDinhController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhDuyetDuToanController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhThanhLapBanQldaController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhLapBenMoiThauController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhLapHoiDongThamDinhController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhDuyetKHLCNTController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhDuyetQuyetToanController.cs` |
| `QLDA.WebApi/Controllers/QuyetDinhDieuChinhController.cs` |
| `QLDA.WebApi/Controllers/VanBanChuTruongController.cs` |
| `QLDA.WebApi/Controllers/VanBanPhapLyController.cs` |
| `QLDA.WebApi/Controllers/BaoCaoBanGiaoSanPhamController.cs` |
| `QLDA.WebApi/Controllers/BaoCaoBaoHanhSanPhamController.cs` |
| `QLDA.WebApi/Controllers/BaoCaoKetQuaKhaoSatController.cs` |
| `QLDA.WebApi/Controllers/DeXuatChuTruongMoiController.cs` |
| `QLDA.WebApi/Controllers/DeXuatChuTruongChuyenTiepController.cs` |
| `QLDA.WebApi/Controllers/DeXuatNhuCauKinhPhiController.cs` |
| `QLDA.WebApi/Controllers/ChuTruongLapKeHoachController.cs` |
| `QLDA.WebApi/Controllers/ToTrinhPheDuyetController.cs` |
| `QLDA.WebApi/Controllers/ToTrinhCoThamDinhController.cs` |
| `QLDA.WebApi/Controllers/ToTrinhKetQuaGoiThauController.cs` |
| `QLDA.WebApi/Controllers/ToTrinhThamDinhNhaThauController.cs` |
| `QLDA.WebApi/Controllers/TamUngController.cs` |
| `QLDA.WebApi/Controllers/ThanhToanController.cs` |
| `QLDA.WebApi/Controllers/NghiemThuController.cs` |
| `QLDA.WebApi/Controllers/KetQuaTrungThauController.cs` |
| `QLDA.WebApi/Controllers/KeHoachLuaChonNhaThauRutGonController.cs` |
| `QLDA.WebApi/Controllers/KeHoachTrienKhaiHangMucController.cs` |
| `QLDA.WebApi/Controllers/KeHoachTrienKhaiChiTietDuAnController.cs` |
| `QLDA.WebApi/Controllers/PrintController.cs` *(8 regions print)* |

#### Search Models (16 files)
| File |
|------|
| `QLDA.WebApi/Models/PhuLucHopDongs/PhuLucHopDongSearchModel.cs` |
| `QLDA.WebApi/Models/PhuLucHopDongs/PhuLucHopDongPrintSearchModel.cs` |
| `QLDA.WebApi/Models/BaoCaoTienDos/BaoCaoTienDoSearchModel.cs` |
| `QLDA.WebApi/Models/BaoCaoTienDos/BaoCaoTienDoPrintSearchModel.cs` |
| `QLDA.WebApi/Models/KhoKhanVuongMacs/KhoKhanVuongMacSearchModel.cs` |
| `QLDA.WebApi/Models/KhoKhanVuongMacs/KhoKhanVuongMacPrintSearchModel.cs` |
| `QLDA.WebApi/Models/TongHopVanBanQuyetDinhs/TongHopVanBanQuyetDinhSearchModel.cs` |
| `QLDA.WebApi/Models/TongHopVanBanQuyetDinhs/TongHopVanBanQuyetDinhPrintSearchModel.cs` |
| `QLDA.WebApi/Models/QuyetDinhLapBanQLDAs/QuyetDinhLapBanQldaSearchModel.cs` |
| `QLDA.WebApi/Models/QuyetDinhLapBenMoiThaus/QuyetDinhLapBenMoiThauSearchModel.cs` |
| `QLDA.WebApi/Models/QuyetDinhLapHoiDongThamDinhs/QuyetDinhLapHoiDongThamDinhSearchModel.cs` |
| `QLDA.WebApi/Models/BaoCaoBanGiaoSanPhams/BaoCaoBanGiaoSanPhamSearchModel.cs` |
| `QLDA.WebApi/Models/BaoCaoBanGiaoSanPhams/BaoCaoBanGiaoSanPhamPrintSearchModel.cs` |
| `QLDA.WebApi/Models/BaoCaoBaoHanhSanPhams/BaoCaoBaoHanhSanPhamSearchModel.cs` |
| `QLDA.WebApi/Models/BaoCaoBaoHanhSanPhams/BaoCaoBaoHanhSanPhamPrintSearchModel.cs` |
| `QLDA.WebApi/Models/DeXuatChuTruongChuyenTieps/DeXuatChuyenTiepPrintSearchModel.cs` |

---

## 6. Print/Export — PrintController regions

PrintController.cs truyền `LoaiDuAnTheoNamId` vào stored procedure `Params` cho 8 endpoint:

| Method | SP | SearchModel |
|--------|----|-------------|
| `InGoiThau` | `usp_In_DanhSach_GoiThau` | `GoiThauPrintSearchDto` |
| `InHopDong` | `usp_In_DanhSach_HopDong` | `HopDongPrintSearchDto` |
| `InPhuLucHopDong` | `usp_In_DanhSach_PhuLucHopDong` | `PhuLucHopDongPrintSearchModel` |
| `InBaoCaoTienDo` | `usp_In_DanhSach_BaoCaoTienDo` | `BaoCaoTienDoPrintSearchModel` |
| `InBaoCaoBaoHanhSanPham` | `usp_In_DanhSach_BaoCaoBaoHanhSanPham` | `BaoCaoBaoHanhSanPhamPrintSearchModel` |
| `InBaoCaoBanGiaoSanPham` | `usp_In_DanhSach_BaoCaoBanGiaoSanPham` | `BaoCaoBanGiaoSanPhamPrintSearchModel` |
| `InKhoKhanVuongMac` | `usp_In_DanhSach_KhoKhanVuongMac` | `KhoKhanVuongMacPrintSearchModel` |
| `InTongHopVanBanQuyetDinh` | `usp_In_DanhSach_TongHopVanBanQuyetDinh` | `TongHopVanBanQuyetDinhPrintSearchModel` |

> Các entity khác (QuyetDinh*, DeXuat*, ToTrinh*, TamUng/ThanhToan/NghiemThu...) **không có** stored procedure print riêng trong PrintController → không cần sửa print.

---

## 7. Verification

### Build
```bash
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```
→ **0 errors**, 22 warnings (XML doc thiếu `<param>` tag cho `loaiDuAnTheoNamId` — không ảnh hưởng runtime, cùng pattern với codebase hiện có).

### Manual test (Swagger)
Test 28+ endpoint `danh-sach-tien-do`/`danh-sach` với `loaiDuAnTheoNamId=1` (Chuẩn bị đầu tư) vs `=3` (Khởi công mới) → kết quả khác nhau.

### Negative test
- Không truyền `loaiDuAnTheoNamId` → filter bị skip (trả full list, không đổi behavior cũ)
- `loaiDuAnTheoNamId=0` → filter bị skip (do check `> 0`)

---

## 8. Risk & Follow-up

| # | Item | Mô tả | Hành động |
|---|------|-------|-----------|
| 1 | Stored procedure chưa hỗ trợ param | 8 SP print/export cần update ở DB side để tận dụng filter. Nếu SP không có param `@LoaiDuAnTheoNamId`, Dapper sẽ ignore param → không break, nhưng print sẽ không filter được. | **DBA/BE update SP** |
| 2 | XML doc warning | Controller param mới thiếu `<param>` tag → 22 warnings CS1573. | Tuỳ chọn: bổ sung XML doc (cosmetic) |
| 3 | FE mapping | FE đã làm UI combobox cho 6 màn hình đợt 1. Đợt 2 (21 entity) FE cần bổ sung UI combobox nếu muốn expose filter. | **FE bổ sung UI** (nếu cần) |

---

## 9. Câu hỏi mở

Không có.
