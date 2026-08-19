# Test Workflow — `danh-sach-tien-do` Tờ trình thẩm định nhà thầu

## Thông tin chung

- **Issue**: `danh-sach-tien-do` thiếu file "Tờ trình kết quả" (6/7 so với chi-tiet)
- **File thay đổi**:
  - `QLDA.Application/ToTrinhThamDinhNhaThau/Queries/ToTrinhThamDinhNhaThauGetDanhSachQuery.cs`
- **Migration**: không
- **Test files**: không có test tự động cho module này (xác minh bằng build + smoke test manual)

## Chạy build & test

```powershell
# Build toàn bộ solution
dotnet build SER.sln

# Toàn bộ tests
dotnet test QLDA.Tests/QLDA.Tests.csproj
```

Kỳ vọng: build 0 Error, các test hiện có không vỡ (chỉ thêm nhánh query mới, không đổi hành vi cũ).

## Smoke test API

```http
# chi-tiet — kỳ vọng 7 file (baseline)
GET /QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/08defd97-eaf5-c226-687a-7b350801bae5/chi-tiet

# danh-sach-tien-do — kỳ vọng mỗi item 7/7 file sau fix
GET /QuanLyDuAn/api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do?DuAnId=08def36f-9d7f-6e89-687a-7b2ea004c65e&BuocId=7040&PageIndex=1&PageSize=10
```

### Đối tượng kiểm tra

| Item `Id` | Trước fix | Sau fix |
| --------- | --------- | ------- |
| `08defd97-eaf5-c226-687a-7b350801bae5` | 6/7 | 7/7 |
| `08defc12-4e20-3b60-687a-7b38f8073d8e` | 6/7 | 7/7 |

Kiểm chứng: mỗi item trong `danhSachTepDinhKem` phải có đủ file `groupType = "ToTrinhQuyetDinh"` (Tờ trình kết quả), không trùng `id` với 6 file còn lại.

## Verify

- [ ] `dotnet build` — 0 Error(s)
- [ ] Test tự động hiện có pass
- [ ] Smoke test: 2 item đều 7/7 file, khớp chi-tiet
- [ ] Item không có ToTrinhQuyetDinh vẫn giữ nguyên (không lỗi)
