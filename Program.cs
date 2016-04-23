using System;
using System.IO;
using static GruntWurk.QuickLog;

namespace GruntWurk {
    class Program {
        public const string APP_NAME = "TextDataSimplifier";
        public static Options options;
        static IniFile spec;
        static int lineCount = 0;
        static int pageCount = 1;

        static void Main(string[] args) {
            try {
                bool keepGoing = LoadCommandLineOptions(args);
                if (keepGoing) {
                    spec = LoadSpecfication(options.SpecFilename);
                    ProcessingControls controls = new ProcessingControls(spec);
                    timestamp(APP_NAME + " Started");

                    TransformData(controls);
                    timestamp(APP_NAME + " Done");
                }
            } catch (Exception ex) {
                log("ERROR: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Parse the command-line options directly into the "options" field of this object.
        /// </summary>
        /// <param name="args">The command line arguments that were passed in from the operating system.</param>
        /// <returns>True if the program should proceed; otherwise, False if the user merely asked for a help screen.</returns>
        private static bool LoadCommandLineOptions(string[] args) {
            options = new Options();
            bool proceed = CommandLine.Parser.Default.ParseArguments(args, options);
            if (proceed) {
                options.Validate();

                InfoEnabled = options.Verbose;
                DebugEnabled = options.Debug;
                LogFilename = options.LogFilename;

                info("\n===============================================================================");
                info(options.ToString());
                info("");
            }
            return proceed;
        }

        /// <summary>
        /// Open the named file and load it into an IniFile object.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>An IniFile object</returns>
        private static IniFile LoadSpecfication(string filename) {
            if (!File.Exists(filename)) {
                throw new FileNotFoundException("Spec file does not exist.", filename);
            }
            IniFile spec = new IniFile();
            spec.LoadFile(filename);
            if (DebugEnabled) {
                spec.Dump();
            }
            return spec;
        }


        private static void TransformData(ProcessingControls controls) {
            lineCount = 0;
            pageCount = 0;
            PageData data = new PageData(controls.prof);

            using (StreamWriter outputFile = new StreamWriter(options.OutputFilename)) {
                outputFile.WriteLine(data.ColumnTitles("\t"));
                if (!File.Exists(options.InputFilename)) {
                    throw new FileNotFoundException("Input file does not exist.", options.InputFilename);
                }
                PageReader page = new PageReader(options.InputFilename);
                while (!page.AtLastPage) {
                    page.FetchNextPage();
                    if (NumberUtils.InRange(page.PageNo, controls.pageStart, controls.pageEnd, page.PageCount)) {
                        foreach (string line in data.DataValues(page.PageNo, page.PageLines, "\t")) {
                            outputFile.WriteLine(line);
                        }
                    }
                    lineCount += page.LineCount;
                    pageCount++;
                }
                info("Total Lines in input file: {0}", lineCount);
                info("Total Pages in input file: {0}", pageCount);
                info("");
            }
        }
    }
}
