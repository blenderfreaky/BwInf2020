namespace p
{
    public class Track
    { 
        public List<GasStation> Stops {get;}

        public static Track EmptyTrack => new Track();

        public float GetPrice()
        {
            float value = 0;
            for (int i = 0; i < Stops.Count - 1; i++)
            {
                value += (Stops[i+1].Position - Stops[i].Position) * Stops[i].PricePerVolumeInEuroPerLiter;
            }
            return value;
        }

        public float GetPriceTo(int pos)
        {
            float value = GetPrice();
            value += (pos - Stops.Last().Position) * Stops.Last().PricePerVolumeInEuroPerLiter;
            return value;
        }

        public float GetPriceTo(GasStation s)
        {
            float value = GetPrice();
            value += GetPriceTo(s.Position);
            return value;
        }

        public Track(GasStation s) => Stops = new List<GasStation> { s };

        public Track(Track t, GasStation s) : this()
        {
            //Stops.AddAll(t.Stops); //TODO try this
            foreach (GasStation ss in t.Stops) Stops.Add(ss);
            Stops.Add(s);
        }

        private Track() => Stops = new List<GasStation>();
    }
}