using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client_Management_System_V4.Models
{
    /// <summary>
    /// Represents physical measurements and vital signs for a client visit
    /// </summary>
    public class Anthropometrics : INotifyPropertyChanged
    {
        private int? _anthropometricsID;
        private DateTime? _assessment_Date = DateTime.Now;
        private string? _bp;
        private int? _pulse;
        private int? _spO2_Percent;
        private string? _pwa;
        private double? _temp;
        private double? _weight;
        private double? _height;
        private string? _zinc_Status;
        private string? _nox_Status;
        private int _clientID;
        private string? _clientName;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int? AnthropometricsID
        {
            get => _anthropometricsID;
            set { _anthropometricsID = value; OnPropertyChanged(); }
        }

        public DateTime? Assessment_Date
        {
            get => _assessment_Date;
            set { _assessment_Date = value; OnPropertyChanged(); }
        }

        public string? BP
        {
            get => _bp;
            set { _bp = value; OnPropertyChanged(); }
        }

        public int? Pulse
        {
            get => _pulse;
            set { _pulse = value; OnPropertyChanged(); }
        }

        public int? SpO2_Percent
        {
            get => _spO2_Percent;
            set { _spO2_Percent = value; OnPropertyChanged(); }
        }

        public string? PWA
        {
            get => _pwa;
            set { _pwa = value; OnPropertyChanged(); }
        }

        public double? Temp
        {
            get => _temp;
            set { _temp = value; OnPropertyChanged(); }
        }

        public double? Weight
        {
            get => _weight;
            set 
            { 
                _weight = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(BMI)); // Notify BMI changed
            }
        }

        public double? Height
        {
            get => _height;
            set 
            { 
                _height = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(BMI)); // Notify BMI changed
            }
        }

        public string? Zinc_Status
        {
            get => _zinc_Status;
            set { _zinc_Status = value; OnPropertyChanged(); }
        }

        public string? NOX_Status
        {
            get => _nox_Status;
            set { _nox_Status = value; OnPropertyChanged(); }
        }

        public int ClientID
        {
            get => _clientID;
            set { _clientID = value; OnPropertyChanged(); }
        }

        // Computed Properties
        public string? ClientName
        {
            get => _clientName;
            set { _clientName = value; OnPropertyChanged(); }
        }

        public double? BMI
        {
            get
            {
                if (Weight.HasValue && Height.HasValue && Height.Value > 0)
                {
                    // BMI = kg / m^2
                    // Height is typically in cm, so convert to meters (Height / 100)
                    double heightInMeters = Height.Value / 100.0;
                    return Math.Round(Weight.Value / (heightInMeters * heightInMeters), 1);
                }
                return null;
            }
        }
    }
}
