using System.Configuration;
using System.Data;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace Nostromo.TrayService
{
    public partial class App : Application
    {
        private TaskbarIcon? _notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs e)
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
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Make sure we clean up the tray icon when exiting
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }

    // Simple relay command implementation
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