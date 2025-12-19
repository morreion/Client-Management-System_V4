using System.Threading.Tasks;
using Client_Management_System_V4.Models;
using System.Collections.Generic;

namespace Client_Management_System_V4.Services
{
    public interface IExcelService
    {
        Task GenerateContactListReportAsync(IEnumerable<Client> clients, List<string> selectedColumns, string filePath);
    }
}
