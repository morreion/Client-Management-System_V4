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
    public class DietVM : ViewModelBase
    {
        private readonly DietRepository _repository;
        private readonly ClientRepository _clientRepository;
        private ObservableCollection<Diet> _dietList;
        private ObservableCollection<Client> _clients;
        private Diet? _selectedDiet;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Diet> DietList
        {
            get => _dietList;
            set
            {
                _dietList = value;
                OnPropertyChanged(nameof(DietList));
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

        public Diet? SelectedDiet
        {
            get => _selectedDiet;
            set
            {
                _selectedDiet = value;
                OnPropertyChanged(nameof(SelectedDiet));
                OnPropertyChanged(nameof(IsSelectionActive));
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
                    _ = SearchAsync();
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

        public bool IsSelectionActive => SelectedDiet != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }

        public DietVM()
        {
            _repository = new DietRepository();
            _clientRepository = new ClientRepository();
            _dietList = new ObservableCollection<Diet>();
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
                
                // Load clients first
                var clients = await _clientRepository.GetAllAsync();
                Clients = new ObservableCollection<Client>(clients);

                // Load diet records
                var records = await _repository.GetAllAsync();
                DietList = new ObservableCollection<Diet>(records);
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
            SelectedDiet = new Diet();
        }

        private async Task SaveAsync()
        {
            if (SelectedDiet == null) return;

            if (SelectedDiet.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedDiet.DietID == null)
                {
                    var id = await _repository.AddAsync(SelectedDiet);
                    SelectedDiet.DietID = id;

                    // Update ClientName logic
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedDiet.ClientID);
                    if (client != null) SelectedDiet.ClientName = client.Name;

                    DietList.Insert(0, SelectedDiet);
                    MessageBox.Show("Diet record added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _repository.UpdateAsync(SelectedDiet);
                    
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedDiet.ClientID);
                    if (client != null) SelectedDiet.ClientName = client.Name;
                    
                    MessageBox.Show("Diet record updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            if (SelectedDiet?.DietID == null) return;

            if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedDiet.DietID.Value);
                    DietList.Remove(SelectedDiet);
                    SelectedDiet = null;
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
                    DietList = new ObservableCollection<Diet>(results);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedDiet = null;
        }
    }
}
