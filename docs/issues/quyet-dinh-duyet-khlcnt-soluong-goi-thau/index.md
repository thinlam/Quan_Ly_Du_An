# `SoLuongGoiThau` đang đặt sai entity — KHLCNT vs QĐ duyệt

> Chưa có số PMIS. Đổi folder thành `docs/issues/{số}/` khi có ticket.

**Trạng thái:** Docs + trace source. **Chưa sửa code** theo task này.

Chi tiết: [report.md](./report.md) · Test: [test-workflow.md](./test-workflow.md)

## Tóm tắt

`SoLuongGoiThau` đang khai báo và map DB trên **`KeHoachLuaChonNhaThau`**. Leader chốt: cột này thuộc **`QuyetDinhDuyetKHLCNT`**. KHLCNT chỉ giữ **4 cột mới** của task field (không kể `Ten` / `LoaiKeHoach` cũ).

## Trace trước khi sửa (6 điểm)

| # | Câu hỏi | Kết quả source hiện tại |
| --- | --- | --- |
| 1 | `SoLuongGoiThau` khai báo file nào? | Domain: `QLDA.Domain/Entities/KeHoachLuaChonNhaThau.cs` (`int?`). **Không** có trên `QuyetDinhDuyetKHLCNT.cs`. |
| 2 | Map xuống bảng nào? | Snapshot: `KeHoachLuaChonNhaThau` → cột `KeHoachLuaChonNhaThau.SoLuongGoiThau`. EF Config KHLCNT **không** khai property (convention). Migration đã add: `20260812044306_AddKeHoachLuaChonNhaThauSoLuongGoiThau`. |
| 3 | `QuyetDinhDuyetKHLCNT` ở đâu? | Entity `QLDA.Domain/Entities/QuyetDinhDuyetKHLCNT.cs`. Config `QLDA.Persistence/Configurations/QuyetDinhDuyetKHLCNTConfiguration.cs`. Snapshot entity **không** có `SoLuongGoiThau`. |
| 4 | DTO / Handler / API? | KHLCNT: Insert/Update/Dto + Mappings + `GetDanhSachQuery` + `POST/PUT/GET api/ke-hoach-lua-chon-nha-thau`. QĐ: `QuyetDinhDuyetKHLCNTModel.soLuongGoiThau` + `ToModel` đọc `entity.KeHoachLuaChonNhaThau?.SoLuongGoiThau`. `ToEntity`/`Update` **không** ghi field này. Không có Validator. |
| 5 | Cần `ef remove` migration vừa gen sai? | **Không** remove `20260812044306_...` (đã add cột đúng bảng cũ, có thể đã apply). Workspace **không** còn migration move chưa apply. Sau khi sửa entity: **add migration mới**. Chỉ `remove` nếu migration **mới** vừa gen vẫn trỏ sai bảng và **chưa** apply/share. |
| 6 | File dự kiến sửa | Xem [report.md](./report.md) mục 6. |

## 4 cột mới **đúng** trên `KeHoachLuaChonNhaThau` (giữ)

Từ entity hiện tại, trừ `Ten` / `LoaiKeHoach` (cũ) và trừ `SoLuongGoiThau` (sai chỗ):

| Property | Kiểu |
| --- | --- |
| `TongDuToan` | `long` |
| `DuToanThamDinh` | `long?` |
| `NguonVonId` | `int?` |
| `ThoiGianThucHien` | `int?` |

Không xóa `Ten`, `LoaiKeHoach`, navigation, không đổi type 4 cột này.

## Expected

```text
KeHoachLuaChonNhaThau  →  không có SoLuongGoiThau
QuyetDinhDuyetKHLCNT   →  int? SoLuongGoiThau  (giữ nullability hiện tại)
```

## Related

- [#178](../178/) — đã persist `SoLuongGoiThau` **trên KHLCNT** (approach cũ, lệch rule này).
- [Chi tiết QĐ thiếu field](../quyet-dinh-duyet-khlcnt-chi-tiet-thieu-field/) — GET chi tiết đang đọc 5 field từ KHLCNT; sau fix chỉ 4 field từ KHLCNT, `soLuongGoiThau` từ QĐ.
