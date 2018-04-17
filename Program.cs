using System;
using System.IO;
using static GruntWurk.QuickLog;
using CommandLine;

namespace GruntWurk {
    class Program {
        public const string APP_NAME = "TextDataSimplifier";
        static IniFile spec;
        static int lineCount = 0;
        static int pageCount = 1;

        static void Main(string[] args) {
            try {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(opts => RunSimplifier(opts));
            } catch (Exception e) {
                log("ERROR: {0}", e);
            }
        }

        private static void RunSimplifier(Options options) {
            options.Validate();

            InfoEnabled = options.Verbose;
            DebugEnabled = options.Debug;
            LogFilename = options.LogFilename;

            info("\n===============================================================================");
            info(options.ToString());
            info("");

            spec = LoadSpecfication(options.SpecFilename);
            ProcessingControls controls = new ProcessingControls(spec);
            timestamp(APP_NAME + " Started");

            TransformData(controls, options);
            timestamp(APP_NAME + " Done");
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


        private static void TransformData(ProcessingControls controls, Options options) {
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
