namespace MedicineChest
{
    internal class Lift
    {
        public Dictionary<string, int> SelectedFloorCounters = new();
        public Dictionary<string, int> CalledFloorCounters = new();
        public int CurrentFloor;
        public HashSet<int> SelectedFloorsLive = new();
        public HashSet<int> SelectedFloorsSnapshot = new();
        public HashSet<int> CalledFloorsLive = new();
        public HashSet<int> CalledFloorsSnapshot = new();

        public Lift()
        {
            foreach (int i in Enumerable.Range(1, 10))
            {
                SelectedFloorCounters.Add($"x_{i}", 0);
            }

            foreach (int i in Enumerable.Range(1, 10))
            {
                CalledFloorCounters.Add($"y_{i}", 0);
            }
        }
    }

    
}
