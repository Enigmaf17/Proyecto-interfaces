from fastapi import FastAPI
import mysql.connector
from config import DB_CONFIG
from pydantic import BaseModel
from typing import Optional


class NuevaPartida(BaseModel):
    jugadores: list[int]          # ids de los jugadores, por ejemplo [1, 2, 3]


class NuevoMovimiento(BaseModel):
    jugador_id: int
    turno: int
    accion: str                   # "jugar", "robar", "uno", "castigo_uno"
    carta: Optional[str] = None   # por ejemplo "rojo_5" o "comodin_mas4"
    color_elegido: Optional[str] = None


class FinPartida(BaseModel):
    ganador_id: int
app = FastAPI(title="API UNO")


def get_db_connection():
    return mysql.connector.connect(**DB_CONFIG)


@app.get("/")
async def root():
    return {"message": "API del UNO funcionando"}


@app.get("/jugadores")
async def obtener_jugadores():
    try:
        conn = get_db_connection()
        cursor = conn.cursor(dictionary=True)

        cursor.execute("SELECT id, nombre FROM jugadores")
        jugadores = cursor.fetchall()

        cursor.close()
        conn.close()

        return jugadores

    except Exception as e:
        return {"status": "Error", "detalle": str(e)}

@app.post("/partidas")
async def crear_partida(datos: NuevaPartida):
    try:
        conn = get_db_connection()
        cursor = conn.cursor()

        cursor.execute("INSERT INTO partidas (fecha_inicio) VALUES (NOW())")
        partida_id = cursor.lastrowid

        for jugador_id in datos.jugadores:
            cursor.execute(
                "INSERT INTO partida_jugador (partida_id, jugador_id) VALUES (%s, %s)",
                (partida_id, jugador_id),
            )

        conn.commit()
        cursor.close()
        conn.close()

        return {"id": partida_id}

    except Exception as e:
        return {"status": "Error", "detalle": str(e)}

@app.post("/partidas/{partida_id}/movimientos")
async def registrar_movimiento(partida_id: int, mov: NuevoMovimiento):
    try:
        conn = get_db_connection()
        cursor = conn.cursor()

        cursor.execute(
            """INSERT INTO movimientos
               (partida_id, jugador_id, turno, accion, carta, color_elegido)
               VALUES (%s, %s, %s, %s, %s, %s)""",
            (partida_id, mov.jugador_id, mov.turno, mov.accion, mov.carta, mov.color_elegido),
        )

        conn.commit()
        movimiento_id = cursor.lastrowid
        cursor.close()
        conn.close()

        return {"id": movimiento_id}

    except Exception as e:
        return {"status": "Error", "detalle": str(e)}


@app.get("/partidas/{partida_id}/movimientos")
async def obtener_movimientos(partida_id: int):
    try:
        conn = get_db_connection()
        cursor = conn.cursor(dictionary=True)

        cursor.execute(
            "SELECT * FROM movimientos WHERE partida_id = %s ORDER BY id",
            (partida_id,),
        )
        movimientos = cursor.fetchall()

        cursor.close()
        conn.close()

        return movimientos

    except Exception as e:
        return {"status": "Error", "detalle": str(e)}

@app.put("/partidas/{partida_id}/finalizar")
async def finalizar_partida(partida_id: int, datos: FinPartida):
    try:
        conn = get_db_connection()
        cursor = conn.cursor()

        cursor.execute(
            "UPDATE partidas SET fecha_fin = NOW(), ganador_id = %s WHERE id = %s",
            (datos.ganador_id, partida_id),
        )
        cursor.execute(
            """UPDATE partida_jugador
               SET resultado = IF(jugador_id = %s, 'ganada', 'perdida')
               WHERE partida_id = %s""",
            (datos.ganador_id, partida_id),
        )

        conn.commit()
        cursor.close()
        conn.close()

        return {"status": "OK"}

    except Exception as e:
        return {"status": "Error", "detalle": str(e)}