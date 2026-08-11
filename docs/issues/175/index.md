# Issue #175 — Cập nhật màn hình Trình duyệt dự toán

> **Nguồn:** PMIS / Redmine issue #175  
> **Nghiệp vụ:** Trình duyệt dự toán — form **Tờ trình phê duyệt dự toán**  
> **Trạng thái:** ✅ BE code xong — chờ user migration + FE  
> **Entity BE:** `QLDA.Domain/Entities/DuToanDauTu.cs`  
> **API chính:** `api/du-toan-dau-tu`

## 1. Mô tả yêu cầu

Chỉ thực hiện 3 thay đổi trên màn **Trình duyệt dự toán**:

| # | Hạng mục | Mô tả |
|---|----------|--------|
| 1 | **Tên dự toán** | Bắt buộc; validate null/empty/whitespace; lưu DB + trả lại chi tiết |
| 2 | **Phương án thiết kế** | Bỏ khỏi form; không bắt nhập/upload; **không** drop column DB |
| 3 | **Công văn đề nghị báo giá** | Upload file bắt buộc (≥1 file); tách GroupType; load đúng khi chi tiết |
| — | **Khác** | Upload file **không** bắt buộc; giữ behavior optional |

### Layout form sau chỉnh sửa

- Tên dự toán *
- Số tờ trình *
- Ngày trình
- Trích yếu
- Tổng dự toán (VNĐ) *
- Tổng dự toán thẩm định giá (VNĐ) *
- Các chi phí trong giai đoạn CBĐT
- Nguồn vốn
- Thời gian thực hiện (năm)
- **Công văn đề nghị báo giá *** — upload file, REQUIRED
- **Khác** — upload file, OPTIONAL

Không còn: **Phương án thiết kế**

### Acceptance / validation cases

| Case | Input | Expect |
|------|-------|--------|
| 1 | Không nhập Tên dự toán | Không lưu |
| 2 | Có Tên, không upload Công văn đề nghị báo giá | Không lưu |
| 3 | Có Tên + ≥1 file Công văn | Lưu thành công |
| 4 | Chỉ upload Khác, không có Công văn | Báo thiếu Công văn đề nghị báo giá |
| 5 | Mở lại bản ghi đã lưu | Tên + file Công văn load đúng |

---

## 2. Kết quả điều tra (pre-implement)

### 2.1. Module BE nào?

| Lớp | Vị trí | Ghi chú |
|-----|--------|---------|
| **FE** | Repo Frontend (**không** có trong `E:/SER`) | Form Tờ trình phê duyệt dự toán |
| **BE API** | `QLDA.WebApi/Controllers/DuToanDauTuController.cs` | Route `api/du-toan-dau-tu` |
| **Application** | `QLDA.Application/DuToanDauTu/` | Commands / Queries / DTO / Mappings |
| **Domain** | `QLDA.Domain/Entities/DuToanDauTu.cs` | Entity tiến độ CBĐT |
| **Persistence** | `QLDA.Persistence/Configurations/DuToanDauTuConfiguration.cs` | EF config |

> **Không nhầm** với `PheDuyetDuToan` (`api/phe-duyet-du-toan`) hay `QuyetDinhDuyetDuToan` (`api/quyet-dinh-duyet-du-toan`).

### 2.2. API endpoints

| Method | Route | Handler |
|--------|-------|---------|
| GET | `{id}/chi-tiet` | `DuToanDauTuGetQuery` + hydrate `GetAttachmentsQuery` |
| POST | `them-moi` | `DuToanDauTuInsertCommand` + `AttachmentBulkInsertOrUpdateCommand` |
| PUT | `cap-nhat` | `DuToanDauTuUpdateCommand` + attachment sync |
| DELETE | `{id}/xoa` | `DuToanDauTuDeleteCommand` |
| GET | `danh-sach-tien-do` | `DuToanDauTuGetPaginatedQuery` |

Body Insert/Update: `DuToanDauTuDto` (Application) — **không** tạo WebApi Model mới.

### 2.3. Field map UI ↔ BE hiện tại

| UI mong muốn | BE property | Có sẵn? |
|--------------|-------------|---------|
| Tên dự toán * | — | ❌ cần thêm `Ten` |
| Số tờ trình * | `SoToTrinh` | ✅ |
| Ngày trình | `NgayTrinh` | ✅ |
| Trích yếu | `TrichYeu` | ✅ |
| Tổng dự toán * | `TongDuToan` | ✅ |
| Tổng dự toán thẩm định giá * | `TongMucDauTu` | ✅ (tên property ≠ label UI; **không đổi** trong scope này) |
| Các chi phí trong giai đoạn CBĐT | `NoiDungChiPhis` ↔ DTO `NoiDungChiPhi` | ✅ |
| Nguồn vốn | `NguonVonId` | ✅ |
| Thời gian thực hiện (năm) | `Nam` | ✅ |
| Công văn đề nghị báo giá * | attachment | ⚠️ đang gộp 1 list `DanhSachTepDinhKem` / `EGroupType.DuToanDauTu` |
| Khác (optional) | attachment | ❌ chưa tách GroupType |
| Phương án thiết kế (bỏ UI) | `PhuongAnThietKeId` | ✅ còn trên Entity/DTO — **giữ cột, bỏ form** |

