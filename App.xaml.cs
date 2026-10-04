using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace AirlyATC;

public partial class App : Application
{
    private static string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Airly", "AirlyPilot.log");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            Log("Starting Airly Pilot.");
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            var window = new MainWindow();
            MainWindow = window;
            window.Show();
            Log("Main window created.");
        }
        catch (Exception ex)
        {
            Log("Startup failure:\n" + ex);
            MessageBox.Show("Airly Pilot could not start.\n\n" + ex.Message + "\n\nA log was saved to:\n" + LogPath, "Airly Pilot", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log("Unhandled UI exception:\n" + e.Exception);
        MessageBox.Show("Airly Pilot encountered an error.\n\n" + e.Exception.Message + "\n\nA log was saved to:\n" + LogPath, "Airly Pilot", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex) Log("Unhandled exception:\n" + ex);
    }

    private static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + message + Environment.NewLine);
        }
        catch { }
    }
}