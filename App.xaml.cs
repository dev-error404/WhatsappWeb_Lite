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
        private const int IpcPort = 49600;

        private static void LogMessage(string message)
        {
            try
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folder = Path.Combine(appData, "WhatsAppWebDesktop");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "ipc.log");
                File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n");
            }
            catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            LogMessage("App OnStartup triggered. Args: " + string.Join(" ", e.Args));
            bool isNewInstance = false;
            try
            {
                _listener = new TcpListener(IPAddress.Loopback, IpcPort);
                _listener.Start();
                isNewInstance = true;
                LogMessage("Socket listener started successfully on port " + IpcPort);
            }
            catch (Exception ex)
            {
                isNewInstance = false;
                LogMessage("Socket bind failed or port in use: " + ex.Message);
            }

            if (!isNewInstance)
            {
                LogMessage("This is not the first instance. Forwarding args and shutting down.");
                SendArgsToRunningInstance(e.Args);
                System.Windows.Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            // Crear la ventana principal de forma manual
            var mainWindow = new MainWindow();
            System.Windows.Application.Current.MainWindow = mainWindow;
            LogMessage("MainWindow instantiated.");

            // Iniciar la escucha de argumentos en segundo plano
            StartSocketServer();

            // Si se inicia automáticamente con Windows, mostramos la ventana brevemente para que
            // WebView2 pueda adjuntarse al HWND e inicializar el tray icon, luego la ocultamos.
            if (e.Args.Contains("--startup"))
            {
                LogMessage("App started in background mode (--startup).");
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                // Mostrar brevemente para que WebView2 e InitializeTrayIcon puedan ejecutarse
                mainWindow.Show();
                // Ocultar después de que el dispatcher procese la inicialización
                mainWindow.Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.HideToTray();
                    LogMessage("Window hidden to tray after startup initialization.");
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
            else
            {
                LogMessage("Showing MainWindow.");
                mainWindow.Show();
                if (e.Args.Length > 0)
                {
                    LogMessage("Handling initial startup arguments.");
                    mainWindow.HandleArguments(e.Args);
                }
            }
        }

        private static void SendArgsToRunningInstance(string[] args)
        {
            try
            {
                string payload = (args != null && args.Length > 0) ? string.Join("|", args) : "--show";
                LogMessage("Connecting to running instance to send args: " + payload);
                using (var client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, IpcPort);
                    using (var stream = client.GetStream())
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(payload);
                        writer.Flush();
                    }
                }
                LogMessage("Args sent successfully.");
            }
            catch (Exception ex)
            {
                LogMessage("Error sending args to running instance: " + ex.Message);
            }
        }

        private static void StartSocketServer()
        {
            LogMessage("Starting socket server thread.");
            Task.Run(async () =>
            {
                while (_listener != null)
                {
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        LogMessage("Client TCP connection accepted.");
                        _ = Task.Run(() => HandleClientConnection(client));
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Socket listener accept loop failed/stopped: " + ex.Message);
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
                    LogMessage("Received args via TCP: " + argsStr);
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var mainWin = System.Windows.Application.Current.MainWindow as MainWindow;
                        LogMessage("MainWindow found: " + (mainWin != null));
                        if (mainWin != null)
                        {
                            string[] parsedArgs = string.IsNullOrEmpty(argsStr) ? new string[] { "--show" } : argsStr.Split('|');
                            mainWin.HandleArguments(parsedArgs);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error handling client connection: " + ex.Message);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LogMessage("App exiting.");
            try
            {
                _listener?.Stop();
            }
            catch { }
            base.OnExit(e);
        }
    }
}

