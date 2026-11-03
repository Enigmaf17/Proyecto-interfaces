# Protocolo de comunicación — Juego UNO en red

Este documento es el contrato entre el servidor y los clientes. Si alguien necesita cambiar un mensaje o un campo, primero se avisa al equipo y se actualiza este archivo.

## Reglas generales

- **Conexión:** TCP con `System.Net.Sockets`. El servidor usa `TcpListener` en el **puerto 5000** y los clientes usan `TcpClient`.
- **Formato:** cada mensaje es **un objeto JSON en una sola línea**, terminado en salto de línea (`\n`).
  - Se escribe con `StreamWriter.WriteLineAsync` y se lee con `StreamReader.ReadLineAsync`.
  - Así cada lectura regresa un mensaje completo, aunque TCP lo entregue partido o pegado a otro.
  - `ReadLineAsync` regresa `null` cuando la otra computadora se desconecta.
- **Codificación:** UTF-8 **sin BOM** (`new UTF8Encoding(false)`). Con BOM, el primer mensaje llega con un carácter invisible al inicio y el JSON no se puede leer.
- **Nombres de campos:** en camelCase. Se usa `new JsonSerializerOptions(JsonSerializerDefaults.Web)`, que convierte `TurnoId` en `turnoId` y no distingue mayúsculas al leer.
- **Campos vacíos:** no se envían (`DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull`).
- **El servidor manda:** solo él tiene la `Partida`. Valida cada acción; si no es válida responde `ERROR` y no cambia nada.

## Cómo se nombran las cartas y los colores

- **Carta:** el nombre de su imagen sin `.png`, igual que en la base de datos: `rojo_5`, `azul_salta`, `verde_reversa`, `amarillo_mas2`, `comodin`, `comodin_mas4`. Sale de `carta.NombreImagen().Replace(".png", "")`.
- **Color:** el nombre del enum `ColorCarta`: `Rojo`, `Amarillo`, `Verde`, `Azul`.

## Clase Mensaje

Todos los mensajes usan la misma clase. Solo se llenan los campos que ese tipo necesita.

| Campo | Tipo en C# | Se usa en |
|---|---|---|
| `tipo` | `string` | Todos |
| `nombre` | `string?` | UNIRSE, DESCONEXION |
| `indice` | `int?` | JUGAR |
| `color` | `string?` | JUGAR (solo comodines) |
| `jugadorId` | `int?` | BIENVENIDA |
| `conectados` | `List<string>?` | SALA |
| `vista` | `VistaJugador?` | ESTADO |
| `texto` | `string?` | ERROR |
| `ganador` | `string?` | FIN |
| `estadisticas` | `List<EstadisticasDto>?` | ESTADISTICAS |

## Mensajes del cliente al servidor

| Tipo | Cuándo | Ejemplo |
|---|---|---|
| UNIRSE | Justo después de conectarse | `{"tipo":"UNIRSE","nombre":"Rosa"}` |
| JUGAR | Al tirar una carta. `indice` es la posición de la carta en `miMano` (empieza en 0). `color` solo en comodines | `{"tipo":"JUGAR","indice":3,"color":"Azul"}` |
| ROBAR | Al dar clic en el mazo | `{"tipo":"ROBAR"}` |
| PASAR | Después de robar una carta jugable y decidir no tirarla | `{"tipo":"PASAR"}` |
| UNO | En tu turno, antes de tirar tu penúltima carta | `{"tipo":"UNO"}` |
| HISTORIAL | Al abrir la ventana de historial | `{"tipo":"HISTORIAL"}` |

## Mensajes del servidor al cliente

| Tipo | Cuándo | Ejemplo |
|---|---|---|
| BIENVENIDA | Al aceptar el UNIRSE | `{"tipo":"BIENVENIDA","jugadorId":2}` |
| SALA | Cada vez que alguien entra a la sala de espera | `{"tipo":"SALA","conectados":["Paul","Rosa"]}` |
| ESTADO | Al iniciar la partida y después de cada movimiento. Cada jugador recibe el suyo | ver abajo |
| ERROR | Una acción no fue válida | `{"tipo":"ERROR","texto":"No es tu turno"}` |
| ESTADISTICAS | Respuesta a HISTORIAL | `{"tipo":"ESTADISTICAS","estadisticas":[{"id":1,"nombre":"Paul","ganadas":3,"perdidas":2}]}` |
| FIN | Alguien se quedó sin cartas | `{"tipo":"FIN","ganador":"Rosa"}` |
| DESCONEXION | Un jugador se desconectó; la partida se cancela | `{"tipo":"DESCONEXION","nombre":"Dalton"}` |

