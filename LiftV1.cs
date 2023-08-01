using SFERA.Math.Combinatorics;
using MoreLinq;

namespace MedicineChest
{
    internal class LiftV1: LiftBase
    {

        override public void OperateLift()
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

                    Console.WriteLine("selectedFloorCounters:");
                    foreach (KeyValuePair<string, int> pair in selectedFloorCounters)
                    {
                        Console.WriteLine("{0} = {1}", pair.Key, pair.Value);
                    }
                    Console.WriteLine("calledFloorCounters:");
                    foreach (KeyValuePair<string, int> pair in calledFloorCounters)
                    {
                        Console.WriteLine("{0} = {1}", pair.Key, pair.Value);
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
                    }
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
                    // Updating lift journey details.
                    Program.UpdateLiftJourneyDetails(currentFloor, timeWhenStopped, liftRiders, liftCallers, optimalPath);
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
                    // Updating lift journey details.
                    Program.UpdateLiftJourneyDetails(currentFloor, timeWhenStopped, liftRiders, liftCallers, optimalPath);
                    // Changing the current stop to the next stop to indicate the lift has arrived at the next stop.
                    currentFloor = nextFloorStop;
                    Console.WriteLine("Lift stopped at current floor = {0}, at t = {1}.", currentFloor, Program.time);
                }
            }
        }
    }
}
