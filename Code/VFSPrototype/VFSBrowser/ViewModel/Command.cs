using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VFSBrowser.ViewModel
{
    public class Command : ICommand
    {

        public delegate bool CommandCanExecute(object parameter);
        private readonly CommandCanExecute _commandCanExecute; 

        public delegate void CommandExecute (object parameter);
        private readonly CommandExecute _commandExecute; 

        public Command(CommandExecute commandExecute, CommandCanExecute commandCanExecute)
        {
            _commandExecute = commandExecute;
            _commandCanExecute = commandCanExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_commandCanExecute != null)
                return _commandCanExecute(parameter);

            return true;
        }

        public void Execute(object parameter)
        {
            _commandExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
