using SFERA.Math.Combinatorics;
using MoreLinq;

namespace MedicineChest
{
    internal class Lift
    {
        public Dictionary<string, int> selectedFloorCounters = new();
        public Dictionary<string, int> calledFloorCounters = new();
        public int currentFloor = 5;
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
        public List<int> route = new();
        public int numberOfStopsThreshold = 9;
        public int numberOfFloors = 10;

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
                int calledFloor = Program.liftCallsDict[call][0];
                calledFloorsLive.Add(calledFloor);
                Console.WriteLine("Lift called from floor {0} at t = {1}, by callerID = {2}", calledFloor, Program.liftCallsDict[call][2], call);
            }
        }

        public void OperateLift()
        {
            int timeWhenStopped = 0;
            int spacesAvailable = liftCapacity;
            // Main Lift Operation Loop.
            Console.WriteLine("Lift at floor = {0}, at t = {1}.", currentFloor, timeWhenStopped);
            while (!Program.terminate)
            {
                timeWhenStopped = Program.time;

                // Behaviour when lift is empty and no lift calls:

                // Corresponds to the lift being empty
                if (selectedFloorsLive.Count == 0 & calledFloorsLive.Count == 0)
                {
                    // return if necessary to floor 5
                    currentFloor = 5;
                    continue;
                }

                // Behaviour when lift is not empty and or there are lift calls:
                if (selectedFloorsLive.Contains(currentFloor) | calledFloorsLive.Contains(currentFloor))
                {
                    Console.WriteLine("Opening lift doors.");
                    Console.WriteLine("Waiting average stop time = {0}", averageStopTime);
                    // Removing current floor from the selected floor sets.
                    selectedFloorsLive.Remove(currentFloor);
                    selectedFloorsSnapshot.Remove(currentFloor);
                    // Resetting counter for current floor in selectedFloorCounters.
                    selectedFloorCounters[$"x_{currentFloor}"] = 0;
                    // Disembarking any relevant riders at current stop.
                    DisembarkRiders(timeWhenStopped);

                    spacesAvailable = liftCapacity - liftRiders.Count;
                    // In this prototype test it is possible to track the number of users in the lift, and this will be used instead of changes in the lift weight.
                    if (liftRiders.Count == liftCapacity)
                    {
                        liftFull = true;
                        Console.WriteLine("Lift is now full.");
                    }

                    else if (CanAllCallersBoard(spacesAvailable))
                    {
                        liftFull = true;
                        Console.WriteLine("Lift is now full.");
                    }
                    else
                    {
                        liftFull = false;
                        // Removing current floor from the called floor sets.
                        calledFloorsLive.Remove(currentFloor);
                        calledFloorsSnapshot.Remove(currentFloor);
                        // Resetting counter for current floor in calledFloorCounters.
                        calledFloorCounters[$"y_{currentFloor}"] = 0;
                    }

                    // Add selected floors for any new riders that boarded.
                    SelectFloors(spacesAvailable, timeWhenStopped);
                    Console.WriteLine("Floors in selectedFloorsLive:");
                    selectedFloorsLive.ForEach(Console.WriteLine);
                    // Updating counters for remaining selected and called floors
                    foreach (int floor in selectedFloorsSnapshot)
                    {
                        selectedFloorCounters[$"x_{floor}"] += 1;
                    }
                    foreach (int floor in calledFloorsSnapshot)
                    {
                        calledFloorCounters[$"y_{floor}"] += 1;
                    }
                    Console.WriteLine("Closing lift doors.");


                    // Updating selected and called floor snapshots.
                    UpdateSnapshots();
                    // Check if lift is empty after updates.
                    if (selectedFloorsSnapshot.Count == 0 & calledFloorsSnapshot.Count == 0)
                    {
                        continue;
                    }

                    // Determining the optimal path for the lift after current stop:

                    List<int> optimalPath = new();
                    Task calculationTask = Task.Run(() => optimalPath = CalculateOptimalPath());
                    // Simulating the average stop time
                    if (Program.QuickMode)
                    {
                        Task[] taskArray = new Task[] { calculationTask, Task.Delay(averageStopTime * 100) };
                        var taskGroup = Task.WhenAll(taskArray);
                        taskGroup.Wait();
                    }
                    else
                    {
                    Task[] taskArray = new Task[] { calculationTask, Task.Delay(averageStopTime * 1000) };
                    var taskGroup = Task.WhenAll(taskArray);
                    taskGroup.Wait();

                    Console.WriteLine("Order of floors in the optimal path:");
                    optimalPath.ForEach(Console.WriteLine);
                    int nextFloorStop = optimalPath[1];
                    // Simulating time taken to travel to next floor stop.
                    Console.WriteLine("Lift moving to floor = {0}.", nextFloorStop);
                    if (Program.QuickMode)
                    {
                        Task task = Task.Delay(liftAdjacentFloorTravelTime * 100 * Math.Abs(currentFloor - nextFloorStop));
                        task.Wait();
                    }
                    else
                    {
                    Task task = Task.Delay(liftAdjacentFloorTravelTime * 1000 * Math.Abs(currentFloor - nextFloorStop));
                    task.Wait();
                    }
                    // Changing the current stop to the next stop to indicate the lift has arrived at the next stop.
                    currentFloor = nextFloorStop;
                    Console.WriteLine("Lift stopped at current floor = {0}, at t = {1}.", currentFloor, Program.time);
                }
                else
                {
                    // Updating selected and called floor snapshots.
                    UpdateSnapshots();
                    // Check if lift is empty after updates.
                    if (selectedFloorsSnapshot.Count == 0 & calledFloorsSnapshot.Count == 0)
                    {
                        continue;
                    }

                    // Determining the optimal path for the lift after current stop:

                    List<int> optimalPath = CalculateOptimalPath();
                    Console.WriteLine("Order of floors in the optimal path:");
                    optimalPath.ForEach(Console.WriteLine);
                    int nextFloorStop = optimalPath[1];
                    // Simulating time taken to travel to next floor stop.
                    Console.WriteLine("Lift moving to floor = {0}.", nextFloorStop);
                    if (Program.QuickMode)
                    {
                        Task task = Task.Delay(liftAdjacentFloorTravelTime * 100 * Math.Abs(currentFloor - nextFloorStop));
                        task.Wait();
                    }
                    else
                    {
                    Task task = Task.Delay(liftAdjacentFloorTravelTime * 1000 * Math.Abs(currentFloor - nextFloorStop));
                    task.Wait();
                    }
                    // Changing the current stop to the next stop to indicate the lift has arrived at the next stop.
                    currentFloor = nextFloorStop;
                    Console.WriteLine("Lift stopped at current floor = {0}, at t = {1}.", currentFloor, Program.time);
                }
            }
        }

        private void DisembarkRiders(int timeWhenStopped)
        {
            // Extracting departing riders
            IEnumerable<int> departingRiders =
            from riderID in liftRiders
            where Program.liftCallsDict[riderID][1] == currentFloor
            select riderID;

            // Updating the journey details of the departing riders
            Program.UpdateCallerJourneyDetails(callerIDs: departingRiders.ToList(), timeDisembarked: timeWhenStopped);
            // removing disembarked riders
            departingRiders.ToList().ForEach((int riderID) => Console.WriteLine("Rider with callerID = {0}, departing at current floor = {1}, at t = {2}",
                                                                                riderID, currentFloor, timeWhenStopped));
            liftRiders.RemoveWhere((int riderID) => Program.liftCallsDict[riderID][1] == currentFloor);
        }

        private void SelectFloors(int spacesAvailable, int timeWhenStopped)
        {
            if (spacesAvailable == 0) return;
            // Obtaining the ID's of callers wanting to board the lift.
            IEnumerable<int> boardingCallers =
                    from caller in liftCallers
                    where Program.liftCallsDict[caller][0] == currentFloor
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
                    where Program.liftCallsDict[caller][0] == currentFloor
                    select caller;
            // Return boolean indicating whether they can all board.
            return (boardingCallers.ToList().Count > spacesAvailable);
        }

        public void UpdateSnapshots()
        {
            // Creating deep copies 
            selectedFloorsSnapshot = new HashSet<int>(selectedFloorsLive);
            calledFloorsSnapshot = new HashSet<int>(calledFloorsLive);
            Console.WriteLine("selected floor snap:");
            selectedFloorsSnapshot.ForEach(Console.WriteLine);
            Console.WriteLine("called floor snap:");
            calledFloorsSnapshot.ForEach(Console.WriteLine);
        }

        private List<int> CalculateOptimalPath()
        {
            // Creating sets to store which floors have exceeded the stops since visited threshold.
            HashSet<int> thresholdExceededSelectedFloors = new();
            HashSet<int> thresholdExceededCalledFloors = new();
            // Adding any floors that have exceeded the threshold to the respective sets.
            foreach (int floor in selectedFloorsSnapshot)
            {
                if (selectedFloorCounters[$"x_{floor}"] >= 9)
                {
                    thresholdExceededSelectedFloors.Add(floor);
                }
            }

            foreach (int floor in calledFloorsSnapshot)
            {
                if (calledFloorCounters[$"y_{floor}"] >= 9)
                {
                    thresholdExceededCalledFloors.Add(floor);
                }
            }

            // Calculating the optimal route as function of the selected and called floor context:

            // Both selected and called floors exist. Lift is not Full.
            if (calledFloorsSnapshot.Count != 0 & selectedFloorsSnapshot.Count != 0 & !liftFull)
            {
                // No Threshold exceeded called floors exist, threshold exceeded selected floors exist.
                if (thresholdExceededCalledFloors.Count == 0 & thresholdExceededSelectedFloors.Count != 0)
                {
                    Console.WriteLine("Prioritising threshold exceeded selected floors.");
                    return RouteOptimiser(thresholdExceededSelectedFloors);
                }

                // Threshold exceeded called floors exist, no threshold exceeded selected floors exist.
                if (thresholdExceededCalledFloors.Count != 0 & thresholdExceededSelectedFloors.Count == 0)
                {
                    Console.WriteLine("Prioritising threshold exceeded called floors.");
                    return RouteOptimiser(thresholdExceededCalledFloors);
                }

                // Threshold exceeded called floors exist, threshold exceeded selected floors exist.
                if (thresholdExceededCalledFloors.Count != 0 & thresholdExceededSelectedFloors.Count != 0)
                {
                    Console.WriteLine("Prioritising threshold exceeded selected/called floors.");
                    List<int> optimalPathForSelectedFloors = RouteOptimiser(thresholdExceededSelectedFloors);
                    Console.WriteLine("Order of floors in optimal path, before incorporating called floors:");
                    optimalPathForSelectedFloors.ForEach(Console.WriteLine);
                    return IncorporateCalledFloors(optimalPathForSelectedFloors, thresholdExceededCalledFloors.ToList());
                }

                // Normal called floors exist, normal selected floors exist.
                if (thresholdExceededCalledFloors.Count == 0 & thresholdExceededSelectedFloors.Count == 0)
                {
                    List<int> optimalPathForSelectedFloors = RouteOptimiser(selectedFloorsSnapshot);
                    Console.WriteLine("Order of floors in optimal path, before incorporating called floors:");
                    optimalPathForSelectedFloors.ForEach(Console.WriteLine);
                    return IncorporateCalledFloors(optimalPathForSelectedFloors, calledFloorsSnapshot.ToList());
                }
            }

            // Both selected and called floors exist. Lift is Full.
            if (calledFloorsSnapshot.Count != 0 & selectedFloorsSnapshot.Count != 0 & liftFull)
            {
                // Only normal selected floors exist.
                if (thresholdExceededSelectedFloors.Count == 0)
                {
                    return RouteOptimiser(selectedFloorsSnapshot);
                }
                else
                {
                    Console.WriteLine("Prioritising threshold exceeded selected floors.");
                    return RouteOptimiser(thresholdExceededSelectedFloors);
                }
            }

            // No called floors, and only normal selected floors exist.
            if (calledFloorsSnapshot.Count == 0 & thresholdExceededSelectedFloors.Count == 0)
            {
                return RouteOptimiser(selectedFloorsSnapshot);
            }

            // No called floors, and threshold exceeded selected floors exist.
            if (calledFloorsSnapshot.Count == 0 & thresholdExceededSelectedFloors.Count != 0)
            {
                Console.WriteLine("Prioritising threshold exceeded selected floors.");
                return RouteOptimiser(thresholdExceededSelectedFloors);
            }

            // No selected floors, and only normal called floors exist.
            if (calledFloorsSnapshot.Count != 0 & selectedFloorsSnapshot.Count == 0 & thresholdExceededCalledFloors.Count == 0)
            {
                return RouteOptimiser(calledFloorsSnapshot);
            }

            // No selected floors, and only threshold exceeded called floors exist.
            if (calledFloorsSnapshot.Count != 0 & selectedFloorsSnapshot.Count == 0 & thresholdExceededCalledFloors.Count != 0)
            {
                Console.WriteLine("Prioritising threshold exceeded called floors.");
                return RouteOptimiser(thresholdExceededCalledFloors);
            }

            // Default return value.
            List<int> emptyPath = new();
            return emptyPath;
        }

        private List<int> RouteOptimiser(HashSet<int> floorSet)
        {
            int numberOfStopsInPath = floorSet.Count;
            List<int> floorList = floorSet.ToList();

            // Ideally would like to obtain all possible paths equivalent to all possible permutations of the number of floors.
            // However this corresponds to n! lists, where n is the number of floors, thus increasing the computation time for large n.
            // Therefore for the case of n floors, where n > 5, all permutations of 5 floors chosen from n floors will be obtained instead.
            // This will potentially impact the path selected and thus the efficiency.
            List<List<int>> allPossiblePaths = new();
            if (floorList.Count > 5)
            {
                numberOfStopsInPath = 5;
                foreach (List<int> path in new Variations<int>(floorList, 5))
                {
                    allPossiblePaths.Add(new List<int>(path));
                }
            }
            else
            {
                foreach (List<int> path in new Permutations<int>(floorList))
                {
                    allPossiblePaths.Add(new List<int>(path));
                }
            }

            // Inserting the current floor at the start of each path.
            for (int l = 0; l < allPossiblePaths.Count; ++l)
            {
                allPossiblePaths[l].Insert(0, currentFloor);
            }
            // Labelling the paths.
            Dictionary<int, List<int>> labelledAllPossiblePaths = new();
            int k = 0;
            foreach (List<int> path in allPossiblePaths)
            {
                labelledAllPossiblePaths.Add(k, path);
                ++k;
            }
            Console.WriteLine("Number of possible paths = {0}", labelledAllPossiblePaths.Count);

            // Calculating the total rider time for each labelled path.
            Dictionary<int, int> timesForAllPaths = new();
            int totalRiderTimeForPath;
            foreach (KeyValuePair<int, List<int>> pair in labelledAllPossiblePaths)
            {
                totalRiderTimeForPath = SumPartialSumsForPath(pair.Value);
                timesForAllPaths.Add(pair.Key, totalRiderTimeForPath);
            }

            // Identifying the minimum time, and then extracting a subset of all paths that give the minimum time.
            int minimumTotalRiderTime = timesForAllPaths.Values.ToList().Min();
            IEnumerable<KeyValuePair<int, int>> indexedMinimumTimes =
            from labelledTime in timesForAllPaths
            where labelledTime.Value == minimumTotalRiderTime
            select labelledTime;

            // Distinguishing, if necessary, multiple paths that produce the minimum value, by calculating the total lift journey time for each path.
            // Then picking any path that also minimises this total lift time. 
            if (indexedMinimumTimes.ToList().Count > 1)
            {
                Dictionary<int, int> indexedLiftTotalTimes = new();
                int minLiftTotalTime = 0;
                foreach (KeyValuePair<int, int> pair in indexedMinimumTimes)
                {
                    indexedLiftTotalTimes.Add(pair.Key, CalculateJthPartialSumQ_j(numberOfStopsInPath, labelledAllPossiblePaths[pair.Key]));
                    minLiftTotalTime = indexedLiftTotalTimes.Values.Min();

                }
                foreach (KeyValuePair<int, int> pair in indexedLiftTotalTimes)
                {
                    if (pair.Value == minLiftTotalTime)
                    {
                        return labelledAllPossiblePaths[pair.Key];

                    }
                }
            }
            return labelledAllPossiblePaths[indexedMinimumTimes.ToList()[0].Key];
        }

        private int CalculateJthPartialSumQ_j(int j, List<int> floors)
        {
            // Calculation of Q_j found in expression T(P).

            List<int> sumTerms = new();
            int i = 0;
            while (i < j)
            {
                sumTerms.Add((Math.Abs(floors[i + 1] - floors[i]) * liftAdjacentFloorTravelTime) + ((j - 1) * averageStopTime));
                i++;
            }
            int Q_j = sumTerms.Sum();
            return Q_j;
        }

        private int eta_j(int floor)
        {
            // Calculation of eta_j found in expression T(P).

            if (calledFloorsSnapshot.Contains(floor) & selectedFloorsSnapshot.Contains(floor))
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        private int SumPartialSumsForPath(List<int> floors)
        {
            List<int> partialSumTerms = new();
            int j = 1;
            int numberOfStopsInPath = floors.Count - 1;
            while (j <= numberOfStopsInPath)
            {
                partialSumTerms.Add(CalculateJthPartialSumQ_j(j, floors));
                ++j;
            }
            // T(P) calculation for path P defined by floors.
            int SumOfPatialSums = 0;
            foreach (int term in partialSumTerms)
            {
                SumOfPatialSums += term * eta_j(floors[partialSumTerms.IndexOf(term) + 1]);
            }
            return SumOfPatialSums;
        }

        public string DetermineDirectionOfMostProbableTravel(int floor)
        {
            if ((floor - 1) > (numberOfFloors - floor))
            {
                return "down";
            }
            else if ((floor - 1) < (numberOfFloors - floor))
            {
                return "up";
            }
            else
            {
                return "down";
            }
        }

        private List<int> IncorporateCalledFloors(List<int> pathForSelectedFloors, List<int> calledFloors)
        {
            // Path for only the selected floors.
            pathForSelectedFloors = new(pathForSelectedFloors);

            // Determining which called floors are passed-by when travelling between selected destination floors:

            List<List<int>> passedByCalledFloorsGoingUp = new();
            List<List<int>> passedByCalledFloorsGoingDown = new();
            List<int> remainingCalledFloors = new();
            for (int t = 0; t < pathForSelectedFloors.Count - 1; ++t)
            {
                foreach (int calledFloor in calledFloors)
                {
                    if (calledFloor > pathForSelectedFloors[t] & calledFloor < pathForSelectedFloors[t + 1])
                    {
                        if (!pathForSelectedFloors.Contains(calledFloor))
                        {
                        passedByCalledFloorsGoingUp.Add(new List<int>(new int[] { t + 1, calledFloor }));
                    }
                    }
                    else if (calledFloor < pathForSelectedFloors[t] & calledFloor > pathForSelectedFloors[t + 1])
                    {
                        if (!pathForSelectedFloors.Contains(calledFloor))
                        {
                        passedByCalledFloorsGoingDown.Add(new List<int>(new int[] { t + 1, calledFloor }));
                    }
                    }
                    else
                    {
                        remainingCalledFloors.Add(calledFloor);
                    }
                }
            }

            // Determining the most probable travel direction of the callers at each floor:

            List<List<int>> calledFloorsGoingInRightDirection = new();

            if (passedByCalledFloorsGoingUp.Count != 0)
            {
                IEnumerable<List<int>> calledFloorsWithUpTravelDirection =
                    from term in passedByCalledFloorsGoingUp
                    where DetermineDirectionOfMostProbableTravel(term[1]) == "up"
                    select term;
                foreach (List<int> term in calledFloorsWithUpTravelDirection)
                {
                    calledFloorsGoingInRightDirection.Add(term);
                    break;
                }
            }
            if (passedByCalledFloorsGoingDown.Count != 0)
            {
                IEnumerable<List<int>> calledFloorsWithDownTravelDirection =
                    from term in passedByCalledFloorsGoingDown
                    where DetermineDirectionOfMostProbableTravel(term[1]) == "down"
                    select term;
                foreach (List<int> term in calledFloorsWithDownTravelDirection)
                {
                    calledFloorsGoingInRightDirection.Add(term);
                    break;
                }
            }

            // Selecting if available the first appropriate caller to pick up:

            if (calledFloorsGoingInRightDirection.Count != 0)
            {
                Console.WriteLine("Passed-by floors with right travel direction = {0}", calledFloorsGoingInRightDirection.Count);
                int p = 0;
                while (p < pathForSelectedFloors.Count)
                {
                    foreach (List<int> term in calledFloorsGoingInRightDirection)
                    {
                        if (term[0] == p & !pathForSelectedFloors.Contains(term[1]))
                        {
                            pathForSelectedFloors.Insert(term[0], term[1]);
                            return pathForSelectedFloors;
                        }
                    }
                    ++p;
                }
            }
            Console.WriteLine("Passed-by floors with right travel direction = {0}", calledFloorsGoingInRightDirection.Count);
            // Treating the remaining called floors as if they were selected floors.
            HashSet<int> expandedFloorSet = new(pathForSelectedFloors.GetRange(1, pathForSelectedFloors.Count - 1));
            remainingCalledFloors.ForEach((int floor) => expandedFloorSet.Add(floor));
            Console.WriteLine("Remaining floors = {0}", expandedFloorSet.Count);
            return RouteOptimiser(expandedFloorSet);
        }
    }
}
