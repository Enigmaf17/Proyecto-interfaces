# Juego UNO

Juego de cartas UNO para 3 jugadores en red, desarrollado en C# con interfaz gráfica en Windows Forms. Cada jugador juega desde su propia computadora y las computadoras se comunican con **sockets TCP**. Todas las partidas y movimientos se guardan en una base de datos MySQL a través de una API hecha en Python con FastAPI.

Proyecto de la materia de Interfaces, Facultad de Ingeniería, UASLP.

Una de las 3 computadoras es al mismo tiempo **servidor y cliente**: corre el servidor del juego y también juega, conectándose a sí misma. Las otras dos solo son clientes.

## Equipo

|Integrante|Primera parte (juego local)|Segunda parte (juego en red)|
|-|-|-|
|Daniel Paul De León Lozano|Administración del repositorio, reglas del juego (`Partida`), API, `ApiCliente`, `ControladorPartida`, imágenes, pantalla de carga e historial|Protocolo, programa de prueba de red, `Servidor`, `ClienteRed` e integración de las pantallas con la red (`SesionRed`)|
|Rosa Itsel Jaramillo Garcia|Interfaz gráfica de la mesa, conexión con la base de datos y README|Pantalla de inicio (`FormInicio`) y mesa en red (`FormMesa`)|
|Dalton Vega Juarez|Clases `Carta`, `Mazo` y `Jugador`, script de la base de datos y diagrama entidad-relación|Mensajes del protocolo (`Mensaje`, `Protocolo`, `TiposMensaje`) y lo que ve cada jugador (`VistaJugador`)|

## Tecnologías

* **C# / .NET 10** con Windows Forms (interfaz) y bibliotecas de clases (lógica y red)
* **Sockets TCP** con `System.Net.Sockets` (`TcpListener` y `TcpClient`)
* **xUnit** para pruebas automáticas
* **Python 3.11+** con **FastAPI** y **uvicorn** (API)
* **MySQL** (base de datos)

## Estructura del repositorio

```
Proyecto-interfaces/
├── UnoLogica/        Reglas del juego (biblioteca de clases C#)
│   ├── Carta.cs, Mazo.cs, Jugador.cs, Partida.cs
│   ├── ApiCliente.cs           Se comunica con la API por HTTP
│   └── ControladorPartida.cs   Une las reglas con el registro en la base de datos
├── UnoRed/           Comunicación en red (biblioteca de clases C#)
│   ├── Servidor.cs             Servidor TCP: valida y aplica cada jugada
│   ├── ClienteRed.cs           Conexión de cada jugador con el servidor
│   ├── Mensaje.cs, TiposMensaje.cs, Protocolo.cs   Mensajes en JSON
│   └── VistaJugador.cs         Lo que ve cada jugador
├── UnoUI/            Interfaz gráfica (Windows Forms)
│   ├── FormInicio.cs           Crear partida o unirse
│   ├── FormMesa.cs             Mesa del juego en red
│   ├── SesionRed.cs            Conecta las pantallas con la red
│   └── Imagenes/               55 imágenes de las cartas
├── UnoPruebas/       Pruebas automáticas (xUnit)
├── PruebaRed/        Programa de consola para probar la conexión entre computadoras
├── docs/
│   └── protocolo.md  Mensajes entre el cliente y el servidor
├── api/              API en Python (FastAPI)
├── database/
│   └── schema.sql    Script para crear la base de datos
└── UnoLogica.slnx    Solución de Visual Studio
```

## Cómo funciona la red

```
Computadora servidor:   FormMesa ↔ ClienteRed ──TCP (127.0.0.1)──┐
                                                                 ├─→ Servidor → ControladorPartida → ApiCliente → API → MySQL
Otras computadoras:     FormMesa ↔ ClienteRed ──TCP (IP)─────────┘
```

* El **servidor** es el único que tiene la partida. Recibe las acciones de los jugadores, revisa que sea su turno y que la jugada sea válida, la aplica con `ControladorPartida` y la guarda en MySQL a través de la API.
* Después de cada movimiento, el servidor le manda a **cada** jugador lo que le toca ver: su propia mano completa y solo cuántas cartas tienen los demás.
* Cada mensaje es un objeto **JSON en una sola línea**. Así cada lectura del socket regresa un mensaje completo, aunque TCP lo entregue partido. La lista de mensajes está en [`docs/protocolo.md`](docs/protocolo.md).
* Solo la computadora servidor necesita la API y MySQL. Los clientes solo necesitan el juego.

## Requisitos

**En las 3 computadoras:**
* Visual Studio con soporte para .NET 10 y la carga de trabajo de **desarrollo de escritorio con .NET**
* Estar conectadas a la **misma red**. La red de la universidad bloquea la comunicación entre computadoras, así que usamos la **zona Wi-Fi de un teléfono**.

**Solo en la computadora servidor:**
* Python 3.11 o más reciente (al instalarlo, marcar **"Add python.exe to PATH"**)
* MySQL Server y MySQL Workbench

## Cómo ejecutar el proyecto

### 1. Clonar el repositorio (las 3 computadoras)

En Visual Studio: **Git > Clonar repositorio** y pegar la URL del repositorio. Después, verificar que **UnoUI** sea el proyecto de inicio (clic derecho > Establecer como proyecto de inicio).

### 2. Preparar la computadora servidor

**Base de datos:** abrir MySQL Workbench, abrir `database/schema.sql` y ejecutarlo (botón del rayo). Esto crea la base `uno` con sus tablas e inserta a los 3 jugadores.

**API:** en una terminal, dentro de la carpeta `api`:

```
pip install -r requirements.txt
copy config.example.py config.py
```

