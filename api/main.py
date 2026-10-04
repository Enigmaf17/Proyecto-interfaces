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