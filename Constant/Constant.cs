using System.IO;

namespace Unicorn.UWP.Constant
{
	public class Constant
	{
		public static string AppDataFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Unicorn Widget");
		public static string DatabaseSource = Path.Combine(AppDataFolder, "unicorn_widget.db");
		public static string LogFolder = Path.Combine(AppDataFolder, "log.txt");
	}
}