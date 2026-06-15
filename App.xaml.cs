using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WhatsAppWebDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static Mutex? _mutex;
        private const string MutexName = "Global\\WhatsAppWebDesktopSingleInstanceMutex";
        private const string PipeName = "WhatsAppLite_IPC_Pipe";

        protected override void OnStartup(StartupEventArgs e)
        {
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // Si ya hay una instancia en ejecución, enviarle los argumentos por la tubería
                if (e.Args.Length > 0)
                {
                    SendArgsToRunningInstance(e.Args);
                }
                System.Windows.Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            // Crear la ventana principal de forma manual
            var mainWindow = new MainWindow();

            // Iniciar el servidor IPC
            StartPipeServer();

            // Si se inicia automáticamente con Windows, mantenemos la ventana oculta en segundo plano.
            if (e.Args.Contains("--startup"))
            {
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            else
            {
                mainWindow.Show();
                if (e.Args.Length > 0)
                {
                    mainWindow.HandleArguments(e.Args);
                }
            }
        }

        private static void SendArgsToRunningInstance(string[] args)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(1000); // Esperar 1 segundo máximo
                    using (var writer = new StreamWriter(client))
                    {
                        writer.Write(string.Join(" ", args));
                        writer.Flush();
                    }
                }
            }
            catch { }
        }

        private static void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                        {
                            server.WaitForConnection();
                            using (var reader = new StreamReader(server))
                            {
                                string argsStr = reader.ReadToEnd();
                                if (!string.IsNullOrEmpty(argsStr))
                                {
                                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        if (System.Windows.Application.Current.MainWindow is MainWindow mainWin)
                                        {
                                            mainWin.HandleArguments(argsStr.Split(' '));
                                        }
                                    }));
                                }
                            }
                        }
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }
            });
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
}

