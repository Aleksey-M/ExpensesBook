using ClosedXML.Excel;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

public static class IncomesXlsxParser
{
    public static async Task<(List<Income>, string? errorMessage)> Parse(Stream xlsxStream, CancellationToken token)
    {
        await Task.Yield(); // для обновления анимации

        const int firstIndex = 0;

        try
        {
            using var workbook = new XLWorkbook(xlsxStream);
            var worksheet = workbook.Worksheet(1);

            var result = new List<Income>();

            foreach (var (row, index) in worksheet.Rows().Select((x, i) => (x, i)))
            {
                await Task.Yield(); // для обновления анимации

                if (firstIndex == index) continue;

                var cell1 = row.Cell(1).Value.ToString();
                if (!DateTimeOffset.TryParse(cell1, out var date))
                {
                    return (new(), $"Неверный формат даты в строке {index}: {cell1}");
                }

                var cell2 = row.Cell(2).Value?.ToString();
                if (!double.TryParse(cell2, out var amounth))
                {
                    return (new(), $"Неверный формат числа в строке {index}: {cell2}");
                }

                var description = row.Cell(3).Value?.ToString();
                if (string.IsNullOrWhiteSpace(description))
                {
                    return (new(), $"Пустое описание в строке {index}");
                }

                result.Add(new Income
                {
                    Id = Guid.NewGuid(),
                    Amounth = amounth,
                    Date = date,
                    Description = description
                });
            }

            return (result, null);
        }
        catch (Exception e)
        {
            return (new(), e.Message);
        }
    }
}
