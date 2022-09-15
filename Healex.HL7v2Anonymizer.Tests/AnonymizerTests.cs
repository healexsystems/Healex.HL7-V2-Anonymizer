using Healex.HL7v2Anonymizer.Services;
using HL7.Dotnetcore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using FluentAssertions;
using static Healex.HL7v2Anonymizer.ReplacementOptions;

namespace Healex.HL7v2Anonymizer.Tests
{
    [TestClass]
    public class AnonymizerTests
    {
        #region helperObjects
        private Anonymizer ruleExpander;
        #endregion
        
        #region Test Methods

        [TestMethod]
        public void Anonymize_RepeatingSegment_AllSegmentsAnonymized()
        {
            var testMessage = CreateDefaultTestMessage();
            var testReplacements = CreateDefaultReplacementOptions();

            var anonymizer = new Anonymizer(testReplacements);
            anonymizer.Anonymize(testMessage);
            var IN1s = testMessage.Segments("IN1");
            IN1s.Count.Should().Be(2);
            IN1s[0].Fields(2).Components(1).Value.Should().Be("InsuranceShort");
            IN1s[1].Fields(2).Components(1).Value.Should().Be("");
            IN1s[0].Fields(2).Components(2).Value.Should().Be("InsuranceLong");
            IN1s[1].Fields(2).Components(2).Value.Should().Be("InsuranceLong");
        }
        
        [TestMethod]
        public void Anonymize_NoRepetitionPid5_PatientNameNotInMessage()
        {
            var testMessage = CreateDefaultTestMessage();
            var testReplacements = CreateDefaultReplacementOptions();
            
            var anonymizer = new Anonymizer(testReplacements);
            anonymizer.Anonymize(testMessage);
            var x = testMessage.SerializeMessage(false);
            x.Should().NotContain("Mustermann").And.NotContain("Maximilian");
        }
        
        [TestMethod]
        public void Anonymize_RepetitionPid5_PatientNameNotInMessage()
        {
            var testMessage = CreateDefaultTestMessage(4);
            var testReplacements = CreateDefaultReplacementOptions();
            
            var anonymizer = new Anonymizer(testReplacements);
            anonymizer.Anonymize(testMessage);

            var x = testMessage.SerializeMessage(false);
            x.Should().NotContain("Mustermann").And.NotContain("Maximilian");
        }
        
        [TestMethod]
        public void AnonymizerTestAdt()
        {
            TestAnonymization(CreateDefaultTestMessage());
        }

        [TestMethod]
        public void AnonymizerTestAdtSegmentOrderWithAnonymization()
        {
            // Setup

            var originalMessage = CreateDefaultTestMessage();
            var message = CreateDefaultTestMessage();

            message.ParseMessage();
            originalMessage.ParseMessage();

            // Execute
            var replacementOptions = CreateDefaultReplacementOptions();
            var anonymizer = new Anonymizer(replacementOptions);
            anonymizer.Anonymize(message);

            // Assert
            Assert.IsTrue(originalMessage.SegmentCount == message.SegmentCount);
            for (var i = 0; i < originalMessage.SegmentCount; i++)
            {
                var originalSegment = originalMessage.Segments().ElementAt(i);
                var messageSegment = message.Segments().ElementAt(i);
                Assert.AreEqual(originalSegment.Value, messageSegment.Value,
                    $"Segments {originalSegment.Name} and {messageSegment.Name} at index {i} should be equal.");
            }
        }

        #endregion

        #region Auxiliar Methods

        private void TestAnonymization(Message originalMessage)
        {
            var anonymizedMessage = new Message(originalMessage.SerializeMessage(false));

            // Setup
            var replacementOptions = CreateDefaultReplacementOptions();
            anonymizedMessage.ParseMessage();
            originalMessage.ParseMessage();

            // Method under test
            var anonymizer = new Anonymizer(replacementOptions);
            anonymizer.Anonymize(anonymizedMessage);

            // Assert
            foreach (SegmentReplacement segment in replacementOptions.Segments)
            {
                foreach (Replacement replacement in segment.Replacements)
                {
                    try
                    {
                        var originalValue = originalMessage.GetValue(replacement.Path);
                        var newValue = anonymizedMessage.GetValue(replacement.Path);

                        if (!string.IsNullOrEmpty(originalValue))
                        {
                            Assert.AreNotEqual(originalValue, newValue);
                            Assert.IsTrue(newValue == replacement.Value ||
                                          newValue == HashGenerator.HashString(originalValue));
                        }
                    }
                    catch (HL7Exception)
                    {
                        // Throws if segment is not present
                        continue;
                    }
                }
            }
        }
        #endregion

