using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using Client_Management_System_V4.Services;
using Client_Management_System_V4.Utilities;
using Microsoft.Win32;

namespace Client_Management_System_V4.ViewModel
{
    public class ReportsVM : ViewModelBase
    {
        private readonly ClientRepository _clientRepository;
        private readonly SupplementRepository _supplementRepository;
        private readonly DistributorRepository _distributorRepository;
        private readonly IPdfService _pdfService;
        private readonly IExcelService _excelService;

        private Client? _selectedClient;
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                // (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged(); 
                // RelayCommand uses CommandManager to requery automatically
            }
        }

        public ObservableCollection<Client> Clients { get; set; } = new();

        public ReportOptions Options { get; set; } = new();

        private bool _isGenerating;
        public bool IsGenerating
        {
            get => _isGenerating;
            set { _isGenerating = value; OnPropertyChanged(); }
        }

        public ICommand GenerateReportCommand { get; }
        public ICommand GeneratePrescriptionReportCommand { get; }
        public ICommand GenerateContactListCommand { get; }
        public ICommand GenerateSupplementsReportCommand { get; }
        public ICommand GenerateDistributorReportCommand { get; }

        // Contact List Properties
        private string _exportFormat = "PDF"; // Default to PDF
        public string ExportFormat
        {
            get => _exportFormat;
            set { _exportFormat = value; OnPropertyChanged(); }
        }

        public bool IncludeName { get; set; } = true;
        public bool IncludeMobile { get; set; } = true;
        public bool IncludeEmail { get; set; }
        public bool IncludeAddress { get; set; }
        public bool IncludeDOB { get; set; }
        public bool IncludeGender { get; set; }
        public bool IncludeOccupation { get; set; }
        public bool IncludeRef { get; set; }

        // Filter Properties
        public ObservableCollection<string> GenderFilters { get; set; } = new ObservableCollection<string> { "All Genders", "Male", "Female" };

        private string _selectedGenderFilter = "All Genders";
        public string SelectedGenderFilter
        {
            get => _selectedGenderFilter;
            set { _selectedGenderFilter = value; OnPropertyChanged(); }
        }

        private bool _filterByAge;
        public bool FilterByAge
        {
            get => _filterByAge;
            set { _filterByAge = value; OnPropertyChanged(); }
        }

        private int _minAge = 0;
        public int MinAge
        {
            get => _minAge;
            set { _minAge = value; OnPropertyChanged(); }
        }

        private int _maxAge = 100;
        public int MaxAge
        {
            get => _maxAge;
            set { _maxAge = value; OnPropertyChanged(); }
        }

        public ReportsVM()
        {
            _clientRepository = new ClientRepository();
            _supplementRepository = new SupplementRepository();
            _distributorRepository = new DistributorRepository();
            _pdfService = new PdfService();
            _excelService = new ExcelService();

            GenerateReportCommand = new RelayCommand(async _ => await GenerateReport(), _ => SelectedClient != null);
            GeneratePrescriptionReportCommand = new RelayCommand(async _ => await GeneratePrescriptionReport(), _ => SelectedClient != null);
            GenerateContactListCommand = new RelayCommand(async _ => await GenerateContactList());
            GenerateSupplementsReportCommand = new RelayCommand(async _ => await GenerateSupplementsReport());
            GenerateDistributorReportCommand = new RelayCommand(async _ => await GenerateDistributorReport());

            LoadClients();
        }

        private async void LoadClients()
        {
            var clients = await _clientRepository.GetAllAsync();
            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
        }

        private async Task GenerateReport()
        {
            if (SelectedClient == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Details|*.pdf",
                FileName = $"Report_{SelectedClient.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsGenerating = true;
                try
                {
                    await _pdfService.GenerateHealthReportAsync(SelectedClient, Options, saveFileDialog.FileName);
                    MessageBox.Show("Health Report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    OpenPdf(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsGenerating = false;
                }
            }
        }

        private async Task GeneratePrescriptionReport()
        {
            if (SelectedClient == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Details|*.pdf",
                FileName = $"Prescription_{SelectedClient.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsGenerating = true;
                try
                {
                    await _pdfService.GeneratePrescriptionReportAsync(SelectedClient, saveFileDialog.FileName);
                    MessageBox.Show("Prescription Report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenPdf(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating prescription report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsGenerating = false;
                }
            }
        }

        private async Task GenerateContactList()
        {
            var cols = new System.Collections.Generic.List<string>();
            if (IncludeName) cols.Add("Name");
            if (IncludeMobile) cols.Add("Mobile");
            if (IncludeEmail) cols.Add("Email");
            if (IncludeAddress) cols.Add("Address");
            if (IncludeDOB) cols.Add("DOB");
            if (IncludeGender) cols.Add("Gender");
            if (IncludeOccupation) cols.Add("Occupation");
            if (IncludeRef) cols.Add("Ref");

            if (!cols.Any())
            {
                MessageBox.Show("Please select at least one column.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Apply Filters
            var filteredClients = Clients.AsEnumerable();

            // Gender Filter
            if (SelectedGenderFilter == "Male")
            {
                filteredClients = filteredClients.Where(c => c.Gender == 1);
            }
            else if (SelectedGenderFilter == "Female")
            {
                filteredClients = filteredClients.Where(c => c.Gender == 0);
            }

            // Age Filter
            if (FilterByAge)
            {
                filteredClients = filteredClients.Where(c => c.Age.HasValue && c.Age.Value >= MinAge && c.Age.Value <= MaxAge);
            }

            var finalClientList = filteredClients.ToList();

            if (!finalClientList.Any())
            {
                MessageBox.Show("No clients match the selected filters.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var isExcel = ExportFormat == "Excel";
            var filter = isExcel ? "Excel Files|*.xlsx" : "PDF Details|*.pdf";
            var extension = isExcel ? "xlsx" : "pdf";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                FileName = $"ContactList_{DateTime.Now:yyyyMMdd}.{extension}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsGenerating = true;
                try
                {
                    if (isExcel)
                    {
                        await _excelService.GenerateContactListReportAsync(finalClientList, cols, saveFileDialog.FileName);
                    }
                    else
                    {
                        await _pdfService.GenerateContactListReportAsync(new ObservableCollection<Client>(finalClientList), cols, saveFileDialog.FileName);
                    }
                    
                    MessageBox.Show("Contact List generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    if (!isExcel) // Optional: Open Excel file too? Usually yes.
                    {
                        OpenPdf(saveFileDialog.FileName);
                    }
                    else
                    {
                         // Open Excel file
                         var p = new System.Diagnostics.Process();
                         p.StartInfo = new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true };
                         p.Start();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating contact list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsGenerating = false;
                }
            }
        }

        private async Task GenerateSupplementsReport()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Details|*.pdf",
                FileName = $"Supplements_Inventory_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsGenerating = true;
                try
                {
                    var supplements = await _supplementRepository.GetAllAsync();
                    await _pdfService.GenerateSupplementsReportAsync(supplements, saveFileDialog.FileName);
                    MessageBox.Show("Supplements Report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenPdf(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating supplements report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsGenerating = false;
                }
            }
        }

        private async Task GenerateDistributorReport()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Details|*.pdf",
                FileName = $"Distributor_List_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsGenerating = true;
                try
                {
                    var distributors = await _distributorRepository.GetAllAsync();
                    await _pdfService.GenerateDistributorReportAsync(distributors, saveFileDialog.FileName);
                    MessageBox.Show("Distributor Report generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenPdf(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating distributor report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsGenerating = false;
                }
            }
        }

        private void OpenPdf(string filePath)
        {
            try 
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true };
                process.Start();
            }
            catch { /* Ignore if fails to open */ }
        }
    }
}
