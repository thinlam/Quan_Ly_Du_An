# QuyetDinhDieuChinh đồng bộ vào PheDuyet Dispatch hub

**Date**: 2026-05-15 07:49
**Severity**: Medium
**Component**: PheDuyetDispatch / QuyetDinhDieuChinh
**Status**: Resolved

## What Happened

Refactored QuyetDinhDieuChinh to share the PheDuyetDispatch workflow hub instead of maintaining its own separate command endpoints. Reduced controller from 12 to 4 endpoints, wired into 4 dispatch handlers, added status codes, and deleted 2 redundant command files.

## The Brutal Truth

This is a classic consolidation that should have happened during initial implementation. QuyetDinhDieuChinh reinvented the wheel with near-identical command handlers (ThamDinh, TrinhPheDuyet) that duplicated the dispatch hub's Trinh/Duyet/TraLai/TuChoi pattern. The refactor was straightforward but required tracing through 4 dispatch command files to wire the new type correctly.

## Technical Details

- **Controller endpoints**: 12 -> 4 (kept: danh-sach, chi-tiet, them-moi, cap-nhat)
- **Deleted endpoints**: DELETE, lich-su, trinh, tham-dinh, trinh-phe-duyet, duyet, tra-lai, tu-choi
- **Wired dispatch handlers**:
  - `PheDuyetDispatchTrinhCommand.cs`
  - `PheDuyetDispatchDuyetCommand.cs`
  - `PheDuyetDispatchTraLaiCommand.cs`
  - `PheDuyetDispatchTuChoiCommand.cs`
- **Status codes added to `TrangThaiPheDuyetCodes.cs`**: DT, ĐTr, ĐD, TL, TC
- **Deleted files**:
  - `QuyetDinhDieuChinhThamDinhCommand.cs`
  - `QuyetDinhDieuChinhTrinhPheDuyetCommand.cs`
- **Build**: 0 errors, 8 pre-existing warnings
- **Tests**: 83 pass

## Key Decision

QuyetDinhDieuChinh workflow now routes through `/api/phe-duyet/{type}/{id}/trinh|duyet|tra-lai|tu-choi` with `type="QuyetDinhDieuChinh"`. History queries reuse `PheDuyetGetLichSuQuery` unchanged -- the type discriminator already handles multiple document types.

## Lessons Learned

- The PheDuyetDispatch hub was designed for multi-type support from the start; extending it required only status code registration and dispatch handler wiring -- minimal code, maximum consolidation
- Separating workflow commands per document type (ThamDinh, TrinhPheDuyet) created maintenance duplication with no functional benefit
- History (lich-su) never needed a separate endpoint -- the shared query already worked across types

## Next Steps

- Monitor runtime behavior of QuyetDinhDieuChinh dispatch flow with real data
- Verify status transitions map correctly to DT/ĐTr/ĐD/TL/TC codes in business logic
- Consider deprecating the standalone QuyetDinhDieuChinh* command files pattern entirely -- new document types should wire into dispatch hub from day one