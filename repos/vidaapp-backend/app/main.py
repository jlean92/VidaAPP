from fastapi import FastAPI,HTTPException
import os
import mysql.connector
import json
from packaging.version import parse as vparse

app = FastAPI(title="VidaApp API")

def get_db_conn():
    return mysql.connector.connect(
        host=os.environ["DB_HOST"],
        port=int(os.environ["DB_PORT"]),
        user=os.environ["DB_USER"],
        password=os.environ["DB_PASSWORD"],
        database=os.environ["DB_NAME"],
        autocommit=True,
    )

@app.get("/health")
def health():
    return {"status": "ok"}

@app.get("/health/db")
def health_db():
    conn = get_db_conn()
    cur = conn.cursor()
    cur.execute("SELECT 1")
    cur.fetchone()
    cur.close()
    conn.close()
    return {"db": "ok"}

@app.get("/update/check")
def update_check():
    current_version = os.getenv("APP_VERSION", "0.0.0")
    latest_path = os.getenv("UPDATE_LATEST_PATH", "/updates/latest.json")

    # Si no existe metadata, devolvemos "sin update" (no petamos)
    if not os.path.exists(latest_path):
        return {
            "hasUpdate": False,
            "currentVersion": current_version,
            "latestVersion": current_version,
            "downloadUrl": None,
            "notes": None,
            "message": "latest.json no encontrado"
        }

    try:
        with open(latest_path, "r", encoding="utf-8") as f:
            meta = json.load(f)

        latest_version = meta.get("latestVersion", current_version)
        download_url = meta.get("downloadUrl")
        notes = meta.get("notes")

        has_update = vparse(latest_version) > vparse(current_version)

        return {
            "hasUpdate": has_update,
            "currentVersion": current_version,
            "latestVersion": latest_version,
            "downloadUrl": download_url,
            "notes": notes
        }

    except Exception as e:
        # Si el JSON está corrupto o falla lectura, devolvemos error claro
        raise HTTPException(status_code=500, detail=f"Error leyendo latest.json: {e}")