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

        private void SelectFloors(int spacesAvailable, int timeWhenStopped)
        {
            if (spacesAvailable == 0) return;
            // Obtaining the ID's of callers wanting to board the lift.
            IEnumerable<int> boardingCallers =
                    from caller in liftCallers
                    where Program.liftCallsDict[caller][0] == CurrentFloor
                    select caller;
            List<int> boardingCallersList = boardingCallers.ToList();

            int i = 0;
            int selectedFloor;
            int boardedCallerID;
            // Adding callers to the lift, subject to space availability
            while (i < (new int[] { spacesAvailable, boardingCallersList.Count }).Min())
            {
                // Add boarded caller to lift riders and add their selected floor to the selected floors set.
                boardedCallerID = boardingCallersList[i];
                liftRiders.Add(boardedCallerID);
                selectedFloor = Program.liftCallsDict[boardedCallerID][1];
                selectedFloorsLive.Add(selectedFloor);
                Console.WriteLine("CallerID = {0} has boarded the lift at t = {1}, and selected to go to floor {2}", boardedCallerID, timeWhenStopped, selectedFloor);
                liftCallers.Remove(boardedCallerID);
                i++;
            }
        }

        private bool CanAllCallersBoard(int spacesAvailable)
        {
            // Getting the list of callers wanting to board the lift.
            IEnumerable<int> boardingCallers =
                    from caller in liftCallers
                    where Program.liftCallsDict[caller][0] == CurrentFloor
                    select caller;
            return (boardingCallers.ToList().Count > spacesAvailable);
        }
    } 
}
