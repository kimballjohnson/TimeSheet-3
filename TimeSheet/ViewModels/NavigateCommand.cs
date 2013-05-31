using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace TimeSheet
{
    public class NavigateCommand : ICommand
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
            string param = (string) parameter;

            switch (param)
            {
                case "Options":
                    var view = new OptionsView();
                    var viewModel = ViewModelLocater.OptionsViewModel;
                    viewModel.BackupValues();
                    view.DataContext = viewModel;
                    ViewModelLocater.MainWindowViewModel.OptionsWindow = view;
                    view.Show();
                    break;
            }
        }
    }
}
