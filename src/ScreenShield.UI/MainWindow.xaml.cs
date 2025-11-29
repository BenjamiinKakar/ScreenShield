using ScreenShield.UI.ViewModels;
using System.Windows;

namespace ScreenShield.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}