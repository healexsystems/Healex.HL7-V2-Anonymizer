namespace Healex.HL7v2Anonymizer {

    public class ReplacementOptions {

        public SegmentReplacement[] Segments { get; set; }

        public class SegmentReplacement {

            public string Segment { get; set; }

            public Replacement[] Replacements { get; set; }

        }

        public class Replacement {

            public string Path { get; set; }

            public string Value { get; set; }

        }
    }
}
