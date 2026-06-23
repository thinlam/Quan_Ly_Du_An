using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Aspose.Cells;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Offices;

public class ImporterHelper(IServiceProvider serviceProvider) : IImporterHelper
{
    private readonly IAsposeHelper _asposeHelper = serviceProvider.GetRequiredService<IAsposeHelper>();
    public AsposeResult GetTemplate(string templatePath, List<List<ComboData>>? comboData = null)
    {
        _asposeHelper.EnsureLicense();
        var workbook = new Workbook(templatePath);

        if (comboData == null || comboData.Count == 0)
        {
            var worksheetData = workbook.Worksheets[0];
            var binding = _asposeHelper.ExtractTemplateBinding(worksheetData);
            worksheetData.Cells.DeleteRow(binding.TemplateRowIndex);
            return new AsposeResult()
            {
                FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, templatePath),
                ContentType = templatePath.GetContentType()
            };
        }


        // BUOC 1: GHI DU LIEU VAO WORKSHEET[1]
        var comboSheet = workbook.Worksheets[1]; // Sheet chua du lieu danh sach
        // Moi phan tu tuong ung voi 1 placeholder ($cbo1, $cbo2, ...)
        var placeholdersDataPositions
            = new List<(int row, int col, int count)>();
        for (var i = 0; i < comboData.Count; i++)
        {
            /*
             * Custom placeholder:
             * $cbo + so luong cbo => vi du co 1 cbo thi $cbo1
             * _childOf[ten_cbo_cha]: dung cho table con vi du $cbo2_childOf[cbo1]
             */
            var cell = comboSheet.Cells.Find("$cbo" + (i + 1), null);

            // Neu khong tim thay placeholder, bo qua
            if (cell == null) continue;

            // Lay style cua o placeholder
            var baseStyle = cell.GetStyle();

            // 3. Tim xem o placeholder thuoc bang nao
            var foundTable = comboSheet.ListObjects.FirstOrDefault(tbl =>
                cell.Row >= tbl.StartRow && cell.Row <= tbl.EndRow && cell.Column >= tbl.StartColumn &&
                cell.Column <= tbl.EndColumn);

            // 4. Ghi gia tri dau tien vao o placeholder (de len) va set style
            cell.PutValue(comboData[i][0].Name);
            cell.SetStyle(baseStyle);

            // 5. Neu co nhieu gia tri hon 1, ta se ghi xuong cac dong tiep theo
            var additionalRows = comboData[i].Count - 1;

            // 6. Neu o nam trong mot bang, ta mo rong bang neu can
            if (foundTable != null && additionalRows > 0)
            {
                var oldStartRow = foundTable.StartRow;
                var oldEndRow = foundTable.EndRow;
                var oldStartCol = foundTable.StartColumn;
                var oldEndCol = foundTable.EndColumn;

                // Tinh dong cuoi moi
                var newEndRow = oldEndRow + additionalRows;

                // Resize bang de bao gom cac dong moi
                foundTable.Resize(oldStartRow, oldStartCol, newEndRow, oldEndCol, true);
            }

            // 7. Ghi cac gia tri con lai vao cung cot, cac dong tiep theo
            for (var j = 1; j < comboData[i].Count; j++)
            {
                var targetRow = cell.Row + j;
                var col = cell.Column;

                var newCell = comboSheet.Cells[targetRow, col];
                newCell.PutValue(comboData[i][j].Name);
                newCell.SetStyle(baseStyle);
            }

            // Luu lai vi tri & so luong dong vua ghi
            placeholdersDataPositions.Add((cell.Row, cell.Column, comboData[i].Count));
        }
        comboSheet.IsVisible = false;

        // BUOC 2: TAO DATA VALIDATION TRONG WORKSHEET[0]
        var mainSheet = workbook.Worksheets[0];

