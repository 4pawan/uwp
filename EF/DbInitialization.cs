using System.Diagnostics;
using System.IO;
using SQLite.Net;

namespace Unicorn.UWP.EF
{
	public static class DbInitialization
	{
		public static void Initialize()
		{
			if (!Directory.Exists(Constant.Constant.AppDataFolder))
				Directory.CreateDirectory(Constant.Constant.AppDataFolder);

			//if (!File.Exists(_databaseFile))
			//	SQLiteConnection.(_databaseFile);
			InitialiseDb();
		}

		private static void InitialiseDb()
		{

			using (SQLiteConnection conn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), Constant.Constant.DatabaseSource))
			{
				var created = conn.CreateTable<TblNotifications>();
				Debug.WriteLine("table created :" + created);
			}

			//using (SQLiteConnection conn = new SQLiteConnection(Constant.Constant.DatabaseSource))
			//{
			//	conn.Open();
			//	string sql = "CREATE TABLE IF NOT EXISTS TblNotifications(Url nvarchar(1000))";

			//	SQLiteCommand command = new SQLiteCommand(sql, conn);
			//	command.ExecuteNonQuery();
			//}
		}

		public static bool TableIsExists<T>(SQLiteConnection conn)
		{
			var q = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
			var cmd = conn.CreateCommand(q, typeof(T).Name);
			return cmd.ExecuteScalar<string>() != null;
		}
	}
}