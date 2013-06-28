using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using TimeSheet.Interfaces;

namespace TimeSheet.ViewModels
{
    public class OptionsViewModel : ViewModelBase, IOptionsViewModel
    {
        #region Constructor

        public OptionsViewModel()
        {
            OptionsViewActionCommand = new OptionsViewActionCommand();
        }

        #endregion

        #region Properties

        public OptionsViewActionCommand OptionsViewActionCommand { get; set; }

        private string _domain;

        public string Domain
        {
            get
            {
                if (string.IsNullOrEmpty(_domain))
                    return Environment.UserDomainName;

                return _domain;
            }
            set
            {
                if (_domain != value)
                {
                    _domain = value;
                    RaisePropertyChanged("Domain");
                }
            }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    RaisePropertyChanged("Username");
                }
            }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    RaisePropertyChanged("Password");
                }
            }
        }

        private bool _specifyUserCredentials;

        public bool SpecifyUserCredentials
        {
            get { return _specifyUserCredentials; }
            set
            {
                if (_specifyUserCredentials != value)
                {
                    _specifyUserCredentials = value;
                    RaisePropertyChanged("SpecifyUserCredentials");
                }
            }
        }

        public bool CredentialsAreValid
        {
            get { return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password); }
        }

        public bool CredentialsChanged
        {
            get
            {
                if (_domain != _domainBackup || _username != _usernameBackup ||
                    _specifyUserCredentials != _specifyUserCredentialsBackup)
                    return true;

                return false;
            }
        }

        #endregion

        #region Protected Methods


        #endregion

        #region Private Fields

        private string _domainBackup;
        private string _usernameBackup;
        private string _passwordBackup;
        private bool _specifyUserCredentialsBackup;

        #endregion

        #region Public Methods

        public void BackupValues()
        {
            _domainBackup = _domain;
            _usernameBackup = _username;
            _passwordBackup = _password;
            _specifyUserCredentialsBackup = _specifyUserCredentials;
        }

        public void RestoreValues()
        {
            _domain = _domainBackup;
            _username = _usernameBackup;
            _password = _passwordBackup;
            _specifyUserCredentials = _specifyUserCredentialsBackup;
        }

        #endregion
    }
}
