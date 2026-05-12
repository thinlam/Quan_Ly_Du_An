# Brainstorm: Unified PheDuyetHistory

## Problem
Nhiều entity (PheDuyetDuToan, PheDuyetNoiDung, KhaiToanKinhPhi, HoSo...) đều cần theo dõi lịch sử phê duyệt. Hiện tại mỗi entity có 1 bảng History riêng → cấu trúc lặp lại, phình DB.

## Decision: Unified PheDuyetHistory (Polymorphic, No FK)

### Entity Structure
```csharp
public class PheDuyetHistory : Entity<Guid>, IAggregateRoot
{
    public string EntityName { get; set; }  // "PheDuyetDuToan", "PheDuyetNoiDung", etc.
    public Guid EntityId { get; set; }      // Polymorphic FK (no constraint)
    public Guid DuAnId { get; set; }
    public long? NguoiXuLyId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset NgayXuLy { get; set; }

    // NavProps (only to shared danh muc tables)
    public DuAn? DuAn { get; set; }
    public DanhMucTrangThaiPheDuyet? TrangThai { get; set; }
}
```

### EntityName Constants
```csharp
public static class PheDuyetEntityNames
{
    public const string PheDuyetDuToan = nameof(PheDuyetDuToan);
    public const string PheDuyetNoiDung = nameof(PheDuyetNoiDung);
    // Future: KhaiToanKinhPhi, HoSo, etc.
}
```

### EF Configuration
- Composite index: `(EntityName, EntityId)` for fast lookup
- No FK constraint on EntityId → any entity can use it
- FK to DuAn and DanhMucTrangThaiPheDuyet only

### Migration Plan
1. Create `PheDuyetHistory` table
2. Migrate data from `PheDuyetDuToanHistory` → set EntityName = "PheDuyetDuToan"
3. Migrate data from `PheDuyetNoiDungHistory` → set EntityName = "PheDuyetNoiDung"
4. Drop old history tables + their FK constraints

### Impact Analysis
| Area | Change |
|------|--------|
| PheDuyetDuToan.cs | Remove `ICollection<PheDuyetDuToanHistory>` nav prop |
| PheDuyetNoiDung.cs | Remove `ICollection<PheDuyetNoiDungHistory>` nav prop |
| Commands (Trinh/Duyet/TraLai/TuChoi) | Write to `PheDuyetHistory` with EntityName |
| History queries | Filter by `EntityName + EntityId` |
| Configurations | Remove old history configs, create new one |

### Pros
- 1 table thay vì N bảng → DRY
- Thêm entity mới = chỉ thêm constant string, 0 migration mới cho history
- Query theo EntityName + EntityId nhanh (composite index)

### Cons
- Không có FK constraint → app phải đảm bảo data integrity
- Query phức tạp hơn 1 chút (phải filter EntityName)

### Risk: LOW
- Polymorphic FK pattern phổ biến, well-tested
- Data migration đơn giản (2 bảng cũ → 1 bảng mới)

---

## Implementation Status (Updated 2026-05-08)

### Completed
| Phase | Description | Status |
|-------|-------------|--------|
| Domain Entity | PheDuyetHistory + PheDuyetEntityNames constants | ✅ Done |
| EF Config | PheDuyetHistoryConfiguration, removed old history configs | ✅ Done |
| Commands | 11 command files updated to use unified PheDuyetHistory | ✅ Done |
| Queries | History queries use EntityName filter | ✅ Done |
| Migration | Migration created and applied | ✅ Done |
| Cleanup | Deleted PheDuyetNoiDung (Application, WebApi, Domain, Persistence layers) | ✅ Done |
| QuanLyPheDuyet | New controller + commands/queries/DTOs for unified approval management | ✅ Done |
| SQLite Provider | WebApi supports `--provider sqlite` CLI arg | ✅ Done |
| Role-based Auth | Duyet/TraLai commands enforce QLDA_LDDV role | ✅ Done |
| Tests | 42 tests (38 passed, 4 skipped, 0 failed) | ✅ Done |

### Test Results
```
Total tests: 42
     Passed: 38
    Skipped: 4
    Failed: 0
```

### Key Commits (Branch: feature/9459)
| Commit | Description |
|--------|-------------|
| `bfe5286` | refactor(PheDuyet): unify history, remove NoiDung, standardize Loai constants |
| `56164f9` | feat(WebApi): add SQLite provider support with CLI arg --provider sqlite |
| `e48324a` | feat(PheDuyet): add QuanLyPheDuyet controller, commands, queries, DTOs |
| `3c933af` | Merge origin/main (HoSoMoiThauDienTu feature, no conflicts) |
