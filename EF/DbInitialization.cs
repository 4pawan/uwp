using System;
using System.Data.SQLite;
using System.IO;
using Unicorn.UWP;

namespace UnicornWidget.Data
{
    internal static class DbInitialization
    {
        private static string DatabaseFile = Path.Combine(AppVariables.AppDataFolder, "unicorn_widget.db");
        internal static void Initialize()
        {
            if (!Directory.Exists(AppVariables.AppDataFolder))
                Directory.CreateDirectory(AppVariables.AppDataFolder);

            if (!File.Exists(DatabaseFile))
                SQLiteConnection.CreateFile(DatabaseFile);
            InitialiseDb();
        }

        private static void InitialiseDb()
        {
            using (SqliteConnection conn = new SQLiteConnection(AppVariables.DatabaseSource))
            {
                conn.Open();
                string sql = "CREATE TABLE IF NOT EXISTS TblNotifications(Url nvarchar(1000))";

                SQLiteCommand command = new SQLiteCommand(sql, conn);
                command.ExecuteNonQuery();
            }
        }
    }
}