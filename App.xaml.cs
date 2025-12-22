using System;
using System.IO;
using System.Windows;
using Client_Management_System_V4.Data;

namespace Client_Management_System_V4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // Set up emergency logging for silent crashes
            AppDomain.CurrentDomain.UnhandledException += (s, args) => 
                LogFatalError(args.ExceptionObject as Exception, "AppDomain Unhandled");
            
            DispatcherUnhandledException += (s, args) => 
            {
                LogFatalError(args.Exception, "Dispatcher Unhandled");
                args.Handled = true;
            };

            base.OnStartup(e);

            try
            {
                DatabaseManager.InitializeDatabase();
                
                // MANUALLY SHOW MAIN WINDOW
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                var fullDetails = GetFullExceptionMessage(ex);
                LogFatalError(ex, "Database or Window Initialization");
                
                MessageBox.Show(
                    $"Failed to start application:\n\n{fullDetails}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private string GetFullExceptionMessage(Exception? ex)
        {
            if (ex == null) return "No exception info.";
            var msg = ex.Message;
            if (ex.InnerException != null)
            {
                msg += "\n\nINNER: " + GetFullExceptionMessage(ex.InnerException);
            }
            return msg;
        }

        private void LogFatalError(Exception? ex, string context)
        {
            try
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "ClientManagementSystemV4");
                Directory.CreateDirectory(appDataPath);
                
                var logPath = Path.Combine(appDataPath, "CMS_Crash_Log.txt");
                var fullDetails = GetFullExceptionMessage(ex);
                var message = $"[{DateTime.Now}] CONTEXT: {context}\nDETAILS: {fullDetails}\nSTACK: {ex?.StackTrace}\n\n";
                
                File.AppendAllText(logPath, message);
                
                // Mirror to desktop
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                File.WriteAllText(Path.Combine(desktopPath, "CMS_CRITICAL_ERROR.txt"), message);
            }
            catch { }
        }
    }
}
