# fix-01 — API `/api/du-an/von-giai-ngan` trả sai Giai đoạn hiện tại của dự án

## Mô tả lỗi

Gọi `GET /api/du-an/von-giai-ngan?Nam=2026` trả về giai đoạn hiện tại của dự án **sai**.

Dữ liệu thực tế với dự án **"dự án NT"**:

```json
{
  "tenDuAn": "dự án NT",
  "tenGiaiDoanHienTai": "Giai đoạn xin chủ trương đầu tư",
  "giaiDoanHienTaiId": null
}
```

Hiện API trả:

- `tenGiaiDoanHienTai = "Giai đoạn xin chủ trương đầu tư"` → **SAI**

## Nghiệp vụ đúng

Dự án này hiện đang ở **Bước 19** (Bảo hành sản phẩm, đảm bảo vận hành hệ thống).

Theo flow nghiệp vụ:

- Bước 19 thuộc **Giai đoạn kết thúc đầu tư**.

Kết quả đúng phải là:

```json
{
  "tenGiaiDoanHienTai": "Giai đoạn kết thúc đầu tư"
}
```

và `giaiDoanHienTaiId` tương ứng với giai đoạn này.

## Tác nhân

- Người dùng xem báo cáo tổng hợp vốn giải ngân (endpoint read-only, không phân quyền đặc biệt ngoài `[Authorize]`).

## Kỳ vọng

- Endpoint `/du-an/von-giai-ngan` trả `tenGiaiDoanHienTai` + `giaiDoanHienTaiId` khớp với giai đoạn của **bước hiện tại** của dự án.
- Không hard-code theo số bước (VD `if (buoc == 19) ...`) — phải dùng đúng nguồn mapping quy trình → giai đoạn.

## Phạm vi

- Sửa code Application. **KHÔNG** backfill / UPDATE / INSERT / DELETE dữ liệu DB.
- **KHÔNG** tạo migration (không đổi schema).
- Không sửa lan sang các API/endpoint khác nếu không cần.

## Liên quan

- `docs/issues/fix-01/report.md` — kết quả trace + root cause + hướng sửa
- `docs/issues/fix-01/journal.md` — nhật ký thực hiện
- `docs/issues/fix-01/test-workflow.md` — cách verify
