---
description: Generate Pull Request title and description from current changes
agent: plan
---

Hãy tạo nội dung Pull Request cho branch hiện tại.

Trước tiên phải kiểm tra:

- `git status`
- `git diff`
- `git diff --staged`
- các commit của branch hiện tại so với `main`

Đọc code thực tế để hiểu thay đổi, không suy diễn từ tên branch.

## Yêu cầu Title

- Theo Conventional Commit.
- Tóm tắt đúng các thay đổi chính.
- Không dùng title chung chung như `Fix bug du an`.
- Nếu có nhiều thay đổi liên quan, có thể nối bằng `;`.

Ví dụ:

`fix: GET chi-tiet QĐ duyệt KHLCNT trả thêm 5 field; docs lỗi đổi QuyTrinhId`

Chỉ dùng ví dụ trên nếu changes thực tế đúng như vậy.

## Yêu cầu Description

Xuất Markdown theo đúng format:

## Summary
- Liệt kê ngắn gọn các thay đổi thực tế.
- Nêu endpoint/module liên quan.
- Không bịa requirement.

## Test plan
- [ ] Các API/case cần test dựa trên changes.
- [ ] Các edge case liên quan.
- [ ] Build/test nếu phù hợp.

Không sửa code.

Chỉ trả ra:

Title:
<PR title>

Description:
<markdown để copy vào GitHub>
