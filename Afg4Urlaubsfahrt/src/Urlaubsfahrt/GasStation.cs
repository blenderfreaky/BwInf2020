namespace Urlaubsfahrt
{
    public class GasStation
    {
        public GasStation(float pricePerVolumeInEuroPerLiter, float position)
        {
            PricePerTank = pricePerVolumeInEuroPerLiter;
            Position = position;
        }

        public float PricePerTank { get; }
        public float Position { get; }
    }
}