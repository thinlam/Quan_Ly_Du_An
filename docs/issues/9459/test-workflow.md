# UC22 - Test Workflow: Quản lý phê duyệt nội dung trình duyệt

## Thông tin chung

- **Issue**: #9459
- **Branch**: `feature/9459-quan-ly-phe-duyet-noi-dung-trinh-duyet`
- **Test file**: `QLDA.Tests/Integration/PheDuyetNoiDungControllerTests.cs`
- **Total tests**: 20 (all passing)

## Chạy test

```bash
# Toàn bộ tests PheDuyetNoiDung
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~PheDuyetNoiDung"

# Test cụ thể
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~PheDuyetNoiDungControllerTests.FullWorkflow_TrinhToPhatHanh"

# Toàn bộ tests PheDuyet (cả DuToan + NoiDung)
dotnet test QLDA.Tests/QLDA.Tests.csproj --filter "FullyQualifiedName~PheDuyet"
```

## Flow trạng thái (NoiDung)

```
CXL ──Duyet──→ DD ──KySo──→ DKS ──ChuyenQLVB──→ DQLVB ──PhatHanh──→ DPH
 │                │
 ├──TuChoi──→ TC   └──ChuyenQLVB──→ DQLVB
 │
 └──TraLai──→ TL ──GuiLai──→ CXL (loop)
```

## Constants Reference

```csharp
// Loai discriminator
TrangThaiPheDuyetCodes.Loai.DuToan  // "DuToan"
TrangThaiPheDuyetCodes.Loai.NoiDung // "NoiDung"

// NoiDung status codes
TrangThaiPheDuyetCodes.NoiDung.ChoXuLy      // "CXL"
TrangThaiPheDuyetCodes.NoiDung.DaDuyet       // "DD"
TrangThaiPheDuyetCodes.NoiDung.TuChoi        // "TC"
TrangThaiPheDuyetCodes.NoiDung.TraLai        // "TL"
TrangThaiPheDuyetCodes.NoiDung.DaKySo        // "DKS"
TrangThaiPheDuyetCodes.NoiDung.DaChuyenQLVB  // "DQLVB"
TrangThaiPheDuyetCodes.NoiDung.DaPhatHanh    // "DPH"

// DuToan status codes
TrangThaiPheDuyetCodes.DuToan.DuThao   // "DT"
TrangThaiPheDuyetCodes.DuToan.DaTrinh  // "ĐTr"
TrangThaiPheDuyetCodes.DuToan.DaDuyet  // "ĐD"
TrangThaiPheDuyetCodes.DuToan.TraLai   // "TL"
TrangThaiPheDuyetCodes.DuToan.Legacy   // "LEG"
```

## DanhMucTrangThaiPheDuyet CRUD

| Endpoint | Method | Role | Description |
|----------|--------|------|-------------|
| `api/danh-muc-trang-thai-phe-duyet/{id}` | GET | Admin | Get by ID (includes Loai) |
| `api/danh-muc-trang-thai-phe-duyet/danh-sach` | GET | Admin | List active |
| `api/danh-muc-trang-thai-phe-duyet/danh-sach-day-du` | GET | Admin | Full paginated list |
| `api/danh-muc-trang-thai-phe-duyet/them-moi` | POST | Admin | Create |
| `api/danh-muc-trang-thai-phe-duyet/cap-nhat` | PUT | Admin | Update |
| `api/danh-muc-trang-thai-phe-duyet/xoa-tam` | DELETE | Admin | Soft delete |

## Ma trận test case (PheDuyetNoiDung)

