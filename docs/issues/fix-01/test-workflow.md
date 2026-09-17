# fix-01 — Test Workflow: von-giai-ngan sai Giai đoạn hiện tại

## Files sửa

- `QLDA.Application/DuAns/Commands/DuAnUpdatePhaseCommand.cs` — root cause (writer)
- `QLDA.Application/DuAns/Queries/TongHopVonGiaiNganQuery.cs` — endpoint projection

## Build

```bash
dotnet build QLDA.WebApi/QLDA.WebApi.csproj
```

## Verify bằng API (server dev chạy localhost:5183)

Lấy token (user có quyền xem báo cáo), sau đó:

```bash
curl -X 'GET' 'http://localhost:5183/api/du-an/von-giai-ngan?Nam=2026' \
  -H 'accept: text/plain' \
  -H 'Authorization: Bearer <TOKEN>'
```

### Kỳ vọng — dự án "dự án NT" (bước hiện tại = Bước 19)

| Field | Trước fix | Sau fix |
|-------|-----------|---------|
| `tenGiaiDoanHienTai` | "Giai đoạn xin chủ trương đầu tư" (SAI) | "Giai đoạn kết thúc đầu tư" |
| `giaiDoanHienTaiId` | `null` | `22` |
| `tenBuoc` | "Bước 19: Bảo hành sản phẩm, đảm bảo vận hành hệ thống trong suốt thời gian sử dụng" | không đổi |

Lý do: bước hiện tại 6912 → master `DmBuoc 435` (QuyTrinh 50) → `GiaiDoanId 22` = "Giai đoạn kết thúc đầu tư".

## Kiểm tra regression — writer

- Chọn 1 dự án còn đang tiến độ, tạo/cập nhật hồ sơ cho 1 bước thuộc giai đoạn sau giai đoạn hiện tại.
- Sau đó kiểm tra `DuAn.GiaiDoanHienTaiId` đã nhảy đúng phase của bước đó (không còn kẹt).
- Đảm bảo không lùi giai đoạn khi thao tác lại trên bước cũ (guard từ `DuAnUpdateStepCommand`).

## Ghi chú

- Không backfill DB, không chạy UPDATE/INSERT/DELETE dữ liệu.
- Không tạo migration.
- Các màn danh sách/dashboard khác chỉ tự đúng khi dự án tiến bước mới (71 dòng lệch cũ không được backfill).
