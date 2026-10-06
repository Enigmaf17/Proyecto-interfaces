# Juego UNO

Juego de cartas UNO para 3 jugadores, desarrollado en C# con interfaz gráfica en Windows Forms. Todas las partidas y movimientos se guardan en una base de datos MySQL a través de una API hecha en Python con FastAPI.

Proyecto de la materia de Interfaces, Facultad de Ingeniería, UASLP.

En esta versión el juego corre en una sola computadora y una sola persona controla a los 3 jugadores, por eso las tres manos se muestran boca arriba. En una versión posterior se jugará en red (esquema host-guest).

## Equipo

|Integrante|Responsabilidad|
|-|-|
|Daniel Paul|Administración del repositorio, reglas del juego (`Partida`), API, `ApiCliente`, `ControladorPartida` e imágenes de las cartas|
|Rosa Jaramillo|Interfaz gráfica (`UnoUI`)|
|Dalton Vega|Clases `Carta`, `Mazo` y `Jugador`, script de la base de datos y diagrama entidad-relación|

## Tecnologías

* **C# / .NET 10** con Windows Forms (interfaz) y una biblioteca de clases (lógica)
* **xUnit** para pruebas automáticas
* **Python 3.11+** con **FastAPI** y **uvicorn** (API)
* **MySQL** (base de datos)

## Estructura del repositorio

```
Proyecto-interfaces/
├── UnoLogica/        Reglas del juego (biblioteca de clases C#)
│   ├── Carta.cs
│   ├── Mazo.cs
│   ├── Jugador.cs
│   ├── Partida.cs
│   ├── ApiCliente.cs           Se comunica con la API por HTTP
│   └── ControladorPartida.cs   Une las reglas con el registro en la base de datos
├── UnoUI/            Interfaz gráfica (Windows Forms)
│   └── Imagenes/     55 imágenes de las cartas
├── UnoPruebas/       Pruebas automáticas (xUnit)
├── api/              API en Python (FastAPI)
│   ├── main.py
│   ├── config.example.py
│   └── requirements.txt
├── database/
│   └── schema.sql    Script para crear la base de datos
└── UnoLogica.slnx    Solución de Visual Studio
```

El juego en C# nunca se conecta directo a MySQL. El flujo es:

```
UnoUI  →  ControladorPartida  →  Partida (reglas)
                              →  ApiCliente  →  API FastAPI  →  MySQL
```

## Requisitos

* Visual Studio con soporte para .NET 10 y la carga de trabajo de **desarrollo de escritorio con .NET**
* Python 3.11 o más reciente (al instalarlo, marcar **"Add python.exe to PATH"**)
* MySQL Server y MySQL Workbench

## Cómo ejecutar el proyecto

### 1\. Clonar el repositorio

En Visual Studio: **Git > Clonar repositorio** y pegar la URL del repositorio.

### 2\. Crear la base de datos

1. Abrir MySQL Workbench y conectarse al servidor local.
2. Abrir el archivo `database/schema.sql` y ejecutarlo (botón del rayo).

Esto crea la base `uno` con sus tablas e inserta a los 3 jugadores. Se puede ejecutar más de una vez sin problema.

### 3\. Prender la API

En una terminal, dentro de la carpeta `api`:

```
python -m venv venv
venv\\Scripts\\activate
pip install -r requirements.txt
copy config.example.py config.py
```

Abrir `config.py` y poner la contraseña de MySQL de tu computadora. Este archivo **no se sube a GitHub** porque contiene la contraseña.

Después, prender la API:

```
uvicorn main:app --reload
```

Para comprobar que funciona, abrir http://127.0.0.1:8000/docs en el navegador y probar `GET /jugadores`.

> Si `venv\\Scripts\\activate` marca un error de "scripts deshabilitados", ejecutar una sola vez:
> `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`

### 4\. Abrir el juego

1. Abrir `UnoLogica.slnx` en Visual Studio.
2. Verificar que **UnoUI** sea el proyecto de inicio (clic derecho > Establecer como proyecto de inicio).
3. Presionar **F5**.

**La API debe estar prendida mientras se juega**, porque cada movimiento se guarda en la base de datos.

## Base de datos

|Tabla|Contenido|
|-|-|
|`jugadores`|Id y nombre de cada jugador|
|`partidas`|Fecha de inicio, fecha de fin y ganador de cada partida|
|`partida\_jugador`|Qué jugadores participaron en cada partida y si la ganaron o la perdieron|
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
* **UNO**: el jugador debe decir "UNO" antes de tirar su penúltima carta; si no lo hace, roba 2 cartas de castigo.
* Al robar, si la carta se puede jugar, el jugador puede tirarla; si no, pasa el turno.
* Cuando el mazo se acaba, se rellena con la pila de descarte (menos la carta de arriba) y se baraja.
* Gana el primer jugador que se queda sin cartas.

### Decisiones de diseño

* La primera carta de la mesa siempre es de número. Si al voltearla sale una carta especial, se deja abajo en la pila de descarte y se voltea otra.
* El Comodín +4 se puede jugar en cualquier momento, sin la restricción de no tener cartas del color actual.

## Flujo de trabajo con Git

* `master` es la versión estable y solo recibe cambios por Pull Request revisado.
* Cada integrante trabaja en su propia rama: `reglas`, `api`, `controlador`, `imagenes`, `cartas-mazo`, `base-datos`, `interfaz`.
* Antes de trabajar se trae lo más reciente de `master` a la rama propia (merge de `master`).
* Commits pequeños y frecuentes, uno por cada avance que funcione.

