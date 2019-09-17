namespace Urlaubsfahrt
{
    public class GasStation
    {
        public GasStation(float pricePerVolumeInEuroPerLiter, float position)
        {
            PricePerVolumeInEuroPerLiter = pricePerVolumeInEuroPerLiter;
            Position = position;
        }

        public float PricePerVolumeInEuroPerLiter { get; }
        public float Position { get; }
    }
}