using CommandLine;

namespace Healex.HL7v2Anonymizer.Options {

    [Verb("process", HelpText = "Reads directory name and processes all the HL7 files")]
    public class Process {


        [Option('d', "directory", Required = true, HelpText = "Directory name where all the HL7 files are located")]
        public string Directory { get; set; }
        
        [Option('o', "output", Required = false, HelpText = "Directory where changed HL7 files are written.")]
        public string OutputDirectory { get; set; }
        
        [Option('a', "append", Required = false, HelpText ="A suffix to append to all filenames. For example '_Anonymized'")]
        public string Suffix { get; set; }
    }
}
