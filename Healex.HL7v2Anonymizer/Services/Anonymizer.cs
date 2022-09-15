using HL7.Dotnetcore;
using System.Linq;
using Healex.HL7v2Anonymizer.Extensions;

namespace Healex.HL7v2Anonymizer.Services
{
    public class Anonymizer
    {
        #region Private Members

        private readonly ReplacementOptions options;

        #endregion

        #region Constructors

        public Anonymizer(ReplacementOptions options)
        {
            this.options = options;
        }

        #endregion

        #region Public API

        public void Anonymize(Message message)
        {
            foreach (var segment in message.Segments())
            {
                var rules = options.Segments.Where(replacement => replacement.Segment == segment.Name)
                    .Select(replacement => replacement.Replacements);
                foreach (var rule in rules)
                {
                    foreach (var replacement in rule)
                    {
                        segment.TrySetValue(replacement.Path, replacement.Value);
                    }
                }
            }
        }
        #endregion
    }
}