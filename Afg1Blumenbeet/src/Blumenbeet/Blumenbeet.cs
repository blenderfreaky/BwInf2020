namespace Blumenbeet
{
    using System;
    using System.Collections.Generic;

    public static class BlumenbeetOptimizer
    {

    }

    public unsafe readonly struct Blumenbeet
    {
        public readonly Color[,] Colors;
        public readonly Customer Customer;

        public int ComputeScore()
        {
            int score = 0;
            for (int i = 0; i < Colors.GetLength(0); i++)
            {
                for (int j = 0; j < Colors.GetLength(1); j++)
                {

                }
            }
        }
    }

    public class Customer
    {
        public List<(Color Color1, Color Color2, int Score)> Preferences { get; }
    }

    public enum Color
    {
        Red,
    }
}
