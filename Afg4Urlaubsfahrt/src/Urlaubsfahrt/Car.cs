namespace Urlaubsfahrt
{
    public readonly struct Car
    {
        /// <summary>
        /// Der Verbrauch in Litern pro Kilomerter.
        /// </summary>
        public readonly decimal FuelUsage;

        /// <summary>
        /// Die Tankkapazität in Litern.
        /// </summary>
        public readonly decimal TankCapacity;

        /// <summary>
        /// Die Distanz die das Auto mit vollem Tank fahren kann.
        /// </summary>
        public readonly decimal TankDistance;

        /// <summary>
        /// Das Benzin, dass am Anfang im Auto ist.
        /// </summary>
        public readonly decimal StartingFuelAmount;

        /// <summary>
        /// Der Weg den das Auto fahren kann, ohne ein mal zu tanken.
        /// </summary>
        public readonly decimal StartingFuelDistance;

        public Car(decimal fuelUsage, decimal tankCapacity, decimal startingFuelAmount)
        {
            FuelUsage = fuelUsage;
            TankCapacity = tankCapacity;
            TankDistance = TankCapacity / FuelUsage;
            StartingFuelAmount = startingFuelAmount;
            StartingFuelDistance = startingFuelAmount / FuelUsage;
        }

        /// <summary>
        /// Der Preis, um mit diesem Auto einen Kilometer zu fahren
        /// </summary>
        /// <param name="station">Die Tankstelle, an der getankt wurde.</param>
        public readonly decimal GetPriceForDistanceAt(GasStation station) => station.Price * FuelUsage;
    }
}