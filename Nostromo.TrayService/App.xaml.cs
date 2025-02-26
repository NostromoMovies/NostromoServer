﻿using System.Configuration;
using System.Data;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Nostromo.Server.Server;
using Nostromo.Server.Utilities;
using Nostromo.Server.Settings;
using SettingsProvider = Nostromo.Server.Settings.SettingsProvider;

namespace Nostromo.TrayService
{
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;
        private ILogger _logger = null;

        private void OnStartup(object sender, StartupEventArgs e)
        {
            // Prevent shutdown when no windows are open
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _notifyIcon = new TaskbarIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                ToolTipText = "Nostromo Server"
            };

            _notifyIcon.ContextMenu = new System.Windows.Controls.ContextMenu
            {
                Items =
                {
                    new System.Windows.Controls.MenuItem
                    {
                        Header = "Exit",
                        Command = new RelayCommand(() =>
                        {
                            _notifyIcon.Dispose();
                            Shutdown();
                        })
                    }
                }
            };

            var logFactory = LoggerFactory.Create( o => o.AddNLog() );
            _logger = logFactory.CreateLogger("Main");

            try
            {
                var settingsProvider = new SettingsProvider(logFactory.CreateLogger<SettingsProvider>());
                Utils.SettingsProvider = settingsProvider;
                var startup = new Startup(logFactory.CreateLogger<Startup>(), settingsProvider);
                startup.Start().ConfigureAwait(true).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Failed to start server");
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Make sure we clean up the tray icon when exiting
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();
    }
}