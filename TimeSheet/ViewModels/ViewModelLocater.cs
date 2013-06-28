using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheet.Interfaces;

namespace TimeSheet.ViewModels
{
    public static class ViewModelLocater
    {
        private static IMainWindowViewModel _mainWindowViewModel;

        public static IMainWindowViewModel MainWindowViewModel
        {
            get
            {
                if (_mainWindowViewModel == null)
                {
                    _mainWindowViewModel = new MainWindowViewModel();
                }

                return _mainWindowViewModel;
            }
        }

        private static IOptionsViewModel _optionsViewModel;

        public static IOptionsViewModel OptionsViewModel
        {
            get
            {
                if (_optionsViewModel == null)
                {
                    _optionsViewModel = new OptionsViewModel();
                }

                return _optionsViewModel;
            }
        }
    }
}
