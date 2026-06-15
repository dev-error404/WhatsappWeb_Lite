using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using SystemTray = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace WhatsAppWebDesktop
{
    public partial class MainWindow : Window
    {
        private SystemTray.NotifyIcon? _notifyIcon;
        private bool _isExiting = false;
        private bool _shownBalloonHelp = false;
        private string? _pendingProtocolUrl;

        public MainWindow()
        {
            InitializeComponent();
            this.StateChanged += OnWindowStateChanged;
            this.Closing += OnWindowClosing;
            
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Configurar el directorio de datos de usuario en AppData local para persistir cookies y sesión.
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string userDataFolder = Path.Combine(appDataPath, "WhatsAppWebDesktop", "User Data");

                // Configurar opciones de entorno desactivando la aceleración por hardware (--disable-gpu)
                var options = new CoreWebView2EnvironmentOptions("--disable-gpu");
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await WvWhatsApp.EnsureCoreWebView2Async(env);

                // Configurar eventos después de inicializar
                WvWhatsApp.CoreWebView2.NewWindowRequested += OnNewWindowRequested;
                WvWhatsApp.NavigationCompleted += OnNavigationCompleted;
                WvWhatsApp.CoreWebView2.DocumentTitleChanged += OnDocumentTitleChanged;

                // Modificar el User-Agent para hacerse pasar por Google Chrome en Windows
                WvWhatsApp.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36";

                // Buscar actualizaciones en GitHub antes de cargar WhatsApp Web
                await CheckForUpdatesAsync();

                // Navegar a WhatsApp Web
                string targetUrl = "https://web.whatsapp.com/";
                if (!string.IsNullOrEmpty(_pendingProtocolUrl))
                {
                    targetUrl = _pendingProtocolUrl;
                    _pendingProtocolUrl = null;
                }
                WvWhatsApp.CoreWebView2.Navigate(targetUrl);

                // Inicializar la bandeja del sistema
                InitializeTrayIcon();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar WebView2: {ex.Message}\n\nAsegúrate de tener instalado Microsoft Edge WebView2 Runtime.", 
                                "Error de inicialización", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                LoadingPanel.Visibility = Visibility.Collapsed;
                WvWhatsApp.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("No se pudo cargar WhatsApp Web. Comprueba tu conexión a Internet.", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            // Bloquea que el enlace se abra dentro de la app de WhatsApp y lo abre en el navegador web predeterminado
            e.Handled = true;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo abrir el enlace: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new SystemTray.NotifyIcon();
            
            // Cargar el icono desde los recursos incrustados
            try
            {
                var resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/logo-whatsapp-png-46068-Windows.ico"));
                if (resourceStream != null)
                {
                    using (var stream = resourceStream.Stream)
                    {
                        _notifyIcon.Icon = new System.Drawing.Icon(stream);
                    }
                }
                else
                {
                    _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon.Text = "WhatsApp Web Desktop";
            _notifyIcon.Visible = true;
            
            // Doble clic sobre el icono abre/restaura la aplicación
            _notifyIcon.DoubleClick += (s, e) => RestoreWindow();

            // Menú contextual para el icono en la bandeja
            var contextMenu = new SystemTray.ContextMenuStrip();
            
            var openItem = new SystemTray.ToolStripMenuItem("Abrir WhatsApp");
            openItem.Click += (s, e) => RestoreWindow();
            contextMenu.Items.Add(openItem);

            contextMenu.Items.Add(new SystemTray.ToolStripSeparator());

            var exitItem = new SystemTray.ToolStripMenuItem("Salir");
            exitItem.Click += (s, e) =>
            {
                _isExiting = true;
                this.Close();
            };
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void RestoreWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                this.Hide();
                
                if (!_shownBalloonHelp)
                {
                    _notifyIcon?.ShowBalloonTip(3000, 
                        "WhatsApp Web Desktop", 
                        "La aplicación se sigue ejecutando en segundo plano. Haz doble clic en el icono de la bandeja para volver a abrir.", 
                        SystemTray.ToolTipIcon.Info);
                    _shownBalloonHelp = true;
                }
            }
            else
            {
                _notifyIcon?.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClick(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaximizeIcon.Data = Geometry.Parse("M 2,0 L 10,0 L 10,8 M 0,2 L 8,2 L 8,10 L 0,10 Z");
            }
            else
            {
                MaximizeIcon.Data = Geometry.Parse("M 0,0 L 0,10 L 10,10 L 10,0 Z");
            }
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Leer el registro de Windows para ver si el inicio automático está activo
                string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(runKey, false))
                {
                    bool isEnabled = key?.GetValue("WhatsAppLite") != null;
                    ChkAutoStart.IsChecked = isEnabled;
                }
            }
            catch { }
            PopSettings.IsOpen = true;
        }

        private void OnAutoStartClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bool enable = ChkAutoStart.IsChecked == true;
                string runKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
                string exePath = Environment.ProcessPath ?? "";

                if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    string possibleExe = Path.ChangeExtension(exePath, ".exe");
                    if (File.Exists(possibleExe))
                    {
                        exePath = possibleExe;
                    }
                }

                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(runKey, true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            key.SetValue("WhatsAppLite", $"\"{exePath}\" --startup");
                        }
                        else
                        {
                            key.DeleteValue("WhatsAppLite", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cambiar la configuración de inicio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                TxtLoadingStatus.Text = "Buscando actualizaciones...";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("WhatsAppLiteUpdater");
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var response = await client.GetAsync("https://api.github.com/repos/dev-error404/WhatsappWeb_Lite/releases/latest");
                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                        if (release != null)
                        {
                            string cleanTag = release.tag_name.TrimStart('v', 'V', ' ');
                            if (Version.TryParse(cleanTag, out Version? latestVersion))
                            {
                                var currentVersion = new Version("1.0.4");
                                if (latestVersion > currentVersion)
                                {
                                    var asset = release.assets.FirstOrDefault(a => a.name.Equals("WhatsAppWebSetup.exe", StringComparison.OrdinalIgnoreCase));
                                    if (asset != null)
                                    {
                                        var result = MessageBox.Show($"Una nueva versión (v{cleanTag}) está disponible.\n¿Deseas descargar e instalar la actualización ahora?", 
                                                                     "Actualización disponible", MessageBoxButton.YesNo, MessageBoxImage.Information);
                                        if (result == MessageBoxResult.Yes)
                                        {
                                            await DownloadAndInstallUpdateAsync(asset.browser_download_url);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores de conexión y continuar para no bloquear el uso de la aplicación
            }
            finally
            {
                TxtLoadingStatus.Text = "Conectando con WhatsApp Web...";
            }
        }

        private async Task DownloadAndInstallUpdateAsync(string downloadUrl)
        {
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "WhatsAppWebSetup.exe");
                
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("WhatsAppLiteUpdater");
                    
                    using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(tempFile))
                    {
                        long? totalBytes = response.Content.Headers.ContentLength;
                        byte[] buffer = new byte[8192];
                        long totalRead = 0;
                        int bytesRead;

                        Dispatcher.Invoke(() => {
                            LoadProgress.IsIndeterminate = false;
                            LoadProgress.Value = 0;
                        });

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;
                            if (totalBytes.HasValue)
                            {
                                double progress = (double)totalRead / totalBytes.Value * 100;
                                Dispatcher.Invoke(() => {
                                    LoadProgress.Value = progress;
                                    TxtLoadingStatus.Text = $"Descargando actualización: {progress:F0}%";
                                });
                            }
                        }
                    }
                }

                // Ejecutar el instalador actualizado y cerrar la aplicación actual
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });

                _isExiting = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar la actualización: {ex.Message}", "Error de actualización", MessageBoxButton.OK, MessageBoxImage.Error);
                Dispatcher.Invoke(() => {
                    LoadProgress.IsIndeterminate = true;
                });
            }
        }

        private void OnDocumentTitleChanged(object? sender, object e)
        {
            if (WvWhatsApp?.CoreWebView2 == null) return;

            string title = WvWhatsApp.CoreWebView2.DocumentTitle;
            int unreadCount = 0;

            if (!string.IsNullOrEmpty(title))
            {
                int openParens = title.IndexOf('(');
                int closeParens = title.IndexOf(')');
                if (openParens == 0 && closeParens > 0)
                {
                    string countStr = title.Substring(1, closeParens - 1);
                    if (int.TryParse(countStr, out int parsedCount))
                    {
                        unreadCount = parsedCount;
                    }
                }
            }

            UpdateUnreadBadge(unreadCount);
        }

        private void UpdateUnreadBadge(int count)
        {
            // Actualizar el título de la ventana y el texto visual
            string version = "1.0.2"; // Nota: release.ps1 reemplaza esto en tiempo de release
            string baseTitle = $"WhatsApp Lite v{version}";
            string displayTitle = count > 0 ? $"({count}) {baseTitle}" : baseTitle;
            
            this.Title = displayTitle;
            if (TxtTitle != null)
            {
                TxtTitle.Text = displayTitle;
            }

            // Actualizar el overlay del icono en la barra de tareas
            try
            {
                this.TaskbarItemInfo ??= new System.Windows.Shell.TaskbarItemInfo();
                this.TaskbarItemInfo.Overlay = CreateBadgeImage(count);
            }
            catch { }

            // Actualizar el texto del tooltip en el Tray Icon si está activo
            if (_notifyIcon != null)
            {
                try
                {
                    _notifyIcon.Text = count > 0 ? $"WhatsApp Lite ({count} mensajes)" : "WhatsApp Lite";
                }
                catch { }
            }
        }

        private ImageSource? CreateBadgeImage(int count)
        {
            if (count <= 0) return null;

            int size = 16;
            var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                // Círculo rojo de fondo
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)); // Rojo
                var pen = new System.Windows.Media.Pen(new SolidColorBrush(System.Windows.Media.Colors.White), 1);
                context.DrawEllipse(brush, pen, new System.Windows.Point(size / 2.0, size / 2.0), size / 2.0 - 0.5, size / 2.0 - 0.5);

                // Texto del número
                string textStr = count > 99 ? "99+" : count.ToString();
                var formattedText = new FormattedText(
                    textStr,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface(new System.Windows.Media.FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                    textStr.Length > 2 ? 7.0 : 8.5,
                    System.Windows.Media.Brushes.White,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );

                double x = (size - formattedText.Width) / 2.0;
                double y = (size - formattedText.Height) / 2.0;
                context.DrawText(formattedText, new System.Windows.Point(x, y));
            }

            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        public void HandleArguments(string[] args)
        {
            string? whatsappUrl = args.FirstOrDefault(a => a.StartsWith("whatsapp://", StringComparison.OrdinalIgnoreCase));
            
            if (whatsappUrl != null)
            {
                string webUrl = whatsappUrl;
                if (webUrl.StartsWith("whatsapp://", StringComparison.OrdinalIgnoreCase))
                {
                    webUrl = "https://web.whatsapp.com/" + webUrl.Substring("whatsapp://".Length);
                }

                if (WvWhatsApp?.CoreWebView2 != null)
                {
                    RestoreWindow();
                    WvWhatsApp.CoreWebView2.Navigate(webUrl);
                }
                else
                {
                    _pendingProtocolUrl = webUrl;
                }
            }
        }
    }

    public class GitHubRelease
    {
        public string tag_name { get; set; } = "";
        public string html_url { get; set; } = "";
        public GitHubAsset[] assets { get; set; } = Array.Empty<GitHubAsset>();
    }

    public class GitHubAsset
    {
        public string name { get; set; } = "";
        public string browser_download_url { get; set; } = "";
    }
}






