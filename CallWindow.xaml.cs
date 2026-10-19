using System;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;

namespace WhatsAppWebDesktop
{
    /// <summary>
    /// Ventana dedicada para las llamadas emergentes de WhatsApp Web (/call/popout)
    /// </summary>
    public partial class CallWindow : Window
    {
        public CallWindow()
        {
            InitializeComponent();
        }

        public WebView2 WebView => WvCall;
    }
}
