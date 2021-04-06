using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static Healex.HL7v2Anonymizer.ReplacementOptions;

namespace Healex.HL7v2Anonymizer.Tests
{
    [TestClass]
    public class AnonymizerTests
    {
        [TestMethod]
        [DeploymentItem("Healex.HL7v2Anonymizer.Tests/TestData", "TestData")]
        public void AnonymizerTestAdt()
        {
            TestAnonymization(File.ReadAllText(@"./TestData/TestAdt1.hl7"));
        }

        [TestMethod]
        [DeploymentItem("Healex.HL7v2Anonymizer.Tests/TestData", "TestData")]
        public void AnonymizerTestOru()
        {
            TestAnonymization(File.ReadAllText(@"./TestData/TestOru1.hl7"));
        }

        public void TestAnonymization(string messageContent)
        {
            var originalMessage = new Message(messageContent);
            var message = new Message(messageContent);

            // Setup
            var replacementOptions = getReplacementOptions();
            message.ParseMessage();

            // Method under test
            var anonymizer = new Anonymizer(replacementOptions);
            anonymizer.Anonymize(message);

            // Assert
            foreach (SegmentReplacement segment in replacementOptions.Segments)
            {
                foreach (Replacement replacement in segment.Replacements)
                {
                    try
                    {
                        var originalValue = originalMessage.GetValue(replacement.Path);
                        var newValue = message.GetValue(replacement.Path);

                        Assert.AreNotEqual(originalValue, newValue);
                        Assert.IsTrue(newValue == replacement.Value || newValue == HashGenerator.HashString(originalValue));
                    }
                    catch (HL7Exception)
                    {
                        // Throws if segment is not present
                        continue;
                    }
                }
            }

            // Output for manual inspection
            var messageAsString = message.SerializeMessage(true);
            Console.WriteLine(messageAsString);
        }

        private static ReplacementOptions getReplacementOptions()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            var replacementOptions = config.GetSection("ReplacementOptions").Get<ReplacementOptions>();
            return replacementOptions;
        }
    }
}
