using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using Nostromo.Server.Utilities;
using Nostromo.Server;
using System.Diagnostics;

namespace Nostromo.TrayService
{
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;
        private ILogger _logger = null!;
        private WebApplication? _webApp;
        private bool _isServerRunning;

        private async void OnStartup(object sender, StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var logFactory = LoggerFactory.Create(o => o.AddNLog());
            _logger = logFactory.CreateLogger("Main");

            try
            {
                // Initialize tray icon
                await InitializeTrayIcon();

                // Create and run the web application using the shared Program.cs
                _webApp = await Program.CreateApp(e.Args);
                await _webApp.StartAsync();
                _isServerRunning = true;

                _logger.LogInformation("Nostromo server started successfully");
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Failed to start server");
                await ShutdownAsync();
            }
        }

        private async Task InitializeTrayIcon()
        {
            _notifyIcon = new TaskbarIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    System.Reflection.Assembly.GetExecutingAssembly().Location),
                ToolTipText = "Nostromo Server"
            };

            _notifyIcon.ContextMenu = new System.Windows.Controls.ContextMenu
            {
                Items =
                {
                    new System.Windows.Controls.MenuItem
                    {
                        Header = "Open Dashboard",
                        Command = new RelayCommand(() =>
                        {
                            if (_isServerRunning)
                            {
                                var settings = Utils.SettingsProvider?.GetSettings();
                                var port = settings?.ServerPort ?? 5000;
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = $"http://localhost:{port}",
                                    UseShellExecute = true
                                });
                            }
                        })
                    },
                    new System.Windows.Controls.MenuItem
                    {
                        Header = "Exit",
                        Command = new RelayCommand(async () => await ShutdownAsync())
                    }
                }
            };
        }

        private async Task ShutdownAsync()
        {
            if (_webApp != null)
            {
                try
                {
                    await _webApp.StopAsync();
                    await _webApp.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error shutting down web server");
                }
            }

            _notifyIcon?.Dispose();
            Shutdown();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await ShutdownAsync();
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