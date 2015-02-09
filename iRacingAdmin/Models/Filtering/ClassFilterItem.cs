using System.Windows.Media;

namespace iRacingAdmin.Models.Filtering
{
    public class ClassFilterItem
    {
        public int CarClassRelSpeed { get; set; }
        public Brush Brush { get; set; }

        public string Text { get { return this.CarClassRelSpeed == -1 ? "All" : ""; } }

        public static ClassFilterItem All()
        {
            return new ClassFilterItem() {CarClassRelSpeed = -1, Brush = Brushes.Transparent};
        }
    }
}