        for (var i = 0; i < comboData.Count; i++)
        {
            var placeholder = "$cbo" + (i + 1);

            // Tim placeholder trong Worksheet[0]
            var findOpts = new FindOptions();
            var startCell = mainSheet.Cells[0, 0];
            var cell = mainSheet.Cells.Find(placeholder, startCell, findOpts);

            if (cell == null)
                continue; // Khong thay placeholder trong sheet[0], bo qua

            // Xoa gia tri o (xoa noi dung placeholder) khi tao DataValidation
            cell.PutValue(string.Empty);

            // Lay toa do vung du lieu da ghi o Worksheet[1]
            var (dataRow, dataCol, dataCount) = placeholdersDataPositions[i];

            // Chuyen toa do (row, col) -> chuoi A1 (vi du: A1, A2, B5,...)
            // Su dung ham GetAbsoluteCellName de tao tham chieu tuyet doi cho start va end
            var startRef = _asposeHelper.GetAbsoluteCellName(dataRow, dataCol);
            var endRef = _asposeHelper.GetAbsoluteCellName(dataRow + dataCount - 1, dataCol);

            // Ghep thanh cong thuc dang: =Sheet2!A1:A5
            var dataSheetName = comboSheet.Name; // Ten Worksheet[1]
            var rangeFormula = $"='{dataSheetName}'!{startRef}:{endRef}";

            // Ap dung DataValidation cho o tim duoc va cac dong phia duoi trong cung cot
            // (vi du: tu o hien tai den dong 1000)
            var area = new CellArea
            {
                StartRow = cell.Row,
                EndRow = 1000, // hoac 1048575 neu muon ap dung cho toan cot
                StartColumn = cell.Column,
                EndColumn = cell.Column
            };
            // Tao validation kieu List
            var idx = mainSheet.Validations.Add(area);
            var validation = mainSheet.Validations[idx];
            validation.Type = ValidationType.List;
            validation.InCellDropDown = true;
            validation.Formula1 = rangeFormula;
        }

        return new AsposeResult()
        {
            FileBytes = _asposeHelper.SaveWorkbookToBytes(workbook, templatePath),
            ContentType = templatePath.GetContentType()
        };
    }

    public List<T> ReadDataFromExcel<T>(Stream excelStream)
        where T : new()
    {
        var records = new List<T>();

        // Mo workbook tu stream
        var workbook = new Workbook(excelStream);
        var worksheet = workbook.Worksheets[0]; // Lay sheet dau tien
        var cells = worksheet.Cells;

        // Lay danh sach cac property public cua T
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        // Tinh toan thong tin [Required] cho moi property (lay attribute 1 lan ngoai vong lap)
        // Neu property co Required thi trong dictionary se luu instance cua RequiredAttribute, nguoc lai null.
        var requiredMap = props.ToDictionary(
            p => p,
            p => p.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute
        );
        var descriptionMap = props.ToDictionary(
            p => p,
            p => p.GetDescription()
        );


        var tables = _asposeHelper.FindTableStartPosition(worksheet);


        // Gia su row 0 la tieu de (vi du: cac o mau vang), du lieu bat dau tu row 1

        foreach (var (name, startRow, startColumn, endRow, endColumn) in tables)
        {
            // Tao instance cua T
            for (int rowIndex = startRow + 2; rowIndex <= endRow; rowIndex++)
            {
                T instance = new();

                try
                {
                    //Tu dong header bo qua header va dong description = 2
                    // Voi moi property, anh xa voi cot tuong ung theo thu tu
                    for (int colIndex = startColumn; colIndex <= endColumn; colIndex++)
                    {
                        //Lay gia tri cua o
                        var cellValue = _asposeHelper.GetCellValueAsString(cells[rowIndex, colIndex]);
                        //tim vi tri cua prop ung voi column bat dau
                        var prop = props[colIndex - startColumn];

                        // Neu property co Required thi kiem tra gia tri khong duoc de trong
                        if (requiredMap[prop] != null && string.IsNullOrWhiteSpace(cellValue))
                        {
                            ManagedException.Throw(
                                $"Truong {descriptionMap[prop]} (dong {rowIndex + 1}) la bat buoc va khong duoc de trong.");
                        }

                        // Chuyen doi gia tri tu string sang kieu du lieu cua property
                        object? convertedValue = cellValue.ConvertStringToPropertyType(prop.PropertyType);

                        // Gan gia tri cho property
                        prop.SetValue(instance, convertedValue, null);
                    }

                    records.Add(instance);
                }
                catch (ManagedException)
                {
                    continue;
                }
            }
        }

        return records;
    }
}