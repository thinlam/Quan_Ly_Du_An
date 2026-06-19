# Issue #9609 — FE Endpoint Mapping (Loại dự án theo năm)

> Quick reference cho FE: các endpoint hỗ trợ param `loaiDuAnTheoNamId`.

## Param chung
```
loaiDuAnTheoNamId : int?   (query string)
```
- `null` hoặc `0` → bỏ filter (trả full list)
- `1` = Chuẩn bị đầu tư | `2` = Chuyển tiếp | `3` = Khởi công mới | `4` = Khối lượng tồn đọng
- Combobox data: `GET /api/danh-muc-loai-du-an-theo-nam`

## Endpoints hỗ trợ filter

### Nhóm Màn hình chính (FE đã có UI)
| Màn hình | Endpoint |
|----------|----------|
| Gói thầu | `GET /api/goi-thau/danh-sach-tien-do` |
| Hợp đồng | `GET /api/hop-dong/danh-sach-tien-do` |
| Phụ lục hợp đồng | `GET /api/phu-luc-hop-dong/danh-sach-tien-do` |
| Báo cáo tiến độ | `GET /api/bao-cao-tien-do/danh-sach-tien-do` |
| Khó khăn vướng mắc | `GET /api/kho-khan-vuong-mac/danh-sach-tien-do` |
| Tổng hợp văn bản quyết định | `GET /api/tong-hop-van-ban-quyet-dinh/danh-sach-day-du` |

### Nhóm Quyết định
| Endpoint |
|----------|
| `GET /api/quyet-dinh-duyet-du-toan/danh-sach-tien-do` |
| `GET /api/quyet-dinh-thanh-lap-bqlda/danh-sach-tien-do` |
| `GET /api/quyet-dinh-thanh-lap-ben-moi-thau/danh-sach-tien-do` |
| `GET /api/quyet-dinh-thanh-lap-hoi-dong-tham-dinh/danh-sach-tien-do` |
| `GET /api/quyet-dinh-duyet-khlcnt/danh-sach-tien-do` |
| `GET /api/quyet-dinh-duyet-quyet-toan/danh-sach-tien-do` |
| `GET /api/quyet-dinh-dieu-chinh/danh-sach` |

### Nhóm Văn bản / Báo cáo
| Endpoint |
|----------|
| `GET /api/van-ban-chu-truong/danh-sach-tien-do` |
| `GET /api/van-ban-phap-ly/danh-sach-tien-do` |
| `GET /api/bao-cao-ban-giao-san-pham/danh-sach-tien-do` |
| `GET /api/bao-cao-bao-hanh-san-pham/danh-sach-tien-do` |
| `GET /api/bao-cao-ket-qua-khao-sat/danh-sach` |

### Nhóm Đề xuất / Tờ trình
| Endpoint |
|----------|
| `GET /api/de-xuat-chu-truong-moi/danh-sach-tien-do` |
| `GET /api/de-xuat-chu-truong-chuyen-tiep/danh-sach-tien-do` |
| `GET /api/de-xuat-nhu-cau-kinh-phi/danh-sach-tien-do` |
| `GET /api/chu-truong-lap-ke-hoach/danh-sach-tien-do` |
| `GET /api/to-trinh-phe-duyet/danh-sach-tien-do` |
| `GET /api/to-trinh-co-tham-dinh/danh-sach-tien-do` |
| `GET /api/to-trinh-ket-qua-goi-thau/danh-sach-tien-do` |
| `GET /api/to-trinh-tham-dinh-nha-thau/danh-sach-tien-do` |

### Nhóm Tài chính / Hồ sơ
| Endpoint |
|----------|
| `GET /api/tam-ung/danh-sach-tien-do` |
| `GET /api/thanh-toan/danh-sach-tien-do` |
| `GET /api/nghiem-thu/danh-sach-tien-do` |
| `GET /api/ho-so-moi-thau-dien-tu/danh-sach-tien-do` |
| `GET /api/ho-so-de-xuat-cap-do-cntt/danh-sach-tien-do` |

### Nhóm Kế hoạch / Kết quả
| Endpoint |
|----------|
| `GET /api/ket-qua-trung-thau/danh-sach-tien-do` |
| `GET /api/ke-hoach-lua-chon-nha-thau-rut-gon/danh-sach-tien-do` |
| `GET /api/ke-hoach-trien-khai-hang-muc/danh-sach-tien-do` |
| `GET /api/ke-hoach-trien-khai-chi-tiet-du-an/danh-sach-tien-do` |

## Print/Export (Excel)
Param `loaiDuAnTheoNamId` hỗ trợ cho 8 endpoint:

```
GET /api/print/danh-sach-goi-thau
GET /api/print/danh-sach-hop-dong
GET /api/print/danh-sach-phu-luc-hop-dong
GET /api/print/danh-sach-bao-cao-tien-do
GET /api/print/danh-sach-bao-cao-bao-hanh-san-pham
GET /api/print/danh-sach-bao-cao-ban-giao-san-pham
GET /api/print/danh-sach-kho-khan-vuong-mac
GET /api/print/danh-sach-tong-hop-van-ban-quyet-dinh
```
> Lưu ý: SP print cần DBA update để filter thực sự tác động (xem report.md mục 8).

## Ví dụ gọi API
```
GET /api/goi-thau/danh-sach-tien-do?duAnId=abc&loaiDuAnTheoNamId=3&pageIndex=0&pageSize=20
```
