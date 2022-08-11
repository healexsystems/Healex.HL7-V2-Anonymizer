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

        public void Anonymize(Message message)
        {
            for (var index = 0; index < message.SegmentCount; index++) {
                var segment = message.Segments()[index]; 
                var substitution = options.Segments.FirstOrDefault(s => s.Segment == segment.Name);
                if (substitution == null) continue;

                var rules = new List<Replacement>();
                rules.AddRange(substitution.Replacements);
                
                foreach (var replacement in substitution.Replacements)
                {
                    var indices = GetIndices(replacement.Path);
                    var fieldIndex = indices[0];
                    if (segment.Fields(fieldIndex) == null || !segment.Fields(fieldIndex).HasRepetitions) continue;
                    
                    for (var i=1; i < segment.Fields(fieldIndex).Repetitions().Count; i++)
                    {
                        var newPath = $"{segment.Name}.{fieldIndex}({i+1})";
                        foreach (var j in indices.Skip(1))
                        {
                            newPath += $".{j}";
                        }
                        rules.Add(new Replacement{Path=newPath, Value=replacement.Value});
                    }

                }

                foreach (var replacement in rules)
                {
                    Console.WriteLine($"{replacement.Path}, {replacement.Value}");
                    var value = Replacement(replacement, message);
                    TryReplaceValue(message, replacement.Path, value);
                }
            }
        }


        #endregion

        #region Auxiliar Methods

        private static List<int> GetIndices(string path)
        {
            var retVal = new List<int>();
            var parts = path.Split(".");
            for (var i = 1; i < parts.Length; i++)
            {
                retVal.Add(int.Parse(parts[i]));
            }
            return retVal;
        }
        
        private static bool TryReplaceValue(Message message, string path, string value) {
            try {
                if(!string.IsNullOrEmpty(message.GetValue(path)))
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
