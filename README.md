<p align="center">
  <img src="logo-whatsapp-png-46068.png" width="110" height="110" alt="WhatsApp Lite Logo" />
</p>

<h1 align="center">WhatsApp Lite</h1>

<p align="center">
  <img src="https://readme-typing-svg.demolab.com?font=Segoe+UI&weight=600&size=18&duration=3500&pause=1000&color=00A884&center=true&vCenter=true&width=600&height=40&lines=Contenedor+de+escritorio+ultra+ligero;Desarrollado+en+C%23+y+WPF+nativo+para+Windows;Consumo+m%C3%ADnimo+de+RAM+y+CPU;Sin+las+sobrecargas+de+Electron+ni+bloqueos" alt="Typing SVG" />
</p>

<p align="center">
  <a href="https://github.com/dev-error404/WhatsappWeb_Lite/releases/latest">
    <img src="https://img.shields.io/github/v/release/dev-error404/WhatsappWeb_Lite?style=for-the-badge&color=00A884&label=Latest%20Version" alt="Latest Release" />
  </a>&nbsp;
  <a href="https://github.com/dev-error404/WhatsappWeb_Lite/blob/main/LICENSE">
    <img src="https://img.shields.io/badge/License-MIT-0078D4?style=for-the-badge" alt="License MIT" />
  </a>&nbsp;
  <img src="https://img.shields.io/badge/Platform-Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white" alt="Platform Windows" />
</p>

---

## 📖 Descripción

**WhatsApp Lite** es un wrapper de escritorio ultra optimizado y ligero para [WhatsApp Web](https://web.whatsapp.com) desarrollado en C# y WPF. Utiliza el motor nativo **Microsoft Edge WebView2 (Chromium)** del sistema operativo, eliminando la sobrecarga que implican los contenedores basados en Electron.

El ejecutable principal ocupa solo **1.2 MB**, consumiendo un mínimo de memoria RAM y CPU. Está diseñado específicamente para evitar los molestos retardos en la escritura y congelamientos visuales que suelen experimentarse en el cliente oficial de la Microsoft Store.

---

## ⚡ Tecnologías y Herramientas (Tech Stack)

<p align="center">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#" />&nbsp;
  <img src="https://img.shields.io/badge/.NET%2010.0-512BD4?style=for-the-badge&logo=.net&logoColor=white" alt=".NET 10.0" />&nbsp;
  <img src="https://img.shields.io/badge/WPF-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="WPF" />&nbsp;
  <img src="https://img.shields.io/badge/WebView2-0078D4?style=for-the-badge&logo=microsoft-edge&logoColor=white" alt="WebView2" />&nbsp;
  <img src="https://img.shields.io/badge/Windows%20Registry-0078D4?style=for-the-badge&logo=windows&logoColor=white" alt="Windows Registry" />
</p>

---

## 🚀 Características Principales

### 🖥️ Integración con Windows
* **Manejo del Protocolo `whatsapp://`**: Asocia la aplicación con enlaces protocolares externos (`api.whatsapp.com/send` o `wa.me`). Al hacer clic en un chat desde el navegador, este se abre directamente dentro de la aplicación.
* **Comunicación Local por Sockets TCP**: Utiliza sockets TCP locales en el puerto `49600` para comunicación IPC (Inter-Process Communication). Esto evita bloqueos de Windows UAC cuando el navegador se ejecuta con permisos estándar y la app principal está elevada como Administrador.
* **Seguimiento en la Bandeja del Sistema (Tray Icon)**: Minimiza al reloj del sistema al presionar cerrar ($\times$), manteniendo notificaciones activas y restaurándose de forma instantánea al hacer doble clic.
* **Arranque con el Sistema**: Opción integrada para iniciar de forma automática con Windows (oculto en segundo plano).

### 🎨 Experiencia de Usuario Pulida
* **Badge de Notificaciones Dinámico**: Dibuja en tiempo real un globo con la cantidad de mensajes no leídos sobre el icono de la barra de tareas y el tooltip del reloj del sistema.
* **Acceso Directo a Descargas**: Botón directo de descargas (flecha hacia abajo) en la barra de título que invoca el diálogo de descargas nativo de WebView2 de forma transparente.
* **Tema Oscuro Integrado**: Interfaz adaptada al tema oscuro de WhatsApp con diseño sin bordes de ventana y soporte completo para Snap Layouts en Windows 11.
* **Enlaces Externos Aislados**: Los links externos compartidos dentro del chat se abren automáticamente en el navegador predeterminado para evitar perder la sesión de WhatsApp Web.

### 🔒 Rendimiento y Privacidad
* **Desactivación de Renderizado por GPU**: Utiliza la directiva `--disable-gpu` para evitar fallos y parpadeos gráficos comunes en WebView2.
* **Persistencia de Sesión Segura**: Almacena de forma dedicada cookies e historial local en `%APPDATA%\WhatsAppWebDesktop\User Data`.
* **Identificación como Google Chrome**: Se camufla bajo el User-Agent de Google Chrome en Windows para evitar alertas de navegador no compatible.
* **Actualizador Integrado**: Verifica nuevos lanzamientos a través de la API de GitHub de manera asíncrona al arrancar el sistema.

---

## 🛠️ Instalación y Compilación

### Compilación Manual
Asegúrate de contar con el **.NET 10.0 SDK** instalado. Abre la terminal en el directorio raíz del proyecto y ejecuta:

```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false
```

El instalador final y el ejecutable optimizado autónomo se encontrarán en las carpetas de salida:
- **Ejecutable compilado**: `bin\Release\net10.0-windows\win-x64\publish\WhatsAppWebDesktop.exe`
- **Asistente de instalación**: `WhatsAppWebSetup.exe` (compilado desde el subproyecto `installer/`).

---

## ⚖️ Descargo de Responsabilidad (Aviso Legal)

> [!IMPORTANT]
> **Este es un cliente alternativo no oficial de código abierto.**
> 
> **WhatsApp Lite** es un software independiente desarrollado por la comunidad y no tiene afiliación, patrocinio, autorización ni ningún tipo de vínculo comercial o legal con **WhatsApp LLC**, **Meta Platforms, Inc.** o cualquiera de sus filiales.
> 
> - **Sin alteraciones de servidor**: Este software funciona estrictamente como un visor web aislado para la plataforma oficial [WhatsApp Web](https://web.whatsapp.com/). No interactúa, altera ni guarda bases de datos en servidores externos.
> - **Licencia**: Distribuido bajo la Licencia MIT. El usuario es responsable del uso conforme a los Términos de Servicio de la plataforma oficial.
