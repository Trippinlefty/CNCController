using System.Windows;
using System.Windows.Controls;

namespace CNCController;

public partial class ProjectsPanel : UserControl
{
    public ProjectsPanel()
    {
        InitializeComponent();
    }
    
    private void AddProjectButton_Click(object sender, RoutedEventArgs e)
    {
        // Example logic to add a new project
        var newProjectName = $"Project {ProjectsListBox.Items.Count + 1}";
        ProjectsListBox.Items.Add(new ListBoxItem { Content = newProjectName });
    }
}