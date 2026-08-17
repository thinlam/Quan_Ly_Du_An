# Issue 182 / G-312 — Reset tiến độ `DuAnBuoc` khi cập nhật Dự án

## 1. Mô tả

API `PUT api/du-an/cap-nhat` sau khi `DuAnUpdateCommand` **luôn** gọi `DuAnBuocCloneCommand`, không phân biệt quy trình có đổi hay `DuAnBuoc` đã có tiến độ. Người dùng có thể mất dữ liệu tiến độ đã nhập.

```http
PUT api/du-an/cap-nhat
```

## 2. Tác nhân / UI

- Người dùng có quyền cập nhật dự án (`GroupAdminOrManager`).
- Màn hình cập nhật dự án: đổi thông tin (ghi chú, lãnh đạo, phòng…) hoặc đổi **Quy trình dự án**.

## 3. Business rule

| Quy trình thay đổi? | Đã nhập tiến độ `DuAnBuoc`? | Xử lý |
|---|---|---|
| Không | Không quan trọng | Không clone — giữ nguyên bước/tiến độ |
| Có | Có (≥ 1 bước đã có tiến độ) | Không clone/reset — không được mất tiến độ |
| Có | Chưa | Được clone lại theo quy trình mới (`DuAnBuocCloneCommand`) |

`them-moi` dự án **không đổi** — vẫn clone + map phòng ban như hiện tại.

## 4. Field nguồn sự thật (source)

- Quy trình dự án: `DuAn.QuyTrinhId` (`int?`, FK danh mục quy trình).
- Tiến độ trên `DuAnBuoc` (user-entered) — chi tiết `report.md` mục 4.
- **Không** coi `PhongPhuTrachChinhId` là tiến độ: lệnh thêm mới tự map phòng phụ trách chính sau clone.

## 5. Tài liệu liên quan

- [`report.md`](./report.md) — khảo sát source, root cause, **cách sửa + snippet** (mục 6–7).
- [`journal.md`](./journal.md) — nhật ký.
- [`test-workflow.md`](./test-workflow.md) — 5 case verify.

## 6. Trạng thái

**Đã code.** `DuAnController.Update` chỉ clone khi QT đổi và chưa có tiến độ. Không migration/schema.
