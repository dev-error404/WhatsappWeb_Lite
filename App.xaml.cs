using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private static TcpListener? _listener;
        private const int IpcPort = 50304;

        protected override void OnStartup(StartupEventArgs e)
        {
            bool isNewInstance = false;
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, IpcPort);
                _listener.Start();
                isNewInstance = true;
            }
            catch (SocketException)
            {
                // El puerto ya está en uso, significa que ya hay una instancia activa
                isNewInstance = false;
            }

            if (!isNewInstance)
            {
                // Si ya hay una instancia en ejecución, enviarle los argumentos por TCP
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

            // Iniciar la escucha de argumentos en segundo plano
            StartSocketServer();

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
                using (var client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, IpcPort);
                    using (var stream = client.GetStream())
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(string.Join("|", args));
                        writer.Flush();
                    }
                }
            }
            catch { }
        }

        private static void StartSocketServer()
        {
            Task.Run(async () =>
            {
                while (_listener != null)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        _ = Task.Run(() => HandleClientConnection(client));
                    }
                    catch
                    {
                        break;
                    }
                }
            });
        }

        private static void HandleClientConnection(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string argsStr = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(argsStr))
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (System.Windows.Application.Current.MainWindow is MainWindow mainWin)
                            {
                                mainWin.HandleArguments(argsStr.Split('|'));
                            }
                        }));
                    }
                }
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _listener?.Stop();
            }
            catch { }
            base.OnExit(e);
        }
    }
}

