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
    public class MedHxVM : ViewModelBase
    {
        private readonly MedHxRepository _repository;
        private readonly ClientRepository _clientRepository;
        private readonly SupplementRepository _supplementRepository; // To load available supplements

        private ObservableCollection<MedHx> _historyList;
        private ObservableCollection<Client> _clients;
        private ObservableCollection<Supplement> _availableSupplements;
        
        // The list of supplements currently associated with the selected history
        private ObservableCollection<MedHxSupplement> _currentSupplements;

        private MedHx? _selectedHistory;
        private string _searchText = string.Empty;
        private bool _isLoading;

        // Fields for Adding a Supplement
        private Supplement? _selectedSupplementToAdd;
        private string _newItemDosage = string.Empty;
        private string _newItemFrequency = string.Empty;
        private string _newItemNotes = string.Empty;

        public ObservableCollection<MedHx> HistoryList
        {
            get => _historyList;
            set { _historyList = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Supplement> AvailableSupplements
        {
            get => _availableSupplements;
            set { _availableSupplements = value; OnPropertyChanged(); }
        }
        public ObservableCollection<MedHxSupplement> CurrentSupplements
        {
            get => _currentSupplements;
            set { _currentSupplements = value; OnPropertyChanged(); }
        }

        public MedHx? SelectedHistory
        {
            get => _selectedHistory;
            set
            {
                _selectedHistory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectionActive));
                
                if (_selectedHistory != null)
                {
                    if (_selectedHistory.Med_HxID.HasValue)
                        LoadDetails(_selectedHistory.Med_HxID.Value);
                    else
                        CurrentSupplements.Clear(); // New Item
                }
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
                    OnPropertyChanged();
                    _ = SearchAsync();
                }
            }
        }
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
        public bool IsSelectionActive => SelectedHistory != null;

        // Supplement Adder Properties
        public Supplement? SelectedSupplementToAdd
        {
            get => _selectedSupplementToAdd;
            set { _selectedSupplementToAdd = value; OnPropertyChanged(); }
        }
        public string NewItemDosage
        {
            get => _newItemDosage;
            set { _newItemDosage = value; OnPropertyChanged(); }
        }
        public string NewItemFrequency
        {
            get => _newItemFrequency;
            set { _newItemFrequency = value; OnPropertyChanged(); }
        }
        public string NewItemNotes
        {
            get => _newItemNotes;
            set { _newItemNotes = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }
        
        public ICommand AddSupplementCommand { get; }
        public ICommand RemoveSupplementCommand { get; } // Requires parameter

        public MedHxVM()
        {
            _repository = new MedHxRepository();
            _clientRepository = new ClientRepository();
            _supplementRepository = new SupplementRepository();

            _historyList = new ObservableCollection<MedHx>();
            _clients = new ObservableCollection<Client>();
            _availableSupplements = new ObservableCollection<Supplement>();
            _currentSupplements = new ObservableCollection<MedHxSupplement>();

            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            AddCommand = new RelayCommand(_ => AddNew());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsSelectionActive);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsSelectionActive);
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            
            AddSupplementCommand = new RelayCommand(_ => AddItem(), _ => SelectedSupplementToAdd != null);
            RemoveSupplementCommand = new RelayCommand(param => RemoveItem(param));
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                Clients = new ObservableCollection<Client>(await _clientRepository.GetAllAsync());
                AvailableSupplements = new ObservableCollection<Supplement>(await _supplementRepository.GetAllAsync());
                HistoryList = new ObservableCollection<MedHx>(await _repository.GetAllAsync());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private async void LoadDetails(int id)
        {
             try
             {
                 var details = await _repository.GetSupplementsByHxIdAsync(id);
                 CurrentSupplements = new ObservableCollection<MedHxSupplement>(details);
             }
             catch (Exception ex) { MessageBox.Show("Failed to load details: " + ex.Message); }
        }

        private void AddNew()
        {
            CurrentSupplements.Clear();
            SelectedHistory = new MedHx();
        }

        private void AddItem()
        {
            if (SelectedSupplementToAdd == null) return;

            var newItem = new MedHxSupplement
            {
                SupplementID = (int)SelectedSupplementToAdd.SupplementID!,
                SupplementName = SelectedSupplementToAdd.Name,
                Dosage = NewItemDosage,
                Frequency = NewItemFrequency,
                Notes = NewItemNotes
            };

            CurrentSupplements.Add(newItem);
            
            // Reset input fields
            SelectedSupplementToAdd = null;
            NewItemDosage = "";
            NewItemFrequency = "";
            NewItemNotes = "";
        }

        private void RemoveItem(object? parameter)
        {
            if (parameter is MedHxSupplement item)
            {
                CurrentSupplements.Remove(item);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedHistory == null) return;
            if (SelectedHistory.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedHistory.Med_HxID == null)
                {
                    // ADD
                    var id = await _repository.AddWithSupplementsAsync(SelectedHistory, CurrentSupplements);
                    SelectedHistory.Med_HxID = id;

                    var c = Clients.FirstOrDefault(x => x.ClientID == SelectedHistory.ClientID);
                    if (c != null) SelectedHistory.ClientName = c.Name;

                    HistoryList.Insert(0, SelectedHistory);
                    MessageBox.Show("Medical History saved!");
                }
                else
                {
                    // UPDATE
                    await _repository.UpdateWithSupplementsAsync(SelectedHistory, CurrentSupplements);
                    
                    var c = Clients.FirstOrDefault(x => x.ClientID == SelectedHistory.ClientID);
                    if (c != null) SelectedHistory.ClientName = c.Name;
                    
                    MessageBox.Show("Medical History updated!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Error saving: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task DeleteAsync()
        {
            if (SelectedHistory?.Med_HxID == null) return;
            if (MessageBox.Show("Delete this record?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedHistory.Med_HxID.Value);
                    HistoryList.Remove(SelectedHistory);
                    SelectedHistory = null;
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
                finally { IsLoading = false; }
            }
        }

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                if (string.IsNullOrWhiteSpace(SearchText))
                     await InitializeAsync();
                else
                {
                    var results = await _repository.SearchAsync(SearchText);
                    HistoryList = new ObservableCollection<MedHx>(results);
                }
            }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedHistory = null;
        }
    }
}
