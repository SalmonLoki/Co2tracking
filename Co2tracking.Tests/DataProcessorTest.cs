using NUnit.Framework;
using Co2tracking;

namespace Co2tracking.Tests
{
    [TestFixture]
    public class DataProcessorTest
    {
        [Test]
        public void decryptData_()
        {
            var key = new byte[] { 0xc4, 0xc6, 0xc0, 0x92, 0x40, 0x23, 0xdc, 0x96 };
            byte[] buffer = { 0xD6, 0x00, 0x00, 0x00, 0xA2, 0x00, 0x00, 0x60 };
            int[] expected = { 0x41, 0x00, 0x00, 0x41, 0x0D, 0x00, 0x00, 0x00 }; //change
            
            DataProcessor dataProcessor = new DataProcessor();
            int[] result = dataProcessor.
            
            Assert.True(true);
        }
    }
}