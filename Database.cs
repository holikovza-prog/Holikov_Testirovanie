using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace TriangleLab3
{
    // Запись в БД: длина1, длина2, длина3, тип_треугольника, сообщение_об_ошибке
    public class TriangleRecord
    {
        public string SideA { get; set; } = string.Empty;
        public string SideB { get; set; } = string.Empty;
        public string SideC { get; set; } = string.Empty;
        public string TriangleType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        // Идентификатор по трем длинам сторон
        public string GetKey() => $"{SideA}|{SideB}|{SideC}";
    }

    // Интерфейс БД
    public interface IDatabase
    {
        void AddRecord(TriangleRecord record);      // Добавление данных
        TriangleRecord? GetRecord(string sideA, string sideB, string sideC);  // Получение данных
        void DeleteRecord(string sideA, string sideB, string sideC);          // Удаление данных
        List<TriangleRecord> GetAllRecords();
        void ClearAll();
        bool HasRecord(string sideA, string sideB, string sideC);
    }

    // Класс b: реализация работы с БД (SQLite)
    public class SQLiteDatabase : IDatabase
    {
        private readonly string _connectionString;

        public SQLiteDatabase(string databasePath = "triangles.db")
        {
            _connectionString = $"Data Source={databasePath};Version=3;";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Triangles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SideA TEXT NOT NULL,
                    SideB TEXT NOT NULL,
                    SideC TEXT NOT NULL,
                    TriangleType TEXT NOT NULL,
                    ErrorMessage TEXT,
                    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    UNIQUE(SideA, SideB, SideC)
                )";

            using var command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }

        public void AddRecord(TriangleRecord record)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string insertQuery = @"
                INSERT OR REPLACE INTO Triangles (SideA, SideB, SideC, TriangleType, ErrorMessage)
                VALUES (@sideA, @sideB, @sideC, @type, @error)";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@sideA", record.SideA);
            command.Parameters.AddWithValue("@sideB", record.SideB);
            command.Parameters.AddWithValue("@sideC", record.SideC);
            command.Parameters.AddWithValue("@type", record.TriangleType);
            command.Parameters.AddWithValue("@error", record.ErrorMessage ?? "");

            command.ExecuteNonQuery();
        }

        public TriangleRecord? GetRecord(string sideA, string sideB, string sideC)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = @"
                SELECT SideA, SideB, SideC, TriangleType, ErrorMessage 
                FROM Triangles 
                WHERE SideA = @sideA AND SideB = @sideB AND SideC = @sideC";

            using var command = new SQLiteCommand(selectQuery, connection);
            command.Parameters.AddWithValue("@sideA", sideA);
            command.Parameters.AddWithValue("@sideB", sideB);
            command.Parameters.AddWithValue("@sideC", sideC);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new TriangleRecord
                {
                    SideA = reader.GetString(0),
                    SideB = reader.GetString(1),
                    SideC = reader.GetString(2),
                    TriangleType = reader.GetString(3),
                    ErrorMessage = reader.GetString(4)
                };
            }
            return null;
        }

        public bool HasRecord(string sideA, string sideB, string sideC)
        {
            return GetRecord(sideA, sideB, sideC) != null;
        }

        public void DeleteRecord(string sideA, string sideB, string sideC)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string deleteQuery = "DELETE FROM Triangles WHERE SideA = @sideA AND SideB = @sideB AND SideC = @sideC";
            using var command = new SQLiteCommand(deleteQuery, connection);
            command.Parameters.AddWithValue("@sideA", sideA);
            command.Parameters.AddWithValue("@sideB", sideB);
            command.Parameters.AddWithValue("@sideC", sideC);
            command.ExecuteNonQuery();
        }

        public List<TriangleRecord> GetAllRecords()
        {
            var records = new List<TriangleRecord>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string selectQuery = "SELECT SideA, SideB, SideC, TriangleType, ErrorMessage FROM Triangles";
            using var command = new SQLiteCommand(selectQuery, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                records.Add(new TriangleRecord
                {
                    SideA = reader.GetString(0),
                    SideB = reader.GetString(1),
                    SideC = reader.GetString(2),
                    TriangleType = reader.GetString(3),
                    ErrorMessage = reader.GetString(4)
                });
            }
            return records;
        }

        public void ClearAll()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var command = new SQLiteCommand("DELETE FROM Triangles", connection);
            command.ExecuteNonQuery();
        }
    }
}