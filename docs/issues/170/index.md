# Issue #170 — Bổ sung field Kế hoạch lựa chọn nhà thầu

**Issue:** #170

**Branch đề xuất:** `feature/ke-hoach-lua-chon-nha-thau-bo-sung-thong-tin`

**API:** `api/ke-hoach-lua-chon-nha-thau`

**Phạm vi flow:** Thêm mới · Cập nhật · Chi tiết · Danh sách

**Trạng thái:** Code done — chờ user tạo migration + apply DB

---

## 1. Yêu cầu nghiệp vụ

| # | Nghiệp vụ | Hành động |
|---|-----------|-----------|
| 1 | Đổi hiển thị "Số QĐ" → "Số tờ trình" | Reuse field số hiện có; không đổi DB column |
| 2 | Tổng dự toán | Thêm mới; bắt buộc; nhập tay |
| 3 | Tổng dự toán thẩm định giá | Thêm mới; không bắt buộc; nhập tay |
| 4 | Nguồn vốn | Thêm mới; CBB từ nguồn vốn của Dự án đang chọn |
| 5 | Thời gian thực hiện (năm) | Thêm mới; lưu số năm |

---

## 2. Survey source

### 2.1. Files hiện tại (module chính — không gồm RutGon)

| Layer | File |
|-------|------|
| Controller | `QLDA.WebApi/Controllers/KeHoachLuaChonNhaThauController.cs` |
| Insert | `QLDA.Application/KeHoachLuaChonNhaThaus/Commands/KeHoachLuaChonNhaThauInsertCommand.cs` |
| Update | `QLDA.Application/KeHoachLuaChonNhaThaus/Commands/KeHoachLuaChonNhaThauUpdateCommand.cs` |
| Delete | `QLDA.Application/KeHoachLuaChonNhaThaus/Commands/KeHoachLuaChonNhaThauDeleteCommand.cs` |
| Chi tiết | `QLDA.Application/KeHoachLuaChonNhaThaus/Queries/KeHoachLuaChonNhaThauQuery.cs` |
| Danh sách | `QLDA.Application/KeHoachLuaChonNhaThaus/Queries/KeHoachLuaChonNhaThauGetDanhSachQuery.cs` |
| DTO | `DTOs/KeHoachLuaChonNhaThauDto.cs`, `InsertDto.cs`, `UpdateDto.cs` |
| Mapping | `KeHoachLuaChonNhaThauMappings.cs` |
| Entity | `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` |
| Base | `QLDA.Domain/Entities/VanBanQuyetDinh.cs` |
| EF Config | `QLDA.Persistence/Configurations/KeHoachLuaChonNhaThauConfiguration.cs` |

Endpoints:

- `POST them-moi`
- `PUT cap-nhat`
- `GET {id}/chi-tiet`
- `GET danh-sach-tien-do`

### 2.2. Entity / DB hiện có

`KeHoachLuaChonNhaThau` kế thừa `VanBanQuyetDinh` (TPT — table riêng `KeHoachLuaChonNhaThau`).

**Trên KHLCNT:** `Ten`, `LoaiKeHoach` (+ navigation `GoiThaus`, `QuyetDinhDuyetKHLCNT`, `DangTaiKeHoachLcntLenMang`).

**Từ base `VanBanQuyetDinh`:** `DuAnId`, `BuocId`, `So`, `Ngay`, `TrichYeu`, `NguoiKy`, `NgayKy`, `CoQuanQuyetDinh`, `Loai`, …

**Chưa có trên entity/DB:**

- `TongDuToan`
- `DuToanThamDinh`
- `NguonVonId`
- `ThoiGianThucHien`

→ **Cần migration mới.**

### 2.3. DTO Insert / Update / Detail / List hiện tại

| Property DTO | Entity | Ghi chú |
|--------------|--------|---------|
| `DuAnId` | `DuAnId` | Insert + Detail/List |
| `BuocId` | `BuocId` | |
| `Ten` | `Ten` | |
| `SoQuyetDinh` | `So` | FE đang hiểu là "Số QĐ" |
| `NgayQuyetDinh` | `Ngay` | |
| `TrichYeu` | `TrichYeu` | |
| `NgayKy` / `NguoiKy` | tương ứng | |
| `DanhSachTepDinhKem` | Attachment | |

DTO implements `IQuyetDinh` (`SoQuyetDinh`, `NgayQuyetDinh`, `TrichYeu`).

Validate insert hiện tại: trùng `So` theo `DuAnId` → message *"Số quyết định đã tồn tại"*.

### 2.4. Nguồn vốn của Dự án (reuse)

