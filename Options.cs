using System.IO;
using CommandLine;

namespace GruntWurk {
    class Options: CommonOptions {
        [Option('f', "file", Required = true, HelpText = "Name of input file to be processed.")]
        public string InputFilename { get; set; }

        [Option('s', "spec", Required = false, HelpText = "Name of corresponding specification file. (Default is the input filename with an INI extension.)")]
        public string SpecFilename { get; set; }

        [Option('o', "outfile", Required = false, HelpText = "Name of output file to be saved. (Default is the input filename with a .PART_{0}.TSV extension, where {0} will be filled in with the part number.)")]
        public string OutputFilename { get; set; }

        [Option('l', "logfile", Required = false, HelpText = "Name of a log file to be appended to.")]
        public string LogFilename { get; set; }

        public void Validate() {
            if (SpecFilename == null) {
                SpecFilename = Path.ChangeExtension(InputFilename, "INI");
            }
            if (OutputFilename == null) {
                OutputFilename = Path.ChangeExtension(InputFilename, "TSV");
            }
        }
        public override string ToString() {
            return string.Format("Input Filename: {0}\nSpec Filename: {1}\nOutput Filename: {2}", InputFilename, SpecFilename, OutputFilename);
        }
    }
}
