using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Reports;

internal static class IncomesXlsxExport
{
    public static async Task<byte[]> ExportExpenses(List<Income> incomes)
    {
        await Task.Delay(1); // для обновления анимации кнопки

        const int firstRow = 2;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Доходы");

        SetColumnsWidths(worksheet);

        SetTableHeader(worksheet);

        SetTableCellsStyles(incomes.Count, firstRow, worksheet);

        int rowNo = firstRow;

        foreach (var income in incomes)
        {
            worksheet.Cell(rowNo, 1).Value = income.Date.ToString("yyyy.MM.dd");
            worksheet.Cell(rowNo, 2).Value = income.Amounth;
            worksheet.Cell(rowNo, 3).Value = income.Description;

            rowNo++;
        }

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);

        return ms.ToArray();
    }

    private static void SetTableCellsStyles(int expensesCount, int firstRow, IXLWorksheet worksheet)
    {
        worksheet.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Range($"B2:B{expensesCount + firstRow - 1}").Style.NumberFormat.Format = "# ##0.00";
        var allCellsStyle = worksheet.Range($"A1:E{expensesCount + firstRow - 1}").Style;

        allCellsStyle.Border.LeftBorder = XLBorderStyleValues.Thin;
        allCellsStyle.Border.RightBorder = XLBorderStyleValues.Thin;
        allCellsStyle.Border.TopBorder = XLBorderStyleValues.Thin;
        allCellsStyle.Border.BottomBorder = XLBorderStyleValues.Thin;
    }

    private static void SetTableHeader(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "Дата";
        worksheet.Cell(1, 2).Value = "Сумма";
        worksheet.Cell(1, 3).Value = "Описание";

        var headerStyle = worksheet.Range("A1:C1").Style;
        headerStyle.Font.SetBold();
        headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerStyle.Fill.BackgroundColor = XLColor.Yellow;
    }

    private static void SetColumnsWidths(IXLWorksheet worksheet)
    {
        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 10;
        worksheet.Column(3).Width = 55;
    }
}
