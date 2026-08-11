# Test workflow — #169 Màn 9667 / KetQuaTrungThau

## 1. Build

```bash
dotnet build SER.sln
```

Expect: 0 error. Không sửa ModelSnapshot thủ công.

## 2. API — `goi-thau/combobox`

| Case | Request | Expect |
|------|---------|--------|
| Backward compat | `GET /api/goi-thau/combobox?...` (không `IsThamDinh`) | Danh sách như trước (không lọc thẩm định) |
| Filter on | `GET /api/goi-thau/combobox?IsThamDinh=true&DuAnId=...` | Chỉ gói thầu có ≥1 `HoSoMoiThauDienTu` với `ThamDinh = true` |
| Filter false/null | `IsThamDinh=false` hoặc omit | Không áp filter thẩm định |

Setup data gợi ý:

1. Gói A: E-HSMT `ThamDinh = true`
2. Gói B: E-HSMT `ThamDinh = false` / null / không có E-HSMT  
→ `IsThamDinh=true` chỉ trả A.

## 3. API — KetQuaTrungThau CRUD

### Create

`POST /api/ket-qua-trung-thau/them-moi` body gồm:

- fields cũ
- `TrangThaiDangTai` (`boolean`: `false` = chưa đăng tải, `true` = đã đăng tải)
- `DanhSachBienBanThuongThao` (file upload theo contract hiện tại)
- `DanhSachTepDinhKem` (nếu có) — không bị xóa nhầm khi sync biên bản

### Detail

`GET /api/ket-qua-trung-thau/{id}/chi-tiet`

- `TrangThaiDangTai` đúng giá trị đã lưu
- `DanhSachBienBanThuongThao` load lại file
- `DanhSachTepDinhKem` không chứa file biên bản (GroupType tách)

### Update

`PUT /api/ket-qua-trung-thau/cap-nhat`

- Đổi `TrangThaiDangTai` (đã ↔ chưa), thêm/xóa file biên bản → persist đúng
- `AutoDeleteMissing` chỉ trong GroupType biên bản

## 4. FE (repo ngoài) — checklist

- [ ] CBB Gói thầu gọi `IsThamDinh=true`
- [ ] Label **Đơn vị trúng thầu**
- [ ] Control upload Biên bản thương thảo bind `DanhSachBienBanThuongThao`
- [ ] CBB Trạng thái đăng tải bind `TrangThaiDangTai` (`boolean` — đã / chưa đăng tải)
- [ ] Create / Detail / Edit bind đủ

## 5. Regression

- [ ] Màn khác dùng `goi-thau/combobox` không truyền `IsThamDinh` vẫn bình thường
- [ ] Attachment cũ `EGroupType.KetQuaTrungThau` không bị mất khi update có biên bản
- [ ] Migration apply lên DB local thành công (cột `TrangThaiDangTai` bit)