using System;
using System.Collections.Generic;
using System.Linq;
using static GruntWurk.QuickLog;
using static GruntWurk.NumberUtils;

namespace GruntWurk {
    class PageProfile {

        public bool HasHeader;
        public bool HasFooter;
        public bool HasCondition;
        public int HeaderLinesStart;
        public int HeaderLinesEnd;
        public int DataLinesStart;
        public int DataLinesEnd;
        public int DataLinesPerRow;
        public int FooterLinesStart;
        public int FooterLinesEnd;
        public int GroupLinesPerRow;
        public List<FieldSpec> HeaderFields;
        public List<FieldSpec> DataFields;
        public List<FieldSpec> FooterFields;
        private List<string> CurrentRow;
        private FieldSpec ConditionFieldSpec;
        private string ConditionValue;
        private string ConditionAction;

        /// <summary>
        /// Constructor
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
                throw new ArgumentException("Invalid Lines specification: " + arg, "Header section");
            }
        }

        // Returns a single string with all of the field labels delimited by the given separator. The header fields (if any) come first, followed by the footer fields (if any), followed by the data fields.
        public string ColumnTitles(string Separator) {
            List<string> ColTitles = new List<string>();
            // TODO Allow a command-line option to suppress the page no
            ColTitles.Add("PageNo");
            if (HasHeader) {
                foreach (FieldSpec fspec in HeaderFields) {
                    ColTitles.Add(fspec.FieldName);
                }
            }
            if (HasFooter) {
                foreach (FieldSpec fspec in FooterFields) {
                    ColTitles.Add(fspec.FieldName);
                }
            }
            foreach (FieldSpec fspec in DataFields) {
                ColTitles.Add(fspec.FieldName);
            }
            return string.Join(Separator, ColTitles);
        }

        /// <summary>
        /// Return a string list of the data from this page.
        /// Each string in the list represents a data "row" (which might consist of multiple lines of text, depending on the "Data Span" specification.)
        /// Each string begins with all of the data that is common to every record on the page (i.e. the page number, any header fields, and any footer fields).
        /// NOTE: If a data row is started on this page, but continues on the next page, then it will be considered part of the next page.
        /// </summary>
        /// <param name="PageNo">Which page number this is (having counted page breaks, that is, regardless of any "page number" field that might exist in the page header or footer.).</param>
        /// <param name="PageLines">The text of this page.</param>
        /// <param name="Separator">The separator to be used between field values in the output (e.g. Tab)</param>
        /// <returns>A string list.</returns>
        public List<string> DataValues(int PageNo, List<string> PageLines, string Separator) {
            List<string> CombinedValues = new List<string>();
            List<string> CommonValues = new List<string>();
            List<string> RecordValues = new List<string>();
            // TODO Allow a command-line option to suppress the page no
            CommonValues.Add(PageNo.ToString());
            if (HasHeader) {
                foreach (FieldSpec fspec in HeaderFields) {
                    CommonValues.Add(StringUtils.Mid(PageLines[fspec.LineNo - 1], fspec.ColNo, fspec.Width).Trim());
                }
            }
            if (HasFooter) {
                foreach (FieldSpec fspec in FooterFields) {
                    CommonValues.Add(StringUtils.Mid(PageLines[fspec.AdjustedLineNo(PageLines.Count) - 1], fspec.ColNo, fspec.Width).Trim());
                }
            }
            for (int LineIndex = DataLinesStart; LineIndex <= AdjustedDataLinesEnd(PageLines.Count); LineIndex++) {
                CurrentRow.Add(PageLines[LineIndex - 1]);
                if (CurrentRow.Count >= DataLinesPerRow) {
                    if (HasCondition) {
                        if (ConditionFieldSpec.LineNo <= CurrentRow.Count) {
                            string trigger = StringUtils.Mid(CurrentRow[ConditionFieldSpec.LineNo - 1], ConditionFieldSpec.ColNo, ConditionFieldSpec.Width);
                            if (trigger == ConditionValue) {
                                if (Program.options.Verbose) {
                                    Console.WriteLine("INFO: {0} action triggered on page {1}, line {2}.", ConditionAction, PageNo, LineIndex);
                                }
                                // if (ConditionAction == "Next Page") {
                                CurrentRow.Clear();
                                break;
                                // } else {
                                // }
                            }
                        }
                    }
                    foreach (FieldSpec fspec in DataFields) {
                        if (fspec.LineNo <= CurrentRow.Count) {
                            RecordValues.Add(StringUtils.Mid(CurrentRow[fspec.LineNo - 1], fspec.ColNo, fspec.Width).Trim());
                        } else {
                            RecordValues.Add("");
                        }
                    }

                    CombinedValues.Add(string.Join(Separator, CommonValues) + Separator + string.Join(Separator, RecordValues));
                    RecordValues.Clear();
                    CurrentRow.Clear();
                }
            }
            return CombinedValues;
        }

        public void SetFooterLines(string arg) {
            try {
                HasFooter = true;
                FooterLinesStart = StartOfRangeSpec(arg);
                FooterLinesEnd = EndOfRangeSpec(arg);
            } catch (FormatException) {
                throw new ArgumentException("Invalid Lines specification: " + arg, "Footer section");
            }
        }
        public void SetDataLines(string arg) {
            DataLinesStart = StartOfRangeSpec(arg);
            DataLinesEnd = EndOfRangeSpec(arg);
        }
        public void SetDataLinesPerRow(string arg) {
            if (!int.TryParse(arg, out DataLinesPerRow)) {
                throw new ArgumentException("Invalid Lines Per Row: " + arg, "Data section");
            }
        }
        public void SetGroupLinesPerRow(string arg) {
            if (!int.TryParse(arg, out GroupLinesPerRow)) {
                throw new ArgumentException("Invalid Lines Per Row: "+ arg,"Data section");
            }
        }

        public void AddHeaderField(string fspec) {
            try {
                HeaderFields.Add(new FieldSpec(fspec));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: " + fspec, "Header section");
            }
        }
        public void AddDataField(string fspec) {
            try {
                DataFields.Add(new FieldSpec(fspec));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: " + fspec, "Data section");
            }

        }
        public void AddFooterField(string fspec) {
            try {
                FooterFields.Add(new FieldSpec(fspec));
            } catch (FormatException) {
                throw new ArgumentException("Invalid field specification: "+fspec,"Footer section");
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
            FooterFields = new List<FieldSpec>();
            CurrentRow = new List<string>();
        }

        private void InitializeFromIniFile(IniFile spec) {
            Dictionary<string, IniFileEntry> submap;
            SetHeaderLines(spec.GetString("Page Header", "Lines", ""));
            SetDataLines(spec.GetString("Data", "Lines", ""));
            SetDataLinesPerRow(spec.GetString("Data", "Lines Per Row", ""));
            SetFooterLines(spec.GetString("Page Footer", "Lines", ""));

            // Header fields
            if (spec.Sections.Keys.Contains("Page Header")) {
                submap = spec.Sections["Page Header"];
                foreach (IniFileEntry entry in submap.Values) {
                    if (entry.key == "Field") {
                        AddHeaderField(entry.value);
                    }
                }
            }


            // Data fields
            if (spec.Sections.Keys.Contains("Data")) {
                submap = spec.Sections["Data"];
                foreach (IniFileEntry entry in submap.Values) {
                    if (entry.key == "Field") {
                        AddDataField(entry.value);
                    }
                }
            }



            // Footer fields
            if (spec.Sections.Keys.Contains("Footer")) {
                submap = spec.Sections["Footer"];
                foreach (IniFileEntry entry in submap.Values) {
                    if (entry.key == "Field") {
                        AddFooterField(entry.value);
                    }
                }
            }

            if (spec.Sections.Keys.Contains("Contition")) {
                submap = spec.Sections["Contition"];
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
        public void Dump() {
            Console.WriteLine("===== Page Profile =====");
            List<string> elements = new List<string>();
            if (HasHeader) {
                elements.Add(string.Format("Header {0}..{1}", HeaderLinesStart, HeaderLinesEnd));
            } else {
                elements.Add("No Header");
            }
            elements.Add(string.Format("Data {0}..{1}/{2}", DataLinesStart, DataLinesEnd, DataLinesPerRow));
            if (HasFooter) {
                elements.Add(string.Format("Footer {0}..{1}", FooterLinesStart, FooterLinesEnd));
            } else {
                elements.Add("No Footer");
            }
            Console.WriteLine(string.Join("; ", elements));
            Console.WriteLine("");
        }



        /// <summary>
        /// Returns the DataLinesEnd spec, adjusted for the fact that it might be 0 or negative, in which case it's an offset with respect to the total count of lines on the current page.
        /// </summary>
        /// <param name="LineCount"></param>
        /// <returns></returns>
        public int AdjustedDataLinesEnd(int LineCount) {
            return (DataLinesEnd > 0) ? DataLinesEnd : LineCount + DataLinesEnd;
        }
    }
}