        #region TestData
        
        private static Message CreateDefaultTestMessage(int repetitionsPid5 = 0)
        {
            var pid5 = "Mustermann^Maximilian^^^^^L^A";
            for (var i = 0; i < repetitionsPid5; i++)
            {
                pid5 += $"~Mustermann{i}^Maximilian{i}^^^^^L^A";
            }

            var messageString =
                "MSH|^~\\&|iMedOne^BTS|Ein1^123456789|Cloverleaf|Cloverleaf|20220101000000||ADT^A01|12345678|P|2.4|||AL|NE\n" +
                "EVN|A01|20220101000000|||NeoGeo|20220101000000|Ein1\n" +
                $"PID|1||101010^^^Ein1^PI||{pid5}||19990101|M|||Musterstraße 1^^Musterstadt^^4830^D^H~^^^^^^O||0180-12345^PRN^PH^^^^018012345~^PRN^FX^~^PRN^CP^~^NET^X.400^|^WPN^PH|Deutsch|verheiratet|KA||||||Malchin|N||D|Arbeiter|||N|||20220101000000\n" +
                "NK1|1|Mustermann^Maxima|Ehefrau|Musterstraße 1^^Musterstadt^^4830^D|0180-12345^NO|||20200101|||||||||||||||||||||||||1010\n" +
                "NK1|2|Mustermann^Maxima|Ehefrau|Musterstraße 1^^Musterstadt^^4830^D|0180-12345^NO|||20200101|||||||||||||||||||||||||101010\n" +
                "PV1|1|I|A4^^^RMED^^N|Notfall||^^^^^N|||123456789^Mustertyp^Maximus^^^Dr. med.^^LANR^^^^^^123456789|R-N||J||||0|||1234567^^^Ein1^VN|Standard||||K||||20220101000000||||||||0101||||||||20220101000000|\n" +
                "PV2|||0101|||||||||Notfall||||||||||||||||||||||||N\n" +
                "IN1|1|VKK^VdAK|123456789^^^DAK^NII~K101^^^DAK^NIIP|DAK|Postfach^^Musterburg^^2000^DE|~|||primaer|||19980202|||R^1|^||19980202|Musterallee^^^^2000^D||||||||||20220101|||||||D123456789|||||||M||||||D123456789\n"+
                "IN1|2|^Mimimimi|123456789^^^DAK^NII~K101^^^DAK^NIIP|DAK|Postfach^^Musterburg^^2000^DE|~|||primaer|||19980202|||R^1|^||19980202|Musterallee^^^^2000^D||||||||||20220101|||||||D123456789|||||||M||||||D123456789\n";

            var message = new Message(messageString);
            message.ParseMessage();
            return message;
        }

