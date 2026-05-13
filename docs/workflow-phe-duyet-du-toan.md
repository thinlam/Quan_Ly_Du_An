# PheDuyetDuToan Workflow

## Overview

PheDuyetDuToan (Budget Approval) is a polymorphic workflow entity used by multiple modules: PheDuyetDuToan (#9583), PhanKhaiKinhPhi (#9467), HoSoDeXuatCapDoCntt (#9488), HoSoMoiThauDienTu (#9473/9485). All share the same state machine and permission pattern via `PheDuyetDispatch*` commands.

## State Machine

```
[Dự thảo] --trình--> [Đã trình] --duyệt--> [Đã duyệt]
                      |
                      +--trả lại--> [Trả lại] --trình--> [Đã trình]
                      |
                      +--từ chối--> [Từ chối]
```

| State | Code | Description |
|-------|------|-------------|
| Dự thảo | `DT` | Initial draft, editable |
| Đã trình | `ĐTr` | Submitted for approval |
| Đã duyệt | `ĐD` | Approved |
| Trả lại | `TL` | Returned with feedback, can re-submit |
| Từ chối | `TC` | Rejected, workflow ends |

## Actions & Permissions

| Action | Endpoint | Permission | From State |
|--------|----------|------------|------------|
| **Tạo mới** | `POST /api/phe-duyet-du-toan/them-moi` | None (middleware) | — |
| **Cập nhật** | `PUT /api/phe-duyet-du-toan/cap-nhat` | Owner or staff | `DT` only |
| **Trình** | `POST /api/phe-duyet/{type}/{id}/trinh` | `PhongBanID == 219` (KH-TC) | `DT`, `TL` |
| **Duyệt** | `POST /api/phe-duyet/{type}/{id}/duyet` | `HasRole(QLDA_LDDV)` | `ĐTr` |
| **Trả lại** | `POST /api/phe-duyet/{type}/{id}/tra-lai` | `HasRole(QLDA_LDDV)` | `ĐTr` |
| **Từ chối** | `POST /api/phe-duyet/{type}/{id}/tu-choi` | `QLDA_LDDV` OR `PhongBanID == P.HC-TH` OR `HasRole(QLDA_QuanTri)` | `ĐTr` |
| **Chuyển phát hành** | `POST /api/phe-duyet/{type}/{id}/chuyen-phat-hanh` | `PhongBanID == P.HC-TH` OR `HasRole(QLDA_LDDV)` | `ĐD` |

## Entity Types

| Entity | Type Code | Feature |
|--------|-----------|---------|
| PheDuyetDuToan | `PheDuyetDuToan` | Phê duyệt dự toán |
| PhanKhaiKinhPhi | `PhanKhaiKinhPhi` | Phân khai kinh phí |
| HoSoDeXuatCapDoCntt | `HoSoDeXuatCapDoCntt` | Hồ sơ đề xuất cấp độ ATTT |
| HoSoMoiThauDienTu | `HoSoMoiThauDienTu` | Hồ sơ mời thầu điện tử |

## Key Files

**Entity:**
- `QLDA.Domain/Entities/PheDuyetDuToan.cs` — main entity (extends VanBanQuyetDinh)
- `QLDA.Domain/Entities/VanBanQuyetDinh.cs` — base entity
- `QLDA.Domain/Entities/PheDuyetHistory.cs` — polymorphic history

**Commands:**
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanInsertOrUpdateCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTrinhCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanDuyetCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTraLaiCommand.cs`
- `QLDA.Application/PheDuyetDuToans/Commands/PheDuyetDuToanTuChoiCommand.cs`

**Dispatch (type-based router):**
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTrinhCommand.cs`
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchDuyetCommand.cs`
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTraLaiCommand.cs`
- `QLDA.Application/QuanLyPheDuyet/Commands/PheDuyetDispatchTuChoiCommand.cs`

**Controllers:**
- `QLDA.WebApi/Controllers/PheDuyetDuToanController.cs`
- `QLDA.WebApi/Controllers/QuanLyPheDuyetController.cs`

**Config:**
- `QLDA.Domain/Constants/TrangThaiPheDuyetCodes.cs`
- `QLDA.Domain/Constants/PheDuyetEntityNames.cs`
- `QLDA.Domain/Constants/RoleConstants.cs`

## Roles Reference

| Role | Constant | Description |
|------|----------|-------------|
| Admin | `QLDA_TatCa` | Full access |
| System Admin | `QLDA_QuanTri` | System admin |
| Staff | `QLDA_ChuyenVien` | Department staff |
| Unit Leader | `QLDA_LDDV` | Lãnh đạo đơn vị |
| Admin Dept | `QLDA_HC_TH` | Phòng Hành chính - Tổng hợp |
| Planning | `QLDA_KH_TC` | Kế hoạch - Tổng hợp |