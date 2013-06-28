using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheet.ViewModels;

namespace TimeSheet.Interfaces
{
    public interface IOptionsViewModel
    {
        OptionsViewActionCommand OptionsViewActionCommand { get; set; }

        string Domain { get; set; }

        string Username { get; set; }

        string Password { get; set; }

        bool SpecifyUserCredentials { get; set; }

        bool CredentialsAreValid { get; }

        bool CredentialsChanged { get; }

        void BackupValues();

        void RestoreValues();
    }
}
