# Issue #9643 — Bổ sung trường Loại hợp đồng & Hình thức hợp đồng cho KetQuaTrungThau

> **Nguồn:** [pmis.vietinfo.tech:8080/issues/9643](https://pmis.vietinfo.tech:8080/issues/9643)
> **Trạng thái:** 📝 Mới ghi nhận — chưa triển khai
> **Entity liên quan:** `QLDA.Domain/Entities/KetQuaTrungThau.cs`
> **Catalog tham chiếu:** `DanhMucLoaiHopDong` (đã có sẵn — `QLDA.Domain/Entities/DanhMuc/DanhMucLoaiHopDong.cs`)

## 1. Mô tả yêu cầu

Bổ sung **2 trường thông tin** cho `KetQuaTrungThau` (Kết quả LCNT):

| # | Loại control | Tên trường          | Kiểu dữ liệu | Bắt buộc | Mô tả |
|---|--------------|---------------------|--------------|----------|-------|
| 1 | **CBB**      | `LoaiHopDong`       | Danh mục `DanhMucLoaiHopDong` (đã có) | Không | Combobox bind vào catalog Loại hợp đồng hiện hữu |
| 2 | **Text**     | `HinhThucHopDong`   | `string`     | Không    | Ô nhập tự do — người dùng gõ vào, không ràng buộc catalog |

> Lưu ý từ issue: "CBB Loại hợp đồng (danh mục đã có sẵn)" → tận dụng `DanhMucLoaiHopDong` đã có, không tạo mới danh mục.

## 2. Phạm vi thay đổi (BE)

### 2.1. Domain — `QLDA.Domain/Entities/KetQuaTrungThau.cs`

Bổ sung 2 property:

```csharp
#region Issue #9643

/// <summary>
/// Loại hợp đồng — liên kết DanhMucLoaiHopDong (đã có sẵn)
/// </summary>
public int? LoaiHopDongId { get; set; }

/// <summary>
/// Hình thức hợp đồng — text tự do nhập tay
/// </summary>
public string? HinhThucHopDong { get; set; }

#endregion

#region Navigation Properties

// ... existing
public DanhMucLoaiHopDong? LoaiHopDong { get; set; }

#endregion
```

### 2.2. Persistence — `QLDA.Persistence/Configurations/KetQuaTrungThauConfiguration.cs`

- `Property(e => e.HinhThucHopDong)` → `.HasMaxLength(...)` (khuyến nghị 250–500, đồng nhất với các text field khác).
- `Property(e => e.LoaiHopDongId)` → FK + index.
- Navigation `LoaiHopDong` → `HasOne(...).WithMany(...).HasForeignKey(e => e.LoaiHopDongId).OnDelete(DeleteBehavior.Restrict)` (mặc định của dự án, tránh cascade xoá nhầm catalog).

### 2.3. Application — DTO + Mapping

- `KetQuaTrungThauDto`: bổ sung `LoaiHopDongId`, `HinhThucHopDong`, `TenLoaiHopDong` (qua navigation).
- Mapping (`KetQuaTrungThauMappings.cs`): copy 2 field mới + `.Include`/navigation property.
- Search DTO: cân nhắc thêm `LoaiHopDongId?` vào `KetQuaTrungThauSearchDto` để lọc theo combobox.
- Insert/Update command: thêm 2 field vào payload.

### 2.4. WebApi — Controller

- `KetQuaTrungThauController.cs`: CRUD đã có sẵn — DTO thay đổi sẽ tự "bổ sung vào CRUD". Đảm bảo:
  - `[HttpGet]` trả `LoaiHopDongId`, `HinhThucHopDong`, `TenLoaiHopDong`.
  - `[HttpPost]` / `[HttpPut]` nhận 2 field mới.
  - Filter list: nếu `request.LoaiHopDongId.HasValue` → `.Where(e => e.LoaiHopDongId == request.LoaiHopDongId)`.

## 3. Phạm vi thay đổi (DB / Migration)

### 3.1. EF Core Migration

Theo quy tắc dự án — chạy migration bằng `QLDA.Migrator`:

```bash
cd QLDA.Migrator
dotnet ef migrations add AddLoaiHopDongAndHinhThucHopDongToKetQuaTrungThau \
    --startup-project . \
    --project ./QLDA.Migrator.csproj
```

Migration sẽ tạo 2 cột:

| Column           | Type           | Nullable | Note |
|------------------|----------------|----------|------|
| `LoaiHopDongId`  | `int`          | YES      | FK → `DanhMucLoaiHopDong(Id)` |
| `HinhThucHopDong`| `nvarchar(...)` | YES     | Độ dài theo `HasMaxLength` đã cấu hình |

> ⚠️ Theo quy tắc dự án (`CLAUDE.md`):
> - **KHÔNG** sửa `AppDbContextModelSnapshot.cs` hoặc các migration cũ.
> - **KHÔNG** edit `.cs` migration sau khi generate.
> - Migration Domain + Configuration + Migrator phải **cùng commit**.

## 4. Phạm vi thay đổi (FE — đối chiếu)

| Hạng mục FE | Mô tả |
|-------------|-------|
| Form thêm/sửa `KetQuaTrungThau` | Thêm 1 CBB `LoaiHopDong` (bind API `DanhMucLoaiHopDong`) + 1 textbox `HinhThucHopDong` |
| Bảng danh sách | (Tuỳ chọn) Hiển thị cột "Loại hợp đồng" / "Hình thức hợp đồng" |
| Search | (Tuỳ chọn) Thêm filter `LoaiHopDongId` |

## 5. Acceptance Criteria

- [ ] Domain: 2 property + 1 navigation được thêm vào `KetQuaTrungThau`.
- [ ] Configuration: FK + index cho `LoaiHopDongId`; `HasMaxLength` cho `HinhThucHopDong`.
- [ ] Migration: tạo mới bằng `QLDA.Migrator`, không sửa file cũ.
- [ ] DTO/Command: 2 field có mặt trong Insert/Update/Response.
- [ ] Controller: CRUD xử lý đầy đủ 2 field mới (kế thừa tự động qua DTO).
- [ ] Build `QLDA.WebApi.csproj` → **0 errors**.
- [ ] (Nếu có search) Filter theo `LoaiHopDongId` hoạt động đúng.

## 6. Liên quan

- `DanhMucLoaiHopDong` — `QLDA.Domain/Entities/DanhMuc/DanhMucLoaiHopDong.cs` (đã có, không tạo mới).
- `KetQuaTrungThauController` — `QLDA.WebApi/Controllers/KetQuaTrungThauController.cs`.
- Issue trước: [#9626](../9626/index.md) — đã có pattern thêm field numeric cho cùng entity.

---

<!-- REGION: Nội dung gốc từ yêu cầu ban đầu (không chỉnh sửa) -->

## 7. Yêu cầu gốc (Issue body)

> **Mô tả**
>
> Bổ sung trường thông tin:
> - **CBB Loại hợp đồng** : (danh mục đã có sẵn)
> - **Text Hình thức hợp đồng**: để nhập thông tin
>
> sau đó tiến hành tìm bảng KetQuaTrungThau bổ sung
> - `loaiHopDongId (int?)` liên kết `DanhMucLoaiHopDong`
> - `hinhThucHopDong (string)`
>
> sau khi bổ sung tạo migration + bổ sung vào CRUD của `KetQuaTrungThauController`.

<!-- END REGION -->
