namespace WeatherParadise.Models
{
    public class Weather
    {
        public string locationName;
        public string period;

        public Weather()
        {

        }

        public string LocationName { get => locationName; set => locationName = value; }
        public string Period { get => period; set => period = value; }

    }
}
