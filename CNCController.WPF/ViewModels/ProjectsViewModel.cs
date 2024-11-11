using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CNCController.ViewModels;

public class ProjectsViewModel : ViewModelBase
{
    private ObservableCollection<string> _projects;
    private string _selectedProject;

    public ObservableCollection<string> Projects
    {
        get => _projects;
        set => SetProperty(ref _projects, value, nameof(Projects));
    }

    public string SelectedProject
    {
        get => _selectedProject;
        set => SetProperty(ref _selectedProject, value, nameof(SelectedProject));
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

    public void LoadProjects() { /* Logic to load available projects */ }
    public void SaveProject() { /* Logic to save current project */ }
    public void SwitchProject(string projectName) { /* Logic to switch to a different project */ }
}