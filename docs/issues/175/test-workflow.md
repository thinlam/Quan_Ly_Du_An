# Test workflow — #175 Trình duyệt dự toán (`DuToanDauTu`)

## 1. Build

```bash
dotnet build SER.sln
```

Expect: 0 error. Không sửa ModelSnapshot thủ công. Migration chỉ do `ef.bat` sinh.

## 2. Migration — **USER tự làm**

Agent không tạo/apply migration. Sau khi Domain + Config có `Ten`:

```bat
ef.bat QLDA add AddDuToanDauTuTen
```

Verify migration chỉ thêm cột `Ten` trên `DuToanDauTu` — **không** drop `PhuongAnThietKeId`.

Apply DB trên môi trường local (không drop database).

## 3. API — Insert / Update validate

Base: `POST /api/du-toan-dau-tu/them-moi` và `PUT /api/du-toan-dau-tu/cap-nhat`

| Case | Body | Expect |
|------|------|--------|
| 1 | Thiếu / null / `""` / whitespace `ten` | 400 — không lưu |
| 2 | Có `ten`, `danhSachTepDinhKem` rỗng/null | 400 — thiếu Công văn đề nghị báo giá |
| 3 | Có `ten` + ≥1 file `danhSachTepDinhKem` | 200 — lưu OK |
| 4 | Có `ten` + chỉ `danhSachTepDinhKemKhac`, không Công văn | 400 — thiếu Công văn |
| 5 | Case 3 + optional `danhSachTepDinhKemKhac` | 200 — cả 2 list lưu đúng GroupType |

## 4. API — Chi tiết

`GET /api/du-toan-dau-tu/{id}/chi-tiet`

- [ ] `ten` đúng giá trị đã lưu
- [ ] `danhSachTepDinhKem` = file Công văn (`GroupType` `DuToanDauTu` / kèm `KySo_` nếu có)
- [ ] `danhSachTepDinhKemKhac` = file Khác (`DuToanDauTu_Khac`)
- [ ] Hai list không lẫn file của nhau
- [ ] Không bắt buộc còn field Phương án thiết kế trên form FE (BE có thể vẫn trả `phuongAnThietKeId` nếu giữ property)

## 5. API — Danh sách

`GET /api/du-toan-dau-tu/danh-sach-tien-do?...`

- [ ] Có `ten` trên từng dòng (nếu projection đã thêm)
- [ ] Attachment (nếu trả) không lẫn GroupType

## 6. FE (repo ngoài) — checklist

- [ ] Input **Tên dự toán *** bind `ten`
- [ ] Ẩn **Phương án thiết kế**
- [ ] Upload **Công văn đề nghị báo giá *** → `danhSachTepDinhKem`
- [ ] Upload **Khác** (optional) → `danhSachTepDinhKemKhac`
- [ ] Create / Edit / Detail bind đủ Case 1–5

## 7. Regression

- [ ] Các field cũ (Số tờ trình, Tổng dự toán, Nguồn vốn, …) vẫn lưu/load
- [ ] Workflow trình/duyệt/trả lại `DuToanDauTu` (nếu dùng) không vỡ
- [ ] Module `PheDuyetDuToan` / `QuyetDinhDuyetDuToan` không bị ảnh hưởng
