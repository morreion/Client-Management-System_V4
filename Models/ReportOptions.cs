using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client_Management_System_V4.Models
{
    public class ReportOptions : INotifyPropertyChanged
    {
        private bool _includeMedicalHistory = true;
        private bool _includeAnthropometrics = true;
        private bool _includeDiet = true;
        private bool _includeTreatment = true;
        private bool _includeEyeAnalysis = true;
        private bool _includeBodySystems = true;

        public bool IncludeMedicalHistory
        {
            get => _includeMedicalHistory;
            set { _includeMedicalHistory = value; OnPropertyChanged(); }
        }

        public bool IncludeAnthropometrics
        {
            get => _includeAnthropometrics;
            set { _includeAnthropometrics = value; OnPropertyChanged(); }
        }

        public bool IncludeDiet
        {
            get => _includeDiet;
            set { _includeDiet = value; OnPropertyChanged(); }
        }

        public bool IncludeTreatment
        {
            get => _includeTreatment;
            set { _includeTreatment = value; OnPropertyChanged(); }
        }

        public bool IncludeEyeAnalysis
        {
            get => _includeEyeAnalysis;
            set { _includeEyeAnalysis = value; OnPropertyChanged(); }
        }

        private bool _includePrescriptions = true;
        public bool IncludePrescriptions
        {
            get => _includePrescriptions;
            set { _includePrescriptions = value; OnPropertyChanged(); }
        }

        public bool IncludeBodySystems
        {
            get => _includeBodySystems;
            set { _includeBodySystems = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
