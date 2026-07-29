# Bổ sung TenLanhDaoPhuTrach khi in Tờ trình lập Ban QLDA — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Khi gọi `GET /api/print/to-trinh-lap-ban-qlda`, merge field `TenLanhDaoPhuTrach` trong file Word phải hiển thị `user_master.HoTen` join theo `DuAn.LanhDaoPhuTrachId = UserMaster.UserPortalId`; nếu không có dữ liệu thì để chuỗi rỗng, không lỗi.

**Architecture:** Không sửa `QuyetDinhLapBanQldaGetQuery` (đang trả domain entity và dùng chung CRUD). Tạo query/DTO print riêng theo đúng pattern đã có của `ToTrinhPheDuyetGetExportQuery` → `ToTrinhPheDuyetExportDto`. `PrintController` chỉ MailMerge, không query `UserMaster` trực tiếp.

**Tech Stack:** .NET 8, MediatR CQRS, EF Core, Aspose.Words MailMerge

## Global Constraints

- Clean Architecture + CQRS: logic đọc nằm ở Query + QueryHandler; không tạo `Application/Services`.
- Không tạo model mới trong `WebApi` nếu đã có DTO ở Application.
- Join bắt buộc: `DuAn.LanhDaoPhuTrachId` = `UserMaster.UserPortalId` (C# property). **Không** join `UserMaster.Id`.
- Không tạo/sửa migration, không sửa `AppDbContextModelSnapshot.cs`, không đổi schema.
- Không lấy tên từ JWT / user đăng nhập / `CreatedBy` / `UpdatedBy`.
- Không ảnh hưởng API print khác và CRUD `QuyetDinhThanhLapBanQldaController`.
- Null-safe: ID null / không tìm thấy user / `HoTen` null → `""`.
- Merge field key giữ nguyên: `TenLanhDaoPhuTrach`.

---

## Current Flow (đã xác minh trong source)

```text
GET /api/print/to-trinh-lap-ban-qlda?id=...&isMauDuThao=...
    → PrintController.InToTrinhThanhLapBanQLDA
    → Mediator.Send(QuyetDinhLapBanQldaGetQuery { Id, IncludeThanhVien = true })
    → trả entity QuyetDinhLapBanQLDA (+ ThanhViens)
    → replacements["TenLanhDaoPhuTrach"] = ""   ← BUG
    → Aspose MailMerge + DataTable ThanhVien
```

### Kiểu dữ liệu đã xác minh

| Field | Property C# | Kiểu | Ghi chú |
|---|---|---|---|
| Lãnh đạo dự án | `DuAn.LanhDaoPhuTrachId` | `long?` | Lưu **UserPortalId**, không phải PK |
| Portal user | `UserMaster.UserPortalId` | `long?` | Column DB: `User_PortalID` |
| Họ tên | `UserMaster.HoTen` | `string?` | |
| PK user | `UserMaster.Id` | `long` | **Không dùng để join** |

### Pattern tham chiếu sẵn có trong project

`QLDA.Application/ToTrinhPheDuyet/Queries/ToTrinhPheDuyetGetExportQuery.cs`:

```csharp
TenLanhDaoPhuTrach = userMaster.GetQueryableSet()
    .Where(u => u.UserPortalId == x.DuAn.LanhDaoPhuTrachId)
    .Select(u => u.HoTen)
    .FirstOrDefault(),
```

`PrintController` phiếu phê duyệt đã map:

```csharp
{ "NguoiDuyet", entity.TenLanhDaoPhuTrach != null ? entity.TenLanhDaoPhuTrach : "" }
```

### Callers của `QuyetDinhLapBanQldaGetQuery` (không được phá)

1. `PrintController.InToTrinhThanhLapBanQLDA` — sẽ chuyển sang print query mới
2. `QuyetDinhThanhLapBanQldaController` (GET by id + update preload) — **giữ nguyên**

---

## File map

| File | Action | Responsibility |
|---|---|---|
| `QLDA.Application/QuyetDinhLapBanQLDAs/DTOs/QuyetDinhLapBanQldaPrintDto.cs` | **Create** | Read model cho in: So, TrichYeu, SoDuThao, TrichYeuDuThao, TenLanhDaoPhuTrach, ThanhViens |
| `QLDA.Application/QuyetDinhLapBanQLDAs/Queries/QuyetDinhLapBanQldaGetPrintQuery.cs` | **Create** | Query + Handler: project DTO + subquery UserMaster qua UserPortalId |
| `QLDA.WebApi/Controllers/PrintController.cs` (~1610–1655) | **Modify** | Gọi print query mới; map `rows.TenLanhDaoPhuTrach` vào replacements |
| `QuyetDinhLapBanQldaGetQuery.cs` | Không sửa | Tránh ảnh hưởng CRUD |
| Domain / Persistence / Migrator | Không sửa | Không đổi schema |

---

### Task 1: Tạo `QuyetDinhLapBanQldaPrintDto`

**Files:**
- Create: `QLDA.Application/QuyetDinhLapBanQLDAs/DTOs/QuyetDinhLapBanQldaPrintDto.cs`
- Reuse: `QLDA.Application/QuyetDinhLapBanQLDAs/DTOs/ThanhVienBanQldaDto.cs` (đã có `Ten`, `ChucVu`, `VaiTro`)

**Interfaces:**
- Consumes: không
- Produces: `QuyetDinhLapBanQldaPrintDto` với property dùng bởi Task 2 và Task 3

- [ ] **Step 1: Tạo file DTO**

```csharp
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;

/// <summary>
/// Read model chỉ dùng cho API in tờ trình lập Ban QLDA.
/// </summary>
public class QuyetDinhLapBanQldaPrintDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public string? SoDuThao { get; set; }
    public string? TrichYeuDuThao { get; set; }

    /// <summary>DuAn.LanhDaoPhuTrachId — thực chất là UserPortalId.</summary>
    public long? LanhDaoPhuTrachId { get; set; }

    /// <summary>user_master.HoTen join theo UserPortalId.</summary>
    public string? TenLanhDaoPhuTrach { get; set; }

    public List<ThanhVienBanQldaDto> ThanhViens { get; set; } = [];
}
```

- [ ] **Step 2: Kiểm tra namespace / compile đơn vị**

Không cần unit test riêng cho DTO POCO. Sang Task 2.

- [ ] **Step 3: Commit (khi user yêu cầu commit)**

```bash
git add QLDA.Application/QuyetDinhLapBanQLDAs/DTOs/QuyetDinhLapBanQldaPrintDto.cs
git commit -m "$(cat <<'EOF'
QLDA: Application - Add print DTO for tờ trình lập Ban QLDA

EOF
)"
```

---

### Task 2: Tạo `QuyetDinhLapBanQldaGetPrintQuery` + Handler

**Files:**
- Create: `QLDA.Application/QuyetDinhLapBanQLDAs/Queries/QuyetDinhLapBanQldaGetPrintQuery.cs`
- Reference pattern: `QLDA.Application/ToTrinhPheDuyet/Queries/ToTrinhPheDuyetGetExportQuery.cs`

**Interfaces:**
- Consumes: `QuyetDinhLapBanQldaPrintDto`, `IRepository<QuyetDinhLapBanQLDA, Guid>`, `IRepository<UserMaster, long>`, `IRepository<DuAn, Guid>` (nếu cần; ưu tiên navigation `x.DuAn` trong Select như export phê duyệt)
- Produces: `IRequestHandler<QuyetDinhLapBanQldaGetPrintQuery, QuyetDinhLapBanQldaPrintDto>`

**Impact analysis (bắt buộc trước khi edit symbol liên quan):**

Trước khi đụng `InToTrinhThanhLapBanQLDA` / tạo handler mới:

```text
impact({ target: "InToTrinhThanhLapBanQLDA", direction: "upstream" })
impact({ target: "QuyetDinhLapBanQldaGetQuery", direction: "upstream" })
```

Kỳ vọng: `QuyetDinhLapBanQldaGetQuery` vẫn phục vụ CRUD; print endpoint chuyển sang query mới → blast radius thấp nếu không sửa GetQuery.

- [ ] **Step 1: Chạy impact analysis (GitNexus MCP hoặc CLI tương đương) và ghi nhận risk**

Nếu HIGH/CRITICAL trên symbol CRUD → dừng, báo user. Nếu chỉ ảnh hưởng print → tiếp tục.

- [ ] **Step 2: Tạo query + handler**

```csharp
using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Queries;

public class QuyetDinhLapBanQldaGetPrintQuery : IRequest<QuyetDinhLapBanQldaPrintDto>
{
    public Guid Id { get; set; }
    public bool ThrowIfNull { get; set; } = true;
}

internal class QuyetDinhLapBanQldaGetPrintQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<QuyetDinhLapBanQldaGetPrintQuery, QuyetDinhLapBanQldaPrintDto>
{
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> _quyetDinh =
        serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();

    private readonly IRepository<UserMaster, long> _userMaster =
        serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();

    public async Task<QuyetDinhLapBanQldaPrintDto> Handle(
        QuyetDinhLapBanQldaGetPrintQuery request,
        CancellationToken cancellationToken = default)
    {
        var userMasterQuery = _userMaster.GetQueryableSet().AsNoTracking();

        var dto = await _quyetDinh.GetOrderedSet()
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(x => new QuyetDinhLapBanQldaPrintDto
            {
                Id = x.Id,
                DuAnId = x.DuAnId,
                So = x.So,
                TrichYeu = x.TrichYeu,
                SoDuThao = x.SoDuThao,
                TrichYeuDuThao = x.TrichYeuDuThao,
                LanhDaoPhuTrachId = x.DuAn!.LanhDaoPhuTrachId,

                // LEFT JOIN semantics: FirstOrDefault → null nếu không match
                // Join đúng khóa nghiệp vụ UserPortalId (KHÔNG dùng u.Id)
                TenLanhDaoPhuTrach = userMasterQuery
                    .Where(u => u.UserPortalId == x.DuAn!.LanhDaoPhuTrachId)
                    .Select(u => u.HoTen)
                    .FirstOrDefault(),

                ThanhViens = x.ThanhViens
                    .Select(tv => new ThanhVienBanQldaDto
                    {
                        Id = tv.Id,
                        Ten = tv.Ten,
                        ChucVu = tv.ChucVu,
                        VaiTro = tv.VaiTro
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        ManagedException.ThrowIf(request.ThrowIfNull && dto == null, "Không tìm thấy dữ liệu");

        return dto!;
    }
}
```

**Lưu ý kỹ thuật:**

1. Subquery `FirstOrDefault` trên `UserPortalId` trùng pattern `ToTrinhPheDuyetGetExportQuery` — EF Core translate được, tránh nhân bản dòng khi có nhiều `ThanhVien`.
2. Hai bên join đều `long?` → không cần cast.
3. Nếu có nhiều user cùng `UserPortalId`, `FirstOrDefault` chọn 1 — chấp nhận được trong phạm vi task (không tạo unique index).
4. Không thêm `.Where(e => !e.IsDeleted)` thừa nếu `GetOrderedSet`/`GetQueryableSet` đã filter theo convention hiện tại của handler gốc (GetQuery dùng `GetOrderedSet`).

- [ ] **Step 3: Đảm bảo MediatR đăng ký handler**

Project đã scan assembly Application → class `internal` handler trong cùng assembly sẽ được đăng ký tự động như các query khác. Không cần DI thủ công.

- [ ] **Step 4: Commit (khi user yêu cầu)**

```bash
git add QLDA.Application/QuyetDinhLapBanQLDAs/Queries/QuyetDinhLapBanQldaGetPrintQuery.cs
git commit -m "$(cat <<'EOF'
QLDA: Application - Add print query resolving LanhDaoPhuTrach via UserPortalId

EOF
)"
```

---

### Task 3: Cập nhật `PrintController.InToTrinhThanhLapBanQLDA`

**Files:**
- Modify: `QLDA.WebApi/Controllers/PrintController.cs` (region `#region Xuất tờ trình thành lập ban qlda`, ~1610–1655)

**Interfaces:**
- Consumes: `QuyetDinhLapBanQldaGetPrintQuery` → `QuyetDinhLapBanQldaPrintDto`
- Produces: không đổi response API (vẫn `File(...)` Word)

**Impact:** Chỉ endpoint print này; không đụng các `#region` print khác.

- [ ] **Step 1: Chạy impact trên `InToTrinhThanhLapBanQLDA`**

Ghi nhận callers (HTTP route) và risk. Nếu HIGH → báo user trước khi sửa.

- [ ] **Step 2: Đổi Mediator call + replacements**

Thay đoạn hiện tại:

```csharp
var rows = await Mediator.Send(new QuyetDinhLapBanQldaGetQuery {
    Id = id,
    IncludeThanhVien= true,
}, cancellationToken);

// ...
{ "TenLanhDaoPhuTrach", ""}// rows.DuAn?.LanhDaoPhuTrachId
```

Bằng:

```csharp
var rows = await Mediator.Send(new QuyetDinhLapBanQldaGetPrintQuery
{
    Id = id,
    ThrowIfNull = true,
}, cancellationToken);

var doc = new Aspose.Words.Document(templatePath);
doc.MailMerge.UseNonMergeFields = true;

DateTime ngayHienTai = DateTime.Now;
var replacements = new Dictionary<string, string>
{
    { "So", rows?.So ?? string.Empty },
    { "TrichYeu", rows?.TrichYeu ?? string.Empty },
    { "SoDuThao", rows?.SoDuThao ?? rows?.So ?? string.Empty },
    { "TrichYeuDuThao", rows?.TrichYeuDuThao ?? rows?.TrichYeu ?? string.Empty },
    { "NgayThangNam", $"Ngày {ngayHienTai:dd} tháng {ngayHienTai:MM} năm {ngayHienTai:yyyy}" },
    { "TenLanhDaoPhuTrach", rows?.TenLanhDaoPhuTrach ?? string.Empty },
};

DataTable dt = rows.ThanhViens?.ToDataTable()
    ?? DataTableConvertExtensions.CreateDataTable<ThanhVienBanQldaDto>("ThanhVien");
dt.TableName = "ThanhVien";
DataSet ds = new DataSet();
ds.Tables.Add(dt);

var bytes = _wordHelper.ExportFromTemplate(templatePath, ds, replacements);

return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    GetDownloadFileName(fileNameTemplate));
```

**Import cần có (nếu chưa):**

```csharp
using QLDA.Application.QuyetDinhLapBanQLDAs.Queries;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
```

Bỏ dependency entity `ThanhVienBanQLDA` trong đoạn này nếu `CreateDataTable` đã chuyển sang `ThanhVienBanQldaDto`.

- [ ] **Step 3: Kiểm tra template Word vẫn dùng đúng field**

Template:

- `isMauDuThao=true` → `PrintTemplates/Word/MauDuTaoThanhLapBanQLDA.docx`
- `isMauDuThao=false` → `PrintTemplates/Word/ToTrinhThanhLapBanQLDA.docx`

Merge field phải là đúng `TenLanhDaoPhuTrach`. Không đổi tên field trừ khi template thực tế khác — nếu khác, dừng và báo user.

- [ ] **Step 4: Commit (khi user yêu cầu)**

```bash
git add QLDA.WebApi/Controllers/PrintController.cs
git commit -m "$(cat <<'EOF'
QLDA: WebApi - Map TenLanhDaoPhuTrach when printing tờ trình lập Ban QLDA

EOF
)"
```

---

### Task 4: Build + verification

**Files:** không tạo file mới

- [ ] **Step 1: Build**

```bash
dotnet build
```

Expected: thành công, không warning mới từ các file vừa thêm/sửa.

- [ ] **Step 2: Kiểm tra phạm vi diff**

```bash
git status
git diff --stat
```

Expected chỉ gồm:

```text
QLDA.Application/.../QuyetDinhLapBanQldaPrintDto.cs
QLDA.Application/.../QuyetDinhLapBanQldaGetPrintQuery.cs
QLDA.WebApi/Controllers/PrintController.cs
```

Không có:

```text
QLDA.Migrator/**
AppDbContextModelSnapshot.cs
bin/**, obj/**
```

- [ ] **Step 3: (Nếu có GitNexus) `detect_changes`**

```text
detect_changes({ scope: "all" })
```

Expected: chỉ process/symbol liên quan print tờ trình lập Ban QLDA.

- [ ] **Step 4: Manual API test checklist**

```bash
curl --request GET \
  --url 'http://localhost:5183/api/print/to-trinh-lap-ban-qlda?id=<GUID>&isMauDuThao=true' \
  --header 'Authorization: Bearer <JWT_TOKEN>' \
  --header 'accept: */*' \
  --output to-trinh.docx
```

| Case | Điều kiện | Kết quả |
|---|---|---|
| 1 | `LanhDaoPhuTrachId` = portal id có `HoTen` | Word hiện đúng họ tên |
| 2 | `LanhDaoPhuTrachId` null | Field trống, API 200 |
| 3 | ID không có trong user_master | Field trống, API 200 |
| 4 | `UserMaster.Id != UserPortalId`, dự án lưu PortalId | Vẫn đúng `HoTen` |
| 5 | `isMauDuThao=true` | Tên đúng + SoDuThao/TrichYeuDuThao như cũ |
| 6 | `isMauDuThao=false` | Tên đúng + template chính thức |

---

## Acceptance Criteria

- [ ] API `/api/print/to-trinh-lap-ban-qlda` vẫn hoạt động
- [ ] Lấy `LanhDaoPhuTrachId` từ `DuAn` của quyết định
- [ ] Join `UserMaster.UserPortalId` (không join `Id`)
- [ ] Lấy `HoTen` → `TenLanhDaoPhuTrach`
- [ ] Print DTO có `TenLanhDaoPhuTrach`
- [ ] `PrintController` map đúng merge field
- [ ] Có dữ liệu → in tên; không có → `""`; không exception
- [ ] Không migration / không đổi schema
- [ ] Không ảnh hưởng CRUD / print API khác
- [ ] `dotnet build` thành công
- [ ] Diff chỉ trong phạm vi task

---

## Báo cáo sau khi code xong (template)

```markdown
## Files changed

- `QLDA.Application/QuyetDinhLapBanQLDAs/DTOs/QuyetDinhLapBanQldaPrintDto.cs`
- `QLDA.Application/QuyetDinhLapBanQLDAs/Queries/QuyetDinhLapBanQldaGetPrintQuery.cs`
- `QLDA.WebApi/Controllers/PrintController.cs`

## Implementation

- Bổ sung `TenLanhDaoPhuTrach` vào print DTO.
- Subquery/LEFT JOIN `DuAn.LanhDaoPhuTrachId` với `UserMaster.UserPortalId`.
- Lấy `UserMaster.HoTen`.
- Map vào merge field `TenLanhDaoPhuTrach`.

## Join mapping

```text
DuAn.LanhDaoPhuTrachId
    =
UserMaster.UserPortalId
```

## Null handling

- `LanhDaoPhuTrachId` null → chuỗi rỗng.
- Không tìm thấy user → chuỗi rỗng.
- `HoTen` null → chuỗi rỗng.

## Verification

- `dotnet build`: ...
- API test: ...
- File Word hiển thị tên lãnh đạo: ...

## Notes

- Không tạo migration.
- Không thay đổi database schema.
- Không ảnh hưởng API print khác / CRUD GetQuery.
```

---

## Self-review (plan vs spec)

| Spec requirement | Task cover |
|---|---|
| Hiểu luồng trước khi sửa | Current Flow section |
| Không join `user_master.Id` | Task 2 subquery `UserPortalId` |
| Ưu tiên lấy tên trong query, không query ở Controller | Task 2 + Task 3 |
| DTO có `TenLanhDaoPhuTrach` | Task 1 |
| Null-safe 4 case | Task 2 FirstOrDefault + Task 3 `?? ""` |
| Không migration / schema | Global Constraints |
| Không ảnh hưởng print/CRUD khác | Không sửa `QuyetDinhLapBanQldaGetQuery` |
| Build + test | Task 4 |
| Impact trước khi sửa | Task 2 Step 1, Task 3 Step 1 |

**Không dùng phương án:** gắn `TenLanhDaoPhuTrach` vào domain entity, hoặc query `UserMaster` trực tiếp trong `PrintController`.
