using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using Client_Management_System_V4.Utilities;

namespace Client_Management_System_V4.ViewModel
{
    public class TreatmentVM : ViewModelBase
    {
        private readonly TreatmentRepository _repository;
        private readonly ClientRepository _clientRepository;
        private ObservableCollection<Treatment> _treatmentList;
        private ObservableCollection<Client> _clients;
        private Treatment? _selectedTreatment;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Treatment> TreatmentList
        {
            get => _treatmentList;
            set { _treatmentList = value; OnPropertyChanged(nameof(TreatmentList)); }
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(nameof(Clients)); }
        }

        public Treatment? SelectedTreatment
        {
            get => _selectedTreatment;
            set
            {
                _selectedTreatment = value;
                OnPropertyChanged(nameof(SelectedTreatment));
                OnPropertyChanged(nameof(IsSelectionActive));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public bool IsSelectionActive => SelectedTreatment != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }

        public TreatmentVM()
        {
            _repository = new TreatmentRepository();
            _clientRepository = new ClientRepository();
            _treatmentList = new ObservableCollection<Treatment>();
            _clients = new ObservableCollection<Client>();

            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            AddCommand = new RelayCommand(_ => AddNew());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsSelectionActive);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsSelectionActive);
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                var clients = await _clientRepository.GetAllAsync();
                Clients = new ObservableCollection<Client>(clients);

                var records = await _repository.GetAllAsync();
                TreatmentList = new ObservableCollection<Treatment>(records);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddNew()
        {
            SelectedTreatment = new Treatment();
        }

        private async Task SaveAsync()
        {
            if (SelectedTreatment == null) return;

            if (SelectedTreatment.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedTreatment.TreatmentID == null)
                {
                    var id = await _repository.AddAsync(SelectedTreatment);
                    SelectedTreatment.TreatmentID = id;

                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedTreatment.ClientID);
                    if (client != null) SelectedTreatment.ClientName = client.Name;

                    TreatmentList.Insert(0, SelectedTreatment);
                    MessageBox.Show("Treatment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _repository.UpdateAsync(SelectedTreatment);
                    
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedTreatment.ClientID);
                    if (client != null) SelectedTreatment.ClientName = client.Name;
                    
                    MessageBox.Show("Treatment updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving treatment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedTreatment?.TreatmentID == null) return;

            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedTreatment.TreatmentID.Value);
                    TreatmentList.Remove(SelectedTreatment);
                    SelectedTreatment = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await InitializeAsync();
                }
                else
                {
                    var results = await _repository.SearchAsync(SearchText);
                    TreatmentList = new ObservableCollection<Treatment>(results);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedTreatment = null;
        }
    }
}
