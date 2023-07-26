namespace MedicineChest
{
    internal class Lift
    {
        public Dictionary<string, int> selectedFloorCounters = new();
        public Dictionary<string, int> calledFloorCounters = new();
        public int CurrentFloor;
        public HashSet<int> selectedFloorsLive = new();
        public HashSet<int> selectedFloorsSnapshot = new();
        public HashSet<int> calledFloorsLive = new();
        public HashSet<int> calledFloorsSnapshot = new();
        public Boolean liftEmpty = true;
        public HashSet<int> liftCallers = new();
        public HashSet<int> liftRiders = new();

        public Lift()
        {
            foreach (int i in Enumerable.Range(1, 10))
            {
                selectedFloorCounters.Add($"x_{i}", 0);
            }

            foreach (int i in Enumerable.Range(1, 10))
            {
                calledFloorCounters.Add($"y_{i}", 0);
            }
        }

        public void UpdateCalledFloors(List<int> callerIDs)
        {
            foreach (int call in callerIDs)
            {
                liftCallers.Add(call);
                int calledFloor = Program.liftCallsDict[call][1];
                calledFloorsLive.Add(calledFloor);
                Console.WriteLine("Lift called from floor {0} at t = {1}, by callerID = {2}", calledFloor, Program.liftCallsDict[call][2], call);
            }
        }
    }

    
}
