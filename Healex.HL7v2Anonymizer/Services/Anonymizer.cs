using HL7.Dotnetcore;
using System;
using System.Linq;
using static Healex.HL7v2Anonymizer.ReplacementOptions;

namespace Healex.HL7v2Anonymizer.Services
{
    public class Anonymizer
    {
        private readonly ReplacementOptions _replacementOptions;

        public Anonymizer(ReplacementOptions replacementOptions)
        {
            _replacementOptions = replacementOptions;
        }

        public bool Anonymize(Message message)
        {
            var isSuccess = true;

            for (int segmentIndex = 0; segmentIndex < message.SegmentCount; segmentIndex++)
            {
                var segment = message.Segments().ElementAt(segmentIndex);
                if (_replacementOptions.Segments.FirstOrDefault(segRep => segRep.Segment == segment.Name)
                    is var segmentReplacement && segmentReplacement != null)
                {
                    // Create new temporary message for each repeating segment 
                    // because we can't set values in all repeating segments at once
                    var tempMessage = AddSegmentAtIndex(segment, segmentIndex);
                    foreach (Replacement replacement in segmentReplacement.Replacements)
                    {
                        var replacementValue = GetReplacementValue(replacement, message);
                        isSuccess = TryReplaceValue(tempMessage, replacement.Path, replacementValue);
                    }
                }
            }

            return isSuccess;
        }

        private Message AddSegmentAtIndex(Segment segment, int segmentIndex)
        {
            var tempMessage = new Message();

            // workaround to ensure the segment gets it absolute (internal) SequenceNo re-assigned in AddNewSegment()
            for (int i = 0; i < segmentIndex; i++)
            {
                tempMessage.AddNewSegment(new Segment("DummySegment", new HL7Encoding()));
            }
            tempMessage.AddNewSegment(segment);
            return tempMessage;
        }

        private string GetReplacementValue(Replacement replacement, Message message)
        {
            if (replacement.Value == "HASH")
            {
                try
                {
                    var valueToHash = message.GetValue(replacement.Path);
                    var hashedValue = HashGenerator.HashString(valueToHash);
                    return hashedValue;
                } catch
                {
                    // Could not find a value to hash in the HL7 message
                }
            }
            return replacement.Value;
        }

        private bool TryReplaceValue(Message message, string path, string replacementValue)
        {
            try
            {
                message.SetValue(path, replacementValue);
            }
            catch (HL7Exception)
            {
                // Throws if segment is not present
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
