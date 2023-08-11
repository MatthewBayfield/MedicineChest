namespace MedicineChest.Tests
{
    using Xunit;
    public class LiftBaseTests
    {
        // Derived class created to get a class instance that derives from LiftBase, as well as to be able to access protected methods.
        internal class Lift : LiftBase
        {
            override public void OperateLift()
            {
            }
        }

        private Lift lift = new();

        public LiftBaseTests() 
        {
            // External Program static variable reinitialisations
            Program.terminate = false;
            Program.liftCallsList.Clear();
            Program.liftCallsDict.Clear();
            Program.QuickMode = false;
            // Initial assignments
            Program.liftCallsDict.Add(1, new List<int>(new int[] { 2, 1, 5 }));
            Program.liftCallsDict.Add(2, new List<int>(new int[] { 1, 9, 17 }));
            Program.liftCallsDict.Add(3, new List<int>(new int[] { 10, 5, 32 }));
        }        

        [Fact]
        public void LiftBase_PopulatesFloorCounterDicts()
        {
            // Act
            // Lift instance creation

            // Assert
            Assert.Equal(10, lift.selectedFloorCounters.Count);
            int value1 = Assert.Contains("x_1", (IDictionary<string, int>)lift.selectedFloorCounters);
            Assert.Equal(0, value1);
            int value2 = Assert.Contains("x_5", (IDictionary<string, int>)lift.selectedFloorCounters);
            Assert.Equal(0, value2);
            int value3 = Assert.Contains("x_10", (IDictionary<string, int>)lift.selectedFloorCounters);
            Assert.Equal(0, value3);

            Assert.Equal(10, lift.calledFloorCounters.Count);
            value1 = Assert.Contains("y_1", (IDictionary<string, int>)lift.calledFloorCounters);
            Assert.Equal(0, value1);
            value2 = Assert.Contains("y_5", (IDictionary<string, int>)lift.calledFloorCounters);
            Assert.Equal(0, value2);
            value3 = Assert.Contains("y_10", (IDictionary<string, int>)lift.calledFloorCounters);
            Assert.Equal(0, value3);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void UpdateCalledFloors_PassedCallerIDList_UpdatesCalledFloorsLiveSet(int ID1, int ID2, int ID3)
        {

            using (StringWriter consoleOutput = new())
            {
                // Arrange               
                Console.SetOut(consoleOutput);

                // Act
                lift.UpdateCalledFloors(new List<int>(new int[] { ID1, ID2, ID3 }));

                // Assert
                Assert.Equal(3, lift.calledFloorsLive.Count);
                Assert.Contains(2, lift.calledFloorsLive);
                Assert.Contains(1, lift.calledFloorsLive);
                Assert.Contains(10, lift.calledFloorsLive);
                using (StringReader reader = new(consoleOutput.ToString()))
                {
                    Assert.Equal($"Lift called from floor 2 at t = 5, by callerID = {ID1}", reader.ReadLine());
                    Assert.Equal($"Lift called from floor 1 at t = 17, by callerID = {ID2}", reader.ReadLine());
                    Assert.Equal($"Lift called from floor 10 at t = 32, by callerID = {ID3}", reader.ReadLine());
                }
            }
        }   
    }
}
