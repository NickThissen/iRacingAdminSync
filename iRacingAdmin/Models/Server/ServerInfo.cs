using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRacingAdmin.Classes;
using iRacingAdmin.Views;
using Newtonsoft.Json;

namespace iRacingAdmin.Models.Server
{
    public class ServerInfo
    {
        private string _name;
        private string _ip;
        private int _port;
        private string _password;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                this.IsNew = false;
            }
        }

        public string Ip
        {
            get { return _ip; }
            set
            {
                _ip = value;
                this.IsNew = false;
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                this.IsNew = false;
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                this.IsNew = false;
            }
        }

        public bool IsNew { get; set; }

        public static ServerInfo Create()
        {
            var s = new ServerInfo();
            s.Name = "<name>";
            s.Ip = "<ip>";
            s.IsNew = true;
            return s;
        }

        public static ServerInfo Local(int port, string password)
        {
            var s = new ServerInfo();
            s.Name = "Local";
            s.Ip = "127.0.0.1";
            s.Port = port;
            s.Password = password;
            return s;
        }

        public static List<ServerInfo> Load()
        {
            var bookmarks = new List<ServerInfo>();
            var file = Path.Combine(Paths.SettingsPath, "bookmarks.json");
            if (File.Exists(file))
            {
                try
                {
                    bookmarks.AddRange(JsonConvert.DeserializeObject<List<ServerInfo>>(File.ReadAllText(file)));
                }
                catch (Exception ex)
                {
                    Log.Instance.WriteLogError("Loading bookmarks.", ex);
                    MessageWindow.ShowException("Error loading server bookmarks.");
                }
            }
            return bookmarks;
        }

        public static void Save(List<ServerInfo> bookmarks)
        {
            var file = Path.Combine(Paths.SettingsPath, "bookmarks.json");
            try
            {
                var save =
                    bookmarks.Where(
                        b => !b.IsNew && !string.IsNullOrWhiteSpace(b.Name) && !string.IsNullOrWhiteSpace(b.Ip) && b.Port > 0)
                        .ToList();

                var json = JsonConvert.SerializeObject(save);
                File.WriteAllText(file, json);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteLogError("Saving bookmarks.", ex);
                MessageWindow.ShowException("Error saving server bookmarks.");
            }
        }
    }
}
