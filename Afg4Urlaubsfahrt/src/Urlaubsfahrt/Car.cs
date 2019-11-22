using System;
using System.Collections.Generic;
using System.Text;

namespace Urlaubsfahrt
{
    public readonly struct Car
    {
        /// <summary>
        /// The fuel usage of the car. measured in Litres / 100Kilometres.
        /// </summary>
        public readonly double FuelUsage;

        /// <summary>
        /// The capacity of the cars tank, measured in Litres.
        /// </summary>
        public readonly double TankCapacity;

        /// <summary>
        /// The distance the car can travel on one tank without refueling, measured in Metres.
        /// </summary>
        public readonly double TankDistance;

        /// <summary>
        /// The amount of fuel the car starts off with, measured in Litres.
        /// </summary>
        public readonly double StartingFuelAmount;

        /// <summary>1
        /// The distance the car can travel on with the fuel it starded with without refueling, measured in Metres.
        /// </summary>
        public readonly double StartingFuelDistance;

        public Car(double fuelUsage, double tankCapacity, double startingFuelAmount)
        {
            FuelUsage = fuelUsage;
            TankCapacity = tankCapacity;
            TankDistance = TankCapacity / FuelUsage;
            StartingFuelAmount = startingFuelAmount;
            StartingFuelDistance = startingFuelAmount / FuelUsage;
        }

        /// <summary>
        /// Get the price for gas at a gas station measured in €/km for this car.
        /// </summary>
        /// <param name="station">The gas station to get the price for.</param>
        /// <returns>The price in € of driving a kilometer with gas from this station.</returns>
        public readonly double GetPriceForDistanceAt(GasStation station) => station.Price * FuelUsage;
    }
}
