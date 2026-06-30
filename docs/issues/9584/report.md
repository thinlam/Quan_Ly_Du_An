# Issue 9584 — Phân quyền theo phòng ban

> Cập nhật: 16/06/2026 — điều chỉnh theo cấu trúc thực tế trong code

## 1. Yêu cầu (đã chốt)

| # | Yêu cầu | Trạng thái | Cách triển khai |
|---|---------|------------|-----------------|
| 1 | BGĐ / Lãnh đạo cấp cao xem/sửa/xóa tất cả dự án | ✅ | `UserID == DuAn.LanhDaoPhuTrachId` |
| 2 | Phòng chuyên môn (Trưởng phòng + Chuyên viên) chỉ thao tác dự án phòng mình | ✅ | `PhongBanID == DonViPhuTrachChinhId` + check `LanhDaoPhuTrachId` |
| 3 | Tương tự cho Gói thầu, Hợp đồng, Văn bản | ✅ | `ApplyDuAnChildVisibility()` |
| 4 | Phân biệt Trưởng phòng (CRUD all) vs Chuyên viên (chỉ bản ghi phân công) | ✅ | Check `user.HasRole(QLDA_LDDV)` vs `user.HasRole(QLDA_ChuyenVien)` |
| 5 | Phòng phối hợp chỉ xem dự án, CRUD màn hình con khi được gán trong `DuAnBuoc` | ✅ | Check `DuAnBuoc.PhongPhuTrachChinhId` + `DuAnBuocPhongBanPhoiHops` |

---

## 2. Kiến trúc hiện tại

### Hệ thống phân quyền 2 tầng

```
Layer 1: JWT Role (Login)          Layer 2: Permission Toggle (Runtime)
┌─────────────────────┐           ┌──────────────────────────────┐
│ QLDA_TatCa (Admin)   │──────────→│ CauHinhVaiTroQuyen table     │
│ QLDA_QuanTri         │           │ (VaiTro, QuyenId, KichHoat) │
│ QLDA_LDDV            │           │                              │
│ QLDA_ChuyenVien      │           │ ↓ PolicyProvider cache       │
│ PhongKHTCId          │           │ ↓ VisibilityFilterExtensions │
│ PhongHCTHId          │           │                              │
└─────────────────────┘           └──────────────────────────────┘
```

### Cấu trúc tổ chức (`DmDonVi`)

```
DmDonVi (cây phân cấp, DonViCapChaId)
│
├─ Cấp CHA (DonViCapChaId = null) ─── BGĐ / Lãnh đạo cấp cao
│      │
│      ├─ Phòng KH-TC ───── Trưởng phòng (QLDA_LDDV) + Chuyên viên (QLDA_ChuyenVien)
│      ├─ Phòng TTTV ───── Trưởng phòng + Chuyên viên
│      ├─ Phòng Nền tảng ─ Trưởng phòng + Chuyên viên
│      ├─ Phòng Hạ tầng ─── Trưởng phòng + Chuyên viên
│      ├─ Phòng HC-TH ───── (xác định bằng PhongHCTHID, không cần role)
│      └─ ... (các phòng khác)
```

---

## 3. 9 tác nhân (chốt)

| # | Tác nhân | Cơ chế xác định | Quyền |
|---|----------|-----------------|-------|
| 1 | **BGĐ / Lãnh đạo cấp cao** | `UserID == DuAn.LanhDaoPhuTrachId` (dropdown từ `sp_GetUsersByRoleName('QLDA_LDDV')`) | Xem/Sửa/Xóa/Phê duyệt dự án được gán |
| 2 | **Phòng KH-TC** (mọi role) | `PhongBanID == PhongKHTCId` | Global bypass — full quyền |
| 3 | **Phòng Kế toán** | `PhongBanID == PhongKeToanID` | CRUD ThanhToan |
| 4 | **Phòng HC-TH** | `PhongBanID == PhongHCTHID` | Tùy module (xem tất cả + một số quyền đặc thù) |
| 5 | **Trưởng phòng phụ trách chính** | `QLDA_LDDV` + `PhongBanID == DuAn.DonViPhuTrachChinhId` | Xem tất cả dự án phòng, CRUD all, phân công, phê duyệt |
| 6 | **Chuyên viên phòng phụ trách chính** | `QLDA_ChuyenVien` + `PhongBanID == DuAn.DonViPhuTrachChinhId` | Xem tất cả dự án phòng, CRUD chỉ bản ghi được phân công |
| 7 | **Trưởng phòng phối hợp** | `QLDA_LDDV` + `PhongBanID` ∈ `DuAnChiuTrachNhiemXuLys(DonViPhoiHop)` | Xem dự án, CRUD màn hình con khi phòng trong bước |
| 8 | **Chuyên viên phòng phối hợp** | `QLDA_ChuyenVien` + `PhongBanID` ∈ `DuAnChiuTrachNhiemXuLys(DonViPhoiHop)` | Xem dự án, CRUD màn hình con theo phân công khi phòng trong bước |
| 9 | **Admin / Quản trị** | `QLDA_TatCa` hoặc `QLDA_QuanTri` | Full quyền |

