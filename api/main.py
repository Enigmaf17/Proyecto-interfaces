from fastapi import FastAPI
import mysql.connector
from config import DB_CONFIG

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