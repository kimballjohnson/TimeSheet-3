using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace TimeSheet
{
    public class OptionsViewActionCommand : ICommand
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
            string param = (string)parameter;

            var optionsVM = ViewModelLocater.OptionsViewModel;
            var mainVM = ViewModelLocater.MainWindowViewModel;

            switch (param)
            {
                case "Save":
                    if (mainVM.OptionsWindow != null)
                    {
                        if(optionsVM.CredentialsChanged)
                        {
                            if (optionsVM.SpecifyUserCredentials && optionsVM.CredentialsAreValid)
                                mainVM.CurrentUser = optionsVM.Domain + "\\" + optionsVM.Username;
                            else
                                mainVM.CurrentUser = Environment.UserDomainName + "\\" + Environment.UserName;
                        }

                        mainVM.OptionsWindow.Close();
                        mainVM.OptionsWindow = null;
                    }
                    break;
                case "Cancel":
                    optionsVM.RestoreValues();
                    if(mainVM.OptionsWindow != null)
                    {
                        mainVM.OptionsWindow.Close();
                       mainVM.OptionsWindow = null;
                    }
                    break;
            }
        }
    }
}
