﻿using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.IoT.Simulator.Admin.Models;
using Microsoft.Azure.EventHubs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Core.ViewModels.Simulator
{
    public class SimulatorViewModel : AppViewModelBase
    {
        IMQTTDeviceClient _mqttClient;
        ITCPClient _tcpClient;
        IUDPClient _udpClient;
        EventHubClient _eventHubClient;

        bool _isConnected;


        public SimulatorViewModel()
        {
            ConnectCommand = new RelayCommand(Connect, CanConnect);
            DisconnectCommand = new RelayCommand(Disconnect, CanDisconnect);
        }

        public async void EditSimulator()
        {
            if (_isConnected)
            {
                await DisconnectAsync();
            }
            await ViewModelNavigation.NavigateAndEditAsync<SimulatorEditorViewModel>(Model.Id);
        }

        private async Task<InvokeResult> LoadSimulator()
        {
            var simulatorResponse = await RestClient.GetAsync<DetailResponse<LagoVista.IoT.Simulator.Admin.Models.Simulator>>($"/api/simulator/{LaunchArgs.ChildId}");
            if (simulatorResponse != null)
            {
                Model = simulatorResponse.Result.Model;
                MessageTemplates = simulatorResponse.Result.Model.MessageTemplates;
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(ConnectionVisible));
            }

            return InvokeResult.Success;
        }
        

        public async void Connect()
        {
            try
            {
                IsBusy = true;
                switch (Model.DefaultTransport.Value)
                {
                    case TransportTypes.AMQP:
                        string connectionString = $"Endpoint=sb://{Model.Name}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={Model.AuthToken}";
                        var bldr = new EventHubsConnectionStringBuilder(connectionString)
                        {
                            EntityPath = Model.HubName
                        };

                        _eventHubClient = EventHubClient.CreateFromConnectionString(bldr.ToString());

                        break;
                    case TransportTypes.MQTT:
                        _mqttClient = SLWIOC.Create<IMQTTDeviceClient>();
                        _mqttClient.BrokerHostName = Model.DefaultEndPoint;
                        _mqttClient.BrokerPort = Model.DefaultPort;
                        _mqttClient.DeviceId = Model.UserName;
                        _mqttClient.Password = Model.Password;

                        var result = await _mqttClient.ConnectAsync();
                        Debug.WriteLine(result);

                        break;
                    case TransportTypes.TCP:
                        _tcpClient = SLWIOC.Create<ITCPClient>();
                        await _tcpClient.ConnectAsync(Model.DefaultEndPoint, Model.DefaultPort);
                        break;
                    case TransportTypes.UDP:
                        _udpClient = SLWIOC.Create<IUDPClient>();
                        await _udpClient.ConnectAsync(Model.DefaultEndPoint, Model.DefaultPort);
                        break;
                }

                RightMenuIcon = Client.Core.ViewModels.RightMenuIcon.None;

                _isConnected = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Popups.ShowAsync(ex.Message);
                _isConnected = false;
            }
            finally
            {
                ConnectCommand.RaiseCanExecuteChanged();
                DisconnectCommand.RaiseCanExecuteChanged();
                IsBusy = false;
            }
        }

        public override bool CanCancel()
        {
            if (_isConnected)
            {
                Disconnect();
                return true;
            }
            else
            {
                return true;
            }
        }



        public async Task DisconnectAsync()
        {
            switch (Model.DefaultTransport.Value)
            {
                case TransportTypes.MQTT:
                    if (_mqttClient != null)
                    {
                        _mqttClient.Disconnect();
                        _mqttClient = null;
                    }
                    break;
                case TransportTypes.TCP:
                    if (_tcpClient != null)
                    {
                        await _tcpClient.DisconnectAsync();
                        _tcpClient.Dispose();
                    }
                    break;
                case TransportTypes.UDP:
                    if (_udpClient != null)
                    {
                        await _udpClient.DisconnectAsync();
                        _udpClient.Dispose();
                    }
                    break;
            }

            _isConnected = false;
            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();

            RightMenuIcon = Client.Core.ViewModels.RightMenuIcon.None;
        }

        public async void Disconnect()
        {
            await DisconnectAsync();
        }

        public bool CanConnect()
        {
            return Model != null && !_isConnected && (Model.DefaultTransport.Value == TransportTypes.AMQP ||
                            Model.DefaultTransport.Value == TransportTypes.MQTT ||
                             Model.DefaultTransport.Value == TransportTypes.AzureEventHub ||
                             Model.DefaultTransport.Value == TransportTypes.AzureIoTHub ||
                             Model.DefaultTransport.Value == TransportTypes.AzureServiceBus ||
                            Model.DefaultTransport.Value == TransportTypes.TCP ||
                            Model.DefaultTransport.Value == TransportTypes.UDP);
        }

        public bool CanDisconnect()
        {
            return Model != null && _isConnected && (Model.DefaultTransport.Value == TransportTypes.AMQP ||
                            Model.DefaultTransport.Value == TransportTypes.MQTT ||
                             Model.DefaultTransport.Value == TransportTypes.AzureEventHub ||
                             Model.DefaultTransport.Value == TransportTypes.AzureIoTHub ||
                             Model.DefaultTransport.Value == TransportTypes.AzureServiceBus ||
                            Model.DefaultTransport.Value == TransportTypes.TCP ||
                            Model.DefaultTransport.Value == TransportTypes.UDP);
        }

        public bool ConnectionVisible
        {
            get
            {
                return Model != null && (Model.DefaultTransport.Value == TransportTypes.AMQP ||
                                 Model.DefaultTransport.Value == TransportTypes.MQTT ||
                                 Model.DefaultTransport.Value == TransportTypes.TCP ||
                             Model.DefaultTransport.Value == TransportTypes.AzureEventHub ||
                             Model.DefaultTransport.Value == TransportTypes.AzureIoTHub ||
                             Model.DefaultTransport.Value == TransportTypes.AzureServiceBus ||
                                 Model.DefaultTransport.Value == TransportTypes.UDP);
            }
        }

        public override void Edit()
        {
            EditSimulator();
        }

        public override Task InitAsync()
        {
            return PerformNetworkOperation(LoadSimulator);
        }

        public override Task ReloadedAsync()
        {
            return PerformNetworkOperation(LoadSimulator);
        }

        List<MessageTemplate> _messageTemplates;
        public List<MessageTemplate> MessageTemplates
        {
            get { return _messageTemplates; }
            set { Set(ref _messageTemplates, value); }
        }

        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand DisconnectCommand { get; set; }

        MessageTemplate _selectedMessageTemplate;
        public MessageTemplate SelectedMessageTemplate
        {
            get { return _selectedMessageTemplate; }
            set
            {
                if (value != null && _selectedMessageTemplate != value)
                {
                    var launchArgs = new ViewModelLaunchArgs()
                    {
                        ViewModelType = typeof(Messages.SendMessageViewModel),
                        Parent = value,
                        LaunchType = LaunchTypes.Other,
                    };

                    if (_eventHubClient != null) launchArgs.Parameters.Add("ehclient", _eventHubClient);
                    if (_tcpClient != null) launchArgs.Parameters.Add("tcpclient", _tcpClient);
                    if (_mqttClient != null) launchArgs.Parameters.Add("mqttclient", _mqttClient);
                    if (_udpClient != null) launchArgs.Parameters.Add("udpclient", _udpClient);

                    ViewModelNavigation.NavigateAsync(launchArgs);
                }

                _selectedMessageTemplate = value;
                RaisePropertyChanged();
            }
        }

        LagoVista.IoT.Simulator.Admin.Models.Simulator _model;
        public LagoVista.IoT.Simulator.Admin.Models.Simulator Model
        {
            get { return _model; }
            set { Set(ref _model, value); }
        }
    }
}
