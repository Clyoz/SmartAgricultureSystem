using SmartAgricultureSystem.ViewModels;
using System.Windows;

namespace SmartAgricultureSystem.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel mViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 设置ViewModel（由ServiceLocator调用）
        /// </summary>
        public void SetViewModel(MainViewModel viewModel)
        {
            mViewModel = viewModel;
            DataContext = mViewModel;
        }
    }
}