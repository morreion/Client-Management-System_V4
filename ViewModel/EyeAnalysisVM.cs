using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32; // For OpenFileDialog
using System.IO;
using Client_Management_System_V4.Models;
using Client_Management_System_V4.Repositories;
using Client_Management_System_V4.Utilities;

namespace Client_Management_System_V4.ViewModel
{
    public class EyeAnalysisVM : ViewModelBase
    {
        private readonly EyeAnalysisRepository _repository;
        private readonly ClientRepository _clientRepository;

        private ObservableCollection<EyeAnalysis> _analysisList;
        private ObservableCollection<Client> _clients;
        // List of ALL possible priority types, with IsSelected state management
        private ObservableCollection<ScleraPriorityType> _priorityTypes;
        
        // List of scans belonging to current selection
        private ObservableCollection<EyeScan> _currentScans;
        
        // New scans to be uploaded on Save
        private ObservableCollection<EyeScan> _newScansToAdd;

        private EyeAnalysis? _selectedAnalysis;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<EyeAnalysis> AnalysisList
        {
            get => _analysisList;
            set { _analysisList = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
        }
        public ObservableCollection<ScleraPriorityType> PriorityTypes
        {
            get => _priorityTypes;
            set { _priorityTypes = value; OnPropertyChanged(); }
        }
        public ObservableCollection<EyeScan> CurrentScans
        {
            get => _currentScans;
            set { _currentScans = value; OnPropertyChanged(); }
        }

        public EyeAnalysis? SelectedAnalysis
        {
            get => _selectedAnalysis;
            set
            {
                _selectedAnalysis = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectionActive));
                
                if (_selectedAnalysis != null && _selectedAnalysis.Eye_AnalysisID.HasValue)
                {
                    LoadDetailsForSelected(_selectedAnalysis.Eye_AnalysisID.Value);
                }
                else if (_selectedAnalysis != null && _selectedAnalysis.Eye_AnalysisID == null)
                {
                    ResetSelectionStates();
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

        public bool IsSelectionActive => SelectedAnalysis != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadLeftEyeCommand { get; }
        public ICommand UploadRightEyeCommand { get; }

        // Manager Overlay Properties
        private bool _isManagerOpen;
        public bool IsManagerOpen
        {
            get => _isManagerOpen;
            set { _isManagerOpen = value; OnPropertyChanged(); }
        }

        private ScleraPriorityType? _selectedManagerPriority;
        public ScleraPriorityType? SelectedManagerPriority
        {
            get => _selectedManagerPriority;
            set { _selectedManagerPriority = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ScleraPriorityType> _managerPriorityList;
        public ObservableCollection<ScleraPriorityType> ManagerPriorityList
        {
            get => _managerPriorityList;
            set { _managerPriorityList = value; OnPropertyChanged(); }
        }

        // Manager Commands
        public ICommand OpenManagerCommand { get; }
        public ICommand CloseManagerCommand { get; }
        public ICommand SavePriorityCommand { get; }
        public ICommand DeletePriorityCommand { get; }
        public ICommand ClearPriorityCommand { get; }

        public EyeAnalysisVM()
        {
            _repository = new EyeAnalysisRepository();
            _clientRepository = new ClientRepository();
            
            _analysisList = new ObservableCollection<EyeAnalysis>();
            _clients = new ObservableCollection<Client>();
            _priorityTypes = new ObservableCollection<ScleraPriorityType>();
            _currentScans = new ObservableCollection<EyeScan>();
            _newScansToAdd = new ObservableCollection<EyeScan>();
            _managerPriorityList = new ObservableCollection<ScleraPriorityType>();

            LoadedCommand = new RelayCommand(async _ => await InitializeAsync());
            AddCommand = new RelayCommand(_ => AddNew());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => IsSelectionActive);
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => IsSelectionActive);
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            CancelCommand = new RelayCommand(_ => CancelEdit());
            
            UploadLeftEyeCommand = new RelayCommand(_ => UploadImage("Left"));
            UploadRightEyeCommand = new RelayCommand(_ => UploadImage("Right"));

            // Manager Command Init
            OpenManagerCommand = new RelayCommand(async _ => await OpenManager());
            CloseManagerCommand = new RelayCommand(_ => CloseManager());
            SavePriorityCommand = new RelayCommand(async _ => await SavePriorityAsync());
            DeletePriorityCommand = new RelayCommand(async _ => await DeletePriorityAsync());
            ClearPriorityCommand = new RelayCommand(_ => SelectedManagerPriority = new ScleraPriorityType()); // 'Add New' mode
        }

        private async Task OpenManager()
        {
             IsManagerOpen = true;
             SelectedManagerPriority = new ScleraPriorityType(); // Default to Add mode
             await LoadManagerList();
        }

        private void CloseManager()
        {
             IsManagerOpen = false;
             // Refresh the main dropdowns logic
             _ = InitializeAsync(); // Reloads priority types
             if (SelectedAnalysis != null && SelectedAnalysis.Eye_AnalysisID.HasValue)
             {
                 LoadDetailsForSelected(SelectedAnalysis.Eye_AnalysisID.Value);
             }
        }

        private async Task LoadManagerList()
        {
             var list = await _repository.GetAllPriorityTypesAsync();
             ManagerPriorityList = new ObservableCollection<ScleraPriorityType>(list);
        }

        private async Task SavePriorityAsync()
        {
             if (SelectedManagerPriority == null)
             {
                 SelectedManagerPriority = new ScleraPriorityType();
             }
             
             if (SelectedManagerPriority == null) return;
             
             if (string.IsNullOrWhiteSpace(SelectedManagerPriority.Priority_Name))
             {
                 MessageBox.Show("Name is required.");
                 return;
             }

             try
             {
                 if (SelectedManagerPriority.ScleraPriorityTypeID == 0)
                     await _repository.AddPriorityTypeAsync(SelectedManagerPriority);
                 else
                     await _repository.UpdatePriorityTypeAsync(SelectedManagerPriority);
                 
                 await LoadManagerList(); // Refresh list
                 MessageBox.Show("Saved.");
                 SelectedManagerPriority = new ScleraPriorityType(); // Ready for next
             }
             catch(Exception ex)
             {
                 MessageBox.Show("Error: " + ex.Message);
             }
        }

        private async Task DeletePriorityAsync()
        {
             if (SelectedManagerPriority == null || SelectedManagerPriority.ScleraPriorityTypeID == 0) return;
             if (MessageBox.Show("Delete this priority type?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
             {
                 try
                 {
                     await _repository.DeletePriorityTypeAsync(SelectedManagerPriority.ScleraPriorityTypeID);
                     await LoadManagerList();
                     SelectedManagerPriority = new ScleraPriorityType();
                 }
                 catch(Exception ex)
                 {
                     MessageBox.Show("Cannot delete: " + ex.Message);
                 }
             }
        }

    // Existing methods...

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                Clients = new ObservableCollection<Client>(await _clientRepository.GetAllAsync());
                AnalysisList = new ObservableCollection<EyeAnalysis>(await _repository.GetAllAsync());
                
                // Load Lookup Data
                PriorityTypes = new ObservableCollection<ScleraPriorityType>(await _repository.GetAllPriorityTypesAsync());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private async void LoadDetailsForSelected(int id)
        {
            try
            {
                // 1. Reset all checkboxes first
                foreach (var p in PriorityTypes) p.IsSelected = false;
                
                // 2. Fetch selected IDs
                var selectedIds = await _repository.GetSelectedPriorityIdsAsync(id);
                foreach (var sid in selectedIds)
                {
                    var match = PriorityTypes.FirstOrDefault(p => p.ScleraPriorityTypeID == sid);
                    if (match != null) match.IsSelected = true;
                }
                // Refresh list view to update checkboxes
                PriorityTypes = new ObservableCollection<ScleraPriorityType>(PriorityTypes);

                // 3. Fetch Images
                var scans = await _repository.GetScansForAnalysisAsync(id);
                CurrentScans = new ObservableCollection<EyeScan>(scans);
                _newScansToAdd.Clear(); // Clear pending adds
            }
            catch (Exception ex) { MessageBox.Show("Error details: " + ex.Message); }
        }

        private void ResetSelectionStates()
        {
            foreach (var p in PriorityTypes) p.IsSelected = false;
            PriorityTypes = new ObservableCollection<ScleraPriorityType>(PriorityTypes);
            CurrentScans.Clear();
            _newScansToAdd.Clear();
        }

        private void AddNew()
        {
            ResetSelectionStates();
            SelectedAnalysis = new EyeAnalysis();
        }

        private void UploadImage(string side)
        {
            if (SelectedAnalysis == null) 
            {
                MessageBox.Show("Please select or create an analysis record first.");
                return;
            }
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(openFileDialog.FileName);
                    var newScan = new EyeScan
                    {
                        Eye_Scan = fileData,
                        Scan_Date = DateTime.Now,
                        Eye_Side = side
                    };
                    
                    // Add to UI immediately
                    CurrentScans.Add(newScan);
                    // Add to pending save queue
                    _newScansToAdd.Add(newScan);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error reading file: " + ex.Message);
                }
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedAnalysis == null) return;
            if (SelectedAnalysis.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedAnalysis.Eye_AnalysisID == null)
                {
                    // Add
                    var id = await _repository.AddWithDetailsAsync(SelectedAnalysis, PriorityTypes, _newScansToAdd);
                    SelectedAnalysis.Eye_AnalysisID = id;

                    var c = Clients.FirstOrDefault(x => x.ClientID == SelectedAnalysis.ClientID);
                    if (c != null) SelectedAnalysis.ClientName = c.Name;
                    
                    AnalysisList.Insert(0, SelectedAnalysis);
                    MessageBox.Show("Analysis saved!");
                }
                else
                {
                    // Update
                    // We pass 'currentScans' logic or specific new scans logic. The repo assumes append for scans.
                    await _repository.UpdateWithDetailsAsync(SelectedAnalysis, PriorityTypes, _newScansToAdd);
                    
                    var c = Clients.FirstOrDefault(x => x.ClientID == SelectedAnalysis.ClientID);
                    if (c != null) SelectedAnalysis.ClientName = c.Name;
                    
                    MessageBox.Show("Analysis updated!");
                }
                _newScansToAdd.Clear(); // Flushed
            }
            catch (Exception ex) { MessageBox.Show("Error saving: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task DeleteAsync()
        {
            if (SelectedAnalysis?.Eye_AnalysisID == null) return;
            if (MessageBox.Show("Delete this analysis?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedAnalysis.Eye_AnalysisID.Value);
                    AnalysisList.Remove(SelectedAnalysis);
                    SelectedAnalysis = null;
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
                    AnalysisList = new ObservableCollection<EyeAnalysis>(results);
                }
            }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedAnalysis = null;
        }
    }
}
