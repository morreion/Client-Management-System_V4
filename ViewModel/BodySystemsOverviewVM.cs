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
    public class BodySystemsOverviewVM : ViewModelBase
    {
        private readonly BodySystemsOverviewRepository _repository;
        private readonly ClientRepository _clientRepository;
        private ObservableCollection<BodySystemsOverview> _overviewList;
        private ObservableCollection<Client> _clients;
        private BodySystemsOverview? _selectedOverview;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public ObservableCollection<BodySystemsOverview> OverviewList
        {
            get => _overviewList;
            set { _overviewList = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Client> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
        }

        public BodySystemsOverview? SelectedOverview
        {
            get => _selectedOverview;
            set
            {
                _selectedOverview = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSelectionActive));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsSelectionActive => SelectedOverview != null;

        // Commands
        public ICommand LoadedCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }

        public BodySystemsOverviewVM()
        {
            _repository = new BodySystemsOverviewRepository();
            _clientRepository = new ClientRepository();
            _overviewList = new ObservableCollection<BodySystemsOverview>();
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
                Clients = new ObservableCollection<Client>(await _clientRepository.GetAllAsync());
                OverviewList = new ObservableCollection<BodySystemsOverview>(await _repository.GetAllAsync());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private void AddNew()
        {
            SelectedOverview = new BodySystemsOverview();
        }

        private async Task SaveAsync()
        {
            if (SelectedOverview == null) return;
            if (SelectedOverview.ClientID <= 0)
            {
                MessageBox.Show("Please select a client.");
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedOverview.Body_Systems_OverviewID == null)
                {
                    var id = await _repository.AddAsync(SelectedOverview);
                    SelectedOverview.Body_Systems_OverviewID = id;

                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedOverview.ClientID);
                    if (client != null) SelectedOverview.ClientName = client.Name;

                    OverviewList.Insert(0, SelectedOverview);
                    MessageBox.Show("Overview saved successfully!");
                }
                else
                {
                    await _repository.UpdateAsync(SelectedOverview);
                    
                    var client = Clients.FirstOrDefault(c => c.ClientID == SelectedOverview.ClientID);
                    if (client != null) SelectedOverview.ClientName = client.Name;

                    MessageBox.Show("Overview updated successfully!");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private async Task DeleteAsync()
        {
            if (SelectedOverview?.Body_Systems_OverviewID == null) return;
            if (MessageBox.Show("Are you sure?", "Delete Overview", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _repository.DeleteAsync(SelectedOverview.Body_Systems_OverviewID.Value);
                    OverviewList.Remove(SelectedOverview);
                    SelectedOverview = null;
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
                    OverviewList = new ObservableCollection<BodySystemsOverview>(results);
                }
            }
            finally { IsLoading = false; }
        }

        private void CancelEdit()
        {
            SelectedOverview = null;
        }
    }
}
