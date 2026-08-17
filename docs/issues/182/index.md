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
| Không | Không quan trọng | Update bình thường. Không clone. |
| Có | Có (≥ 1 bước đã có tiến độ) | **Reject.** Message `"Quy trình không thể đổi"`. Không đổi `DuAn.QuyTrinhId`, không clone/reset bước, không save field khác của request. |
| Có | Chưa | Cho đổi QT + clone bước theo quy trình mới (`DuAnBuocCloneCommand`) |

`them-moi` dự án **không đổi** — vẫn clone + map phòng ban như hiện tại.

> **Không** hiểu “đã phát sinh `DuAnBuoc`” = chỉ cần có dòng bước. `them-moi` luôn clone bước. Predicate tiến độ = user đã nhập (mục 4 `report.md`), giống phase 1.

## 4. Field nguồn sự thật (source)

- Quy trình dự án: `DuAn.QuyTrinhId` (`int?`, FK danh mục quy trình).
- Tiến độ trên `DuAnBuoc` (user-entered) — chi tiết `report.md` mục 4.
- **Không** coi `PhongPhuTrachChinhId` là tiến độ: lệnh thêm mới tự map phòng phụ trách chính sau clone.

## 5. Tài liệu liên quan

- [`report.md`](./report.md) — khảo sát + cách sửa. **Phase 2 (chặn đổi QT): mục 11.**
- [`journal.md`](./journal.md) — nhật ký.
- [`test-workflow.md`](./test-workflow.md) — case verify (T4 đổi: reject).

## 6. Trạng thái

**Phase 1 (G-312 skip clone): đã merge PR #184.**  
**Phase 2 (không cho đổi `QuyTrinhId` khi đã có tiến độ): ĐÃ CODE.** Validate trong `DuAnUpdateCommandHandler` trước `entity.Update()`. Không migration/schema.