        private static ReplacementOptions CreateDefaultReplacementOptions()
        {
            // we are only testing with PID per default
            var replacementOptions = new ReplacementOptions
            {
                Segments = new[]
                {
                    new SegmentReplacement
                    {
                        Segment = "PID",
                        Replacements = new[]
                        {
                            new Replacement { Path = "PID.2.1", Value = "HASH" },
                            new Replacement { Path = "PID.3.1", Value = "HASH" },
                            new Replacement { Path = "PID.4.1", Value = "HASH" },
                            new Replacement { Path = "PID.5.1", Value = "Family name" },
                            new Replacement { Path = "PID.5.2", Value = "Given name" },
                            new Replacement { Path = "PID.6.1", Value = "Family name" },
                            new Replacement { Path = "PID.6.2", Value = "Given name" },
                            new Replacement { Path = "PID.6.3", Value = "Second name" },
                            new Replacement { Path = "PID.9.1", Value = "Family name" },
                            new Replacement { Path = "PID.9.2", Value = "Given name" },
                            new Replacement { Path = "PID.9.3", Value = "Second name" },
                            new Replacement { Path = "PID.7", Value = "20000101" },
                            new Replacement { Path = "PID.11.1", Value = "Street" },
                            new Replacement { Path = "PID.11.2", Value = "Other" },
                            new Replacement { Path = "PID.11.3", Value = "City" },
                            new Replacement { Path = "PID.11.4", Value = "State" },
                            new Replacement { Path = "PID.11.5", Value = "A104" },
                            new Replacement { Path = "PID.11.6", Value = "DEU" },
                            new Replacement { Path = "PID.13.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "PID.13.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "PID.13.5", Value = "255" },
                            new Replacement { Path = "PID.13.6", Value = "026" },
                            new Replacement { Path = "PID.13.7", Value = "867-5309" },
                            new Replacement { Path = "PID.13.11", Value = "Speeddial" },
                            new Replacement { Path = "PID.13.12", Value = "867-5309" },
                            new Replacement { Path = "PID.14.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "PID.14.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "PID.14.5", Value = "255" },
                            new Replacement { Path = "PID.14.6", Value = "026" },
                            new Replacement { Path = "PID.14.7", Value = "867-5309" },
                            new Replacement { Path = "PID.14.12", Value = "867-5309" },
                            new Replacement { Path = "PID.18.1", Value = "MRN" },
                            new Replacement { Path = "PID.20.1", Value = "0112358" },
                            new Replacement { Path = "PID.29.1", Value = "20000101" },
                        }
                    },
                    new SegmentReplacement
                    {
                        Segment = "NK1",
                        Replacements = new[]
                        {
                            new Replacement { Path = "NK1.2.1", Value = "Family Name" },
                            new Replacement { Path = "NK1.2.2", Value = "Given Name" },
                            new Replacement { Path = "NK1.2.3", Value = "Second Name" },
                            new Replacement { Path = "NK1.4.1", Value = "Street Address" },
                            new Replacement { Path = "NK1.4.2", Value = "Other Designation" },
                            new Replacement { Path = "NK1.4.3", Value = "City" },
                            new Replacement { Path = "NK1.4.4", Value = "State or Province" },
                            new Replacement { Path = "NK1.4.5", Value = "A104" },
                            new Replacement { Path = "NK1.4.6", Value = "DEU" },
                            new Replacement { Path = "NK1.5.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "NK1.5.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "NK1.5.5", Value = "255" },
                            new Replacement { Path = "NK1.5.6", Value = "026" },
                            new Replacement { Path = "NK1.5.7", Value = "1-800-273-8255" },
                            new Replacement { Path = "NK1.6.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "NK1.6.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "NK1.6.5", Value = "255" },
                            new Replacement { Path = "NK1.6.6", Value = "026" },
                            new Replacement { Path = "NK1.6.7", Value = "1-800-273-8255" },
                            new Replacement { Path = "NK1.10", Value = "Job" },
                            new Replacement { Path = "NK1.12", Value = "Employee Number" },
                            new Replacement { Path = "NK1.13", Value = "Organization Name" },
                            new Replacement { Path = "NK1.16", Value = "20000101" },
                            new Replacement { Path = "NK1.26.1", Value = "Family Name" },
                            new Replacement { Path = "NK1.26.2", Value = "Given Name" },
                            new Replacement { Path = "NK1.26.3", Value = "Second Name" },
                            new Replacement { Path = "NK1.30.1", Value = "Family Name" },
                            new Replacement { Path = "NK1.30.2", Value = "Given Name" },
                            new Replacement { Path = "NK1.30.3", Value = "Second Name" },
                            new Replacement { Path = "NK1.31.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "NK1.31.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "NK1.31.5", Value = "255" },
                            new Replacement { Path = "NK1.31.6", Value = "026" },
                            new Replacement { Path = "NK1.31.7", Value = "1-800-273-8255" },
                            new Replacement { Path = "NK1.32.1", Value = "Street Address" },
                            new Replacement { Path = "NK1.32.2", Value = "Other Designation" },
                            new Replacement { Path = "NK1.32.3", Value = "City" },
                            new Replacement { Path = "NK1.32.4", Value = "State or Province" },
                            new Replacement { Path = "NK1.32.5", Value = "A104" },
                            new Replacement { Path = "NK1.32.6", Value = "DEU" },
                            new Replacement { Path = "NK1.37", Value = "Social Security Number" }
                        }
                    },
                    new SegmentReplacement
                    {
                        Segment = "IN1",
                        Replacements = new[]
                        {
                            new Replacement { Path = "IN1.2.1", Value = "InsuranceShort" },
                            new Replacement { Path = "IN1.2.2", Value = "InsuranceLong" },
                            new Replacement { Path = "IN1.6.1", Value = "Family Name" },
                            new Replacement { Path = "IN1.6.2", Value = "Given Name" },
                            new Replacement { Path = "IN1.6.3", Value = "Second name" },
                            new Replacement { Path = "IN1.7", Value = "HASH" },
                            new Replacement { Path = "IN1.11.1", Value = "Name" },
                            new Replacement { Path = "IN1.11.3", Value = "1" },
                            new Replacement { Path = "IN1.11.4", Value = "0112358" },
                            new Replacement { Path = "IN1.16.1", Value = "Familiy Name" },
                            new Replacement { Path = "IN1.16.2", Value = "Given Name" },
                            new Replacement { Path = "IN1.16.3", Value = "Second Name" },
                            new Replacement { Path = "IN1.19.1", Value = "Street Address" },
                            new Replacement { Path = "IN1.19.2", Value = "Other Designation" },
                            new Replacement { Path = "IN1.19.3", Value = "City" },
                            new Replacement { Path = "IN1.19.4", Value = "State or Province" },
                            new Replacement { Path = "IN1.19.5", Value = "A104" },
                            new Replacement { Path = "IN1.19.6", Value = "DEU" },
                            new Replacement { Path = "IN1.44.1", Value = "Street Address" },
                            new Replacement { Path = "IN1.44.2", Value = "Other Designation" },
                            new Replacement { Path = "IN1.44.3", Value = "City" },
                            new Replacement { Path = "IN1.44.4", Value = "State or Province" },
                            new Replacement { Path = "IN1.44.5", Value = "A104" },
                            new Replacement { Path = "IN1.44.6", Value = "DEU" },
                            new Replacement { Path = "IN1.18", Value = "20000101" }
                        }
                    },
                    new SegmentReplacement
                    {
                        Segment = "IN2",
                        Replacements = new[]
                        {
                            new Replacement { Path = "IN2.1.1", Value = "HASH" },
                            new Replacement { Path = "IN2.2", Value = "Social Security" },
                            new Replacement { Path = "IN2.3.1", Value = "Id Number" },
                            new Replacement { Path = "IN2.3.2", Value = "Family Name" },
                            new Replacement { Path = "IN2.3.3", Value = "Given Name" },
                            new Replacement { Path = "IN2.3.3", Value = "Given Name" },
                            new Replacement { Path = "IN2.3.4", Value = "Second Name" },
                            new Replacement { Path = "IN2.6", Value = "0112358" },
                            new Replacement { Path = "IN2.7.1", Value = "Family Name" },
                            new Replacement { Path = "IN2.7.2", Value = "Given Name" },
                            new Replacement { Path = "IN2.7.3", Value = "Second Name" },
                            new Replacement { Path = "IN2.8", Value = "0112358" },
                            new Replacement { Path = "IN2.10", Value = "Military ID number" },
                            new Replacement { Path = "IN2.25.1", Value = "HASH" },
                            new Replacement { Path = "IN2.26.1", Value = "HASH" },
                            new Replacement { Path = "IN2.40.1", Value = "Family Name" },
                            new Replacement { Path = "IN2.40.2", Value = "Given Name" },
                            new Replacement { Path = "IN2.40.3", Value = "Second Name" },
                            new Replacement { Path = "IN2.46", Value = "Job title" },
                            new Replacement { Path = "IN2.49.1", Value = "Family Name" },
                            new Replacement { Path = "IN2.49.2", Value = "Given Name" },
                            new Replacement { Path = "IN2.49.3", Value = "Second Name" },
                            new Replacement { Path = "IN2.50.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "IN2.50.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "IN2.50.5", Value = "255" },
                            new Replacement { Path = "IN2.50.6", Value = "026" },
                            new Replacement { Path = "IN2.50.7", Value = "867-5309" },
                            new Replacement { Path = "IN2.52.1", Value = "Family Name" },
                            new Replacement { Path = "IN2.52.2", Value = "Given Name" },
                            new Replacement { Path = "IN2.52.3", Value = "Second Name" },
                            new Replacement { Path = "IN2.53.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "IN2.53.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "IN2.53.5", Value = "255" },
                            new Replacement { Path = "IN2.53.6", Value = "026" },
                            new Replacement { Path = "IN2.53.7", Value = "867-5309" },
                            new Replacement { Path = "IN2.63.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "IN2.63.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "IN2.63.5", Value = "255" },
                            new Replacement { Path = "IN2.63.6", Value = "026" },
                            new Replacement { Path = "IN2.63.7", Value = "867-5309" },
                            new Replacement { Path = "IN2.64.1", Value = "1-800-273-8255" },
                            new Replacement { Path = "IN2.64.4", Value = "foo@bar.xyz" },
                            new Replacement { Path = "IN2.64.5", Value = "255" },
                            new Replacement { Path = "IN2.64.6", Value = "026" },
                            new Replacement { Path = "IN2.64.7", Value = "867-5309" }
                        }
                    }
                }
            };
            
            return replacementOptions;
        }
        #endregion
    }
}