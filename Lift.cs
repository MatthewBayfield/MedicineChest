namespace MedicineChest
{
    internal class Lift
    {
        public Dictionary<string, int> selectedFloorCounters = new();
        public Dictionary<string, int> calledFloorCounters = new();
        public int CurrentFloor = 5;
        public HashSet<int> selectedFloorsLive = new();
        public HashSet<int> selectedFloorsSnapshot = new();
        public HashSet<int> calledFloorsLive = new();
        public HashSet<int> calledFloorsSnapshot = new();
        public Boolean liftFull = false;
        public HashSet<int> liftCallers = new();
        public HashSet<int> liftRiders = new();
        public int averageStopTime = 30;
        public int liftCapacity = 8;
        public int liftAdjacentFloorTravelTime = 10;

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

        private void DisembarkRiders(int timeWhenStopped)
        {
            // adding Output CSV entries for departing riders
            IEnumerable<int> departingRiders =
            from riderID in liftRiders
            where Program.liftCallsDict[riderID][1] == CurrentFloor
            select riderID;
            Program.WriteToOutputCSV(departingRiders.ToList(), timeWhenStopped);
            // removing disembarked riders
            liftRiders.RemoveWhere((int riderID) => Program.liftCallsDict[riderID][1] == CurrentFloor);
            departingRiders.ToList().ForEach((int riderID) => Console.WriteLine("Rider with callerID = {0}, departing at current floor = {1}, at t = {2}",
                                                                                riderID, CurrentFloor, timeWhenStopped));
        }
    } 
}
