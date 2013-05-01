using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Startup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ApplicationLifetimeManager _manager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _manager = new ApplicationLifetimeManager();
            _manager.Startup();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _manager.Exit();
        }
    }
}
