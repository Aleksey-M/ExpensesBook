using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

internal static class ExpensesXlsxParser
{
    public static async Task<(List<ParsedExpense> expenses, string? errorMessage)> Parse(Stream xlsxStream, CancellationToken token)
    {
        await Task.Yield(); // для обновления анимации

        const int firstIndex = 0;

        try
        {
            using var workbook = new XLWorkbook(xlsxStream);
            var worksheet = workbook.Worksheet(1);

            var result = new List<ParsedExpense>();

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
                    return (new(), $"Пустое описание расходов в строке {index}");
                }

                var categoryName = row.Cell(4).Value?.ToString();
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    return (new(), $"Не указана категория расходов в строке {index}");
                }

                var groupName = row.Cell(5).Value?.ToString();

                result.Add(new ParsedExpense(
                    new Expense
                    {
                        Id = Guid.NewGuid(),
                        Amounth = amounth,
                        Date = date,
                        Description = description
                    },
                    categoryName,
                    groupName
                ));
            }

            return (result, null);
        }
        catch (Exception e)
        {
            return (new(), e.Message);
        }
    }
}
