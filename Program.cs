using System.Timers;

namespace MedicineChest
{
    internal class Program
    {
        public static int time = 0;
        private static System.Timers.Timer timer = new(1000);
        private static int LastCallTime;
        public static string[] InputCSVHeadings = {};
        public static List<List<int>> liftCallsList = new();
        public static Dictionary<int, List<int>> liftCallsDict = new();
        private static Lift lift = new();
        private static Dictionary<int, List<int>> CallerJourneyDetails = new();
        private static List<List<object>> LiftJourneyDetails = new();

        static void Main(string[] args)
        {
            // Load user input CSV file.
            ProcessInputCSV();
            LastCallTime = liftCallsList.Last()[3];
            //start the timer.
            timer.AutoReset = true;
            timer.Elapsed += UpdateTime;
            timer.Start();
            Console.Clear();
            Console.WriteLine("Lift now operational, timer started.");
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
                // add code here for writing total time to CSV
                Environment.Exit(0);
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
                    Console.WriteLine("Please enter the file path of the csv file, not enclosed in quotes.");
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

        public static int ConvertStrToInt(string str)
        {
            return int.Parse(str);
        }
    }
}