| Thành phần | Chi tiết |
|------------|----------|
| Junction | `DuAnNguonVon` (`LeftId` = DuAnId, `RightId` = NguonVonId) |
| Danh mục | `DanhMucNguonVon` |
| DuAn DTO | `DanhSachNguonVon: List<int>?` |
| CBB API sẵn có | `GET api/danh-muc-nguon-von/danh-sach?duAnId={guid}` |
| Query filter | `DanhMucNguonVonGetDanhSachQuery` — filter `DuAnNguonVons` theo `LeftId` |
| Pattern lưu child | `GoiThau.NguonVonId`, `DuToanDauTu.NguonVonId` → `int?` |

**Không tạo** danh mục / API nguồn vốn mới.

### 2.5. Pattern kiểu dữ liệu tham chiếu trong project

| Field | Tham chiếu | Kiểu |
|-------|-----------|------|
| Tiền / dự toán | `DuToanDauTu.TongDuToan` | `long?` / DB `bigint` |
| Năm thực hiện | `GoiThau.ThoiGianThucHienGoiThau` | `int?` |
| Nguồn vốn FK | nhiều entity | `int?` + nav `DanhMucNguonVon` |

---

## 3. Quyết định thiết kế

### 3.1. "Số QĐ" → "Số tờ trình"

| Layer | Quyết định |
|-------|------------|
| DB column `VanBanQuyetDinh.So` | **Giữ nguyên** — không rename / không migration cho field này |
| DTO property `SoQuyetDinh` | **Giữ nguyên** — thuộc `IQuyetDinh`; đổi tên sẽ phá FE + nhiều module dùng chung interface |
| Mapping `SoQuyetDinh` ↔ `So` | Giữ |
| FE / Swagger label | Đổi hiển thị thành **"Số tờ trình"** |
| Message validate trùng | Cập nhật copy: *"Số tờ trình đã tồn tại"* (optional, cùng commit nếu sửa InsertCommand) |

**Lý do:** Task yêu cầu không rename DB nếu không cần; property API đang map đúng dữ liệu số văn bản.

### 3.2. Field mới trên `KeHoachLuaChonNhaThau`

| Nghiệp vụ | Property | Kiểu C# | Required | DB |
|-----------|----------|---------|----------|-----|
| Tổng dự toán | `TongDuToan` | `long` | **Có** | `bigint` NOT NULL (hoặc nullable + validate app — ưu tiên validate app với `long` / `long?` + ThrowIf null/≤0 theo convention hiện tại) |
| Tổng DT thẩm định giá | `DuToanThamDinh` | `long?` | Không | `bigint` NULL |
| Nguồn vốn | `NguonVonId` | `int?` | Không bắt buộc trừ khi BA yêu cầu thêm | `int` NULL + FK `DanhMucNguonVon` |
| Thời gian thực hiện (năm) | `ThoiGianThucHien` | `int?` | Không bắt buộc trừ khi BA yêu cầu thêm | `int` NULL |

**Ghi chú validate `TongDuToan`:** bắt buộc ở Insert/Update handler (`ManagedException.ThrowIf`).

**Ghi chú `NguonVonId`:** khi có giá trị, validate thuộc `DuAnNguonVon` của `DuAnId` đang lập KHLCNT (Insert lấy từ DTO; Update lấy từ entity.DuAnId).

### 3.3. Response Nguồn vốn (chi tiết / danh sách)

Follow pattern `GoiThau`: trả `NguonVonId` trên DTO.

Tên nguồn vốn: FE lấy từ CBB / `api/danh-muc-nguon-von`. Nếu list cần hiển thị tên ngay, bổ sung `TenNguonVon` trong projection (`e.NguonVon!.Ten`) — **chỉ khi** list UI cần; mặc định đủ `NguonVonId`.

### 3.4. Migration

- **Có** — migration mới sau khi sửa Domain + EF Configuration.
- Không sửa migration cũ.
- Không sửa tay `AppDbContextModelSnapshot.cs`.
- Không drop DB / column ngoài scope.

Tạo qua convention project: `ef.bat add ...` (sau khi entity + configuration xong).

### 3.5. Phạm vi ngoài task (không làm)

- `KeHoachLuaChonNhaThauRutGon` và module liên quan khác
- Refactor `IQuyetDinh` / rename `SoQuyetDinh` toàn solution
- Đổi schema `VanBanQuyetDinh`
- Tạo danh mục nguồn vốn mới
- FE (chỉ ghi chú contract cho FE)

---

## 4. Đồng bộ 4 API

### Thêm mới (`POST them-moi`)

Request nhận thêm:

- `SoQuyetDinh` (nghiệp vụ: Số tờ trình)
- `TongDuToan` (required)
- `DuToanThamDinh` (optional)
- `NguonVonId` (optional; CBB từ dự án)
- `ThoiGianThucHien` (optional; năm)

Persist qua `InsertDto.ToEntity()` + validate.

