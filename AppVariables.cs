using System;
using System.IO;

namespace Unicorn.UWP
{
    internal static class AppVariables
    {
        internal static string AppDataFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Unicorn Widget");
        internal static string DatabaseSource = "data source=" + Path.Combine(AppDataFolder, "unicorn_widget.db");
        internal static string LogFolder = Path.Combine(AppDataFolder, "log.txt");
    }
}