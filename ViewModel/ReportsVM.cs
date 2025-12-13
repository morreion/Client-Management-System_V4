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
        private readonly IPdfService _pdfService;

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

        // Contact List Properties
        public bool IncludeName { get; set; } = true;
        public bool IncludeMobile { get; set; } = true;
        public bool IncludeEmail { get; set; }
        public bool IncludeAddress { get; set; }
        public bool IncludeDOB { get; set; }
        public bool IncludeGender { get; set; }
        public bool IncludeOccupation { get; set; }
        public bool IncludeRef { get; set; }

        public ReportsVM()
        {
            _clientRepository = new ClientRepository();
            _pdfService = new PdfService();

            GenerateReportCommand = new RelayCommand(async _ => await GenerateReport(), _ => SelectedClient != null);
            GeneratePrescriptionReportCommand = new RelayCommand(async _ => await GeneratePrescriptionReport(), _ => SelectedClient != null);
            GenerateContactListCommand = new RelayCommand(async _ => await GenerateContactList());

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

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Details|*.pdf",
                FileName = $"ContactList_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsGenerating = true;
                try
                {
                    await _pdfService.GenerateContactListReportAsync(Clients, cols, saveFileDialog.FileName);
                    MessageBox.Show("Contact List generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    OpenPdf(saveFileDialog.FileName);
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
