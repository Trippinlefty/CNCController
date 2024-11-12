using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CNCController.Core.Services.Configuration;
using CNCController.Core.Services.RelayCommand;

namespace CNCController.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configService;
        private string _selectedMaterial;
        private string _selectedBit;
        private double _detailAngle;
        private int _cutSettingValue;

        public SettingsViewModel(IConfigurationService configService)
        {
            _configService = configService;

            // Initialize the options (These could be populated from configuration or predefined values)
            MaterialOptions = new ObservableCollection<string> { "Wood", "Aluminum", "Plastic" };
            BitOptions = new ObservableCollection<string> { "1/8 inch", "1/4 inch", "V-bit" };

            // Default selections
            SelectedMaterial = MaterialOptions[0];
            SelectedBit = BitOptions[0];

            // Initialize commands
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
        }

        public ObservableCollection<string> MaterialOptions { get; }
        public ObservableCollection<string> BitOptions { get; }

        public string SelectedMaterial
        {
            get => _selectedMaterial;
            set => SetProperty(ref _selectedMaterial, value, nameof(SelectedMaterial));
        }

        public string SelectedBit
        {
            get => _selectedBit;
            set => SetProperty(ref _selectedBit, value, nameof(SelectedBit));
        }

        public double DetailAngle
        {
            get => _detailAngle;
            set => SetProperty(ref _detailAngle, value, nameof(DetailAngle));
        }

        public int CutSettingValue
        {
            get => _cutSettingValue;
            set => SetProperty(ref _cutSettingValue, value, nameof(CutSettingValue));
        }

        public ICommand SaveSettingsCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetProperty<T>(ref T backingField, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task SaveSettingsAsync()
        {
            // Save settings using the configuration service
            await _configService.UpdateConfigAsync("SelectedMaterial", SelectedMaterial);
            await _configService.UpdateConfigAsync("SelectedBit", SelectedBit);
            await _configService.UpdateConfigAsync("DetailAngle", DetailAngle.ToString());
            await _configService.UpdateConfigAsync("CutSettingValue", CutSettingValue.ToString());
        }
    }
}
