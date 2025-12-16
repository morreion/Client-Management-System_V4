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
    public class SupplementsVM : ViewModelBase
    {
        private readonly SupplementRepository _repository;
        private readonly DistributorRepository _distributorRepository;
        private ObservableCollection<Supplement> _supplements;
        private ObservableCollection<Distributor> _distributors;
        private Supplement? _selectedSupplement;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<Supplement> Supplements
        {
            get => _supplements;
            set
            {
                _supplements = value;
                OnPropertyChanged(nameof(Supplements));
            }
        }

        public ObservableCollection<Distributor> Distributors
        {
            get => _distributors;
            set
            {
                _distributors = value;
                OnPropertyChanged(nameof(Distributors));
            }
        }

        public Supplement? SelectedSupplement
        {
            get => _selectedSupplement;
            set
            {
                _selectedSupplement = value;
                OnPropertyChanged(nameof(SelectedSupplement));
                OnPropertyChanged(nameof(IsSupplementSelected));
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
                    _ = SearchSupplements();
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

        public bool IsSupplementSelected => SelectedSupplement != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }

        // Distributor Manager Overlay Properties
        private bool _isDistributorManagerOpen;
        public bool IsDistributorManagerOpen
        {
            get => _isDistributorManagerOpen;
            set
            {
                _isDistributorManagerOpen = value;
                OnPropertyChanged(nameof(IsDistributorManagerOpen));
            }
        }

        private Distributor? _selectedManagerDistributor;
        public Distributor? SelectedManagerDistributor
        {
            get => _selectedManagerDistributor;
            set
            {
                _selectedManagerDistributor = value;
                OnPropertyChanged(nameof(SelectedManagerDistributor));
            }
        }

        private ObservableCollection<Distributor> _distributorManagerList;
        public ObservableCollection<Distributor> DistributorManagerList
        {
            get => _distributorManagerList;
            set
            {
                _distributorManagerList = value;
                OnPropertyChanged(nameof(DistributorManagerList));
            }
        }

        // Distributor Manager Commands
        public ICommand OpenDistributorManagerCommand { get; }
        public ICommand CloseDistributorManagerCommand { get; }
        public ICommand SaveDistributorCommand { get; }
        public ICommand DeleteDistributorCommand { get; }
        public ICommand ClearDistributorCommand { get; }

        public SupplementsVM()
        {
            _repository = new SupplementRepository();
            _distributorRepository = new DistributorRepository();
            _supplements = new ObservableCollection<Supplement>();
            _distributors = new ObservableCollection<Distributor>();
            _distributorManagerList = new ObservableCollection<Distributor>();

            // Initialize commands
            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            AddCommand = new RelayCommand(_ => AddSupplement());
            SaveCommand = new RelayCommand(async _ => await SaveSupplement(), _ => IsSupplementSelected);
            DeleteCommand = new RelayCommand(async _ => await DeleteSupplement(), _ => IsSupplementSelected);
            SearchCommand = new RelayCommand(async _ => await SearchSupplements());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            
            // Distributor Manager Commands
            OpenDistributorManagerCommand = new RelayCommand(async _ => await OpenDistributorManager());
            CloseDistributorManagerCommand = new RelayCommand(_ => CloseDistributorManager());
            SaveDistributorCommand = new RelayCommand(async _ => await SaveDistributorManagerAsync());
            DeleteDistributorCommand = new RelayCommand(async _ => await DeleteDistributorManagerAsync());
            ClearDistributorCommand = new RelayCommand(_ => SelectedManagerDistributor = new Distributor());
        }

        private async Task OpenDistributorManager()
        {
            IsDistributorManagerOpen = true;
            SelectedManagerDistributor = new Distributor(); // Default to Add mode
            await LoadDistributorManagerList();
        }

        private void CloseDistributorManager()
        {
            IsDistributorManagerOpen = false;
            // Refresh the dropdown list in the main view
            _ = LoadDataAsync(); 
        }

        private async Task LoadDistributorManagerList()
        {
             try 
             {
                var list = await _distributorRepository.GetAllAsync();
                DistributorManagerList = new ObservableCollection<Distributor>(list);
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Error loading distributors: {ex.Message}");
             }
        }

        private async Task SaveDistributorManagerAsync()
        {
             if (SelectedManagerDistributor == null) return;
             
             if (string.IsNullOrWhiteSpace(SelectedManagerDistributor.Name))
             {
                 MessageBox.Show("Distributor Name is required.");
                 return;
             }

             try
             {
                 IsLoading = true;
                 if (SelectedManagerDistributor.DistributorID == null)
                 {
                     var id = await _distributorRepository.AddAsync(SelectedManagerDistributor);
                     SelectedManagerDistributor.DistributorID = id;
                     MessageBox.Show("Distributor added!");
                 }
                 else
                 {
                     await _distributorRepository.UpdateAsync(SelectedManagerDistributor);
                     MessageBox.Show("Distributor updated!");
                 }
                 
                 await LoadDistributorManagerList();
                 SelectedManagerDistributor = new Distributor(); // Reset to Add New
             }
             catch(Exception ex)
             {
                 MessageBox.Show("Error saving distributor: " + ex.Message);
             }
             finally { IsLoading = false; }
        }

        private async Task DeleteDistributorManagerAsync()
        {
             if (SelectedManagerDistributor == null || SelectedManagerDistributor.DistributorID == null) return;
             if (MessageBox.Show("Delete this distributor?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
             {
                 try
                 {
                     IsLoading = true;
                     await _distributorRepository.DeleteAsync(SelectedManagerDistributor.DistributorID.Value);
                     await LoadDistributorManagerList();
                     SelectedManagerDistributor = new Distributor();
                 }
                 catch(Exception ex)
                 {
                     MessageBox.Show("Cannot delete: " + ex.Message);
                 }
                 finally { IsLoading = false; }
             }
        }

        private async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                
                // Load Distributors for ComboBox
                var distributors = await _distributorRepository.GetAllAsync();
                Distributors = new ObservableCollection<Distributor>(distributors);

                // Load Supplements
                var supplements = await _repository.GetAllAsync();
                Supplements = new ObservableCollection<Supplement>(supplements);
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

        private void AddSupplement()
        {
            SelectedSupplement = new Supplement();
        }

        private async Task SaveSupplement()
        {
            if (SelectedSupplement == null) return;

            // Validation
            if (string.IsNullOrWhiteSpace(SelectedSupplement.Name))
            {
                MessageBox.Show("Please enter a name for the supplement.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedSupplement.DistributorID <= 0)
            {
                MessageBox.Show("Please select a distributor.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedSupplement.SupplementID == null)
                {
                    // Add new
                    var id = await _repository.AddAsync(SelectedSupplement);
                    SelectedSupplement.SupplementID = id;

                    // Update DistributorName for display
                    var distributor = Distributors.FirstOrDefault(d => d.DistributorID == SelectedSupplement.DistributorID);
                    if (distributor != null)
                        SelectedSupplement.DistributorName = distributor.Name;

                    Supplements.Add(SelectedSupplement);
                    MessageBox.Show("Supplement added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Update existing
                    await _repository.UpdateAsync(SelectedSupplement);
                    
                    // Refresh the list item
                    var updatedItem = Supplements.FirstOrDefault(s => s.SupplementID == SelectedSupplement.SupplementID);
                    if (updatedItem != null)
                    {
                        updatedItem.Name = SelectedSupplement.Name;
                        updatedItem.Type = SelectedSupplement.Type;
                        updatedItem.Description = SelectedSupplement.Description;
                        updatedItem.Usage = SelectedSupplement.Usage;
                        updatedItem.DistributorID = SelectedSupplement.DistributorID;
                        
                        var distributor = Distributors.FirstOrDefault(d => d.DistributorID == SelectedSupplement.DistributorID);
                        if (distributor != null)
                            updatedItem.DistributorName = distributor.Name;
                    }
                    
                    MessageBox.Show("Supplement updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving supplement: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteSupplement()
        {
            if (SelectedSupplement?.SupplementID == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedSupplement.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedSupplement.SupplementID.Value);
                    Supplements.Remove(SelectedSupplement);
                    SelectedSupplement = null;
                    MessageBox.Show("Supplement deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting supplement: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task SearchSupplements()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    var supplements = await _repository.GetAllAsync();
                    Supplements = new ObservableCollection<Supplement>(supplements);
                }
                else
                {
                    var results = await _repository.SearchAsync(SearchText);
                    Supplements = new ObservableCollection<Supplement>(results);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching supplements: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            SelectedSupplement = null;
        }
    }
}
