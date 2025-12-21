using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client_Management_System_V4.Data;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using Client_Management_System_V4.Utilities;
using System.Linq;

namespace Client_Management_System_V4.ViewModel
{
    /// <summary>
    /// ViewModel for Client management with full CRUD operations
    /// </summary>
    public class ClientVM : ViewModelBase
    {
        private readonly ClientRepository _repository;
        private ObservableCollection<Client> _clients = new();
        private Client? _selectedClient;
        private string _searchText = string.Empty;
        private bool _isLoading;
        private bool _isEditMode;

        #region Properties

        /// <summary>
        /// Collection of all clients for DataGrid binding
        /// </summary>
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Currently selected client from DataGrid
        /// </summary>
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsClientSelected));
            }
        }

        /// <summary>
        /// Search text for filtering clients
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    _ = SearchClientsAsync();
                }
            }
        }

        // Filters
        public ObservableCollection<string> GenderFilters { get; set; } = new ObservableCollection<string> { "All Genders", "Male", "Female" };

        private string _selectedGenderFilter = "All Genders";
        public string SelectedGenderFilter
        {
            get => _selectedGenderFilter;
            set 
            { 
                _selectedGenderFilter = value; 
                OnPropertyChanged(); 
                _ = SearchClientsAsync(); 
            }
        }

        private bool _filterByAge;
        public bool FilterByAge
        {
            get => _filterByAge;
            set 
            { 
                _filterByAge = value; 
                OnPropertyChanged(); 
                _ = SearchClientsAsync(); 
            }
        }

        private int _minAge = 0;
        public int MinAge
        {
            get => _minAge;
            set 
            { 
                _minAge = value; 
                OnPropertyChanged(); 
                if (FilterByAge) _ = SearchClientsAsync(); 
            }
        }

        private int _maxAge = 100;
        public int MaxAge
        {
            get => _maxAge;
            set 
            { 
                _maxAge = value; 
                OnPropertyChanged(); 
                if (FilterByAge) _ = SearchClientsAsync(); 
            }
        }

        /// <summary>
        /// Loading indicator
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Edit mode flag
        /// </summary>
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if a client is selected
        /// </summary>
        public bool IsClientSelected => SelectedClient != null;

        #endregion

        #region Commands

        public ICommand LoadedCommand { get; }
        public ICommand LoadClientsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ShowAllCommand { get; }
        public ICommand AddNewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        #endregion

        public ClientVM()
        {
            _repository = new ClientRepository();

            // Initialize commands
            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            LoadClientsCommand = new RelayCommand(async _ => await LoadClientsAsync());
            SearchCommand = new RelayCommand(async _ => await SearchClientsAsync());
            ShowAllCommand = new RelayCommand(async _ => 
            {
                SearchText = string.Empty;
                await SearchClientsAsync();
            });
            AddNewCommand = new RelayCommand(_ => AddNewClient());
            SaveCommand = new RelayCommand(async _ => await SaveClientAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteClientAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());

            // Don't call async initialization here - it will be called from LoadedCommand
        }

        #region Methods

        /// <summary>
        /// Initializes and loads clients from database
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadClientsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads all clients from database
        /// </summary>
        private async Task LoadClientsAsync()
        {
            try
            {
                IsLoading = true;
                var clients = await _repository.GetAllAsync();
                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading clients: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Searches clients by name or email
        /// </summary>
        private async Task SearchClientsAsync()
        {
            try
            {
                IsLoading = true;
                
                IEnumerable<Client> clients;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    clients = await _repository.GetAllAsync();
                }
                else
                {
                    clients = await _repository.SearchAsync(SearchText);
                }

                // Apply Filters
                if (SelectedGenderFilter == "Male")
                {
                    clients = clients.Where(c => c.Gender == 1);
                }
                else if (SelectedGenderFilter == "Female")
                {
                    clients = clients.Where(c => c.Gender == 0);
                }

                if (FilterByAge)
                {
                    clients = clients.Where(c => c.Age.HasValue && c.Age.Value >= MinAge && c.Age.Value <= MaxAge);
                }

                Clients = new ObservableCollection<Client>(clients);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching clients: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Adds a new client
        /// </summary>
        private void AddNewClient()
        {
            SelectedClient = new Client
            {
                Date_First_Consultation = DateTime.Today
            };
            IsEditMode = true;
        }

        /// <summary>
        /// Saves the current client (add or update)
        /// </summary>
        private async Task SaveClientAsync()
        {
            if (SelectedClient == null) return;

            // Validation
            if (string.IsNullOrWhiteSpace(SelectedClient.Name))
            {
                MessageBox.Show("Client name is required.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedClient.Email))
            {
                var email = SelectedClient.Email.Trim();
                var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(email, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    MessageBox.Show($"The email address '{email}' is not valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                SelectedClient.Email = email;
            }

            try
            {
                IsLoading = true;

                if (SelectedClient.ClientID == 0)
                {
                    // Add new client
                    var newId = await _repository.AddAsync(SelectedClient);
                    SelectedClient.ClientID = newId;
                    Clients.Add(SelectedClient);
                    MessageBox.Show("Client added successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Update existing client
                    SelectedClient.Date_Last_Consultation = DateTime.Today;
                    var success = await _repository.UpdateAsync(SelectedClient);
                    
                    if (success)
                    {
                        MessageBox.Show("Client updated successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadClientsAsync();
                    }
                }

                IsEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving client: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the selected client
        /// </summary>
        private async Task DeleteClientAsync()
        {
            if (SelectedClient == null || SelectedClient.ClientID == 0) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {SelectedClient.Name}?\n\nThis will also delete all associated health records.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                var success = await _repository.DeleteAsync(SelectedClient.ClientID);
                
                if (success)
                {
                    Clients.Remove(SelectedClient);
                    SelectedClient = null;
                    MessageBox.Show("Client deleted successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting client: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cancels the current edit
        /// </summary>
        private void CancelEdit()
        {
            if (SelectedClient?.ClientID == 0)
            {
                SelectedClient = null;
            }
            else
            {
                // Reload to discard changes
                _ = LoadClientsAsync();
            }
            IsEditMode = false;
        }

        #endregion
    }
}
