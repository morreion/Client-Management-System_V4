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

        public ICommand ClientCommand { get; set; }
        public ICommand MedHxCommand { get; set; }
        public ICommand AntropometricsCommand { get; set; }
        public ICommand BodySystemsOverviewCommand { get; set; }
        public ICommand EyeAnalysisCommand { get; set; }
        public ICommand DietCommand { get; set; }
        public ICommand TreatmentCommand { get; set; }
        public ICommand PrescriptionCommand { get; set; }
        public ICommand SupplementsCommand { get; set; }
        public ICommand DistributorCommand { get; set; }

        private void Client(object obj) => CurrentView = new ClientVM();
        private void MedHx(object obj) => CurrentView = new MedHxVM();
        private void Antropometrics(object obj) => CurrentView = new AntropometricsVM();
        private void BodySystemsOverview(object obj) => CurrentView = new BodySystemsOverviewVM();
        private void EyeAnalysis(object obj) => CurrentView = new EyeAnalysisVM();
        private void Diet(object obj) => CurrentView = new DietVM();
        private void Treatment(object obj) => CurrentView = new TreatmentVM();
        private void Prescription(object obj) => CurrentView = new PrescriptionVM();
        private void Supplements(object obj) => CurrentView = new SupplementsVM();
        private void Distributor(object obj) => CurrentView = new DistributorVM();

        public NavigationVM()
        {
            ClientCommand = new RelayCommand(Client);
            MedHxCommand = new RelayCommand(MedHx);
            AntropometricsCommand = new RelayCommand(Antropometrics);
            BodySystemsOverviewCommand = new RelayCommand(BodySystemsOverview);
            EyeAnalysisCommand = new RelayCommand(EyeAnalysis);
            DietCommand = new RelayCommand(Diet);
            TreatmentCommand = new RelayCommand(Treatment);
            PrescriptionCommand = new RelayCommand(Prescription);
            SupplementsCommand = new RelayCommand(Supplements);
            DistributorCommand = new RelayCommand(Distributor);

            // Startup Page
            CurrentView = new ClientVM();
        }
    }
}
