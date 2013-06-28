using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using TimeSheet.Interfaces;
using TimeSheet.Services;

namespace TimeSheet.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        protected static IDataService DataService { get; set; }

        static ViewModelBase()
        {
            DataService = new DataService();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