### 2.4. `Tên dự toán`

- Entity / DTO / Mapping / snapshot **không** có `Ten` / `TenDuToan`.
- Đề xuất property: **`Ten`** (`string`) — cùng pattern `KeHoachLuaChonNhaThau.Ten`.
- Validate Insert/Update: `string.IsNullOrWhiteSpace` → không lưu.
- **Cần migration** thêm cột trên bảng `DuToanDauTu`.

### 2.5. `Phương án thiết kế`

- DB: `PhuongAnThietKeId` (`int?`) + FK `DmPhuongAnThietKe`.
- Map đầy đủ Insert / Update / Detail / List (`TenPhuongAnThietKe`).
- **Không** drop column, **không** migration xóa.
- Scope: FE bỏ control; BE thôi bắt buộc (có thể giữ property để tương thích payload cũ).

### 2.6. Attachment — hiện trạng & đề xuất

**Hiện tại**

- 1 list: `DanhSachTepDinhKem`
- `GroupType` = `EGroupType.DuToanDauTu`
- Get chi tiết: `GetAttachmentsQuery(GroupIds)` **không** truyền `BaseGroupTypes` → lấy mọi file theo `GroupId`
- List query: subquery attachment cũng không filter GroupType
- **Chưa** validate số lượng file bắt buộc

**Pattern reuse:** `QuyetDinhDuyetDuToanController` — `DanhSachTepDinhKem` + `DanhSachTepDinhKemKhac` / `QuyetDinhDuyetDuToan` + `QuyetDinhDuyetDuToan_Khac`.

**Đề xuất (Approach A — khuyến nghị)**

| UI | GroupType | DTO list | Bắt buộc? |
|----|-----------|----------|-----------|
| Công văn đề nghị báo giá | **Reuse** `DuToanDauTu` | `DanhSachTepDinhKem` | Yes (≥1) |
| Khác | **Thêm** `DuToanDauTu_Khac` | `DanhSachTepDinhKemKhac` | No |

- Write: 2 lần `AttachmentBulkInsertOrUpdateCommand`, mỗi lần 1 `GroupTypes`, `AutoDeleteMissing = true`
- Read: 2 lần `GetAttachmentsQuery` với `BaseGroupTypes` tương ứng
- List: `AttachmentSubquery.ExpandGroupTypes` + filter `Contains` theo từng GroupType (không gọi Mediator trong projection)
- Validate ≥1 file Công văn ở Controller (hoặc Command nhận DTO) trước khi lưu — **không** áp sang `Khac`

**Không chọn**

- Approach B: tạo `DuToanDauTu_CongVanDeNghiBaoGia` + `_Khac` → file cũ dưới `DuToanDauTu` lệch / cần migrate data
- Approach C: 1 list + flag FE → dễ lẫn file

### 2.7. Migration

| Thay đổi | Cần migration? |
|----------|----------------|
| Thêm `Ten` trên `DuToanDauTu` | ✅ |
| Thêm `EGroupType.DuToanDauTu_Khac` | ❌ (string trên `TepDinhKem`) |
| Bỏ UI Phương án thiết kế | ❌ (giữ cột) |

**User tự tạo migration tay** (`ef.bat QLDA add …`) sau Domain + Persistence.Configuration. Agent không chạy migration. **Không** sửa migration cũ / **không** sửa tay `AppDbContextModelSnapshot.cs`.

### 2.8. FE

Repo FE **không** trong workspace. BE mở API/DTO; FE bind label `*`, ẩn Phương án thiết kế, tách 2 upload zone.

---

## 3. Acceptance Criteria

- [ ] `Ten` bắt buộc trên Insert/Update; null/empty/whitespace không lưu
- [ ] Chi tiết / danh sách trả `Ten` đúng
- [ ] Form không còn bắt Phương án thiết kế; không drop DB column
- [ ] Công văn đề nghị báo giá: ≥1 file (`EGroupType.DuToanDauTu` / `DanhSachTepDinhKem`)
- [ ] Khác optional (`DuToanDauTu_Khac` / `DanhSachTepDinhKemKhac`) — không inherit validate bắt buộc
- [ ] Chi tiết load đúng 2 list, không lẫn GroupType
- [ ] Case 1–5 như bảng trên
- [ ] Build OK; migration chỉ khi thêm `Ten`
- [ ] Không tạo WebApi Model mới; không refactor ngoài phạm vi

---

## 4. Tài liệu trong issue folder

| File | Mục đích |
|------|----------|
| [report.md](./report.md) | Design kỹ thuật + file sửa + checklist code |
| [journal.md](./journal.md) | Nhật ký công việc |
| [test-workflow.md](./test-workflow.md) | Cách verify sau implement |
