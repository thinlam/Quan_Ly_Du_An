# Quy trình cập nhật tài liệu

## Tổng quan

Quy trình này đảm bảo tài liệu luôn được cập nhật đồng bộ với code changes, theo methodology kLOC (thousands of lines of code) để đo lường completeness của documentation.

## Nguyên tắc cập nhật

1. **Cập nhật ngay**: Mỗi khi thay đổi code, cập nhật tài liệu tương ứng
2. **Review bắt buộc**: Code review phải bao gồm documentation review
3. **kLOC measurement**: Đảm bảo tỷ lệ documentation/code >= 0.1 (10%)
4. **Tiếng Việt**: Tất cả tài liệu bằng tiếng Việt
5. **Version control**: Tài liệu được version control cùng code

## Quy trình cập nhật theo task

### 1. Phân tích task
- Xác định scope của thay đổi
- Đánh giá ảnh hưởng đến tài liệu
- Ước tính kLOC impact

### 2. Cập nhật tài liệu tương ứng

#### Thay đổi API
- Cập nhật `api.md` với endpoints mới/sửa
- Cập nhật Swagger comments trong code
- Test API documentation

#### Thay đổi business logic
- Cập nhật `features.md` với use cases mới
- Cập nhật `architecture.md` nếu thay đổi design

#### Thay đổi database
- Cập nhật `data-model.md` với schema changes
- Cập nhật ERD nếu có thay đổi relationships

#### Thay đổi kiến trúc
- Cập nhật `architecture.md` với diagrams mới
- Cập nhật `README.md` nếu cần

### 3. Đo lường kLOC

#### Công thức tính kLOC
```
kLOC_code = (lines_of_code) / 1000
kLOC_docs = (lines_of_documentation) / 1000
Documentation_Ratio = kLOC_docs / kLOC_code
```

#### Target ratios
- **Minimum**: 0.1 (10% documentation per 1000 lines code)
- **Target**: 0.15-0.2 (15-20%)
- **Excellent**: >0.25 (25%)

### 4. Scripts tự động

#### Script đếm kLOC
```bash
#!/bin/bash
# count-kloc.sh

echo "=== kLOC Measurement Report ==="
echo "Generated on: $(date)"
echo

# Count code lines (C# files)
CODE_FILES=$(find . -name "*.cs" -not -path "./*/bin/*" -not -path "./*/obj/*" | wc -l)
CODE_LINES=$(find . -name "*.cs" -not -path "./*/bin/*" -not -path "./*/obj/*" -exec cat {} \; | wc -l)
CODE_KLOC=$(echo "scale=3; $CODE_LINES/1000" | bc)

echo "Code Statistics:"
echo "- Files: $CODE_FILES"
echo "- Lines: $CODE_LINES"
echo "- kLOC: $CODE_KLOC"
echo

# Count documentation lines (Markdown files)
DOC_FILES=$(find ./docs -name "*.md" | wc -l)
DOC_LINES=$(find ./docs -name "*.md" -exec cat {} \; | wc -l)
DOC_KLOC=$(echo "scale=3; $DOC_LINES/1000" | bc)

echo "Documentation Statistics:"
echo "- Files: $DOC_FILES"
echo "- Lines: $DOC_LINES"
echo "- kLOC: $DOC_KLOC"
echo

# Calculate ratio
if (( $(echo "$CODE_KLOC > 0" | bc -l) )); then
    RATIO=$(echo "scale=3; $DOC_KLOC/$CODE_KLOC" | bc)
    PERCENTAGE=$(echo "scale=1; $RATIO*100" | bc)

    echo "Documentation Ratio: $RATIO ($PERCENTAGE%)"

    # Assessment
    if (( $(echo "$RATIO >= 0.25" | bc -l) )); then
        echo "Assessment: EXCELLENT (≥25%)"
    elif (( $(echo "$RATIO >= 0.15" | bc -l) )); then
        echo "Assessment: GOOD (15-25%)"
    elif (( $(echo "$RATIO >= 0.1" | bc -l) )); then
        echo "Assessment: ACCEPTABLE (10-15%)"
    else
        echo "Assessment: INSUFFICIENT (<10%) - NEEDS IMPROVEMENT"
    fi
else
    echo "No code files found!"
fi
```

