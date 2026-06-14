using System;
using System.Threading;
using System.Windows;

namespace WhatsAppWebDesktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private static Mutex? _mutex;
    private const string MutexName = "Global\\WhatsAppWebDesktopSingleInstanceMutex";

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, MutexName, out bool isNewInstance);

        if (!isNewInstance)
        {
            System.Windows.MessageBox.Show("WhatsApp Web Desktop ya se está ejecutando.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            System.Windows.Application.Current.Shutdown();
            return;
        }

        base.OnStartup(e);

        // Crear la ventana principal de forma manual
        var mainWindow = new MainWindow();

        // Si se inicia automáticamente con Windows, mantenemos la ventana oculta en segundo plano.
        if (e.Args.Contains("--startup"))
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
        else
        {
            mainWindow.Show();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_mutex != null)
        {
            try
            {
                _mutex.ReleaseMutex();
            }
            catch (ObjectDisposedException) { }
            catch (ApplicationException) { }
            _mutex.Dispose();
        }
        base.OnExit(e);
    }
}

