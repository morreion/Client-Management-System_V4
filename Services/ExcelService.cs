using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client_Management_System_V4.Models;
using ClosedXML.Excel;

namespace Client_Management_System_V4.Services
{
    public class ExcelService : IExcelService
    {
        public Task GenerateContactListReportAsync(IEnumerable<Client> clients, List<string> selectedColumns, string filePath)
        {
            return Task.Run(() =>
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Contact List");

                    // 1. Add Headers
                    for (int i = 0; i < selectedColumns.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = selectedColumns[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#10B981"); // Green theme
                        worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
                    }

                    // 2. Add Data
                    int row = 2;
                    foreach (var client in clients)
                    {
                        for (int i = 0; i < selectedColumns.Count; i++)
                        {
                            var colName = selectedColumns[i];
                            var value = GetClientValue(client, colName);
                            worksheet.Cell(row, i + 1).Value = value;
                        }
                        row++;
                    }

                    // 3. Format Table
                    var range = worksheet.Range(1, 1, row - 1, selectedColumns.Count);
                    range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    
                    worksheet.Columns().AdjustToContents();

                    workbook.SaveAs(filePath);
                }
            });
        }

        private string GetClientValue(Client client, string columnName)
        {
            return columnName switch
            {
                "Name" => client.Name,
                "Mobile" => client.Mobile ?? "",
                "Email" => client.Email ?? "",
                "Address" => client.Address ?? "",
                "DOB" => client.DOB?.ToString("dd/MM/yyyy") ?? "",
                "Gender" => client.GenderDisplay,
                "Occupation" => client.Occupation ?? "",
                "Ref" => client.Ref ?? "",
                _ => ""
            };
        }
    }
}
