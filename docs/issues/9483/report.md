# Issue #9483 - Quyết định điều chỉnh

## Feature
Điều chỉnh QĐ phê duyệt dự toán/dự án/kế hoạch thuê dịch vụ CNTT theo yêu cầu riêng (các lần nếu có).

## Entities & Naming

| Spec Name                | Actual Name              |
| ------------------------ | ------------------------ |
| QuyetDinhDieuChinhDuToan | **QuyetDinhDieuChinh**   |
| ThongTinDieuChinhDuToan  | **ThongTinDieuChinhChiPhi**|

## Actors
CB.PCT, LĐ.PCT, GĐ/PGĐ, CB.PKH-TC, LD.PKH-TC, P.HC-TH, HĐTĐ

## Điều chỉnh nội dung QĐ
- **Loại điều chỉnh**: Mục tiêu/quy mô, TMĐT, Tiến độ, Chủ đầu tư, Tạm dừng, Nguồn vốn, Cơ cấu TMĐT
- **Số QĐ, ngày, trích yếu, lần, lý do, đính kèm file**
- **TMĐT & chi phí** (Xây lắp, thiết bị, khác, dự phòng) - chỉ hiển thị khi loại điều chỉnh = TMĐT

## UI Rules
- **ChiPhi section** - chỉ hiển thị khi `LoaiDieuChinhId` = "Điều chỉnh tổng mức đầu tư"
- **Readonly**: Chờ thẩm định, Chờ duyệt → không cho chỉnh sửa/xóa

## API Endpoints

| Method | Endpoint                                   | Purpose              |
| ------ | ------------------------------------------ | -------------------- |
| GET    | `/api/quyet-dinh-dieu-chinh/danh-sach`     | Danh sách            |
| GET    | `/api/quyet-dinh-dieu-chinh/{id}/chi-tiet` | Chi tiết             |
| POST   | `/api/quyet-dinh-dieu-chinh/them-moi`       | Thêm mới             |
| PUT    | `/api/quyet-dinh-dieu-chinh/cap-nhat`        | Cập nhật             |

### Query Params (GET /danh-sach)
| Param | Type | Description |
|-------| ---- | ----------- |
| `duAnId` | Guid? | Filter by dự án |
| `pheDuyetEntityName` | string? | Entity type |
| `pheDuyetEntityId` | Guid? | Entity ID |
| `buocId` | int? | Filter by bước phê duyệt |
| `globalFilter` | string? | Search |
| `page` | int (default: 1) | Page |
| `pageSize` | int (default: 20) | Page size |

## Quy trình phê duyệt (Dispatch Hub)

Dùng chung `QuanLyPheDuyetController` với `{type} = "QuyetDinhDieuChinh"`:

| Method | Endpoint                                 | Purpose            |
| ------ | ---------------------------------------- | ------------------ |
| POST   | `/api/phe-duyet/{type}/{id}/trinh`        | Trình phê duyệt    |
| POST   | `/api/phe-duyet/{type}/{id}/duyet`        | Duyệt              |
| POST   | `/api/phe-duyet/{type}/{id}/tra-lai`      | Trả lại (cần lý do)|
| POST   | `/api/phe-duyet/{type}/{id}/tu-choi`      | Từ chối (cần lý do)|

### Request Models
```typescript
interface PheDuyetActionModel {
  NoiDung: string;  // lý do
}
```

### Examples
```
POST /api/phe-duyet/QuyetDinhDieuChinh/{id}/trinh
Body: { "NoiDung": "Trình phê duyệt điều chỉnh" }

POST /api/phe-duyet/QuyetDinhDieuChinh/{id}/duyet
Body: (empty)

POST /api/phe-duyet/QuyetDinhDieuChinh/{id}/tra-lai
Body: { "NoiDung": "Cần bổ sung chi phí" }
```

## Types Endpoint

| Method | Endpoint                | Purpose                        |
| ------ | ---------------------- | ------------------------------ |
| GET    | `/api/phe-duyet/types`  | Lấy danh sách loại đối tượng phê duyệt |

### PheDuyetTypeItemDto
```typescript
interface PheDuyetTypeItemDto {
  Key: string;    // e.g., "QuyetDinhDieuChinh"
  Label: string;  // e.g., "Quyết định điều chỉnh"
}
```

### Fixed Types
| Key                   | Label                     |
| --------------------- | ------------------------- |
| `PheDuyetDuToan`      | Phê duyệt dự toán         |
| `QuyetDinhDieuChinh`  | Quyết định điều chỉnh     |

## Status Codes
- 1: Mới tạo
- 2: Chờ thẩm định
- 3: Chờ duyệt
- 4: Đã duyệt
- 5: Trả lại

## Model DTOs

### QuyetDinhDieuChinhModel (Request)
```typescript
interface QuyetDinhDieuChinhModel {
  Id: Guid;
  PheDuyetEntityId: Guid;
  PheDuyetEntityName: string;
  DuAnId: Guid;
  SoQuyetDinh?: string;
  NgayQuyetDinh?: Date;
  TrichYeu?: string;
  LoaiDieuChinhId: number;
  LyDo?: string;
  TepDinhKem?: string;
  ChiPhi?: ThongTinDieuChinhChiPhiModel;
}

interface ThongTinDieuChinhChiPhiModel {
  TongMucDauTu?: number;
  ChiPhiXayLap?: number;
  ChiPhiThietBi?: number;
  ChiPhiKhac?: number;
  ChiPhiDuPhong?: number;
}
```

### QuyetDinhDieuChinhListItemDto
```typescript
interface QuyetDinhDieuChinhListItemDto {
  Id: Guid;
  SoQuyetDinh: string;
  NgayQuyetDinh: Date;
  TrichYeu: string;
  LoaiDieuChinh: string;
  TenDuAn: string;
  TrangThai: number;
}
```

### QuyetDinhDieuChinhChiTietDto
```typescript
interface QuyetDinhDieuChinhChiTietDto {
  Id: Guid;
  PheDuyetEntityId: Guid;
  PheDuyetEntityName: string;
  DuAnId: Guid;
  SoQuyetDinh: string;
  NgayQuyetDinh: Date;
  TrichYeu: string;
  NoiDungChinh: string;
  Lan: number;
  LyDo: string;
  TepDinhKem: string;
  LoaiDieuChinhId: number;
  ChiPhi: ThongTinDieuChinhChiPhiDto;
}
```

## Tables
| Table                   | Purpose                              |
| ----------------------- | ------------------------------------ |
| `QuyetDinhDieuChinh`    | Số QĐ, ngày, trích yếu, lần, lý do, file |
| `ThongTinDieuChinhChiPhi`| TMĐT, Chi phí (Xây lắp, thiết bị, khác, dự phòng) |

## Related Entities
| Entity | Used For |
|--------|----------|
| `DuAn` | PheDuyetEntityName = "DuAn" |
| `KeHoachLuaChonNhaThau` | PheDuyetEntityName = "KeHoachLuaChonNhaThau" |
| `LoaiDieuChinh` | Lookup loại điều chỉnh |

## Implementation Files
| File | Status |
| ---- | -------|
| `QLDA.Application/QuyetDinhDieuChinhs/*` | ✅ Existed |
| `QLDA.Domain/QuyetDinhDieuChinhs/*` | ✅ Existed |
| `QLDA.Persistence/Configurations/QuyetDinhDieuChinhConfiguration.cs` | ✅ Existed |
