using Microsoft.VisualStudio.TestTools.UnitTesting;
using Healex.HL7v2Anonymizer.Services;


namespace Healex.HL7v2Anonymizer.Tests
{
    [TestClass]
    public class HashGeneratorTests
    {
        [TestMethod]
        public void TestGetStringHash()
        {
            var testPid = "48234029834029834";
            var hashedtestPid = HashGenerator.HashString(testPid);

            Assert.IsNotNull(hashedtestPid);
            Assert.AreEqual(hashedtestPid, "750678352");
        }
    }
}