### Cập nhật (`PUT cap-nhat`)

Cho phép cập nhật các field trên; `Update()` mapping + validate `TongDuToan` / `NguonVonId`.

### Chi tiết (`GET {id}/chi-tiet`)

`ToDto()` trả đủ field mới (+ `NguonVonId`).

### Danh sách (`GET danh-sach-tien-do`)

Bổ sung projection trong `KeHoachLuaChonNhaThauGetDanhSachQuery`.

Controller: **không đổi signature** nếu chỉ mở rộng DTO.

---

## 5. Chỗ còn thiếu cần bổ sung (gap)

### 5.1. Domain / DB — thiếu hoàn toàn

| Property | Có trên entity hiện tại? | Có trên DB? | Cần làm |
|----------|--------------------------|-------------|---------|
| `TongDuToan` | Không | Không | Thêm entity + migration |
| `DuToanThamDinh` | Không | Không | Thêm entity + migration |
| `NguonVonId` (+ nav `NguonVon`) | Không | Không | Thêm entity + FK config + migration |
| `ThoiGianThucHien` | Không | Không | Thêm entity + migration |

`So` / `SoQuyetDinh`: **đã có** — chỉ đổi label FE + (optional) message validate.

### 5.2. Application — thiếu trên mọi luồng

| Chỗ | Insert | Update | Detail (`ToDto`) | List (projection) |
|-----|--------|--------|------------------|-------------------|
| `TongDuToan` | thiếu | thiếu | thiếu | thiếu |
| `DuToanThamDinh` | thiếu | thiếu | thiếu | thiếu |
| `NguonVonId` | thiếu | thiếu | thiếu | thiếu |
| `ThoiGianThucHien` | thiếu | thiếu | thiếu | thiếu |
| Validate `TongDuToan` required | thiếu | thiếu | — | — |
| Validate `NguonVonId` thuộc dự án | thiếu | thiếu | — | — |
| Message trùng số → "Số tờ trình…" | message cũ *"Số quyết định…"* | — | — | — |

### 5.3. Không thiếu / không đụng

| Hạng mục | Ghi chú |
|----------|---------|
| Controller endpoints | Giữ nguyên; DTO mở rộng là đủ |
| CBB nguồn vốn API | Đã có `GET api/danh-muc-nguon-von/danh-sach?duAnId=` |
| `KeHoachLuaChonNhaThauRutGon` | Ngoài scope |
| Rename DB `VanBanQuyetDinh.So` | Không làm |
| Rename `IQuyetDinh.SoQuyetDinh` | Không làm |

### 5.4. Files cần sửa

1. `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs`
2. `QLDA.Persistence/Configurations/KeHoachLuaChonNhaThauConfiguration.cs`
3. Migration mới `QLDA.Migrator/Migrations/` (EF sinh snapshot)
4. `…/DTOs/KeHoachLuaChonNhaThauDto.cs`
5. `…/DTOs/KeHoachLuaChonNhaThauInsertDto.cs`
6. `…/DTOs/KeHoachLuaChonNhaThauUpdateDto.cs`
7. `…/KeHoachLuaChonNhaThauMappings.cs`
8. `…/Commands/KeHoachLuaChonNhaThauInsertCommand.cs`
9. `…/Commands/KeHoachLuaChonNhaThauUpdateCommand.cs`
10. `…/Queries/KeHoachLuaChonNhaThauGetDanhSachQuery.cs`
11. Docs: cập nhật `report.md` / `journal.md` sau khi xong

Controller: **không sửa** (trừ khi cần XML comment / không bắt buộc).

---

## 6. Các bước code (implementation plan)

> Agent/dev làm theo thứ tự. Mỗi bước xong thì tick. Không refactor ngoài scope.

### Bước 0 — Chuẩn bị

- [ ] Checkout / tạo branch `feature/ke-hoach-lua-chon-nha-thau-bo-sung-thong-tin`
- [ ] Đọc lại §3 quyết định thiết kế trong file này
- [ ] (GitNexus) `impact` trước khi sửa symbol chính nếu MCP available

### Bước 1 — Domain entity

File: `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs`

- [ ] Thêm:
  - `public long TongDuToan { get; set; }` — required ở app layer
  - `public long? DuToanThamDinh { get; set; }`
  - `public int? NguonVonId { get; set; }`
  - `public int? ThoiGianThucHien { get; set; }`
  - `public DanhMucNguonVon? NguonVon { get; set; }` (nav)
- [ ] `using` namespace `DanhMuc` nếu cần

### Bước 2 — EF Configuration

File: `QLDA.Persistence/Configurations/KeHoachLuaChonNhaThauConfiguration.cs`

- [ ] Giữ config `LoaiKeHoach` hiện có
- [ ] Thêm FK `NguonVon` giống `DuToanDauTuConfiguration`:

