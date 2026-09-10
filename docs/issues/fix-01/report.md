# fix-01 — Implementation Report: von-giai-ngan trả sai Giai đoạn hiện tại

## Status

| Hạng mục | Trạng thái |
|----------|-----------|
| Trace + root cause | ✅ Xong |
| Fix writer `DuAnUpdatePhaseCommand` | ✅ Done |
| Fix projection `TongHopVonGiaiNganQuery` | ✅ Done |
| Build / verify | ✅ Build Application pass; WebApi chỉ lock-file khi app đang chạy |

## Trace đầy đủ

```
GET /api/du-an/von-giai-ngan?Nam=2026
    ↓
DuAnController.GetTongHopVonGiaiNgan  (QLDA.WebApi/Controllers/DuAnController.cs:79)
    ↓  Mediator.Send(TongHopVonGiaiNganQuery(Nam, 0))
TongHopVonGiaiNganQueryHandler        (QLDA.Application/DuAns/Queries/TongHopVonGiaiNganQuery.cs:37)
    ↓  .Include(d => d.GiaiDoanHienTai).Include(d => d.BuocHienTai)
    ↓  projection dòng 53–78
BaoCaoDuAnDto                         (QLDA.Application/DuAns/DTOs/BaoCaoDuAnDto.cs)
    TenGiaiDoanHienTai  ← d.GiaiDoanHienTai.Ten
    GiaiDoanHienTaiId   ← (KHÔNG được gán → luôn null)
```

### Field liên quan

| Entity | Field | Vai trò |
|--------|-------|---------|
| `DuAn` | `BuocHienTaiId` (int?) → `DuAnBuoc` | Bước hiện tại (denormalized) |
| `DuAn` | `GiaiDoanHienTaiId` (int?) → `DanhMucGiaiDoan` | Giai đoạn hiện tại (denormalized) |
| `DuAnBuoc` | `BuocId` → `DmBuoc` (master step) | Bước instance của dự án |
| `DmBuoc` | `GiaiDoanId` → `DanhMucGiaiDoan` | **Mapping bước → giai đoạn (nguồn chuẩn)** |
| `DanhMucGiaiDoan` | `Stt` | Thứ tự giai đoạn (dùng bởi writer) |

## Dữ liệu thực tế (DB VI_DACDT — read-only)

Dự án **"dự án NT"** `08DEDFDB-50A1-3067-687A-7B122003CE93`:

| Trường | Giá trị | Ghi chú |
|--------|---------|---------|
| `QuyTrinhId` | 50 | |
| `BuocHienTaiId` | 6912 | DuAnBuoc "Bước 19: Bảo hành sản phẩm…" |
| `DuAnBuoc.BuocId` | 435 | master step QuyTrinh 50 |
| `DmBuoc 435.GiaiDoanId` | **22** | = "Giai đoạn kết thúc đầu tư" ✅ mapping ĐÚNG |
| `DuAn.GiaiDoanHienTaiId` | **19** | = "Giai đoạn xin chủ trương đầu tư" ❌ **cũ/stale** |

⇒ Mapping **bước → giai đoạn** (`DmBuoc.GiaiDoanId`) là **đúng**. Lỗi nằm ở cột denormalized `DuAn.GiaiDoanHienTaiId` bị **kẹt/stale**.

### Root cause

`DuAn.GiaiDoanHienTaiId` chỉ được duy nhất 1 writer duy trì: `DuAnUpdatePhaseCommand` (`QLDA.Application/DuAns/Commands/DuAnUpdatePhaseCommand.cs:36-56`).

Logic cũ:

```csharp
if (currentPhase == null || currentPhase.Stt < latestPhase.Stt)  // dòng 47
    SetPhase(...);
```

- Các giai đoạn mới `DmGiaiDoan` id **15–22** (dùng bởi QuyTrinh 50/46/…) đều có **`Stt = 0`** (bộ giai đoạn cũ id 5–8 có `Stt = 1..4`).
- Khi dự án tiến từ Bước 19 (phase 22) so với phase hiện tại 19: `currentPhase.Stt(0) < latestPhase.Stt(0)` → `0 < 0 = false` ⇒ **không bao giờ nâng giai đoạn**.

