using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WashMachine.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executionAction;

        public RelayCommand(Action<object> executionAction)
        {
            _executionAction = executionAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            _executionAction(parameter);
        }
    }
}
