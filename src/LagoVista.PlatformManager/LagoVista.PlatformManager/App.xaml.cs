﻿//#define ENV_LOCAL
#define ENV_DEV
//#define ENV_PROD

using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PlatformManager.Core.ViewModels;
using LagoVista.XPlat.Core.Services;
using LagoVista.XPlat.Core.Views;
using System;

using Xamarin.Forms;

namespace LagoVista.PlatformManager
{
    public partial class App : Application
    {
        static App _instance;


        public App()
        {
            InitializeComponent();

            _instance = this;

            InitServices();
        }

        private void InitServices()
        {
#if ENV_PROD
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "api.nuviot.com",
            };
#elif ENV_DEV
            var serverInfo = new ServerInfo()
            {
                SSL = true,
                RootUrl = "dev-api.nuviot.com",
            };
#elif ENV_LOCAL
            var serverInfo = new ServerInfo()
            {
                SSL = false,
                RootUrl = "localhost:5001",
            };
#endif

            /* Configuring he IoC is something like this...be warned
             * 
             * https://www.youtube.com/watch?v=7-FbfkUD78w
             */

            var clientAppInfo = new ClientAppInfo();

            SLWIOC.RegisterSingleton<IClientAppInfo>(clientAppInfo);
            SLWIOC.RegisterSingleton<IAppConfig>(new AppConfig());

            var navigation = new ViewModelNavigation(this);
            LagoVista.XPlat.Core.Startup.Init(this, navigation);
            LagoVista.Client.Core.Startup.Init(serverInfo);

            navigation.Add<MainViewModel, Views.MainView>();
            navigation.Add<SplashViewModel, Views.SplashView>();

            navigation.Add<MonitorInstanceViewModel, Views.MonitorInstanceView>();
            navigation.Add<InstanceViewModel, Views.InstanceView>();
            navigation.Add<ListenerViewModel, Views.ListenerView>();
            navigation.Add<PipelineViewModel, Views.PipelineView>();
            navigation.Add<PlannerViewModel, Views.PlannerView>();

            navigation.Start<SplashViewModel>();

            SLWIOC.RegisterSingleton<IViewModelNavigation>(navigation);
        }

        public static App Instance { get { return _instance; } }

        public void HandleURIActivation(Uri uri)
        {
            var logger = SLWIOC.Get<ILogger>();
            if (this.MainPage == null)
            {
                logger.AddCustomEvent(LogLevel.Error, "App_HandleURIActivation", "Main Page Null");
            }
            else
            {
                var page = this.MainPage as LagoVistaNavigationPage;
                if (page != null)
                {
                    page.HandleURIActivation(uri);
                }
                else
                {

                    logger.AddCustomEvent(LogLevel.Error, "App_HandleURIActivation", "InvalidPageType - Not LagoVistaNavigationPage", new System.Collections.Generic.KeyValuePair<string, string>("type", this.MainPage.GetType().Name));
                }
            }

        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
    