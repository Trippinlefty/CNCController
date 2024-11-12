using System.Collections.Generic;
using System.ComponentModel;

namespace CNCController.ViewModels;

public class PreviewViewModel : ViewModelBase
{
    private string _gCodePreviewData;

    public string GCodePreviewData
    {
        get => _gCodePreviewData;
        set => SetProperty(ref _gCodePreviewData, value, nameof(GCodePreviewData));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T backingField, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value)) return false;
        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // Load and update preview data as needed
    public void LoadGCodePreview(string gCode) { /* Logic to load G-code for preview */ }
}