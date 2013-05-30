using System;
using System.Linq;
using System.Windows.Input;

namespace TimeSheet
{
    public class GetDataCommand : ICommand
    {

        private bool canExecute = true;

        public void SetCanExecute(bool value)
        {
            if (canExecute != value)
            {
                canExecute = value;
                if(CanExecuteChanged != null)
                    CanExecuteChanged(this, new EventArgs());
            }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            ViewModelLocater.MainWindowViewModel.GetData();
        }
    }
}