#### Chạy script
```bash
chmod +x count-kloc.sh
./count-kloc.sh
```

### 5. CI/CD Integration

#### GitHub Actions workflow
```yaml
name: Documentation Check
on:
  pull_request:
    paths:
      - '**.cs'
      - 'docs/**'

jobs:
  documentation-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Count kLOC
        run: |
          chmod +x count-kloc.sh
          ./count-kloc.sh > kloc-report.txt

      - name: Check documentation ratio
        run: |
          RATIO=$(grep "Documentation Ratio" kloc-report.txt | grep -oP '\d+\.\d+')
          if (( $(echo "$RATIO < 0.1" | bc -l) )); then
            echo "Documentation ratio too low: $RATIO"
            exit 1
          fi

      - name: Upload report
        uses: actions/upload-artifact@v3
        with:
          name: kloc-report
          path: kloc-report.txt
```

### 6. Checklist cập nhật tài liệu

#### Pre-commit checklist
- [x] API changes documented in `api.md`
- [x] New features added to `features.md`
- [x] Schema changes reflected in `data-model.md`
- [x] Architecture changes updated in `architecture.md`
- [x] README updated if needed
- [x] kLOC ratio maintained ≥10%

#### Code review checklist
- [ ] Documentation accuracy verified
- [ ] Vietnamese language used consistently
- [ ] Diagrams updated if needed
- [ ] Links and references working
- [ ] Formatting consistent

### 7. Templates cho tài liệu

#### Template API documentation
```markdown
### {HTTP_METHOD} {endpoint}
**Mô tả**: {description}

**Parameters**:
- `{param}`: {description} ({type}, {required})

**Request Body**:
```json
{example}
```

**Response**:
```json
{example}
```

**Error Codes**:
- `400`: {error_description}
- `404`: {error_description}
```

#### Template feature documentation
```markdown
### {Feature Name}
**Mô tả**: {description}

**Use Cases**:
1. {use_case_1}
2. {use_case_2}

**Business Rules**:
- {rule_1}
- {rule_2}

**Data Flow**:
{description}
```

### 8. Maintenance

#### Định kỳ review
- **Hàng tuần**: Check kLOC ratio
- **Hàng tháng**: Review documentation completeness
- **Hàng quý**: Update architecture diagrams
- **Hàng năm**: Major documentation overhaul

#### Cleanup
- Remove outdated documentation
- Update deprecated API docs
- Archive old versions
- Consolidate duplicate content

### 9. Tools hỗ trợ

#### Documentation tools
- **Markdown editors**: VS Code, Typora
- **Diagram tools**: Mermaid, Draw.io
- **API testing**: Postman, Swagger UI
- **Version control**: Git, GitHub

#### Automation tools
- **kLOC counter**: Custom scripts
- **Link checker**: markdown-link-check
- **Spell checker**: cspell
- **Formatter**: Prettier

### 10. Metrics và KPIs

#### Documentation KPIs
- **Completeness**: kLOC ratio ≥15%
- **Accuracy**: <5% error rate in documentation
- **Timeliness**: Documentation updated within 1 week of code changes
- **Usability**: Developer satisfaction score >4/5

#### Monitoring
- Track kLOC ratio over time
- Monitor documentation update frequency
- Measure time to find information
- Survey developer satisfaction

### 11. Training

#### Onboarding cho developers
- Documentation standards training
- kLOC measurement explanation
- Tool usage training
- Best practices workshop

#### Continuous learning
- Monthly documentation tips
- Tool updates
- Process improvements
- Success stories sharing

## Phụ lục

### A. Glossary
- **kLOC**: Thousand Lines of Code
- **Spec Kit**: Specification documentation package
- **ERD**: Entity Relationship Diagram
- **API**: Application Programming Interface

### B. References
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Documentation Best Practices](https://www.writethedocs.org/guide/)
- [kLOC Measurement](https://en.wikipedia.org/wiki/Source_lines_of_code)

### C. Contact
- **Documentation Owner**: {name}
- **Technical Lead**: {name}
- **DevOps Team**: {contact}