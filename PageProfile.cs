using System;
using System.Collections.Generic;
using System.Linq;
using static GruntWurk.QuickLog;
using static GruntWurk.NumberUtils;
using static GruntWurk.StringUtils;

namespace GruntWurk {
    /// <summary>
    /// Describes a paginated data file (aka a "line printer report"), including specifications for the page header (if any),
    /// page footer (if any), and group headers (if any). 
    /// Also specifies whether data "rows" consist of one line each, or are wrapped onto continuation lines.
    /// Each band (page header, page foorter, group header, and data row) can have multiple fields defined.
    /// </summary>
    public class PageProfile {

        public bool HasHeader;
        public bool HasFooter;
        public bool HasGroup;
        public bool HasCondition;
        public int HeaderLinesStart;
        public int HeaderLinesEnd;
        public int DataLinesStart;
        public int DataLinesEnd;
        public int DataLinesPerRow;
        public int FooterLinesStart;
        public int FooterLinesEnd;
        public int GroupLinesPerRow;
        public FieldSpec GroupTrigger;
        public List<FieldSpec> HeaderFields;
        public List<FieldSpec> DataFields;
        public List<FieldSpec> GroupFields;
        public List<FieldSpec> FooterFields;
        public FieldSpec ConditionFieldSpec;
        public string ConditionValue;
        public string ConditionAction;

        /// <summary>
        /// Constructor #1
        /// </summary>
        public PageProfile() {
            Clear();
        }

        /// <summary>
        /// Constructor #2
        /// </summary>
        /// <param name="spec"></param>
        public PageProfile(IniFile spec) {
            Clear();
            InitializeFromIniFile(spec);
        }

        public void SetHeaderLines(string arg) {
            try {
                HasHeader = true;
                HeaderLinesStart = StartOfRangeSpec(arg);
                HeaderLinesEnd = EndOfRangeSpec(arg);
            } catch (FormatException) {
                throw new ArgumentException("Invalid Lines specification: " + arg, "Page Header section");
            }
        }


        public void SetFooterLines(string arg) {
            if (arg == "") {
                return;
            }
            try {
                HasFooter = true;
                FooterLinesStart = StartOfRangeSpec(arg);
                FooterLinesEnd = EndOfRangeSpec(arg);
            } catch (FormatException) {
                throw new ArgumentException("Invalid Lines specification: " + arg, "Footer section");
            }
        }
        public void SetDataLines(string arg) {
            if (arg == "") {
                return;
            }
            DataLinesStart = StartOfRangeSpec(arg);
            DataLinesEnd = EndOfRangeSpec(arg);
        }
        public void SetDataLinesPerRow(string arg) {
            if (arg == "") {
                return;
            }
            if (!int.TryParse(arg, out DataLinesPerRow)) {
                throw new ArgumentException("Invalid Lines Per Row: " + arg, "Data section");
            }
        }
        public void SetGroupLinesPerRow(string arg) {
            if (arg == "") {
                return;
            }
            HasGroup = true;
            if (!int.TryParse(arg, out GroupLinesPerRow)) {
                throw new ArgumentException("Invalid Lines Per Row: " + arg, "Data section");
            }
        }
        public void SetGroupTrigger(string arg) {
            if (arg == "") {
                return;
            }
            HasGroup = true;
            GroupTrigger = new FieldSpec(arg);
        }
        public void AddHeaderField(string arg) {
            if (arg == "") {
                return;
            }
            try {
                HeaderFields.Add(new FieldSpec(arg));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: " + arg, "Page Header section");
            }
        }
        public void AddDataField(string arg) {
            if (arg == "") {
                return;
            }
            try {
                DataFields.Add(new FieldSpec(arg));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: " + arg, "Data section");
            }

        }
        public void AddGroupField(string arg) {
            if (arg == "") {
                return;
            }
            try {
                GroupFields.Add(new FieldSpec(arg));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: " + arg, "Group Header section");
            }
        }
        public void AddFooterField(string arg) {
            if (arg == "") {
                return;
            }
            try {
                FooterFields.Add(new FieldSpec(arg));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: " + arg, "Page Footer section");
            }
        }
        private void Clear() {
            HasHeader = false;
            HeaderLinesStart = 0;
            HeaderLinesEnd = 0;
            DataLinesStart = 1;
            DataLinesEnd = 0;
            DataLinesPerRow = 1;
            HasFooter = false;
            HasCondition = false;
            FooterLinesStart = 0;
            FooterLinesEnd = 0;
            GroupLinesPerRow = 0;
            HeaderFields = new List<FieldSpec>();
            DataFields = new List<FieldSpec>();
            GroupFields = new List<FieldSpec>();
            FooterFields = new List<FieldSpec>();
        }