### Role mapping (chốt)

| Role | Mapping | Ghi chú |
|------|---------|---------|
| `QLDA_TatCa` | Admin hệ thống | Giữ |
| `QLDA_QuanTri` | Quản trị viên | Giữ |
| `QLDA_LDDV` | BGĐ + Trưởng phòng | Bao gồm cả BGĐ (đã gộp với role `QLDA_LD` cũ) |
| `QLDA_ChuyenVien` | Chuyên viên trong phòng | Giữ |
| ~~`QLDA_LD`~~ | **ĐÃ BỎ** | — |
| ~~`QLDA_HC_TH`~~ | **ĐÃ BỎ** (thay bằng `PhongHCTHID`) | — |

---

## 4. Ma trận quyền chi tiết

| Hành động | BGĐ (1) | KH-TC (2) | Kế toán (3) | HC-TH (4) | TP phụ trách (5) | CV phụ trách (6) | TP phối hợp (7) | CV phối hợp (8) | Admin (9) |
|-----------|---------|-----------|-------------|-----------|------------------|------------------|------------------|-----------------|-----------|
| Xem dự án | ✅ | ✅ | ✅ | ✅ | ✅ Phòng mình | ✅ Phòng mình | ✅ Dự án tham gia | ✅ Dự án tham gia | ✅ |
| CRUD DuAn | ✅ | ✅ | ❌ | ⚠️ | ✅ | ⚠️ Phân công | ❌ | ❌ | ✅ |
| Phân công CV | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ |
| Phê duyệt | ✅ | ✅ | ❌ | ⚠️ | ✅ | ❌ | ❌ | ❌ | ✅ |
| CRUD màn hình trong bước (khi phòng mình trong bước) | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ⚠️ Phân công | ✅ | ⚠️ Phân công | ✅ |
| CRUD HĐ/GT/VB trong dự án tham gia | ✅ | ✅ | ⚠️ | ⚠️ | ✅ | ⚠️ Phân công | ✅ (trong bước) | ⚠️ Phân công (trong bước) | ✅ |

**Chú thích:**
- ⚠️ Phân công: chỉ thao tác bản ghi có `NguoiPhuTrachChinhId == UserID` hoặc `NguoiXuLyChinhId == UserID`.
- ⚠️ Module: tùy theo phạm vi quyền của phòng (xem code thực tế).

---

## 5. Cơ chế xác định danh tính

### A. Theo **PhongBanID** (config trong `appsettings.json`)

| Phòng | Config key | File |
|-------|-----------|------|
| KH-TC | `PhongKHTCId` | `QLDA.WebApi/ConfigurationOptions/AppSettings.cs:11` *(trước là `PhongKHTCID` uppercase, đã gộp về 1)* |
| Kế toán | `PhongKeToanID` | `QLDA.WebApi/ConfigurationOptions/AppSettings.cs:14` |
| HC-TH | `PhongHCTHID` | `QLDA.WebApi/ConfigurationOptions/AppSettings.cs:16` |

### B. Theo **UserID** (cá nhân)

| Tác nhân | Cơ chế | Code tham chiếu |
|----------|--------|-----------------|
| BGĐ / Trưởng phòng được gán | `DuAn.LanhDaoPhuTrachId == UserID` | `QLDA.Application/Authorization/Providers/BuocAuthorizationProvider.cs:31-33` |
| Chuyên viên được phân công | `DuAnCongViec.NguoiPhuTrachChinhId == UserID` hoặc `DeXuatChuTruongMoi.NguoiXuLyChinhId == UserID` | `DuAnCongViecMappings.cs:6` |

### C. Theo **Role** (JWT claim)

| Role | Quyền mặc định | File tham chiếu |
|------|----------------|-----------------|
| `QLDA_TatCa` | Full (Tao/Sua/Xoa/PheDuyet) | `CauHinhVaiTroQuyenConfiguration.cs:38-45` |
| `QLDA_QuanTri` | Full | `CauHinhVaiTroQuyenConfiguration.cs:46-49` |
| `QLDA_LDDV` | XemTatCa + PheDuyetActions | `CauHinhVaiTroQuyenConfiguration.cs:50-55` |
| `QLDA_ChuyenVien` | XemTheoPhong + Tao/Sua | `CauHinhVaiTroQuyenConfiguration.cs:56-68` |

