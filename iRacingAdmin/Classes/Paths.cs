using System;
using System.IO;

namespace iRacingAdmin.Classes
{
    public static class Paths
    {
        public static void LoadPaths(string iracingPath)
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NickThissen", "iRacingAdminSync");
            if (!Directory.Exists(_settingsPath)) Directory.CreateDirectory(_settingsPath);

            _configPath = Path.Combine(_settingsPath, "config");
            if (!Directory.Exists(_configPath)) Directory.CreateDirectory(_configPath);

            _configBackupPath = Path.Combine(_configPath, "backup");
            if (!Directory.Exists(_configBackupPath)) Directory.CreateDirectory(_configBackupPath);

            _iracingPath = iracingPath;
        }

        private static string _settingsPath;
        public static string SettingsPath
        {
            get { return _settingsPath; }
        }

        private static string _configPath;
        public static string ConfigPath
        {
            get { return _configPath; }
        }

        private static string _configBackupPath;
        public static string ConfigBackupPath
        {
            get { return _configBackupPath; }
        }

        private static string _iracingPath;
        public static string IRacingPath
        {
            get { return _iracingPath; }
        }
        
        public static string GetHelmetPath(string carFolderName, int id)
        {
            return Path.Combine(IRacingPath, "paint", carFolderName, "helmet_" + id + ".tga");
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }

        public static bool ComparePaths(string path1, string path2)
        {
            return NormalizePath(path1) == NormalizePath(path2);
        }
    }
}
