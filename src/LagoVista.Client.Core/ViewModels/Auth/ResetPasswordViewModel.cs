﻿using LagoVista.Client.Core.Net;
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Client.Core.ViewModels.Auth
{
    public class ResetPasswordViewModel : IoTAppViewModelBase
    {
        IRawRestClient _rawRestClient;

        public ResetPasswordViewModel(IRawRestClient rawRestClient)
        {
            _rawRestClient = rawRestClient;

            ResetPasswordCommand = new RelayCommand(ResetPassword);
            CancelCommand = new RelayCommand(() => ViewModelNavigation.GoBackAsync());
        }

        public void ResetPassword()
        {

        }
 
        public RelayCommand ResetPasswordCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
    }
}
