using System.Timers;

namespace MedicineChest
{
    internal class Program
    {
        public static int time = 0;
        private static System.Timers.Timer timer = new(1000);
        private static int LastCallTime;
        public static string[] InputCSVHeadings = {};
        private static List<List<int>> liftCallsList = new();
        private static Dictionary<int, List<int>> liftCallsDict = new();

        static void Main(string[] args)
        {
            // Load user input CSV file.
            ProcessInputCSV();
            LastCallTime = liftCallsList.Last()[3];
            // create lift object and start the timer.
            Lift lift = new();
            timer.AutoReset = true;
            timer.Elapsed += UpdateTime;
            timer.Start();
            while (time < LastCallTime | lift.selectedFloorsLive.Count != 0 | lift.calledFloorsLive.Count != 0)
            {

            }
        }

        public static void UpdateTime(Object? sender, ElapsedEventArgs e)
        {
            time += 1;
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