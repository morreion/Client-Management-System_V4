using System.Threading.Tasks;
using Client_Management_System_V4.Models;

namespace Client_Management_System_V4.Services
{
    public interface IPdfService
    {
        Task GenerateHealthReportAsync(Client client, ReportOptions options, string filePath);
        Task GeneratePrescriptionReportAsync(Client client, string filePath);
        Task GenerateContactListReportAsync(System.Collections.Generic.IEnumerable<Client> clients, System.Collections.Generic.List<string> selectedColumns, string filePath);
        Task GenerateSupplementsReportAsync(System.Collections.Generic.IEnumerable<Supplement> supplements, string filePath);
        Task GenerateDistributorReportAsync(System.Collections.Generic.IEnumerable<Distributor> distributors, string filePath);
    }
}