Abrir `config.py` y poner la contraseña de MySQL de esa computadora. Este archivo **no se sube a GitHub** porque contiene la contraseña. Después, prender la API y dejar la terminal abierta:

```
uvicorn main:app --reload
```

Para comprobar que funciona, abrir http://127.0.0.1:8000/docs en el navegador y probar `GET /jugadores`.

**Red:** conectar la computadora a la zona Wi-Fi y marcar esa red como **Privada** (Configuración > Red e Internet > Wi-Fi > la red > Tipo de perfil de red).

### 3. Jugar

1. **Computadora servidor:** presionar **F5**, elegir el nombre y dar **Crear partida**. La pantalla muestra la IP de la computadora. Si Windows pregunta por el firewall, permitir el acceso en **redes privadas**.
2. **Las otras dos computadoras:** presionar **F5**, elegir su nombre, escribir la IP del servidor y dar **Unirse**.
3. Cuando los 3 jugadores están en la sala de espera, la partida empieza sola.

Para probar en una sola computadora: abrir 3 ventanas (clic derecho en UnoUI > Depurar > Iniciar nueva instancia), crear la partida en una y unirse desde las otras dos con la IP `127.0.0.1`.

### Si un cliente no se puede conectar

* Revisar que las 3 computadoras estén en la misma zona Wi-Fi y que la IP sea la que muestra la pantalla del servidor.
* Si el firewall del servidor sigue bloqueando, abrir PowerShell **como administrador** en la computadora servidor y ejecutar:

```
New-NetFirewallRule -DisplayName "UNO servidor" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

* Para revisar solo la red, sin el juego, se puede usar el proyecto **PruebaRed**: en el servidor se elige la opción 1 y en los clientes la opción 2.

## Protocolo

|Del cliente al servidor|Del servidor al cliente|
|-|-|
|`UNIRSE`, `JUGAR`, `ROBAR`, `PASAR`, `UNO`, `HISTORIAL`|`BIENVENIDA`, `SALA`, `ESTADO`, `ERROR`, `ESTADISTICAS`, `FIN`, `DESCONEXION`|

El detalle de cada mensaje, con ejemplos en JSON, está en [`docs/protocolo.md`](docs/protocolo.md). El servidor escucha en el **puerto 5000**.

## Base de datos

|Tabla|Contenido|
|-|-|
|`jugadores`|Id y nombre de cada jugador|
|`partidas`|Fecha de inicio, fecha de fin y ganador de cada partida|
|`partida_jugador`|Qué jugadores participaron en cada partida y si la ganaron o la perdieron|
|`movimientos`|Log del juego: cada jugada, robo, "UNO" y castigo, con su turno y fecha|

## API

|Método|Ruta|Uso|
|-|-|-|
|GET|`/jugadores`|Lista de jugadores|
|POST|`/partidas`|Crea una partida y devuelve su id|
|POST|`/partidas/{id}/movimientos`|Guarda un movimiento en el log|
|GET|`/partidas/{id}/movimientos`|Consulta el log de una partida|
|PUT|`/partidas/{id}/finalizar`|Guarda al ganador y el resultado de cada jugador|
|GET|`/jugadores/{id}/estadisticas`|Partidas ganadas y perdidas de un jugador|

La documentación interactiva está en http://127.0.0.1:8000/docs cuando la API está prendida.

## Pruebas

En Visual Studio: **Prueba > Explorador de pruebas > Ejecutar todas**.

Las pruebas de `ApiClienteTests` y `ControladorPartidaTests` se conectan a la API real, así que **solo pasan si la API está prendida**. Las demás pruebas funcionan sin la API.

## Reglas implementadas

* Mazo oficial de 108 cartas; se reparten 7 cartas a cada jugador.
* Una carta se puede jugar si coincide en color, en número o en tipo con la carta de arriba, o si es comodín.
* **Salta**: el siguiente jugador pierde su turno.
* **Reversa**: cambia el sentido del juego.
* **+2**: el siguiente jugador roba 2 cartas y pierde su turno.
* **Comodín**: el jugador elige el nuevo color.
* **Comodín +4**: el jugador elige color y el siguiente roba 4 cartas y pierde su turno.
* **UNO**: el jugador debe decir "UNO" en su turno, antes de tirar su penúltima carta; si no lo hace, roba 2 cartas de castigo.
* Al robar, si la carta se puede jugar, el jugador puede tirarla o pasar; si no, pierde el turno.
* Cuando el mazo se acaba, se rellena con la pila de descarte (menos la carta de arriba) y se baraja.
* Gana el primer jugador que se queda sin cartas.
* Si un jugador se desconecta, la partida se cancela y se avisa a los demás.

### Decisiones de diseño

* La primera carta de la mesa siempre es de número. Si al voltearla sale una carta especial, se deja abajo en la pila de descarte y se voltea otra.
* El Comodín +4 se puede jugar en cualquier momento, sin la restricción de no tener cartas del color actual.

## Flujo de trabajo con Git

* `master` es la versión estable y solo recibe cambios por Pull Request revisado.
* Cada integrante trabaja en sus propias ramas. Primera parte: `reglas`, `api`, `controlador`, `imagenes`, `historial`, `cartas-mazo`, `base-datos`, `interfaz`, `documentacion`. Segunda parte: `protocolo`, `servidor`, `cliente-red`, `integracion-red`, `mensajes`, `vista-jugador`, `pantalla-inicio`, `mesa-red`.
* Antes de trabajar se trae lo más reciente de `master` a la rama propia (merge de `origin/master`).
* Commits pequeños y frecuentes, uno por cada avance que funcione.
