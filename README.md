<img src="logo-whatsapp-png-46068.png" width="90" height="90" align="right" alt="WhatsApp Lite Logo" />

# WhatsApp Lite

Un contenedor (wrapper) de escritorio ultra ligero y de alto rendimiento para [WhatsApp Web](https://web.whatsapp.com) desarrollado en C# y WPF utilizando el motor nativo Microsoft Edge WebView2 (Chromium).

**WhatsApp Lite** se compila en un ejecutable autónomo de apenas **1.2 MB**, consume una fracción de la memoria RAM de clientes pesados basados en Electron y está diseñado para ofrecer una experiencia fluida, evitando los retrasos al escribir y en llamadas que suelen ocurrir en el cliente oficial de la Microsoft Store.

---

## Características Principales

- ⚡ **Ultra ligero**: El ejecutable del programa ocupa solo 1.2 MB.
- 🚀 **Motor Chromium**: Utiliza WebView2 (el motor de Microsoft Edge basado en Chromium), garantizando máxima compatibilidad y velocidad.
- 🎮 **Aceleración por hardware desactivada**: Configurado con la directiva de renderizado `--disable-gpu` por defecto para evitar parpadeos de pantalla o incompatibilidades de controladores gráficos.
- 📱 **Camuflaje de dispositivo**: Modifica internamente el `User-Agent` para identificarse limpiamente como **Google Chrome (Windows)** en la lista de dispositivos vinculados de tu teléfono celular.
- 🔑 **Sesión persistente**: Guarda de forma segura las cookies y la base de datos local en `%APPDATA%\WhatsAppWebDesktop\User Data` para que solo tengas que escanear el código QR una vez.
- 🔗 **Gestión inteligente de enlaces**: Los enlaces externos de Internet que presiones en los chats se abrirán automáticamente en tu navegador predeterminado (como Chrome o Edge) para no perder tu pantalla de WhatsApp.
- 📥 **Actualizaciones automáticas**: Al abrirse, la aplicación consulta asíncronamente los releases de GitHub. Si hay una versión superior disponible, te ofrecerá descargarla e instalarla automáticamente al instante.
- ⚙️ **Inicio automático configurable**: Incorpora un botón de configuración (engranaje) en la barra de título donde puedes activar o desactivar el inicio automático con Windows. Al arrancar con el sistema, se inicia minimizado en segundo plano.
- 🔔 **Bandeja del sistema (Tray)**: Al hacer clic en cerrar ($\times$), la aplicación se oculta al lado del reloj del sistema. Sigue activa en segundo plano para recibir notificaciones y se restaura rápidamente haciendo doble clic sobre su icono.

---

## Compilación Manual

### Requisitos previos
- **.NET 10.0 SDK** (o superior)
- **WebView2 Runtime** (preinstalado en Windows 10 y 11)

### Pasos para compilar la aplicación

Para generar el ejecutable optimizado en un solo archivo, abre la terminal en la carpeta raíz del proyecto y ejecuta:

```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false
```

El archivo ejecutable compilado se generará en la siguiente ruta:
`bin\Release\net10.0-windows\win-x64\publish\WhatsAppWebDesktop.exe`

---

## Descargo de Responsabilidad (Aviso Legal)

> [!IMPORTANT]
> **Este es un cliente no oficial de código abierto.**
> 
> **WhatsApp Lite** es un proyecto independiente de desarrollo de software y no está afiliado, autorizado, patrocinado, respaldado ni conectado de ninguna manera con **WhatsApp LLC**, **Meta Platforms, Inc.** ni ninguna de sus filiales o subsidiarias. El sitio oficial de WhatsApp se encuentra en [https://www.whatsapp.com](https://www.whatsapp.com).
> 
> - **Sin modificaciones en el servidor**: Esta aplicación no modifica, intercepta, descifra ni altera los servidores o las comunicaciones de la plataforma de mensajería.
> - **Contenedor Web**: Funciona exclusivamente como un visor (wrapper) de la página web oficial y sin modificaciones de [WhatsApp Web](https://web.whatsapp.com/).
> - **Cumplimiento**: El software se proporciona "tal cual" bajo la licencia MIT. Es responsabilidad del usuario final hacer un uso de esta herramienta conforme a las Condiciones de Servicio de la plataforma oficial.
