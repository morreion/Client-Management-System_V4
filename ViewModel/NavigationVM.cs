using Client_Management_System_V4.Utilities;
using System.Windows.Input;

namespace Client_Management_System_V4.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        private object? _currentView;
        public object? CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand ClientCommand { get; }
        public ICommand MedHxCommand { get; }
        public ICommand AntropometricsCommand { get; }
        public ICommand BodySystemsOverviewCommand { get; }
        public ICommand EyeAnalysisCommand { get; }
        public ICommand DietCommand { get; }
        public ICommand TreatmentCommand { get; }
        public ICommand PrescriptionCommand { get; }
        public ICommand SupplementsCommand { get; }
        public ICommand DistributorCommand { get; }
        public ICommand ReportsCommand { get; }
        public ICommand ScannedNotesCommand { get; }

        private void SafeNavigate(object viewModel)
        {
            CurrentView = viewModel;
        }

        public NavigationVM()
        {
            ClientCommand = new RelayCommand(_ => SafeNavigate(new ClientVM()));
            DistributorCommand = new RelayCommand(_ => SafeNavigate(new DistributorVM()));
            SupplementsCommand = new RelayCommand(_ => SafeNavigate(new SupplementsVM()));
            MedHxCommand = new RelayCommand(_ => SafeNavigate(new MedHxVM()));
            AntropometricsCommand = new RelayCommand(_ => SafeNavigate(new AntropometricsVM()));
            DietCommand = new RelayCommand(_ => SafeNavigate(new DietVM()));
            TreatmentCommand = new RelayCommand(_ => SafeNavigate(new TreatmentVM()));
            PrescriptionCommand = new RelayCommand(_ => SafeNavigate(new PrescriptionVM()));
            BodySystemsOverviewCommand = new RelayCommand(_ => SafeNavigate(new BodySystemsOverviewVM()));
            EyeAnalysisCommand = new RelayCommand(_ => SafeNavigate(new EyeAnalysisVM()));
            ReportsCommand = new RelayCommand(_ => SafeNavigate(new ReportsVM()));
            ScannedNotesCommand = new RelayCommand(_ => SafeNavigate(new ScannedNotesVM()));

            // Default view
            SafeNavigate(new ClientVM());
        }
    }
}
