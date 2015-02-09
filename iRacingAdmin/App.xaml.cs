using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Alchemy;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.ViewModels;
using iRacingAdmin.Sync;
using iRacingAdmin.Views;
using iRacingSimulator;

namespace iRacingAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Dictionary<Type, SdkViewModel> _sdkModels;

        private MainViewModel _mainModel;
        public MainViewModel MainModel { get { return _mainModel; } }

        public static App Instance
        {
            get { return (App) Application.Current; }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Get path to iRacing folder
            var settings = iRacingAdmin.Properties.Settings.Default;
            if (string.IsNullOrWhiteSpace(settings.IRacingPath))
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var path = Path.Combine(documentsPath, "iRacing");

                settings.IRacingPath = path;
                settings.Save();
            }

            Paths.LoadPaths(settings.IRacingPath);


            // Initialize
            _sdkModels = new Dictionary<Type, SdkViewModel>();

            Simulator.Initialize();
            SyncManager.Initialize(this.Dispatcher);
            
            _mainModel = new MainViewModel();
            _mainModel.View.Show();
            this.MainWindow = _mainModel.View;
            
            // Start the SDK
            Connection.Instance.SubSessionIdChanged += OnSubSessionIdChanged;
            Simulator.Instance.SessionInfoUpdated += OnSessionInfoUpdated;
            Simulator.Instance.TelemetryUpdated += OnTelemetryUpdated;

            var args = Environment.GetCommandLineArgs();
            if (args.Contains("-sim"))
            {
                // Simulate iracing connection for testing
                Connection.Instance.StartSimulate();
            }
            else
            {
                Connection.Instance.Start();
            }

            // React when the sync state changes
            SyncManager.Instance.StateUpdated += OnSyncStateUpdated;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.HandleException(e.ExceptionObject as Exception);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
        {
            this.HandleException(ex.Exception);
        }

        private void HandleException(Exception ex)
        {
            if (ex == null) ex = new Exception("Unknown exception.");
            Log.Instance.WriteLogError("Unhandled exception", ex);
            MessageWindow.ShowException(ex.Message);
        }

        public void LogError(string action, Exception ex)
        {
            Log.Instance.WriteLogError(action, ex);
            
            // Show popup
            MainModel.ShowError(action, ex);
        }

        public void ShowLog()
        {
            Process.Start(Log.Instance.Path);
        }

        private void OnSyncStateUpdated(object sender, EventArgs eventArgs)
        {
            // Update the sync state across all models on the dispatcher thread
            this.Dispatcher.Invoke(SyncUpdateThread);
        }

        private int _isUpdatingState = 0;

        private void SyncUpdateThread()
        {
            // If already updating - ignore this update
            if (Interlocked.CompareExchange(ref _isUpdatingState, 1, 0) == 0)
            {
                try
                {
                    // Get the new state
                    var state = SyncManager.Instance.State;

                    // Update it
                    SyncStateUpdate.Update(state);

                    // Send to all viewmodels
                    // Viewmodels should handle the state update SYNCHRONOUSLY!!
                    foreach (var model in _sdkModels)
                    {
                        try
                        {
                            model.Value.OnSyncStateUpdated();
                        }
                        catch (Exception ex)
                        {
                            App.Instance.LogError("Updating sync state.", ex);
                        }
                    }
                }
                finally
                {
                    _isUpdatingState = 0;
                }
            }
            else
            {
                Log.Instance.LogInfo("Ignored state update: already updating state.");
            }
        }

        private void OnSubSessionIdChanged(object sender, EventArgs e)
        {
            _mainModel.IsWaitingForConnection = Connection.Instance.SubSessionId == null;
        }

        private void OnSessionInfoUpdated(object sender, iRacingSdkWrapper.SdkWrapper.SessionInfoUpdatedEventArgs e)
        {
            // iRacing session info updated - send to all viewmodels
            //this.Dispatcher.Invoke(() =>
            //{
                foreach (var model in _sdkModels)
                {
                    model.Value.OnSessionInfoUpdated(e);
                }
            //});
        }

        private void OnTelemetryUpdated(object sender, iRacingSdkWrapper.SdkWrapper.TelemetryUpdatedEventArgs e)
        {
            // iRacing telemetry updated - send to all viewmodels
            //this.Dispatcher.Invoke(() =>
            //{
                foreach (var model in _sdkModels)
                {
                    model.Value.OnTelemetryUpdated(e);

                }
            //});
        }

        public void RegisterSdkModel(SdkViewModel model)
        {
            // Register a viewmodel so it receives SDK updates
            var type = model.GetType();
            this.UnregisterSdkModel(model);
            _sdkModels.Add(type, model);
        }

        public void UnregisterSdkModel(SdkViewModel model)
        {
            // Unregister a viewmodel so it stops receiving SDK updates
            var type = model.GetType();
            if (_sdkModels.ContainsKey(type))
            {
                _sdkModels.Remove(type);
            }
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up
            Connection.Instance.Stop();
            SyncManager.Instance.Disconnect();
            Server.Server.Instance.Stop();

            WebSocketClient.Shutdown();
            WebSocketServer.Shutdown();

            Thread.Sleep(500);

            base.OnExit(e);
        }
    }
}
