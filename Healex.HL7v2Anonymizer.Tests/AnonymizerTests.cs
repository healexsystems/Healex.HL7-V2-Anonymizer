using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using static Healex.HL7v2Anonymizer.ReplacementOptions;

namespace Healex.HL7v2Anonymizer.Tests {

    [TestClass]
    public class AnonymizerTests {

        #region Test Methods

        [TestMethod]
        [DeploymentItem("Healex.HL7v2Anonymizer.Tests/TestData", "TestData")]
        public void AnonymizerTestAdt() {
            TestAnonymization(File.ReadAllText(@"./TestData/TestAdt1.hl7"));
        }

        [TestMethod]
        [DeploymentItem("Healex.HL7v2Anonymizer.Tests/TestData", "TestData")]
        public void AnonymizerTestOru() {
            TestAnonymization(File.ReadAllText(@"./TestData/TestOru1.hl7"));
        }

        [TestMethod]
        [DeploymentItem("Healex.HL7v2Anonymizer.Tests/TestData", "TestData")]
        public void AnonymizerTestAdtSegmentOrderWithAnonymization() {
            // Setup
            var messageContent = File.ReadAllText(@"./TestData/TestAdt1.hl7");
            var originalMessage = new Message(messageContent);
            var message = new Message(messageContent);

            message.ParseMessage();
            originalMessage.ParseMessage();

            // Execute
            var replacementOptions = GetReplacementOptions();
            var anonymizer = new Anonymizer(replacementOptions);
            anonymizer.Anonymize(message);

            // Assert
            Assert.IsTrue(originalMessage.SegmentCount == message.SegmentCount);
            for (var i = 0; i < originalMessage.SegmentCount; i++) {
                var originalSegment = originalMessage.Segments().ElementAt(i);
                var messageSegment = message.Segments().ElementAt(i);
                Assert.AreEqual(originalSegment.Value, messageSegment.Value, $"Segments {originalSegment.Name} and {messageSegment.Name} at index {i} should be equal.");
            }
        }

        #endregion

        #region Auxiliar Methods

        private void TestAnonymization(string messageContent) {
            var originalMessage = new Message(messageContent);
            var message = new Message(messageContent);

            // Setup
            var replacementOptions = GetReplacementOptions();
            message.ParseMessage();
            originalMessage.ParseMessage();

            // Method under test
            var anonymizer = new Anonymizer(replacementOptions);
            anonymizer.Anonymize(message);

            // Assert
            foreach (SegmentReplacement segment in replacementOptions.Segments) {
                foreach (Replacement replacement in segment.Replacements) {
                    try {
                        var originalValue = originalMessage.GetValue(replacement.Path);
                        var newValue = message.GetValue(replacement.Path);

                        Assert.AreNotEqual(originalValue, newValue);
                        Assert.IsTrue(newValue == replacement.Value || newValue == HashGenerator.HashString(originalValue));
                    }
                    catch (HL7Exception) {
                        // Throws if segment is not present
                        continue;
                    }
                }
            }

            // Output for manual inspection
            var messageAsString = message.SerializeMessage(true);
            var originalMessageAsString = originalMessage.SerializeMessage(true);
            Console.WriteLine(messageAsString);
        }

        private static ReplacementOptions GetReplacementOptions() {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            var replacementOptions = config.GetSection("ReplacementOptions").Get<ReplacementOptions>();
            return replacementOptions;
        }

        #endregion

    }
}
