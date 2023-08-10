namespace MedicineChest.Tests
{
    using Xunit;
    public class LiftBaseTests
    {
        // class created to test inherited constructor 
        internal class Lift : LiftBase
        {
            override public void OperateLift()
            {
            }
        }

        [Fact]
        public void LiftBase_PopulatesFloorCounterDicts()
        {
            // Act
            Lift lift = new();

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
    }
}
