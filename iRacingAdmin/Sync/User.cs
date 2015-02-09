using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Alchemy.Classes;
using Newtonsoft.Json;

namespace iRacingAdmin.Sync
{
    [Serializable]
    public class User
    {
        public int CustId { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (string.IsNullOrEmpty(this.ShortName))
                {
                    this.ShortName = this.Name;
                }
            }
        }

        public string ShortName { get; set; }

        public UserColor UserColor { get; set; }

        [JsonIgnore]
        public Color Color
        {
            get
            {
                if (this.UserColor != null) return this.UserColor.Color;
                return Colors.Transparent;
            }
        }

        public bool IsRegistered { get; set; }
        public bool IsConnected { get; set; }
        public bool IsHost { get; set; }

        public int Ping { get; set; }
        public DateTime? LastPingSent { get; set; }
        public DateTime? LastPongReceived { get; set; }
    }

    [Serializable]
    public class UserConnection
    {
        public UserConnection()
        {
            this.LockObject = new object();
        }

        [JsonIgnore]
        public object LockObject { get; set; }

        [JsonIgnore]
        public UserContext Context { get; set; }
        public string ClientAddress { get; set; }
        public User User { get; set; }

        [JsonIgnore]
        public string Username
        {
            get { return this.IsRegistered ? this.User.Name : "(unknown client)"; }
        }

        [JsonIgnore]
        public bool IsRegistered { get { return this.User != null; } }
    }

    [Serializable]
    public class UserColor
    {
        public UserColor(int order, Color color, int count = 0)
        {
            this.Order = order;
            this.Color = color;
            this.Count = count;
        }

        public int Order { get; set; }
        public int Count { get; set; }
        public Color Color { get; set; }

        [JsonIgnore]
        public static UserColor Offline
        {
            get
            {
                return new UserColor(0, Color.FromRgb(234, 67, 51), 0);
            }
        }
    }

    public static class UserColors
    {
        private static List<UserColor> _colors;

        static UserColors()
        {
            _colors = new List<UserColor>();
            _colors.Add(new UserColor(1, Color.FromRgb(234, 67, 51), 0));
            _colors.Add(new UserColor(2, Color.FromRgb(128, 186, 69), 0));
            _colors.Add(new UserColor(3, Color.FromRgb(251, 134, 51), 0));
            _colors.Add(new UserColor(4, Color.FromRgb(131, 122, 229), 0));
            _colors.Add(new UserColor(6, Color.FromRgb(254, 229, 56), 0));
            _colors.Add(new UserColor(7, Color.FromRgb(224, 51, 143), 0));
            _colors.Add(new UserColor(8, Color.FromRgb(138, 159, 131), 0));
            _colors.Add(new UserColor(9, Color.FromRgb(243, 181, 59), 0));
            _colors.Add(new UserColor(10, Color.FromRgb(246, 142, 217), 0));
            _colors.Add(new UserColor(11, Color.FromRgb(51, 115, 242), 0));
            _colors.Add(new UserColor(12, Color.FromRgb(131, 145, 159), 0));
            _colors.Add(new UserColor(13, Color.FromRgb(182, 208, 51), 0));
        }

        public static void SetColor(User user)
        {
            // Sort by count, then by order
            var colors = _colors.OrderBy(c => c.Count).ThenBy(c => c.Order);

            var color = colors.First();
            color.Count += 1;

            user.UserColor = color;
        }

        public static void RemoveColor(User user)
        {
            if (user.UserColor != null)
            {
                var color = _colors.SingleOrDefault(c => c.Order == user.UserColor.Order);
                if (color != null) color.Count -= 1;
            }
        }

        public static void CountColor(User user)
        {
            if (user.UserColor != null)
            {
                var color = _colors.SingleOrDefault(c => c.Order == user.UserColor.Order);
                if (color != null) color.Count += 1;
            }
        }
    }
}
