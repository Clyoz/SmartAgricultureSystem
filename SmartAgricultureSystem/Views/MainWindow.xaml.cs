using SmartAgricultureSystem.ViewModels;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}