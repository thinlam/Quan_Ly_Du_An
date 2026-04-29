# Role Claim Policy - FE Mapping Guide

**Feature:** #9583 - Phân quyền dữ liệu theo phòng ban
**Ngày:** 29/04/2026
**Branch:** `feat/9583-role-claim-policy-visibility`

---

## 1. Tổng quan

Hệ thống phân quyền dữ liệu dựa trên **Role-Permission toggle** (bảng bật/tắt). Frontend cần:

1. Gọi API để lấy cấu hình quyền hiện tại
2. Ẩn/hiện UI elements dựa trên quyền của user
3. Hiển thị màn cấu hình quyền (chỉ Admin)

---

## 2. API Endpoints

### 2.1. Lấy danh sách cấu hình quyền

```
GET /api/cau-hinh-vai-tro-quyen/danh-sach?vaiTro={vaiTro}&nhomQuyen={nhomQuyen}
```

**Query params:**
| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `vaiTro` | string | No | Lọc theo tên vai trò (VD: `QLDA_ChuyenVien`) |
| `nhomQuyen` | string | No | Lọc theo nhóm (VD: `DuAn`, `GoiThau`) |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "vaiTro": "QLDA_TatCa",
      "quyenId": 1,
      "quyenMa": "DuAn.XemTatCa",
      "quyenTen": "Xem tất cả dự án",
      "nhomQuyen": "DuAn",
      "kichHoat": true
    }
  ]
}
```

### 2.2. Cập nhật bật/tắt quyền (Admin only)

```
PUT /api/cau-hinh-vai-tro-quyen/cap-nhat
Authorization: Bearer {token}
// Requires role: QLDA_TatCa or QLDA_QuanTri
```

**Body:**
```json
{
  "items": [
    { "id": 5, "kichHoat": false },
    { "id": 6, "kichHoat": true }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "data": 2
}
```

---

## 3. Permission Keys (FE Constants)

FE nên định nghĩa constants khớp với BE `PermissionConstants`:

```typescript
// permission-constants.ts
export const PERMISSIONS = {
  // DuAn
  DUAN_XEM_TAT_CA: 'DuAn.XemTatCa',
  DUAN_XEM_THEO_PHONG: 'DuAn.XemTheoPhong',
  DUAN_TAO: 'DuAn.Tao',
  DUAN_SUA: 'DuAn.Sua',
  DUAN_XOA: 'DuAn.Xoa',
  DUAN_PHE_DUYET: 'DuAn.PheDuyet',

  // GoiThau
  GOITHAU_XEM_TAT_CA: 'GoiThau.XemTatCa',
  GOITHAU_XEM_THEO_PHONG: 'GoiThau.XemTheoPhong',
  GOITHAU_TAO: 'GoiThau.Tao',
  GOITHAU_SUA: 'GoiThau.Sua',
  GOITHAU_XOA: 'GoiThau.Xoa',

  // HopDong
  HOPDONG_XEM_TAT_CA: 'HopDong.XemTatCa',
  HOPDONG_XEM_THEO_PHONG: 'HopDong.XemTheoPhong',
  HOPDONG_TAO: 'HopDong.Tao',
  HOPDONG_SUA: 'HopDong.Sua',
  HOPDONG_XOA: 'HopDong.Xoa',

  // VanBan
  VANBAN_XEM_TAT_CA: 'VanBan.XemTatCa',
  VANBAN_XEM_THEO_PHONG: 'VanBan.XemTheoPhong',
  VANBAN_TAO: 'VanBan.Tao',
  VANBAN_SUA: 'VanBan.Sua',
  VANBAN_XOA: 'VanBan.Xoa',

  // ThanhToan
  THANHTOAN_QUAN_LY: 'ThanhToan.QuanLy',
} as const;

export type PermissionKey = typeof PERMISSIONS[keyof typeof PERMISSIONS];
```

---

## 4. Role Constants (FE)

```typescript
// role-constants.ts
export const ROLES = {
  QLDA_TAT_CA: 'QLDA_TatCa',       // Toàn quyền
  QLDA_QUAN_TRI: 'QLDA_QuanTri',   // Quản trị hệ thống
  QLDA_CHUYEN_VIEN: 'QLDA_ChuyenVien', // Nhân viên
  QLDA_LD: 'QLDA_LD',              // Lãnh đạo
  QLDA_LDDV: 'QLDA_LDDV',         // Lãnh đạo đơn vị
} as const;

export const ADMIN_ROLES = [ROLES.QLDA_TAT_CA, ROLES.QLDA_QUAN_TRI];
```

---

## 5. Role → Permission Mapping (Default)

| Role | Xem tất cả | Xem theo phòng | Tạo | Sửa | Xóa | Phê duyệt |
|------|-----------|---------------|-----|-----|-----|-----------|
| `QLDA_TatCa` | ✅ All | ✅ (implied) | ✅ All | ✅ All | ✅ All | ✅ All |
| `QLDA_QuanTri` | ✅ All | ✅ (implied) | ✅ All | ✅ All | ✅ All | ✅ All |
| `QLDA_LDDV` | ✅ DuAn, GoiThau, HopDong, VanBan | — | — | — | — | — |
| `QLDA_LD` | ✅ DuAn, GoiThau, HopDong, VanBan | — | — | — | — | — |
| `QLDA_ChuyenVien` | — | ✅ All modules | ✅ All | ✅ All | — | — |

---

## 6. FE Permission Check Pattern

### 6.1. Từ JWT Token (client-side check)

JWT token đã chứa `Roles` và `Permissions` claims. FE có thể decode token để check nhanh:

```typescript
// Kiểm tra user có quyền tạo dự án không
function canCreateDuAn(token: DecodedToken): boolean {
  return token.Permissions?.includes(PERMISSIONS.DUAN_TAO) ?? false;
}

// Kiểm tra user có xem tất cả dự án không
function canViewAllDuAn(token: DecodedToken): boolean {
  return token.Permissions?.includes(PERMISSIONS.DUAN_XEM_TAT_CA) ?? false;
}
```

### 6.2. Server-side visibility (automatic)

**Không cần FE xử lý** — BE tự động filter data theo quyền:
- User có `DuAn.XemTatCa` → API trả tất cả dự án
- User có `DuAn.XemTheoPhong` → API chỉ trả dự án của phòng user
- User không có cả 2 → API trả mảng rỗng

### 6.3. UI ẩn/hiện theo permission

```typescript
// Ví dụ: ẩn nút "Thêm mới dự án" nếu user không có quyền
{hasPermission(PERMISSIONS.DUAN_TAO) && (
  <Button onClick={handleCreate}>Thêm mới</Button>
)}

// Ẩn nút "Phê duyệt" nếu không có quyền
{hasPermission(PERMISSIONS.DUAN_PHE_DUYET) && (
  <Button onClick={handleApprove}>Phê duyệt</Button>
)}
```

---

## 7. Admin Config UI

Màn cấu hình quyền chỉ dành cho `QLDA_TatCa` và `QLDA_QuanTri`.

### UI Layout gợi ý

```
┌─────────────────────────────────────────────────────┐
│ Cấu hình phân quyền                    [Lưu thay đổi]│
├─────────┬───────────────────────────────────────────┤
│ Vai trò │ DuAn │ GoiThau │ HopDong │ VanBan │ ThanhToan│
├─────────┼──────┼─────────┼─────────┼────────┼──────────┤
│ TatCa   │ ■ALL │ ■ALL    │ ■ALL    │ ■ALL   │ ■ALL     │
│ QuanTri │ ■ALL │ ■ALL    │ ■ALL    │ ■ALL   │ ■ALL     │
│ LDDV    │ ☑View│ ☑View   │ ☑View   │ ☑View  │ □        │
│ LD      │ ☑View│ ☑View   │ ☑View   │ ☑View  │ □        │
│ Chuyen  │ ☑Own │ ☑Own    │ ☑Own    │ ☑Own   │ □        │
│         │ ☑Tao │ ☑Tao    │ ☑Tao    │ ☑Tao   │          │
│         │ ☑Sua │ ☑Sua    │ ☑Sua    │ ☑Sua   │          │
└─────────┴──────┴─────────┴─────────┴────────┴──────────┘

■ = fixed (always on)   ☑ = toggle on   □ = toggle off
```

### NhomQuyen values cho FE filter

```typescript
export const PERMISSION_GROUPS = [
  { key: 'DuAn', label: 'Dự án' },
  { key: 'GoiThau', label: 'Gói thầu' },
  { key: 'HopDong', label: 'Hợp đồng' },
  { key: 'VanBan', label: 'Văn bản' },
  { key: 'ThanhToan', label: 'Thanh toán' },
] as const;
```

---

## 8. Workflow tích hợp FE

```
1. User login → FE decode JWT → lấy Roles + Permissions
2. FE hiển thị UI dựa trên Permissions (ẩn/hiện buttons, menus)
3. FE gọi GET danh-sach → BE auto-filter data theo quyền
4. Admin vào màn Cấu hình → gọi GET/PUT toggle API
5. Admin bật/tắt quyền → FE gọi PUT cap-nhat → DB cập nhật
6. User refresh → PolicyProvider reload → quyền mới áp dụng
```

---

## 9. Lưu ý quan trọng

1. **Permissions từ JWT vs DB:** JWT chứa permissions tại thời điểm login. Nếu admin thay đổi quyền, user cần re-login hoặc FE cần refresh token.
2. **Data visibility tự động:** FE không cần gửi permission parameter — BE tự filter dựa trên role của user.
3. **PhongKeToanID vẫn hoạt động:** Hardcode cũ vẫn giữ song song, sẽ remove sau.
4. **Cache:** PolicyProvider cache role-permission mapping trong request scope. Thay đổi DB sẽ có hiệu lực ở request tiếp theo.