```csharp
builder.HasOne(e => e.NguonVon)
    .WithMany()
    .HasForeignKey(e => e.NguonVonId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired(false);
```

- [ ] Không cần config đặc biệt cho `long` / `int?` trừ khi project chỗ khác gắn `HasPrecision` — follow snapshot các entity tiền `bigint` hiện có

### Bước 3 — Migration

- [ ] `ef.bat QLDA add AddKeHoachLuaChonNhaThauBoSungThongTin` (hoặc tên tương đương)
- [ ] Review migration sinh ra: chỉ `AddColumn` trên `KeHoachLuaChonNhaThau` (+ FK nếu có)
- [ ] **Không** sửa tay migration cũ / snapshot
- [ ] Nếu migration sai: `ef.bat QLDA remove` rồi add lại

### Bước 4 — DTO

Files: `KeHoachLuaChonNhaThauDto`, `InsertDto`, `UpdateDto`

- [ ] Thêm 4 property: `TongDuToan`, `DuToanThamDinh`, `NguonVonId`, `ThoiGianThucHien`
- [ ] **Giữ** `SoQuyetDinh` (không rename)
- [ ] Insert: `TongDuToan` kiểu `long` (hoặc `long?` + validate handler — chọn 1 và thống nhất)
- [ ] Update: cùng shape field mới

### Bước 5 — Mapping

File: `KeHoachLuaChonNhaThauMappings.cs`

- [ ] `ToEntity(InsertDto)`: map 4 field mới
- [ ] `ToDto(entity)`: map 4 field mới (+ `NguonVonId`)
- [ ] `Update(entity, UpdateDto)`: gán 4 field mới

### Bước 6 — Insert command

File: `KeHoachLuaChonNhaThauInsertCommand.cs`

- [ ] Validate: `TongDuToan` bắt buộc (ThrowIf null/default theo convention đã chọn)
- [ ] Validate: nếu `NguonVonId` có giá trị → phải thuộc `DuAnNguonVon` của `Dto.DuAnId`
- [ ] Đổi message trùng số: *"Số tờ trình đã tồn tại"* (thay *"Số quyết định đã tồn tại"*)
- [ ] Inject `IRepository<DuAnNguonVon, …>` hoặc query qua pattern sẵn có nếu cần

### Bước 7 — Update command

File: `KeHoachLuaChonNhaThauUpdateCommand.cs`

- [ ] Validate `TongDuToan` required
- [ ] Validate `NguonVonId` thuộc `entity.DuAnId` khi có giá trị
- [ ] Mapping đã cover persist

### Bước 8 — Danh sách projection

File: `KeHoachLuaChonNhaThauGetDanhSachQuery.cs`

- [ ] Trong `.Select(e => new KeHoachLuaChonNhaThauDto { … })` bổ sung 4 field
- [ ] Không gọi Mediator trong projection

### Bước 9 — Chi tiết

- [ ] Không đổi Controller nếu `ToDto` đã map đủ (GetQuery trả entity → `ToDto`)
- [ ] Smoke-check response chi tiết có đủ field mới

### Bước 10 — Build + verify

- [ ] `dotnet build` (solution / project liên quan) — hết compile error
- [ ] Manual / API check 4 luồng theo §7
- [ ] Cập nhật `report.md` (Files Changed, Status)
- [ ] Cập nhật `journal.md`

### Bước 11 — Commit / PR (khi user yêu cầu)

- [ ] Domain + Persistence.Configuration + Migrator **cùng 1 commit group** (rule project)
- [ ] Application/DTO/commands có thể cùng hoặc tách commit logic — nhưng migration không tách khỏi Domain/Config
- [ ] `detect_changes` trước commit nếu GitNexus available

---

## 7. Expected result (acceptance)

1. Thêm mới KHLCNT lưu đủ thông tin mới.
2. `TongDuToan` bắt buộc.
3. `DuToanThamDinh` cho phép null.
4. Nguồn vốn chọn từ nguồn vốn của dự án (`NguonVonId` + CBB `duAnId`).
5. Thời gian thực hiện lưu số năm (`int?`).
6. Update đọc/ghi đúng.
7. Chi tiết trả đủ field.
8. Danh sách không thiếu field cần hiển thị.
9. Không ảnh hưởng module khác / không đụng RutGon / không rename `VanBanQuyetDinh.So`.

---

## 8. Ghi chú mở (chốt khi implement nếu BA bổ sung)

- `NguonVonId` / `ThoiGianThucHien` có bắt buộc không? → hiện **optional** theo mô tả task (chỉ nêu required cho Tổng dự toán).
- Có cần `TenNguonVon` trên list/detail DTO không? → mặc định **không**; chỉ `NguonVonId`.
