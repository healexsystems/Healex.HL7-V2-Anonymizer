using HL7.Dotnetcore;
using System;
using static Healex.HL7v2Anonymizer.ReplacementOptions;

namespace Healex.HL7v2Anonymizer
{
    public class Anonymizer
    {
        private ReplacementOptions _replacementOptions;

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
                    // We use references so this overwrites the original segments
                    var tempMessage = new Message();
                    tempMessage.AddNewSegment(segment);

                    foreach (Replacement replacement in segmentReplacement.Replacements)
                    {
                        tryReplaceValue(replacement, tempMessage);
                    }
                }
            }
            return isSuccess;
        }

        private bool tryReplaceValue(Replacement replacement, Message message)
        {
            try
            {
                message.SetValue(replacement.Path, replacement.Value);
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