### D. Quan hệ 1-1: `QLDA_LDDV` ↔ `DuAn.LanhDaoPhuTrachId`

```
Nhóm user QLDA_LDDV (BGĐ + Trưởng phòng)
        ↓ sp_GetUsersByRoleName('QLDA_LDDV')
Dropdown chọn lãnh đạo phụ trách ở màn hình DuAn
        ↓
DuAn.LanhDaoPhuTrachId (long? = UserID)
        ↓
Check userId == LanhDaoPhuTrachId → bypass
        ↓
BuocAuthorizationProvider.CanExecuteStepAsync() + FilterVisibleSteps()
```

---

## 6. Files liên quan

| File | Vai trò |
|------|---------|
| `Domain/Constants/RoleConstants.cs` | Định nghĩa 4 roles (sau khi bỏ QLDA_LD, QLDA_HC_TH) |
| `Domain/Constants/PermissionConstants.cs` | Permission keys + default mapping |
| `Domain/Entities/CauHinhVaiTroQuyen.cs` | Entity role-permission toggle |
| `Domain/Entities/DanhMuc/DanhMucQuyen.cs` | Danh mục quyền (seed data) |
| `Application/Providers/IPolicyProvider.cs` | Interface check permission |
| `Application/Providers/PolicyProvider.cs` | Implementation: load DB → cache → check |
| `Application/Common/Extensions/VisibilityFilterExtensions.cs` | IQueryable filter theo permission |
| `Application/Authorization/Providers/BuocAuthorizationProvider.cs` | Bước-level + DuAn-level authorization |
| `Application/UserMasters/Queries/GetUserByRoleNameQuery.cs` | Load user theo role cho dropdown Lãnh đạo |
| `WebApi/Controllers/CauHinhVaiTroQuyenController.cs` | API config quyền runtime |
| `WebApi/ConfigurationOptions/AppSettings.cs` | Config PhongKHTCId, PhongKeToanID, PhongHCTHId |
| `Application/Providers/IAppSettingsProvider.cs` | Interface đọc config phòng ID |
| `Application/Common/DTOs/UserInfo.cs` | UserInfo.PhongBanID (từ JWT claim) |

---

## 7. Gaps cần xử lý

| # | Gap | Mô tả | Ưu tiên |
|---|-----|-------|---------|
| 1 | CUD theo phòng | CUD commands hiện chỉ check role, chưa áp `BuocAuthorizationProvider` | High |
| 2 | Phân biệt Trưởng phòng vs Chuyên viên | Cần check `user.HasRole(QLDA_LDDV)` để cho full CRUD; `QLDA_ChuyenVien` chỉ CRUD theo phân công | High |
| 3 | Phòng phối hợp CRUD màn hình trong bước | Cần filter CUD theo `DuAnBuoc.PhongPhuTrachChinhId` + `DuAnBuocPhongBanPhoiHops` | Medium |
| 4 | Chuyên viên CRUD theo phân công | Cần check `record.NguoiPhuTrachChinhId == UserID` cho `DuAnCongViec`; `NguoiXuLyChinhId == UserID` cho `DeXuatChuTruongMoi` | Medium |
| 5 | Validate khi gán `LanhDaoPhuTrachId` | Nên validate user được chọn có role `QLDA_LDDV` | Low |
| 6 | Phòng theo dõi | `DonViTheoDoi` enum tồn tại nhưng chưa có junction entity trong `DuAnBuoc` — chưa cần xử lý trong issue này | — |

---

## 8. Lưu ý quan trọng

- **KHÔNG có `PhongBgdID`** trong `appsettings.json`. BGĐ xác định theo `DuAn.LanhDaoPhuTrachId == UserID`.
- **KHÔNG còn role `QLDA_LD`** — đã gộp vào `QLDA_LDDV` (chỉ còn comment trong code, sắp xóa).
- **KHÔNG còn role `QLDA_HC_TH`** — phòng HC-TH xác định bằng `PhongHCTHID`.
- **"Phòng theo dõi"** (`DonViTheoDoi`): enum tồn tại, chưa có junction entity riêng trong `DuAnBuoc`. Hiện tại chỉ `DonViPhoiHop` được triển khai đầy đủ.
- **Phân công chuyên viên** dùng các field: `DuAnCongViec.NguoiPhuTrachChinhId`, `DeXuatChuTruongMoi.NguoiXuLyChinhId`, `NguoiTaoId` (cho audit).
