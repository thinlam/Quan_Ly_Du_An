# Nhật ký công việc — API chi tiết Quyết định duyệt KHLCNT thiếu 5 field

## 2026-08-18

- Trace flow `Controller → Query/Handler → DTO/Model → Mapping → Entity → EF Config`.
- Xác định nguyên nhân: 5 field nằm trên `KeHoachLuaChonNhaThau`; luồng chi tiết thiếu
  `Include`, thiếu property trên Model, thiếu mapping `ToModel`.
- Rà soát phạm vi: endpoint `danh-sach-tien-do` cũng thiếu; `them-moi`/`cap-nhat` không bị ảnh hưởng.
- Viết tài liệu phân tích (`index.md`, `report.md`, `test-workflow.md`, `journal.md`).
