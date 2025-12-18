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
    public class PrescriptionVM : ViewModelBase
    {
        private readonly PrescriptionRepository _repository;
        private readonly ClientRepository _clientRepository;
        private readonly SupplementRepository _supplementRepository; // To load available supplements

        private ObservableCollection<Prescription> _prescriptionList;
        private ObservableCollection<Client> _clients;
        private ObservableCollection<Supplement> _availableSupplements;
        
        // Detailed collection for the CURRENT selected prescription
        private ObservableCollection<PrescriptionSupplement> _currentSupplements;

        private Prescription? _selectedPrescription;
        private PrescriptionSupplement? _selectedDetailItem; // Item selected in the mini-grid
        
        // Add Item Form Properties
        private Supplement? _itemToAddSupplement;
        private string _itemToAddBreakfast = string.Empty;
        private string _itemToAddLunch = string.Empty;
        private string _itemToAddDinner = string.Empty;
        private string _itemToAddBedtime = string.Empty;

        private string _searchText = string.Empty;
        private bool _isLoading;

        // -- Properties --

        public ObservableCollection<Prescription> PrescriptionList
        {
            get => _prescriptionList;
            set { _prescriptionList = value; OnPropertyChanged(); }
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

        public ObservableCollection<PrescriptionSupplement> CurrentSupplements
        {
            get => _currentSupplements;
            set { _currentSupplements = value; OnPropertyChanged(); }
        }

        public Prescription? SelectedPrescription
        {
            get => _selectedPrescription;
            set
            {
                _selectedPrescription = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectionActive));
                
                // Clear the staging inputs to prevent stale data
                ResetAddForm();

                // When selection changes, load the supplements for this prescription
                if (_selectedPrescription != null && _selectedPrescription.PrescriptionID.HasValue)
                {
                    LoadSupplementsForSelected(_selectedPrescription.PrescriptionID.Value);
                }
                else if (_selectedPrescription != null && _selectedPrescription.PrescriptionID == null)
                {
                    // New/Empty record
                    CurrentSupplements = new ObservableCollection<PrescriptionSupplement>();
                }
            }
        }

        public PrescriptionSupplement? SelectedDetailItem
        {
             get => _selectedDetailItem;
             set { _selectedDetailItem = value; OnPropertyChanged(); }
        }

        // -- Add Item Form --
        public Supplement? ItemToAddSupplement
        {
            get => _itemToAddSupplement;
            set { _itemToAddSupplement = value; OnPropertyChanged(); }
        }
        public string ItemToAddBreakfast { get => _itemToAddBreakfast; set { _itemToAddBreakfast = value; OnPropertyChanged(); } }
        public string ItemToAddLunch { get => _itemToAddLunch; set { _itemToAddLunch = value; OnPropertyChanged(); } }
        public string ItemToAddDinner { get => _itemToAddDinner; set { _itemToAddDinner = value; OnPropertyChanged(); } }
        public string ItemToAddBedtime { get => _itemToAddBedtime; set { _itemToAddBedtime = value; OnPropertyChanged(); } }

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

        public bool IsSelectionActive => SelectedPrescription != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }
        
        public ICommand AddDetailItemCommand { get; }
        public ICommand RemoveDetailItemCommand { get; }

        public PrescriptionVM()
        {
            _repository = new PrescriptionRepository();
            _clientRepository = new ClientRepository();
            _supplementRepository = new SupplementRepository();
            
            _prescriptionList = new ObservableCollection<Prescription>();
            _clients = new ObservableCollection<Client>();
            _availableSupplements = new ObservableCollection<Supplement>();
            _currentSupplements = new ObservableCollection<PrescriptionSupplement>();

            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            AddCommand = new RelayCommand(_ => AddNew());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsSelectionActive);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsSelectionActive);
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            
            AddDetailItemCommand = new RelayCommand(_ => AddItemToCurrentList());
            RemoveDetailItemCommand = new RelayCommand(_ => RemoveItemFromCurrentList());
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                Clients = new ObservableCollection<Client>(await _clientRepository.GetAllAsync());
                AvailableSupplements = new ObservableCollection<Supplement>(await _supplementRepository.GetAllAsync());
                PrescriptionList = new ObservableCollection<Prescription>(await _repository.GetAllAsync());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private async void LoadSupplementsForSelected(int prescriptionId)
        {
            try
            {
                var sups = await _repository.GetSupplementsByPrescriptionIdAsync(prescriptionId);
                CurrentSupplements = new ObservableCollection<PrescriptionSupplement>(sups);
            }
            catch (Exception ex) { MessageBox.Show("Error loading details: " + ex.Message); }
        }

        private void AddNew()
        {
            CurrentSupplements = new ObservableCollection<PrescriptionSupplement>();
            SelectedPrescription = new Prescription();
        }

        private void AddItemToCurrentList()
        {
            if (ItemToAddSupplement == null || ItemToAddSupplement.SupplementID == null) return;

            // Check duplicate?
            if (CurrentSupplements.Any(x => x.SupplementID == ItemToAddSupplement.SupplementID))
            {
                MessageBox.Show("This supplement is already in the list.");
                return;
            }

            var newItem = new PrescriptionSupplement
            {
                SupplementID = ItemToAddSupplement.SupplementID.Value,
                SupplementName = ItemToAddSupplement.Name,
                Breakfast = ItemToAddBreakfast,
                Lunch = ItemToAddLunch,
                Dinner = ItemToAddDinner,
                Bedtime = ItemToAddBedtime
            };

            CurrentSupplements.Add(newItem);


            ResetAddForm();
        }

        private void ResetAddForm()
        {
            ItemToAddSupplement = null;
            ItemToAddBreakfast = "";
            ItemToAddLunch = "";
            ItemToAddDinner = "";
            ItemToAddBedtime = "";
        }

        private void RemoveItemFromCurrentList()
        {
            if (SelectedDetailItem != null)
            {
                CurrentSupplements.Remove(SelectedDetailItem);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedPrescription == null) return;
            if (SelectedPrescription.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedPrescription.PrescriptionID == null)
                {
                    // Add New
                    var id = await _repository.AddWithSupplementsAsync(SelectedPrescription, CurrentSupplements);
                    SelectedPrescription.PrescriptionID = id;
                    
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedPrescription.ClientID);
                    if (client != null) SelectedPrescription.ClientName = client.Name;

                    PrescriptionList.Insert(0, SelectedPrescription);
                    MessageBox.Show("Prescription added!");
                }
                else
                {
                    // Update
                    await _repository.UpdateWithSupplementsAsync(SelectedPrescription, CurrentSupplements);
                    
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedPrescription.ClientID);
                    if (client != null) SelectedPrescription.ClientName = client.Name;

                    MessageBox.Show("Prescription updated!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedPrescription?.PrescriptionID == null) return;
            if (MessageBox.Show("Delete this prescription?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedPrescription.PrescriptionID.Value);
                    PrescriptionList.Remove(SelectedPrescription);
                    SelectedPrescription = null;
                    CurrentSupplements.Clear();
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
                    PrescriptionList = new ObservableCollection<Prescription>(results);
                }
            }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedPrescription = null;
            CurrentSupplements.Clear();
        }
    }
}
