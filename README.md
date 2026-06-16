# WhatsApp Lite

Un contenedor de escritorio ultra ligero para **WhatsApp Web** desarrollado en C# y WPF que utiliza el motor nativo **Microsoft Edge WebView2 (Chromium)**.

A diferencia del cliente oficial de la Microsoft Store (que suele sufrir de retardos en la escritura y llamadas) y de los wrappers tradicionales basados en Electron (que consumen cientos de megabytes de memoria RAM), **WhatsApp Lite** compila en un único ejecutable de solo **1.2 MB** y está optimizado para consumir el mínimo de recursos del sistema.

---

## ¿Por qué WhatsApp Lite?

La aplicación oficial de escritorio de WhatsApp y los contenedores web habituales tienen dos problemas principales: alto consumo de recursos y lentitud en equipos de gama media. Este proyecto soluciona ambos problemas encapsulando la web oficial dentro de WebView2 de Microsoft, aprovechando la aceleración y compatibilidad de Chromium sin la sobrecarga de un navegador completo independiente.

---

## Características Destacadas

### Integración con el Sistema (Windows)
* **Asociación del protocolo `whatsapp://`**: Soporta enlaces de redirección externa (como `api.whatsapp.com/send` o `wa.me`). Al hacer clic en un enlace de chat en navegadores externos, Windows sugerirá abrirlo con WhatsApp Lite y lo enviará directamente al chat de la instancia activa.
* **Control de Instancia Única vía Sockets TCP**: Si intentas abrir otra instancia de la aplicación (o haces clic en un enlace web `whatsapp://`), la nueva instancia le transfiere los argumentos de forma segura a la principal a través de un puerto local de comunicación (`49600`), levantando la ventana y navegando al chat sin abrir ventanas repetidas.
* **Minimizar a la Bandeja del Sistema (Tray)**: Al cerrar la ventana, la aplicación se oculta al área de notificación al lado del reloj. Sigue recibiendo notificaciones en segundo plano y se restaura al instante haciendo doble clic.
* **Inicio Automático Configurable**: Permite habilitar desde el menú de opciones que el programa inicie automáticamente junto con Windows de forma oculta en segundo plano.

### Experiencia de Usuario (UX)
* **Contador de Mensajes Pendientes (Badge)**: Dibuja dinámicamente un indicador con el conteo de mensajes no leídos sobre el icono del programa en la barra de tareas y actualiza el texto de la bandeja del sistema.
* **Acceso Directo a Descargas**: Incluye un botón dedicado en la barra de título que despliega el historial y gestor de descargas nativo del WebView2, evitando tener que usar atajos de teclado complejos.
* **Ventana Oscura Sin Bordes**: Interfaz moderna integrada con el sistema operativo que incluye controles personalizados y soporte nativo para los Snap Layouts de Windows 11.
* **Gestión de Enlaces Externos**: Todos los enlaces web externos compartidos en los chats se abren automáticamente en el navegador predeterminado del sistema operativo para no interrumpir tu sesión de chat.

### Optimización y Privacidad
* **Aceleración por Hardware Desactivada**: Configurado con renderizado seguro por CPU (`--disable-gpu`) para evitar parpadeos visuales típicos de WebView2.
* **Sesión Persistente**: Los datos de perfil, cookies de inicio de sesión y caché se almacenan localmente en `%APPDATA%\WhatsAppWebDesktop\User Data`.
* **Identificación Limpia (User-Agent)**: Se identifica ante los servidores de WhatsApp como **Google Chrome (Windows)**, evitando restricciones de acceso y apareciendo de forma clara en la lista de dispositivos vinculados en tu móvil.
* **Actualización Asíncrona**: Consulta periódica a la API de GitHub para verificar nuevos lanzamientos publicados y permite actualizar el ejecutable de forma automatizada.

---

## Compilación e Instalación

### Requisitos
* .NET 10.0 SDK o superior.
* Microsoft Edge WebView2 Runtime (incluido por defecto en Windows 10 y 11).

### Compilar desde la consola
Para realizar una compilación optimizada en un único archivo ejecutable libre de dependencias externas:

```powershell
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=false
```

El ejecutable resultante estará en la carpeta:
`bin\Release\net10.0-windows\win-x64\publish\WhatsAppWebDesktop.exe`

---

## Descargo de Responsabilidad (Aviso Legal)

> [!IMPORTANT]  
> **Este es un cliente no oficial de código abierto.**
> 
> **WhatsApp Lite** es un proyecto de desarrollo independiente y no tiene afiliación, patrocinio, autorización ni ningún tipo de vínculo comercial con **WhatsApp LLC**, **Meta Platforms, Inc.** o cualquiera de sus filiales.
> 
> - **Sin alteraciones de servidor**: Este software actúa estrictamente como un visor web aislado para la plataforma oficial [WhatsApp Web](https://web.whatsapp.com/). No intercepta, altera ni almacena mensajes.
> - **Responsabilidad**: La aplicación se distribuye bajo la licencia MIT. El usuario es responsable de utilizar esta herramienta de conformidad con las condiciones y términos del servicio oficial de WhatsApp.
