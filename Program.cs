using System;
using System.IO;
using static GruntWurk.QuickLog;

namespace GruntWurk {
    class Program {
        const string APP_NAME = "TextDataSimplifier";
        public static Options options;
        static int PartNoStart;
        static int PartNoEnd;
        static IniFile spec;
        static int lineCount = 0;
        static int pageCount = 1;

        static void Main(string[] args) {
            try {
                bool keepGoing = LoadCommandLineOptions(args);
                if (keepGoing) {
                    spec = LoadSpecfication(options.SpecFilename);
                    timestamp(APP_NAME + " Started");
                    DetermineWhichPartsToProcess();

                    for (int PartNo = PartNoStart; PartNo <= PartNoEnd; PartNo++) {
                        TransformFilePart(PartNo);
                    }
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

        private static void DetermineWhichPartsToProcess() {
            if (options.PartNumber == 0) {
                PartNoStart = 1;
                var parts = spec.FindMaxSectionName("PART ").Trim().Split(StringUtils.JUST_SPACE, 3);
                string s = (parts.Length >= 2) ? parts[1] : "";
                try {
                    PartNoEnd = int.Parse(s);
                } catch (Exception) {
                    PartNoEnd = PartNoStart;
                }
            } else {
                PartNoStart = options.PartNumber;
                PartNoEnd = PartNoStart;
            }
        }

        private static void TransformFilePart(int PartNo) {
            if (options.Verbose) {
                Console.WriteLine("Processing Part {0}...", PartNo);
            }
            lineCount = 0;
            pageCount = 0;

            PageProfile prof = new PageProfile();
            prof.InitializeFromIniFile(PartNo, spec);
            if (options.Debug) {
                prof.Dump();
            }
            using (StreamWriter outputFile = new StreamWriter(string.Format(options.OutputFilename, PartNo.ToString()))) {
                outputFile.WriteLine(prof.ColumnTitles("\t"));
                if (!File.Exists(options.InputFilename)) {
                    throw new FileNotFoundException("Input file does not exist.", options.InputFilename);
                }
                PageReader page = new PageReader(string.Format(options.InputFilename, options.PartNumber));
                while (!page.AtLastPage) {
                    page.FetchNextPage();
                    if (NumberUtils.InRange(page.PageNo, prof.PageStart, prof.PageEnd, page.PageCount)) {
                        foreach (string line in prof.DataValues(page.PageNo, page.PageLines, "\t")) {
                            outputFile.WriteLine(line);
                        }
                    }
                    lineCount += page.LineCount;
                    pageCount++;
                }
                if (options.Verbose) {
                    Console.WriteLine("Total Lines in input file: {0}", lineCount);
                    Console.WriteLine("Total Pages in input file: {0}", pageCount);
                    Console.WriteLine("");
                }
            }
        }
    }
}
