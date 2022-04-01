using HL7.Dotnetcore;
using System;
using System.Linq;
using static Healex.HL7v2Anonymizer.ReplacementOptions;
using System.Collections.Generic;

namespace Healex.HL7v2Anonymizer.Services {

    public class Anonymizer {

        #region Private Members

        private readonly ReplacementOptions options;

        #endregion

        #region Constructors

        public Anonymizer(ReplacementOptions options) {
            this.options = options;
        }

        #endregion

        #region Public API

        public void Anonymize(Message message) {
            for (var index = 0; index < message.SegmentCount; index++) {
                var segment = message.Segments()[index]; 
                var substitution = options.Segments.FirstOrDefault(s => s.Segment == segment.Name);
                if (substitution != null) {
                    foreach (var replacement in substitution.Replacements) {
                        var value = Replacement(replacement, message);
                        TryReplaceValue(message, replacement.Path, value);
                    }
                }
            }
        }


        #endregion

        #region Auxiliar Methods

        private static bool TryReplaceValue(Message message, string path, string value) {
            try {
                message.SetValue(path, value);
            }
            catch (HL7Exception) {
                // Throws if segment is not present
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private static string Replacement(Replacement replacement, Message message) {
            if (replacement.Value == "HASH") {
                try {
                    var value = message.GetValue(replacement.Path);
                    var hash = HashGenerator.HashString(value);
                    return hash;
                }
                catch { }
            }
            return replacement.Value;
        }

        #endregion

    }
}
