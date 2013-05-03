using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using VFSBrowser.View;
using VFSBrowser.ViewModel;

namespace Startup
{
    public class ApplicationLifetimeManager
    {
        private IMainViewModel _mainViewModel;
        private MainWindow _mainWindow;

        public void Startup()
        {
            var container = new UnityContainer().LoadConfiguration();
            container.RegisterInstance(container);

            _mainViewModel = container.Resolve<IMainViewModel>();
            container.RegisterInstance(_mainViewModel);

            _mainWindow = new MainWindow(_mainViewModel);
            _mainWindow.Show();
        }

        public void Exit()
        {
            if (_mainViewModel != null) _mainViewModel.Dispose();
        }
    }
}