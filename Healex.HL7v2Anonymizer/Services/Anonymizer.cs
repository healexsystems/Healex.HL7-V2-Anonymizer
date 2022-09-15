using System;
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
                var rules = options.Segments.Where(replacement => replacement.Segment == segment.Name).ToList();
                if (rules.Count > 1)
                {
                    throw new ArgumentException(
                        $"Found multiple replacement configurations for segment {segment.Name}");
                }
                var rule = rules.FirstOrDefault();
                if (rule == null) continue;
                foreach (var replacement in rule.Replacements)
                {
                    segment.TrySetValue(replacement.Path, replacement.Value);
                }
            }
        }
        #endregion
    }
}