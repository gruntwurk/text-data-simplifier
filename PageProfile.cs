using System;
using System.Collections.Generic;
using System.Linq;
using static GruntWurk.QuickLog;

namespace GruntWurk {
    class PageProfile {
        public int PartNo;
        public int PageStart;
        public int PageEnd;
        public bool HasHeader;
        public bool HasFooter;
        public bool HasCondition;
        public int HeaderLinesStart;
        public int HeaderLinesEnd;
        public int DataLinesStart;
        public int DataLinesEnd;
        public int DataRows;
        public int FooterLinesStart;
        public int FooterLinesEnd;
        public List<FieldSpec> HeaderFields;
        public List<FieldSpec> DataFields;
        public List<FieldSpec> FooterFields;
        private List<string> CurrentRow;
        private FieldSpec ConditionFieldSpec;
        private string ConditionValue;
        private string ConditionAction;

        public PageProfile() {
            PageStart = 1;
            PageEnd = 0;
            HasHeader = false;
            HeaderLinesStart = 0;
            HeaderLinesEnd = 0;
            DataLinesStart = 1;
            DataLinesEnd = 0;
            DataRows = 1;
            HasFooter = false;
            HasCondition = false;
            FooterLinesStart = 0;
            FooterLinesEnd = 0;
            HeaderFields = new List<FieldSpec>();
            DataFields = new List<FieldSpec>();
            FooterFields = new List<FieldSpec>();
            CurrentRow = new List<string>();
        }

        public void SetPages(string arg) {
            try {
                PageStart = StartOfRangeSpec(arg);
                PageEnd = EndOfRangeSpec(arg);
            } catch (FormatException) {
                Invalid("Pages", arg);
            }
        }

        public void SetHeaderLines(string arg) {
            try {
                HasHeader = true;
                HeaderLinesStart = StartOfRangeSpec(arg);
                HeaderLinesEnd = EndOfRangeSpec(arg);
            } catch (FormatException) {
                Invalid("Header Lines", arg);
            }
        }