| # | Endpoint | Method | Role | Test case |
|---|----------|--------|------|-----------|
| 1 | `/api/phe-duyet-noi-dung/danh-sach` | GET | Any | GetDanhSach_ReturnsOk |
| 2 | `/{id}/chi-tiet` | GET | Any | GetChiTiet_ExistingId_ReturnsOk |
| 3 | `/{id}/chi-tiet` | GET | Any | GetChiTiet_NonExistentId_ReturnsFailure |
| 4 | `/{vbqdId}/trinh` | POST | Any | Trinh_CreatesPheDuyetNoiDung |
| 5 | `/{vbqdId}/trinh` | POST | Any | Trinh_DuplicateVBQD_ReturnsFailure |
| 6 | `/{id}/duyet` | POST | BGĐ (LDDV) | Duyet_AsBgdUser_ReturnsOk |
| 7 | `/{id}/duyet` | POST | Non-BGĐ | Duyet_AsNonBgdUser_ReturnsFailure |
| 8 | `/{id}/duyet` | POST | BGĐ | Duyet_WhenNotChoXuLy_ReturnsFailure |
| 9 | `/{id}/tu-choi` | POST | BGĐ | TuChoi_AsBgdUser_WithReason_ReturnsOk |
| 10 | `/{id}/tu-choi` | POST | BGĐ | TuChoi_WithoutReason_ReturnsFailure |
| 11 | `/{id}/tra-lai` | POST | BGĐ | TraLai_AsBgdUser_WithReason_ReturnsOk |
| 12 | `/{id}/ky-so` | POST | BGĐ | KySo_WhenDaDuyet_ReturnsOk |
| 13 | `/{id}/ky-so` | POST | BGĐ | KySo_WhenNotDaDuyet_ReturnsFailure |
| 14 | `/{id}/chuyen-qlvb` | POST | BGĐ | ChuyenQLVB_WhenDaDuyet_ReturnsOk |
| 15 | `/{id}/chuyen-qlvb` | POST | BGĐ | ChuyenQLVB_WhenDaKySo_ReturnsOk |
| 16 | `/{id}/phat-hanh` | POST | HC-TH | PhatHanh_AsHcthUser_ReturnsOk |
| 17 | `/{id}/phat-hanh` | POST | Non-HC-TH | PhatHanh_AsNonHcthUser_ReturnsFailure |
| 18 | `/{id}/gui-lai` | POST | Any | GuiLai_WhenTraLai_ReturnsOk |
| 19 | `/{id}/gui-lai` | POST | Any | GuiLai_WhenNotTraLai_ReturnsFailure |
| 20 | Full flow | - | Mixed | FullWorkflow_TrinhToPhatHanh |

### Full workflow test (end-to-end)

Test `FullWorkflow_TrinhToPhatHanh` chạy toàn bộ flow:
1. Tạo VBQD mới
2. **Trinh** → tạo PheDuyetNoiDung (CXL)
3. **Duyet** (BGĐ) → CXL → DD
4. **KySo** (BGĐ) → DD → DKS
5. **ChuyenQLVB** (BGĐ) → DKS → DQLVB
6. **PhatHanh** (HC-TH) → DQLVB → DPH
7. Kiểm tra **LichSu** có đầy đủ history

## Role mapping trong test

| Client | Roles | Mô tả |
|--------|-------|-------|
| AuthedClient | QLDA_QuanTri, QLDA_TatCa | Admin mặc định |
| BgdClient | QLDA_QuanTri, QLDA_LDDV | BGĐ (Duyet/TuChoi/TraLai/KySo/ChuyenQLVB) |
| HcthClient | QLDA_HC_TH | P.HC-TH (PhatHanh) |

## Refactoring Notes

- `DanhMucTrangThaiPheDuyetDuToan` → `DanhMucTrangThaiPheDuyet` (shared entity, table `DmTrangThaiPheDuyet`)
- `TrangThaiPheDuyetDuToanCodes` + `TrangThaiPheDuyetNoiDungCodes` → `TrangThaiPheDuyetCodes` (nested classes)
- Added `Loai` discriminator (constants: `TrangThaiPheDuyetCodes.Loai.DuToan` / `.NoiDung`)
- All PheDuyetDuToan commands updated to use `TrangThaiPheDuyetCodes.DuToan.*` + `DanhMucTrangThaiPheDuyet`
- All PheDuyetNoiDung commands updated to use `TrangThaiPheDuyetCodes.NoiDung.*`
- CRUD wired into shared `DanhMucGetQuery`/`DanhMucGetDanhSachQuery`/`DanhMucInsertOrUpdateCommand` infrastructure

## Lưu ý khi test trên server thật

1. Chạy migration: `dotnet ef migrations add AddPheDuyetNoiDung`
2. Cấu hình `PhongHCTHID` trong `appsettings.json` (ID phòng HC-TH thực tế)
3. Seed data `DmTrangThaiPheDuyet` (10 statuses with Loai) tự động qua migration
4. Role `QLDA_HC_TH` cần được thêm vào hệ thống role
