# Issue #9592 - Phòng ban phụ trách chính & phòng ban phối hợp

## Feature
Hiển thị Phòng ban phụ trách chính và Danh sách Phòng ban phối hợp trong endpoint `DuAnBuocGetTreeListQuery` để FE hiển thị trên giao diện.

## Entities & Naming

| Spec Name                  | Actual Name                    |
| -------------------------- | ------------------------------ |
| `PhongPhuTrachChinhId`     | **PhongPhuTrachChinhId** (long?) |
| `PhongBanPhuTrachChinh`    | **PhongBanPhuTrachChinh** (string?) - tên đơn vị |
| `DanhSachPhongBanPhoiHops` | **DanhSachPhongBanPhoiHops** (List<string>) - danh sách tên đơn vị |

## API Endpoints

### GET - Lấy danh sách bước
| Method | Endpoint                                  | Purpose     |
| ------ | ----------------------------------------- | ----------- |
| GET    | `/api/du-an-buoc/danh-sach/{duAnId}`      | Lấy danh sách bước |

#### Response (DuAnBuocStateDto)
```typescript
interface DuAnBuocStateDto {
  // ... existing fields ...
  PhongPhuTrachChinhId: number | null;   // ID phòng ban phụ trách chính
  PhongBanPhuTrachChinh: string | null;  // Tên phòng ban phụ trách chính
  DanhSachPhongBanPhoiHops: string[];    // Danh sách tên phòng ban phối hợp
}
```

### POST - Tạo mới bước
| Method | Endpoint                        | Purpose |
| ------ | ------------------------------- | ------- |
| POST   | `/api/du-an-buoc/them-moi`      | Tạo mới |

#### Request (DuAnBuocCreateDto)
```typescript
interface DuAnBuocCreateDto {
  DuAnId: Guid;
  BuocId: number;
  TenBuoc?: string;
  NgayDuKienBatDau?: Date;
  NgayDuKienKetThuc?: Date;
  GhiChu?: string;
  TrachNhiemThucHien?: string;
  DanhSachManHinh?: number[];
  PhongPhuTrachChinhId?: number;         // ID phòng ban phụ trách chính
  DanhSachPhongBanPhoiHopIds?: number[];  // Danh sách ID phòng ban phối hợp
}
```

### PUT - Cập nhật bước
| Method | Endpoint                          | Purpose |
| ------ | --------------------------------- | ------- |
| PUT    | `/api/du-an-buoc/cap-nhat`        | Cập nhật |

#### Request (DuAnBuocUpdateDto)
```typescript
interface DuAnBuocUpdateDto {
  Id: number;
  TenBuoc?: string;
  Used?: boolean;
  NgayDuKienBatDau?: Date;
  NgayDuKienKetThuc?: Date;
  DanhSachManHinh?: number[];
  PhongPhuTrachChinhId?: number;         // ID phòng ban phụ trách chính
  DanhSachPhongBanPhoiHopIds?: number[];  // Danh sách ID phòng ban phối hợp
}
```

## FE Mapping

| Backend Field              | FE Display                |
| -------------------------- | ------------------------- |
| `PhongBanPhuTrachChinh`    | Trường "Phòng ban phụ trách chính" |
| `DanhSachPhongBanPhoiHops` | Trường "Phòng ban phối hợp" (danh sách) |

## Implementation Details

### DTO Changes
- **File**: `QLDA.Application/DuAnBuocs/DTOs/DuAnBuocStateDto.cs`
- **Added Fields**:
  - `PhongPhuTrachChinhId` (long?) - ID từ LeftOuterJoin với DanhMucDonVi
  - `PhongBanPhuTrachChinh` (string?) - Tên đơn vị từ DanhMucDonVi
  - `DanhSachPhongBanPhoiHops` (List<string>) - Danh sách tên từ DuAnBuocPhongBanPhoiHop join DanhMucDonVi

### Query Implementation (GET)
- **File**: `QLDA.Application/DuAnBuocs/Queries/DuAnBuocGetTreeListQuery.cs`
- **LeftOuterJoin pattern** với `DanhMucDonVi` (legacy table - không có navigation property)
- Sử dụng `GroupJoin` để lấy PhongBanPhoiHops theo từng DuAnBuoc

### Create Implementation (POST)
- **File**: `QLDA.Application/DuAnBuocs/Commands/DuAnBuocCreateCommand.cs`
- Set `PhongPhuTrachChinhId` trên entity
- Tạo `DuAnBuocPhongBanPhoiHops` từ `DanhSachPhongBanPhoiHopIds`

### Update Implementation (PUT)
- **File**: `QLDA.Application/DuAnBuocs/Commands/DuAnBuocUpdateCommand.cs`
- Update `PhongPhuTrachChinhId`
- Clear và re-add `DuAnBuocPhongBanPhoiHops` từ `DanhSachPhongBanPhoiHopIds`

### Join Logic
```
DuAnBuoc
  └─ LeftOuterJoin → DanhMucDonVi (PhongPhuTrachChinhId → Id)
  └─ Join → DuAnBuocPhongBanPhoiHop → DanhMucDonVi (RightId → Id)
```

## Related Entities

| Entity                  | Purpose                                      |
| ----------------------- | -------------------------------------------- |
| `DuAnBuoc`             | Thực thể chính có PhongPhuTrachChinhId       |
| `DuAnBuocPhongBanPhoiHop` | Junction table (LeftId=int, RightId=long)   |
| `DanhMucDonVi`         | Legacy table - tên đơn vị (DmDonVi)          |

## Implementation Files

| File | Status |
| ---- | ------ |
| `QLDA.Application/DuAnBuocs/DTOs/DuAnBuocStateDto.cs` | ✅ Updated |
| `QLDA.Application/DuAnBuocs/DTOs/DuAnBuocCreateDto.cs` | ✅ Updated |
| `QLDA.Application/DuAnBuocs/DTOs/DuAnBuocUpdateDto.cs` | ✅ Updated |
| `QLDA.Application/DuAnBuocs/Queries/DuAnBuocGetTreeListQuery.cs` | ✅ Updated |
| `QLDA.Application/DuAnBuocs/Commands/DuAnBuocCreateCommand.cs` | ✅ Updated |
| `QLDA.Application/DuAnBuocs/Commands/DuAnBuocUpdateCommand.cs` | ✅ Updated |