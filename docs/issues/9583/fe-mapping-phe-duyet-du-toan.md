# FE Mapping Report: Phê duyệt dự toán - Workflow

> Issue: #9583 | Branch: `feat/9583-phe-duyet-du-toan-workflow`

## 1. Tổng quan

Chức năng phê duyệt dự toán bổ sung workflow trạng thái:

```
Dự thảo → Đã trình → Đã duyệt
                  ↘ Trả lại → (sửa) → Đã trình
```

## 2. API Endpoints

### Endpoints mới

| Method | URL | Request Body | Permission | Precondition |
|--------|-----|-------------|------------|-------------|
| POST | `/api/phe-duyet-du-toan/{id}/trinh` | none | PhongBanID=219 (KH-TC) | Status = DT hoặc TL |
| POST | `/api/phe-duyet-du-toan/{id}/duyet` | none | Role BGĐ | Status = ĐTr |
| POST | `/api/phe-duyet-du-toan/{id}/tra-lai` | `{ "noiDung": "string" }` | Role BGĐ | Status = ĐTr |

### Response format

Tất cả trả về `ResultApi`:

```json
// Success
{ "result": true, "data": 1, "message": null }

// Error (ManagedException → HTTP 200, result=false)
{ "result": false, "data": null, "message": "Chỉ phòng KH-TC có quyền trình phê duyệt dự toán" }
```

**Lưu ý:** ManagedException trả HTTP 200, không phải HTTP 400. Check `result` field.

### Endpoints hiện có (không thay đổi)

| Method | URL | Ghi chú |
|--------|-----|---------|
| GET | `/api/phe-duyet-du-toan/{id}/chi-tiet` | Response thêm field mới |
| POST | `/api/phe-duyet-du-toan/them-moi` | Không đổi |
| PUT | `/api/phe-duyet-du-toan/cap-nhat` | Không đổi |
| DELETE | `/api/phe-duyet-du-toan/{id}/xoa` | Không đổi |

## 3. DTO Response — Field mới

`PheDuyetDuToanDto` bổ sung 3 field:

| Field | Type | Default | Mô tả |
|-------|------|---------|-------|
| `trangThaiId` | `int` | `1` | FK → DmTrangThaiPheDuyetDuToan |
| `nguoiXuLyId` | `long?` | `null` | USER_MASTER.UserPortalId — người trình |
| `nguoiGiaoViecId` | `long?` | `null` | USER_MASTER.UserPortalId — người duyệt (BGĐ) |

## 4. Danh mục trạng thái

Table: `DmTrangThaiPheDuyetDuToan`

| Id | Ma | Tên | Mô tả |
|----|-----|-----|-------|
| 1 | `DT` | Dự thảo | Ban đầu, có thể sửa |
| 2 | `ĐTr` | Đã trình | Đã gửi duyệt, chờ BGĐ |
| 3 | `ĐD` | Đã duyệt | Đã phê duyệt |
| 4 | `TL` | Trả lại | BGĐ trả lại, cần sửa |

### Quy tắc chuyển trạng thái

```
DT ──Trình──→ ĐTr ──Duyệt──→ ĐD
                │
                └──Trả lại──→ TL ──Trình──→ đTr (vòng lặp)
```

- **Trình**: PhongBanID=219, status hiện tại phải là `DT` hoặc `TL`
- **Duyệt**: Role BGĐ, status phải là `ĐTr`
- **Trả lại**: Role BGĐ, status phải là `ĐTr`, bắt buộc `noiDung`

## 5. Request Models

### Trả lại (POST `{id}/tra-lai`)

```json
{
  "noiDung": "Lý do trả lại"  // Bắt buộc, không được rỗng
}
```

### Tạo/Cập nhật (không đổi)

`PheDuyetDuToanModel` — không có field mới. `trangThaiId` tự set = 1 khi tạo.

## 6. Permission Matrix

| Action | KH-TC (PhongBanID=219) | BGĐ (Role) | Khác |
|--------|:----------------------:|:----------:|:----:|
| Tạo mới | ✓ | ✓ | ✓ |
| Sửa draft | ✓ | ✓ | ✓ |
| **Trình** | **✓** | ✗ | ✗ |
| **Duyệt** | ✗ | **✓** | ✗ |
| **Trả lại** | ✗ | **✓** | ✗ |

## 7. Gợi ý FE

### Status Badge

| Status | Color | Label |
|--------|-------|-------|
| DT | `gray` / `default` | Dự thảo |
| ĐTr | `blue` / `warning` | Đã trình |
| ĐD | `green` / `success` | Đã duyệt |
| TL | `red` / `error` | Trả lại |

### Button Visibility

```
IF trangThaiId == 1 (DT) && currentUser.PhongBanID == 219:
    SHOW "Trình phê duyệt" button

IF trangThaiId == 2 (ĐTr) && currentUser.HasRole("BGĐ"):
    SHOW "Duyệt" button
    SHOW "Trả lại" button

IF trangThaiId == 4 (TL) && currentUser.PhongBanID == 219:
    SHOW "Trình lại" button (enable edit first)
```

### Confirmation Popups

- **Trình**: "Bạn có chắc chắn trình phê duyệt dự toán này?"
- **Duyệt**: "Bạn có chắc chắn duyệt phê duyệt dự toán này?"
- **Trả lại**: Popup nhập lý do (textarea, required) → "Bạn có chắc chắn trả lại?"

### Lấy thông tin User

`nguoiXuLyId` và `nguoiGiaoViecId` là `USER_MASTER.UserPortalId`. Cần Join qua API user để hiển thị tên.
