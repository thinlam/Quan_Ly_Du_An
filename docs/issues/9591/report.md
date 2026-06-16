# Report #9591 — Cấu hình phòng ban thực hiện và thời gian dự kiến cho từng bước

> Cập nhật: 16/06/2026 — điều chỉnh theo cấu trúc thực tế trong code

## 1. Yêu cầu chi tiết

### Các thông tin cần bổ sung cho từng bước (`DuAnBuoc`):

| Field | Type | Nguồn / Ràng buộc |
|-------|------|-------------------|
| `NgayBatDauDuKien` | `DateTime?` | Ngày bắt đầu dự kiến của bước |
| `NgayKetThucDuKien` | `DateTime?` | Ngày kết thúc dự kiến của bước |
| `PhongPhuTrachChinhId` | `long?` | Chọn 1 từ danh sách phòng phụ trách chính + phòng phối hợp của dự án |
| `DuAnBuocPhongBanPhoiHops` | junction | Chọn từ danh sách phòng phối hợp của dự án |

**Mục đích:** Cấu hình phòng ban thực hiện cho từng bước cụ thể, gắn thời gian dự kiến. Từ đó phân quyền thao tác màn hình theo bước dựa trên cấu hình này.

---

## 2. Quy trình clone cấu hình (đã xác minh)

### Hiện trạng: Auto-clone từ `DanhMucBuoc` (template)

```
Tạo DuAn (chọn QuyTrinhId)
        ↓
Load DanhMucBuoc theo QuyTrinhId
        ↓
Clone 100% sang DuAnBuoc:
  • PhongPhuTrachChinhId       ← DuAn.DonViPhuTrachChinhId
  • DuAnBuocPhongBanPhoiHops   ← DuAnChiuTrachNhiemXuLys(DonViPhoiHop)
        ↓
User tùy chỉnh qua DuAnBuocController
```

**Files liên quan:**
- `QLDA.Domain/Entities/DuAnBuoc.cs:29` — `PhongPhuTrachChinhId`
- `QLDA.Domain/Entities/DuAnBuoc.cs:45` — `DuAnBuocPhongBanPhoiHops` (junction)
- `QLDA.Domain/Entities/DuAnBuocPhongBanPhoiHop.cs` — junction entity
- `QLDA.Domain/Entities/DanhMuc/DanhMucBuocPhongBanPhoiHop.cs` — junction ở template

### Điều chỉnh cần làm ở `DuAnBuocController`

| Hành động | Hiện tại | Cần thêm |
|-----------|----------|----------|
| Hiển thị danh sách phòng khả dụng | Có | Lấy từ `DuAn.DonViPhuTrachChinhId` + `DuAnChiuTrachNhiemXuLys(DonViPhoiHop)` |
| Validate chọn phòng | Có | Phải nằm trong danh sách phòng của dự án |
| Lưu `NgayBatDauDuKien` / `NgayKetThucDuKien` | Có | Thêm 2 field vào entity + DTO + migration |
| Lưu `PhongPhuTrachChinhId` + `DuAnBuocPhongBanPhoiHops` | Có | UI cho phép chỉnh sửa riêng từng bước |

---

## 3. Quy tắc phân quyền & Ví dụ

### Nguyên tắc chung (9 tác nhân)

| # | Tác nhân | Cơ chế xác định |
|---|----------|-----------------|
| 1 | **BGĐ / Lãnh đạo cấp cao** | `UserID == DuAn.LanhDaoPhuTrachId` |
| 2 | **Phòng KH-TC** | `PhongBanID == PhongKHTCID` |
| 3 | **Phòng Kế toán** | `PhongBanID == PhongKeToanID` |
| 4 | **Phòng HC-TH** | `PhongBanID == PhongHCTHID` |
| 5 | **Trưởng phòng phụ trách chính** | `QLDA_LDDV` + `PhongBanID == DuAn.DonViPhuTrachChinhId` |
| 6 | **Chuyên viên phòng phụ trách chính** | `QLDA_ChuyenVien` + `PhongBanID == DuAn.DonViPhuTrachChinhId` |
| 7 | **Trưởng phòng phối hợp** | `QLDA_LDDV` + `PhongBanID` ∈ `DuAnChiuTrachNhiemXuLys(DonViPhoiHop)` |
| 8 | **Chuyên viên phòng phối hợp** | `QLDA_ChuyenVien` + `PhongBanID` ∈ `DuAnChiuTrachNhiemXuLys(DonViPhoiHop)` |
| 9 | **Admin / Quản trị** | `QLDA_TatCa` hoặc `QLDA_QuanTri` |

### Quy tắc thao tác trong bước (`DuAnBuoc`)

Phòng được thao tác trong bước khi **1 trong 3 điều kiện** sau đúng:
1. Là tác nhân 1, 2, 9 (full quyền).
2. Phòng của user == `DuAnBuoc.PhongPhuTrachChinhId`.
3. Phòng của user ∈ `DuAnBuoc.DuAnBuocPhongBanPhoiHops`.

### Ví dụ cụ thể

Giả sử dự án do phòng **Trung tâm tư vấn** phụ trách chính, phòng **Nền tảng** phối hợp.

#### Bước 1: Đề xuất danh mục (có 3 màn hình)
- **Cấu hình bước:** Phòng **Nền tảng** phụ trách, phòng **Hạ tầng** phối hợp (sau khi user tùy chỉnh).
- **Quyền thao tác:** 4 phòng (KHTC, BGĐ, Nền tảng, Hạ tầng).
- Các phòng còn lại: chỉ xem, không thao tác.

#### Bước 2: Lập và trình (có 4 màn hình)
- **Cấu hình bước:** Phòng **Trung tâm tư vấn** phụ trách, không có phối hợp.
- **Quyền thao tác:** 3 phòng (KHTC, BGĐ, Trung tâm tư vấn).
- Các phòng còn lại: chỉ xem.

---

## 4. Phạm vi áp dụng cấu hình

| Loại màn hình | Áp dụng cấu hình theo bước |
|---------------|---------------------------|
| Màn hình trong `DuAnBuoc` (Hợp đồng, Gói thầu, Văn bản, ...) | ✅ Có |
| Màn hình `DuAn` (CRUD dự án) | ❌ Không — dùng `DuAn.DonViPhuTrachChinhId` + `LanhDaoPhuTrachId` |
| Màn hình `DeXuatChuTruongMoi` (đề xuất) | ⚠️ Riêng — dùng `DeXuatDonViXuLy` |

---

## 5. Lưu ý quan trọng

- **KHÔNG có "phòng theo dõi" trong bước.** Enum `EChiuTrachNhiemXuLy.DonViTheoDoi` tồn tại nhưng chưa được triển khai cho `DuAnBuoc`. `DuAnBuocPhongBanPhoiHops` chỉ chứa phòng phối hợp.
- **Trưởng phòng = `QLDA_LDDV`** (đã gộp với BGĐ, role `QLDA_LD` cũ đã bỏ).
- **Phân biệt Trưởng phòng vs Chuyên viên** trong cùng phòng:
  - Trưởng phòng (`QLDA_LDDV`): CRUD all trong phòng, được phân công + phê duyệt.
  - Chuyên viên (`QLDA_ChuyenVien`): chỉ thao tác bản ghi được phân công (`NguoiPhuTrachChinhId == UserID` hoặc `NguoiXuLyChinhId == UserID`).
- **Phòng HC-TH** xác định theo `PhongHCTHID` trong `appsettings.json`, không dùng role `QLDA_HC_TH` (đã bỏ).
