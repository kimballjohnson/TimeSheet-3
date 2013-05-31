using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeSheet
{
    public static class ViewModelLocater
    {
        private static MainWindowViewModel _mainWindowViewModel;

        public static MainWindowViewModel MainWindowViewModel
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

        private static OptionsViewModel _optionsViewModel;

        public static OptionsViewModel OptionsViewModel
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
