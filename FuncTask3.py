import azure.functions as func
import logging
import pyodbc
from datetime import datetime

server = 'SDAPP-server.postgres.database.azure.com'
database = 'postgres'
username = 'kosiiito'
password = 'albolio4ko'
driver = '{ODBC Driver 17 for SQL Server}'

def calculate_daily_average_ratings():
    try:
        conn = pyodbc.connect('DRIVER='+driver+';SERVER='+server+';PORT=5432;DATABASE='+database+';UID='+username+';PWD='+ password)
        cursor = conn.cursor()
        cursor.execute("SELECT Title, AVG(Rating) AS AverageRating FROM Reviews WHERE Date = ? GROUP BY Title", datetime.now().date())
        rows = cursor.fetchall()
        for row in rows:
            title, average_rating = row
            cursor.execute("UPDATE Films SET AverageRating = ? WHERE Title = ?", average_rating, title)
        conn.commit()
        return True
    except Exception as e:
        logging.error(f"Error: {str(e)}")
        return False
    finally:
        cursor.close()
        conn.close()

def funcTask3(mytimer: func.TimerRequest) -> None:
    if calculate_daily_average_ratings():
        logging.info("Daily average ratings calculated and updated successfully")
    else:
        logging.error("Failed to calculate daily average ratings")