        private void InitializeFromIniFile(IniFile spec) {
            List<IniFileEntry> fieldEntries;
            SetHeaderLines(spec.GetString("Page Header", "Lines", ""));
            SetDataLines(spec.GetString("Data", "Lines", ""));
            SetDataLinesPerRow(spec.GetString("Data", "Lines Per Row", ""));
            SetGroupLinesPerRow(spec.GetString("Group Header", "Lines Per Row", ""));
            SetGroupTrigger(spec.GetString("Group Header", "Trigger", ""));
            SetFooterLines(spec.GetString("Page Footer", "Lines", ""));

            // Header fields
            fieldEntries = spec.GetAll("Page Header");
            foreach (IniFileEntry entry in fieldEntries) {
                if (entry.key == "Field") {
                    AddHeaderField(entry.value);
                }
            }

            // Data fields
            fieldEntries = spec.GetAll("Data");
            foreach (IniFileEntry entry in fieldEntries) {
                if (entry.key == "Field") {
                    AddDataField(entry.value);
                }
            }

            // Group fields
            fieldEntries = spec.GetAll("Group Header");
            foreach (IniFileEntry entry in fieldEntries) {
                if (entry.key == "Field") {
                    AddGroupField(entry.value);
                }
            }

            // Footer fields
            fieldEntries = spec.GetAll("Page Footer");
            foreach (IniFileEntry entry in fieldEntries) {
                if (entry.key == "Field") {
                    AddFooterField(entry.value);
                }
            }

            // TODO Change this to a field spec (like the group trigger)
            if (spec.Sections.ContainsKey("CONDITION")) {
                Dictionary<string, IniFileEntry> submap = spec.Sections["CONDITION"];
                if (submap.Count > 0) {
                    if (submap.Keys.Contains("FIELD") && submap.Keys.Contains("VALUE")) {
                        HasCondition = true;
                        ConditionFieldSpec = new FieldSpec(submap["FIELD"].value);
                        ConditionValue = submap["VALUE"].value;
                        if (submap.Keys.Contains("ACTION")) {
                            ConditionAction = submap["ACTION"].value;
                            if (ConditionAction != "Next Page") {
                                throw new ArgumentException("Invalid Condition Action. Only 'Next Page' is currenly supported.");
                            }
                        } else {
                            log("Condition action of 'Next Page' was assumed, even though it was not specified.");
                            ConditionAction = "Next Page";
                        }
                    } else {
                        throw new ArgumentException("Field or Value specification is missing", "Contition");
                    }
                }
            }
        }
        public void Dump() {
            log("===== Page Profile =====");
            List<string> elements = new List<string>();
            if (HasHeader) {
                elements.Add(string.Format("Page Header {0}..{1}", HeaderLinesStart, HeaderLinesEnd));
            } else {
                elements.Add("No Page Header");
            }
            if (HasGroup) {
                elements.Add(string.Format("Group Header {0} lines per row", GroupLinesPerRow));
                elements.Add(string.Format("Group Header trigger {0} @ line {1}, col {2}", GroupTrigger.FieldName, GroupTrigger.LineNo, GroupTrigger.ColNo));
            } else {
                elements.Add("No Group Header");
            }
            elements.Add(string.Format("Data {0}..{1}/{2}", DataLinesStart, DataLinesEnd, DataLinesPerRow));
            if (HasFooter) {
                elements.Add(string.Format("Page Footer {0}..{1}", FooterLinesStart, FooterLinesEnd));
            } else {
                elements.Add("No Page Footer");
            }
            log(string.Join("; ", elements));
            log("");
        }

        /// <summary>
        /// Returns the DataLinesEnd spec, adjusted for the fact that it might be 0 or negative, in which case it's an offset with respect to the total count of lines on the current page (or whatever count is passed in).
        /// </summary>
        /// <param name="LineCount"></param>
        /// <returns></returns>
        public int AdjustedDataLinesEnd(int LineCount) {
            return (DataLinesEnd > 0) ? DataLinesEnd : LineCount + DataLinesEnd;
        }
    }
}
