using VFSBrowser.View;
using VFSBrowser.ViewModel;

namespace VFSBrowser
{
    public class ApplicationLifetimeManager
    {
        private MainViewModel _mainViewModel;
        private MainWindow _mainWindow;

        public void Startup()
        {
            _mainViewModel = new MainViewModel();
            _mainWindow = new MainWindow { DataContext = _mainViewModel };
            _mainWindow.Show();
        }

        public void Exit()
        {
            if (_mainViewModel != null) _mainViewModel.Dispose();
        }
    }
}