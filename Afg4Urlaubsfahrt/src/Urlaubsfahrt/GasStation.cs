namespace Urlaubsfahrt
{
    public class GasStation
    {
        public GasStation(double pricePerVolumeInEuroPerLiter, double position)
        {
            Price = pricePerVolumeInEuroPerLiter;
            Position = position;
        }

        public double Price { get; }
        public double Position { get; }
    }
}