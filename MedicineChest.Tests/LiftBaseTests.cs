namespace MedicineChest.Tests
{
    using Moq;
    using Xunit;
    public class LiftBaseTests
    {
        // Derived class created to get a class instance that derives from LiftBase, as well as to be able to access protected methods.
        internal class Lift : LiftBase
        {
            override public void OperateLift()
            {
            }

            internal void DisembarkRidersWrapper(int timeWhenStopped)
            {
                base.DisembarkRiders(timeWhenStopped);
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

        [Theory]
        [InlineData(50, 7)]
        public void DisembarkRiders_AtCurrentFloorAndTime_RemovesRidersFromList(int timeWhenStopped, int currentFloor)
        {
            using (StringWriter consoleOutput = new())
            {
                // Arrange
                Program.liftCallsDict.Add(4, new List<int>(new int[] { 3, currentFloor, 25 }));
                Program.liftCallsDict.Add(5, new List<int>(new int[] { 5, currentFloor, 40 }));
                lift.liftRiders.Add(1);
                lift.liftRiders.Add(2);
                lift.liftRiders.Add(3);
                lift.liftRiders.Add(4);
                lift.liftRiders.Add(5);
                lift.currentFloor = currentFloor;
                Console.SetOut(consoleOutput);

                // Act
                lift.DisembarkRidersWrapper(timeWhenStopped);

                // Assert
                Assert.Equal(3, lift.liftRiders.Count);
                Assert.DoesNotContain(4, lift.liftRiders);
                Assert.DoesNotContain(5, lift.liftRiders);
                using (StringReader reader = new(consoleOutput.ToString()))
                {
                    Assert.Equal($"Rider with callerID = 4, departing at current floor = {currentFloor}, at t = {timeWhenStopped}", reader.ReadLine());
                    Assert.Equal($"Rider with callerID = 5, departing at current floor = {currentFloor}, at t = {timeWhenStopped}", reader.ReadLine());
                }
            }
        }        
    }
}
