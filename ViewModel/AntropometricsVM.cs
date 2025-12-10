using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using Client_Management_System_V4.Utilities;

namespace Client_Management_System_V4.ViewModel
{
    public class AntropometricsVM : ViewModelBase
    {
        private readonly AnthropometricsRepository _repository;
        private readonly ClientRepository _clientRepository;
        private ObservableCollection<Anthropometrics> _anthropometricsList;
        private ObservableCollection<Client> _clients;
        private Anthropometrics? _selectedAnthropometrics;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Anthropometrics> AnthropometricsList
        {
            get => _anthropometricsList;
            set
            {
                _anthropometricsList = value;
                OnPropertyChanged(nameof(AnthropometricsList));
            }
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set
            {
                _clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        public Anthropometrics? SelectedAnthropometrics
        {
            get => _selectedAnthropometrics;
            set
            {
                if (_selectedAnthropometrics != null)
                    _selectedAnthropometrics.PropertyChanged -= SelectedAnthropometrics_PropertyChanged;

                _selectedAnthropometrics = value;

                if (_selectedAnthropometrics != null)
                    _selectedAnthropometrics.PropertyChanged += SelectedAnthropometrics_PropertyChanged;

                OnPropertyChanged(nameof(SelectedAnthropometrics));
                OnPropertyChanged(nameof(IsSelectionActive));
            }
        }

        // Listen for property changes in the model to update calculated BMI in real-time
        private void SelectedAnthropometrics_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Anthropometrics.Weight) || e.PropertyName == nameof(Anthropometrics.Height))
            {
                 // Notify BMI might have changed (though BMI property is computed getter, View needs notification)
                OnPropertyChanged(nameof(SelectedAnthropometrics)); 
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
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

        public bool IsSelectionActive => SelectedAnthropometrics != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }

        public AntropometricsVM()
        {
            _repository = new AnthropometricsRepository();
            _clientRepository = new ClientRepository();
            _anthropometricsList = new ObservableCollection<Anthropometrics>();
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
                
                // Load clients first for ComboBox
                var clients = await _clientRepository.GetAllAsync();
                Clients = new ObservableCollection<Client>(clients);

                // Load records
                var records = await _repository.GetAllAsync();
                AnthropometricsList = new ObservableCollection<Anthropometrics>(records);
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
            SelectedAnthropometrics = new Anthropometrics();
        }

        private async Task SaveAsync()
        {
            if (SelectedAnthropometrics == null) return;

            if (SelectedAnthropometrics.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedAnthropometrics.AnthropometricsID == null)
                {
                    var id = await _repository.AddAsync(SelectedAnthropometrics);
                    SelectedAnthropometrics.AnthropometricsID = id;

                    // Update ClientName
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedAnthropometrics.ClientID);
                    if (client != null) SelectedAnthropometrics.ClientName = client.Name;

                    AnthropometricsList.Insert(0, SelectedAnthropometrics); // Add to top
                    MessageBox.Show("Record added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _repository.UpdateAsync(SelectedAnthropometrics);
                    
                    // Refresh view item if needed (though binding usually handles it)
                     var client = Clients.FirstOrDefault(c => c.ClientID == SelectedAnthropometrics.ClientID);
                    if (client != null) SelectedAnthropometrics.ClientName = client.Name;
                    
                    MessageBox.Show("Record updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedAnthropometrics?.AnthropometricsID == null) return;

            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedAnthropometrics.AnthropometricsID.Value);
                    AnthropometricsList.Remove(SelectedAnthropometrics);
                    SelectedAnthropometrics = null;
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
                    AnthropometricsList = new ObservableCollection<Anthropometrics>(results);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedAnthropometrics = null;
        }
    }
}