Kết quả: `tenBuoc` nhảy đúng (Bước 19) nhưng `GiaiDoanHienTai` bị **đóng băng** ở giai đoạn đầu tiên từng được set.

### Mức độ ảnh hưởng (dữ liệu)

- Tổng dự án: 205. Dự án có `BuocHienTaiId`: 157.
- Số dự án **lệch** (`GiaiDoanHienTaiId` null hoặc ≠ phase của bước hiện tại): **71/157**.
- Số dự án có phase mà không có bước: 0 (phase luôn đi kèm bước hiện tại).

⇒ Đây là lỗi **dữ liệu config (`DmGiaiDoan.Stt`)** cộng hưởng **lỗi logic writer** (`DuAnUpdatePhaseCommand` so `Stt`), **không phải** lỗi mapping `DmBuoc → GiaiDoan`.

### Thiếu sót phụ ở endpoint

- Projection `TongHopVonGiaiNganQuery` **không gán** `GiaiDoanHienTaiId` cho DTO ⇒ output luôn `null`.
- Projection chỉ đọc cột denormalized, **thiếu fallback** `BuocHienTai.Buoc.GiaiDoan` như convention của các query anh em (`DuAnGetDanhSachQuery.cs:87`, `BaoCaoDuAnGetDanhSachQuery.cs:78`).

## Hướng sửa (code-only)

### 1. Fix writer — `DuAnUpdatePhaseCommand`

Bỏ so sánh `Stt`. `BuocHienTaiId` đã được `DuAnUpdateStepCommand` guard **chỉ tiến-tới**, nên giai đoạn hợp lệ luôn là phase của **bước hiện tại** (`BuocHienTai.Buoc.GiaiDoanId`). Sửa handler thành: query bước hiện tại → `ExecuteUpdate` set `GiaiDoanHienTaiId` theo phase đó.

- Không đổi chữ ký command (≈60 controllers gọi `step → phase`, không ảnh hưởng compile).
- Không thể lùi giai đoạn vì bước hiện tại không lùi.

### 2. Fix projection — `TongHopVonGiaiNganQuery`

- `TenGiaiDoanHienTai`: lấy từ `BuocHienTai.Buoc.GiaiDoan.Ten` (nguồn chuẩn), fallback `GiaiDoanHienTai.Ten` khi không có bước.
- `GiaiDoanHienTaiId`: lấy từ `BuocHienTai.Buoc.GiaiDoanId`, fallback `GiaiDoanHienTaiId`.

Lý do lấy bước làm nguồn chuẩn: dữ liệu hiện hữu đang stale và **không được backfill DB**, nên chỉ có thể tự suy đúng từ bước hiện tại.

## Kết quả mong đợi

Với "dự án NT" (BuocHienTai 6912 → master 435 → GiaiDoan 22):

```json
{
  "tenGiaiDoanHienTai": "Giai đoạn kết thúc đầu tư",
  "giaiDoanHienTaiId": 22
}
```

## Phạm vi ảnh hưởng

- Sửa đúng **2 file Application**:
  - `QLDA.Application/DuAns/Commands/DuAnUpdatePhaseCommand.cs`
  - `QLDA.Application/DuAns/Queries/TongHopVonGiaiNganQuery.cs`
- Không đổi entity / schema / DTO contract / migration.
- `TongHopVonGiaiNganQuery` chỉ phục vụ endpoint này.
- Các reader khác của `GiaiDoanHienTaiId` (danh-sach, export, dashboard, theo-dõi…) **không bị đụng**.

> Lưu ý: do **không backfill DB**, các màn list/dashboard khác với 71 dòng lệch cũ vẫn sai cho đến khi dự án đó tiến bước mới (tự đúng sau fix này). Chỉ endpoint này đúng ngay với dữ liệu cũ.

## Risk

- Low. Chạy `impact` (gitnexus) trên `DuAnUpdatePhaseCommand` + `TongHopVonGiaiNganQueryHandler` trước khi edit; `detect_changes()` trước khi commit (theo AGENTS.md).