## VistaJugador (lo que viaja dentro de ESTADO)

Es lo que ve **un** jugador: su propia mano completa y solo cuántas cartas tienen los demás.

| Campo | Tipo en C# | Significado |
|---|---|---|
| `miId` | `int` | Id del jugador que recibe el mensaje |
| `miMano` | `List<string>` | Sus cartas, en orden |
| `jugadores` | `List<JugadorVisible>` | Los 3 jugadores en orden de turno: `id`, `nombre`, `cartas` (cuántas tiene) |
| `cartaArriba` | `string` | La carta de arriba de la pila de descarte |
| `colorActual` | `string` | El color que se debe seguir (puede venir de un comodín) |
| `turnoId` | `int` | Id del jugador en turno |
| `sentidoHorario` | `bool` | Sentido del juego |
| `cartasEnMazo` | `int` | Cartas que quedan en el mazo |
| `cartaRobada` | `string?` | Solo para quien acaba de robar una carta jugable: debe tirarla o pasar |
| `ultimoMovimiento` | `string?` | Texto para mostrar, por ejemplo `"Dalton jugó rojo_7"` |

Ejemplo de lo que recibe Rosa:

```json
{"tipo":"ESTADO","vista":{"miId":2,"miMano":["rojo_5","azul_salta","comodin"],"jugadores":[{"id":1,"nombre":"Paul","cartas":6},{"id":2,"nombre":"Rosa","cartas":3},{"id":3,"nombre":"Dalton","cartas":4}],"cartaArriba":"rojo_7","colorActual":"Rojo","turnoId":2,"sentidoHorario":true,"cartasEnMazo":71,"ultimoMovimiento":"Dalton jugó rojo_7"}}
```

## Flujo de una partida

1. **Sala de espera.** Cada cliente se conecta y manda `UNIRSE` con su nombre. El nombre debe ser uno de los jugadores de la base de datos (`GET /jugadores`) y no estar ya conectado. El servidor responde `BIENVENIDA` a ese cliente y manda `SALA` a todos.
2. **Inicio.** Con 3 jugadores conectados, el servidor llama `ControladorPartida.IniciarAsync` y manda un `ESTADO` a cada uno.
3. **Turnos.** Solo el jugador con `turnoId` puede mandar `JUGAR`, `ROBAR`, `PASAR` o `UNO`. A los demás se les responde `ERROR`.
4. **Robar.**
   - Si la carta robada **no** se puede jugar, el servidor pasa el turno solo (`PasarTurnoAsync`).
   - Si **sí** se puede jugar, el servidor manda el `ESTADO` con `cartaRobada`. El jugador solo puede tirar esa carta (está al final de `miMano`) o mandar `PASAR`.
5. **Después de cada movimiento**, el servidor manda un `ESTADO` nuevo a los 3 clientes.
6. **Fin.** Cuando hay ganador, el servidor manda el último `ESTADO` y luego `FIN`. `ControladorPartida` ya guarda el resultado en la base de datos.
7. **Desconexión.** Si un cliente se cae, el servidor manda `DESCONEXION` a los demás y la partida se cancela.

## Errores que puede mandar el servidor

| Situación | `texto` |
|---|---|
| Acción fuera de turno | `No es tu turno` |
| Carta que no coincide | `Esa carta no se puede jugar` |
| Comodín sin color | `Elige un color para el comodín` |
| Después de robar, intenta tirar otra carta | `Solo puedes tirar la carta que robaste o pasar` |
| PASAR sin haber robado | `Primero roba una carta` |
| Nombre que no existe o ya está conectado | `Ese jugador no existe o ya está conectado` |
| Mensaje que no se pudo leer | `Mensaje no válido` |
