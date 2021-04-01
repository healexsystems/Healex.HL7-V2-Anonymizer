using HL7.Dotnetcore;
using System;
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
            foreach (SegmentReplacement segmentReplacement in _replacementOptions.Segments)
            {
                var segments = message.Segments(segmentReplacement.Segment);
                foreach (Segment segment in segments)
                {
                    // Create new temporary message for each repeating segment 
                    // because we can't set values in all repeating segments at once
                    var tempMessage = new Message();
                    tempMessage.AddNewSegment(segment);

                    foreach (Replacement replacement in segmentReplacement.Replacements)
                    {
                        var replacementValue = GetReplacementValue(replacement, message);
                        TryReplaceValue(tempMessage, replacement.Path, replacementValue);
                    }
                }
            }
            return isSuccess;
        }

        private string GetReplacementValue(Replacement replacement, Message message)
        {
            if (replacement.Value == "HASH")
            {
                var valueToHash = message.GetValue(replacement.Path);
                var hashedValue = HashGenerator.HashString(valueToHash);
                return hashedValue;
            }
            else
            {
                return replacement.Value;
            }
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
