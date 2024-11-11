using System.Windows;
using System.Windows.Controls;

namespace CNCController;

public partial class TopMenuBar : UserControl
{
    public TopMenuBar()
    {
        InitializeComponent();
    }
    
    private void CarveButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Starting the Carve process...");
        // Call your carve logic here
    }

    private void JogButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Starting the Jog process...");
        // Call your jog logic here
    }
}