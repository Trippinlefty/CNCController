using System.Windows;
using CNCController.ViewModels;

namespace CNCController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CNCViewModel _viewModel;
        public MainWindow(CNCViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}