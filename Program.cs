using System.Timers;

namespace MedicineChest
{
    internal class Program
    {
        public static bool terminate = false;
        public static int time = 0;
        private static System.Timers.Timer timer = new(1000);
        private static int LastCallTime;
        public static string[] InputCSVHeadings = {};
        public static List<List<int>> liftCallsList = new();
        public static Dictionary<int, List<int>> liftCallsDict = new();
        private static Lift lift = new();
        private static Dictionary<int, List<int>> CallerJourneyDetails = new();
        private static List<List<object>> LiftJourneyDetails = new();
        private static string? OutputCSVDirectoryPath;
        public static bool QuickMode = false;

        static void Main(string[] args)
        {
            // Load user input CSV file.
            ProcessInputCSV();
            LastCallTime = liftCallsList.Last()[3];
            // Get user to specify a valid output directory to store the output CSV.
            OutputCSVDirectoryPath = GetOutputCSVDirectory();
            SetQuickModeParam();
            //start the timer.
            if (QuickMode)
            {
                timer.Interval = 100;
            }
            timer.AutoReset = true;
            timer.Elapsed += UpdateTime;
            timer.Start();
            Console.Clear();
            Console.WriteLine("Lift now operational, timer started.");
            // Start the lift.
            Task liftTask = new Task(lift.OperateLift);
            liftTask.Start();
            liftTask.Wait();
        }

        public static void UpdateTime(Object? sender, ElapsedEventArgs e)
        {
            time += 1;
            List<int> newCallers = new();
            IEnumerable<List<int>> timeMatches =
                from liftCall in liftCallsList
                where liftCall[3] == time
                select liftCall;
            foreach ( List<int> liftCall in timeMatches )
            {
                newCallers.Add(liftCall[0]);
            }
            if (newCallers.Count != 0)
            {
                lift.UpdateCalledFloors(newCallers);
            }
            // Ends program once all lift calls have occured and the lift is once again empty.
            if (time > LastCallTime & lift.selectedFloorsLive.Count == 0 & lift.calledFloorsLive.Count == 0)
            {
                timer.Stop();
                Console.WriteLine("Lift is now empty, no more calls exist.");
                terminate = true;
                // add code here for writing total time to CSV
            }

        }

        private static void ProcessInputCSV()
        {
            string currentWorkingDirectory = Directory.GetCurrentDirectory();
            string? filePath;
            string relativeFilePath;
            while (true)
            {
                try
                {
                    Console.WriteLine("Please enter the absolute file path of the csv file, not enclosed in quotes.");
                    filePath = Console.ReadLine();
                    if (filePath == null)
                    {
                        continue;
                    }
                    relativeFilePath = Path.GetRelativePath(currentWorkingDirectory, filePath);
                    if (Path.Exists(relativeFilePath))
                    {
                        break;
                    }
                    else if (filePath == "exit")
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("No such file path exists or the file path is invalid. Please check the file path is correct, or enter \"exit\" to exit the program.");
                    }
                }

                catch (IOException e)
                {
                    Console.WriteLine("No such file path exists or the file path is invalid. Please check the file path is correct.");
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("No such file path exists or the file path is invalid. Please check the file path is correct.");
                    Console.WriteLine(e.Message);
                }
            }

            try
            {
                using (var sr = new StreamReader(relativeFilePath))
                {
                    string? nextLine;
                    nextLine = sr.ReadLine();
                    if (nextLine != null)
                    {
                        InputCSVHeadings = nextLine.Split(',');
                        do
                        {
                            nextLine = sr.ReadLine();
                            if (nextLine != null)
                            {
                                List<int> nextRecord = new List<int>();
                                nextRecord.AddRange(Array.ConvertAll(nextLine.Split(','), new Converter<string, int>(ConvertStrToInt)));
                                int callerID = nextRecord[0];
                                liftCallsDict.Add(callerID, nextRecord.GetRange(1, 3));
                                liftCallsList.Add(nextRecord);
                            }
                        } while (nextLine != null);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("There was an issue when trying to open the file using the file path provided.");
                Console.WriteLine("Please restart the program and try again, if the problem persists check the file and or file path.");
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("There was an issue when trying to open the file using the file path provided.");
                Console.WriteLine("Please restart the program and try again, if the problem persists check the file and or file path.");
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an issue when trying to open the file using the file path provided.");
                Console.WriteLine("Please restart the program and try again, if the problem persists check the file and or file path.");
                Console.WriteLine(e.Message);
                Environment.Exit(0);
            }
        }

        private static string GetOutputCSVDirectory()
        {
            string currentWorkingDirectory = Directory.GetCurrentDirectory();
            string? path;
            string relativePath;
            while (true)
            {
                try
                {
                    Console.WriteLine("Please enter the absolute path to a valid directory to hold the output csv file, not enclosed in quotes.");
                    path = Console.ReadLine();
                    if (path == null)
                    {
                        continue;
                    }
                    relativePath = Path.GetRelativePath(currentWorkingDirectory, path);
                    if (Path.Exists(relativePath))
                    {
                        return path;
                    }
                    else if (path == "exit")
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("No such directory path exists or the directory path is invalid. Please check the path is correct, or enter \"exit\" to exit the program.");
                    }
                }

                catch (IOException e)
                {
                    Console.WriteLine("No such directory path exists or the directory path is invalid. Please check the path is correct.");
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("No such directory path exists or the directory path is invalid. Please check the path is correct.");
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void SetQuickModeParam()
        {
            string? input = "r";
            Console.WriteLine("In this prototype algorithm, the average lift stop time (20s), and the time to travel between adjacent floors (10s) are simulated.");
            Console.WriteLine("By default the algorthm runs in real-time; but it can run in a quick mode whereby the timer is sped up.");
            Console.WriteLine("To run in Quick mode please enter \"q\", or otherwise enter \"r\".");
            while (true)
            {
                try
                {
                    
                    input = Console.ReadLine();
                    if (input == null)
                    {
                        continue;
                    }
                    if (input == "q")
                    {
                        QuickMode = true;
                        break;
                    }
                    else if (input == "r")
                    {
                        QuickMode = false;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid entry, please enter either \"q\" or \"r\".");
                    }
                }

                catch (IOException e)
                {
                    Console.WriteLine("Invalid entry, please enter either \"q\" or \"r\".");
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("Invalid entry, please enter either \"q\" or \"r\".");
                    Console.WriteLine(e.Message);
                }
            }
        }

        public static int ConvertStrToInt(string str)
        {
            return int.Parse(str);
        }

        public static void UpdateLiftJourneyDetails(int currentFloor, int time, HashSet<int> liftRidersAfterStop, HashSet<int> liftCallersAfterStop, List<int> routeAfterStop)
        {
            List<object> liftRecord = new List<object>(new object[] { time, currentFloor, liftRidersAfterStop, liftCallersAfterStop, routeAfterStop });
            LiftJourneyDetails.Add(liftRecord);
        }

        public static void UpdateCallerJourneyDetails(List<int> callerIDs, int? timeboarded = null, int? timeDisembarked = null)
        {
            foreach (int callerID in callerIDs)
            {
                if (!CallerJourneyDetails.ContainsKey(callerID))
                {
                    CallerJourneyDetails.Add(callerID, new List<int>());
                }
                if (timeboarded != null)
                {
                    CallerJourneyDetails[callerID].Add((int)timeboarded);
                }
                if (timeDisembarked != null)
                {
                    CallerJourneyDetails[callerID].Add((int)timeDisembarked);
                }
            }
        }

        public static void WriteToOutputCSV()
        {

        }
    }
}