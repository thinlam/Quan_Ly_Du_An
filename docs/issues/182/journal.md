# Nhật ký công việc — Issue 182

## 2026-08-14

- Khảo sát source trước khi code (G-312 / reset `DuAnBuoc` khi `PUT api/du-an/cap-nhat`):
  - `DuAnController.Update` / `Create`
  - `DuAnUpdateCommand` + `DuAn.Update()` (`QuyTrinhId` bị ghi đè trên entity tracked)
  - `DuAnBuocCloneCommand` (sync theo `BuocId`: insert / update metadata / delete bước thừa; wipe `NgayDuKien*` vì TODO tắt tính ngày)
  - Entity `DuAn.QuyTrinhId`, `DuAnBuoc` (field tiến độ)
  - `DuAnBuocMapPhongBanCommand` chỉ chạy lúc thêm mới → không lấy `PhongPhuTrachChinhId` làm “đã nhập tiến độ”
- Viết `index.md`, `report.md`, `journal.md`, `test-workflow.md`.
- Bổ sung `report.md` mục 6–7: hướng sửa, inject repo, snippet `Update` + `HasDuAnBuocTienDoAsync`, map 3 case, file đụng tới, rủi ro.
- **Đã code** `DuAnController.Update`: đọc `oldQuyTrinhId` AsNoTracking trước `DuAnUpdateCommand`; chỉ `DuAnBuocCloneCommand` khi QT đổi và `HasDuAnBuocTienDoAsync` = false.
- Test T1–T5 trong `QLDA.Tests/Integration/DuAnControllerTests.cs`.
- Không migration. Không commit/push đến khi user review diff.

### Việc tiếp theo

- User tự migration nếu cần (issue này không đổi schema).
- Review diff rồi commit khi user yêu cầu.