        // Returns a single string with all of the field labels delimited by the given separator. The hedaer fields (if any) come first, followed by the footer fields (if any), folled by the data fields.
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
                if (CurrentRow.Count >= DataRows) {
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
                Invalid("Footer Lines", arg);
            }
        }
        public void SetDataLines(string arg) {
            DataLinesStart = StartOfRangeSpec(arg);
            DataLinesEnd = EndOfRangeSpec(arg);
        }
        public void SetDataRows(string arg) {
            if (!int.TryParse(arg, out DataRows)) {
                Invalid("Data Rows", arg);
            }
        }
        /// <exception cref="FormatException">Only the caller knows how to report the issue in context.</exception>
        public int StartOfRangeSpec(string RangeSpec) {
            try {
                int p = RangeSpec.IndexOf("..");
                if (p > 0) {
                    string part = RangeSpec.Substring(0, p);
                    if (part == "N") {
                        return 0;
                    } else if (part.Substring(0, 1) == "N") {
                        part = part.Substring(1);
                    }
                    return int.Parse(part);
                }
                return int.Parse(RangeSpec);
            } catch (FormatException e) {
                throw e;
            }
        }
        /// <exception cref="FormatException">Only the caller knows how to report the issue in context.</exception>
        public int EndOfRangeSpec(string RangeSpec) {
            try {
                int p = RangeSpec.IndexOf("..");
                if (p > 0) {
                    string part = RangeSpec.Substring(p + 2).Trim().ToUpper();
                    if (part == "N") {
                        return 0;
                    } else if (part.Substring(0, 1) == "N") {
                        part = part.Substring(1);
                    }
                    return int.Parse(part);
                }
                return 0;
            } catch (FormatException e) {
                throw e;
            }
        }
        public void AddHeaderField(string FieldName, string spec) {
            try {
                HeaderFields.Add(new FieldSpec(FieldName, spec));
            } catch (FormatException) {
                Invalid("Header Field " + FieldName, spec);
            }
        }
        public void AddDataField(string FieldName, string spec) {
            try {
                DataFields.Add(new FieldSpec(FieldName, spec));
            } catch (FormatException) {
                Invalid("Data Field " + FieldName, spec);
            }

        }
        public void AddFooterField(string FieldName, string spec) {
            try {
                FooterFields.Add(new FieldSpec(FieldName, spec));
            } catch (FormatException) {
                Invalid("Footer Field " + FieldName, spec);
            }
        }
        public void InitializeFromIniFile(int PartNo, IniFile spec) {
            this.PartNo = PartNo;
            string PartNoText = PartNo.ToString();
            string MainPartSection = "PART " + PartNoText;
            Dictionary<string, IniFileEntry> submap;
            submap = spec.Sections[MainPartSection];
            if (submap.Keys.Contains("PAGES")) {
                SetPages(submap["PAGES"].value);
            }
            if (submap.Keys.Contains("HEADER LINES")) {
                SetHeaderLines(submap["HEADER LINES"].value);
            }
            if (submap.Keys.Contains("DATA LINES")) {
                SetDataLines(submap["DATA LINES"].value);
            }
            if (submap.Keys.Contains("LINES PER ROW")) {
                SetDataRows(submap["LINES PER ROW"].value);
            }
            if (submap.Keys.Contains("FOOTER LINES")) {
                SetFooterLines(submap["FOOTER LINES"].value);
            }

            // Header fields
            string FieldsSection = MainPartSection + " HEADER FIELDS";
            if (spec.Sections.Keys.Contains(FieldsSection)) {
                submap = spec.Sections[FieldsSection];
                foreach (string FieldName in submap.Keys) {
                    AddHeaderField(FieldName, submap[FieldName].value);
                }
            }

            // Data fields
            FieldsSection = MainPartSection + " DATA FIELDS";
            if (spec.Sections.Keys.Contains(FieldsSection)) {
                submap = spec.Sections[FieldsSection];
                foreach (string FieldName in submap.Keys) {
                    AddDataField(FieldName, submap[FieldName].value);
                }
            }

            // Footer fields
            FieldsSection = MainPartSection + " FOOTER FIELDS";
            if (spec.Sections.Keys.Contains(FieldsSection)) {
                submap = spec.Sections[FieldsSection];
                foreach (string FieldName in submap.Keys) {
                    AddFooterField(FieldName, submap[FieldName].value);
                }
            }

            string ConditionSection = MainPartSection + " CONDITION";
            if (spec.Sections.Keys.Contains(ConditionSection)) {
                submap = spec.Sections[ConditionSection];
                if (submap.Keys.Contains("FIELD") && submap.Keys.Contains("VALUE")) {
                    HasCondition = true;
                    ConditionFieldSpec = new FieldSpec("Condition", submap["FIELD"].value);
                    ConditionValue = submap["VALUE"].value;
                    if (submap.Keys.Contains("ACTION")) {
                        ConditionAction = submap["ACTION"].value;
                        if (ConditionAction != "Next Page") {
                            Invalid("Condition Action",ConditionAction);
                        }
                    } else {
                        log("Condition action of 'Next Page' was assumed, even though it was not specified.");
                        ConditionAction = "Next Page";
                    }
                } else {
                    Invalid(ConditionSection, "Field or Value specification is missing");
                }

            }

        }
        public void Dump() {
            Console.WriteLine("===== Page Profile for Part {0} =====", PartNo.ToString());
            List<string> elements = new List<string>();
            elements.Add(string.Format("Part {0}", PartNo));
            elements.Add(string.Format("Pages {0}..{1}", PageStart, PageEnd));
            if (HasHeader) {
                elements.Add(string.Format("Header {0}..{1}", HeaderLinesStart, HeaderLinesEnd));
            } else {
                elements.Add("No Header");
            }
            elements.Add(string.Format("Data {0}..{1}/{2}", DataLinesStart, DataLinesEnd, DataRows));
            if (HasFooter) {
                elements.Add(string.Format("Footer {0}..{1}", FooterLinesStart, FooterLinesEnd));
            } else {
                elements.Add("No Footer");
            }
            Console.WriteLine(string.Join("; ", elements));
            Console.WriteLine("");
        }

        public void Invalid(string ParameterName, string arg) {
            log(string.Format("Invalid {0} value: {1}", ParameterName, arg));
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
