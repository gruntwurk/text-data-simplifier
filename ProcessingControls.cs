using System.IO;
using static GruntWurk.NumberUtils;
using static GruntWurk.QuickLog;

namespace GruntWurk {
    /// <summary>
    /// This immutable data object contains strongly typed and validated copies of certain specifications that are expressed in the given INI file.
    /// </summary>
    class ProcessingControls {
        string _inputFileType;
        string _outputFileType;
        string _delimiterChar = ",";
        public int _pageStart;
        public int _pageEnd;
        PageProfile _prof;


        // This data object is immutable (No set-accessors on any of the properties)
        public string inputFileType { get { return _inputFileType; } }
        public string outputFileType { get { return _outputFileType; } }
        public string delimiterChar { get { return _delimiterChar; } }
        public int pageStart {  get { return _pageStart; } }
        public int pageEnd { get { return _pageEnd; } }
        public PageProfile prof { get { return _prof; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="spec"></param>
        public ProcessingControls(IniFile spec) {
            LoadControlSpecifications(spec);
            _prof = new PageProfile(spec);
        }

        private void LoadControlSpecifications(IniFile spec) {
            _inputFileType = spec.GetString("File", "Type", "").ToUpper();
            if (_inputFileType != "PAGINATED") {
                throw new FileLoadException(Program.APP_NAME + " currently only understands Paginated files. Spec file must positively specify Type=Paginated in the [File] section.");
            }

            _outputFileType = spec.GetString("Output", "Type", "TSV").ToUpper();
            if (_outputFileType == "CSV") {
                _delimiterChar = ",";
            } else if (_outputFileType == "TSV") {
                _delimiterChar = "\t";
            } else {
                throw new FileLoadException(Program.APP_NAME + " currently only outputs CSV and TSV files.");
            }

            _delimiterChar = spec.GetString("Output", "Delimiter", delimiterChar).ToUpper();
            if (_delimiterChar == "TAB" || _delimiterChar == "\\T") {
                _delimiterChar = "\t";
            }

            string arg = spec.GetString("Job", "Pages", "1..N");
            _pageStart = NumberUtils.StartOfRangeSpec(arg);
            _pageEnd = NumberUtils.EndOfRangeSpec(arg);


        }
       

    }

}
