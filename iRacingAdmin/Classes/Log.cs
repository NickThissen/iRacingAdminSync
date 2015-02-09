using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRacingAdmin.Classes
{
    public class Log
    {
        private string _path;

        private Log(string path)
        {
            _path = path;

            if (!File.Exists(_path))
            {
                var builder = new StringBuilder();
                builder.AppendLine("Log file created at " + DateTime.Now.ToUniversalTime() + " (GMT)");
                File.WriteAllText(_path, builder.ToString());
            }
        }

        private static Log _instance;
        public static Log Instance
        {
            get { return _instance ?? (_instance = new Log(System.IO.Path.Combine(Paths.SettingsPath, "log.txt"))); }
        }

        public string Path
        {
            get { return _path; }
        }

        private void WriteLog(StringBuilder contents)
        {
            var builder = new StringBuilder();
            builder.Append(contents);
            builder.AppendLine("-----------");
            builder.AppendLine();
            builder.AppendLine();

            var prevContents = File.ReadAllText(_path);
            builder.Append(prevContents);
            File.WriteAllText(_path, builder.ToString());
        }

        public void WriteLogError(string action, Exception ex)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Error: " + DateTime.Now.ToUniversalTime() + " (GMT)");
            builder.AppendLine(action);
            builder.AppendLine(ex.ToString());
            this.WriteLog(builder);
        }

        public void LogInfo(params string[] lines)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Info: " + DateTime.Now.ToUniversalTime() + " (GMT)");
            foreach (var line in lines)
            {
                builder.AppendLine(line);
            }
            this.WriteLog(builder);
        }
    }
}
