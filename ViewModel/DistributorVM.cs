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
    public class DistributorVM : ViewModelBase
    {
        private readonly DistributorRepository _repository;
        private ObservableCollection<Distributor> _distributors;
        private Distributor? _selectedDistributor;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Distributor> Distributors
        {
            get => _distributors;
            set
            {
                _distributors = value;
                OnPropertyChanged(nameof(Distributors));
            }
        }

        public Distributor? SelectedDistributor
        {
            get => _selectedDistributor;
            set
            {
                _selectedDistributor = value;
                OnPropertyChanged(nameof(SelectedDistributor));
                OnPropertyChanged(nameof(IsDistributorSelected));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    _ = SearchDistributors();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsDistributorSelected => SelectedDistributor != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand LoadDistributorsCommand { get; }

        public DistributorVM()
        {
            _repository = new DistributorRepository();
            _distributors = new ObservableCollection<Distributor>();

            // Initialize commands
            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            AddCommand = new RelayCommand(_ => AddDistributor());
            SaveCommand = new RelayCommand(async _ => await SaveDistributor(), _ => IsDistributorSelected);
            DeleteCommand = new RelayCommand(async _ => await DeleteDistributor(), _ => IsDistributorSelected);
            SearchCommand = new RelayCommand(async _ => await SearchDistributors());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            LoadDistributorsCommand = new RelayCommand(async _ => await LoadDistributorsAsync());
        }

        private async Task InitializeAsync()
        {
            await LoadDistributorsAsync();
        }

        private async Task LoadDistributorsAsync()
        {
            try
            {
                IsLoading = true;
                var distributors = await _repository.GetAllAsync();
                Distributors = new ObservableCollection<Distributor>(distributors);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading distributors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddDistributor()
        {
            SelectedDistributor = new Distributor();
        }

        private async Task SaveDistributor()
        {
            if (SelectedDistributor == null) return;

            // Validation
            if (string.IsNullOrWhiteSpace(SelectedDistributor.Name))
            {
                MessageBox.Show("Please enter a name for the distributor.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedDistributor.DistributorID == null)
                {
                    // Add new
                    var id = await _repository.AddAsync(SelectedDistributor);
                    SelectedDistributor.DistributorID = id;
                    Distributors.Add(SelectedDistributor);
                    MessageBox.Show("Distributor added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Update existing
                    await _repository.UpdateAsync(SelectedDistributor);
                    
                    // Refresh the list
                    var updatedDistributor = Distributors.FirstOrDefault(d => d.DistributorID == SelectedDistributor.DistributorID);
                    if (updatedDistributor != null)
                    {
                        updatedDistributor.Name = SelectedDistributor.Name;
                        updatedDistributor.Address = SelectedDistributor.Address;
                        updatedDistributor.Work_Phone = SelectedDistributor.Work_Phone;
                        updatedDistributor.Mobile = SelectedDistributor.Mobile;
                        updatedDistributor.Email = SelectedDistributor.Email;
                        updatedDistributor.Website = SelectedDistributor.Website;
                    }
                    
                    MessageBox.Show("Distributor updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadDistributorsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving distributor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteDistributor()
        {
            if (SelectedDistributor?.DistributorID == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedDistributor.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedDistributor.DistributorID.Value);
                    Distributors.Remove(SelectedDistributor);
                    SelectedDistributor = null;
                    MessageBox.Show("Distributor deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting distributor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task SearchDistributors()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadDistributorsAsync();
                }
                else
                {
                    var results = await _repository.SearchAsync(SearchText);
                    Distributors = new ObservableCollection<Distributor>(results);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching distributors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            SelectedDistributor = null;
        }
    }
}
