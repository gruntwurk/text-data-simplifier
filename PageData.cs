using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GruntWurk.QuickLog;
using static GruntWurk.NumberUtils;
using static GruntWurk.StringUtils;

namespace GruntWurk {
    /// <summary>
    /// Companion class for the PageProfile. This one works with actual data (as specified by the profile).
    /// </summary>
    class PageData {
        PageProfile prof;
        List<string> CurrentRow;
        List<string> GroupValues;

        public PageData(PageProfile prof) {
            this.prof = prof;
            Clear();
        }

        private void Clear() {
            CurrentRow = new List<string>();
            GroupValues = new List<string>();
        }
        // Returns a single string with all of the field labels delimited by the given separator. The header fields (if any) come first, followed by the footer fields (if any), followed by the data fields.
        public string ColumnTitles(string Separator) {
            List<string> ColTitles = new List<string>();
            // TODO Allow field specs to include a directive to keep the field out of the output
            if (prof.HasHeader) {
                foreach (FieldSpec fspec in prof.HeaderFields) {
                    ColTitles.Add(fspec.FieldName);
                }
            }
            if (prof.HasFooter) {
                foreach (FieldSpec fspec in prof.FooterFields) {
                    ColTitles.Add(fspec.FieldName);
                }
            }
            if (prof.HasGroup) {
                foreach (FieldSpec fspec in prof.GroupFields) {
                    ColTitles.Add(fspec.FieldName);
                }
            }
            foreach (FieldSpec fspec in prof.DataFields) {
                ColTitles.Add(fspec.FieldName);
            }
            return string.Join(Separator, ColTitles);
        }


        /// <summary>
        /// Return a string list of the data from this page.
        /// Each string in the list represents a data "row" (which might consist of multiple lines of text, depending on the "Lines Per Row" specification.)
        /// Each string begins with all of the data that is common to every record on the page (i.e. the page number, any header fields, and any footer fields).
        /// NOTE: If a data row is started on this page, but continues on the next page, then it will be considered part of the next page.
        /// </summary>
        /// <param name="PageNo">Which page number this is (having counted page breaks, that is, regardless of any "page number" field that might exist in the page header or footer.).</param>
        /// <param name="PageLines">The text of this page.</param>
        /// <param name="Separator">The separator to be used between field values in the output (e.g. Tab)</param>
        /// <returns>A string list.</returns>
        public List<string> DataValues(int PageNo, List<string> PageLines, string Separator) {
            List<string> ExtractedDataFromCurrentPage = new List<string>();
            List<string> CommonValues = new List<string>();
            List<string> RecordValues = new List<string>();
            // Note: the GroupValues list is defined as a class field, since groups can span pages
            // TODO Allow field specs to include a directive to keep the field out of the output
            if (prof.HasHeader) {
                ExtractDataFromCurrentPage(CommonValues, prof.HeaderFields, PageLines);
            }
            if (prof.HasFooter) {
                ExtractDataFromCurrentPage(CommonValues, prof.FooterFields, PageLines);
            }
            bool CurrentlyProcessingAGroupHeader = false;
            for (int LineIndex = prof.DataLinesStart; LineIndex <= prof.AdjustedDataLinesEnd(PageLines.Count); LineIndex++) {
                CurrentRow.Add(PageLines[LineIndex - 1]);

                if (CurrentRow.Count == 1) {
                    // We just started a new row. The question is, is this a regular data row or a group-header row?
                    if (prof.HasGroup && GroupTriggered()) {
                        CurrentlyProcessingAGroupHeader = true;
                        GroupValues.Clear();
                    }
                }

                if (CurrentlyProcessingAGroupHeader && CurrentRow.Count >= prof.GroupLinesPerRow) {
                    // We reached the bottom of the group header. Go ahead and process it
                    ExtractDataFromCurrentRow(GroupValues, prof.GroupFields);
                    CurrentlyProcessingAGroupHeader = false;
                    CurrentRow.Clear();
                } else if (!CurrentlyProcessingAGroupHeader && CurrentRow.Count >= prof.DataLinesPerRow) {
                    if (prof.HasCondition) {
                        if (prof.ConditionFieldSpec.LineNo <= CurrentRow.Count) {
                            string trigger = prof.ConditionFieldSpec.CorrespondingValue(CurrentRow);
                            if (trigger == prof.ConditionValue) {
                                info("INFO: {0} action triggered on page {1}, line {2}.", prof.ConditionAction, PageNo, LineIndex);
                                // if (ConditionAction == "Next Page") {
                                CurrentRow.Clear();
                                break;
                                // } else {
                                // }
                            }
                        }
                    }
                    if (!IsEmpty(CurrentRow)) {
                        ExtractDataFromCurrentRow(RecordValues, prof.DataFields);
                        string outputLine = string.Join(Separator, CommonValues) + ((CommonValues.Count > 0) ? Separator : "");
                        outputLine += string.Join(Separator, GroupValues) + ((GroupValues.Count > 0) ? Separator : "");
                        outputLine += string.Join(Separator, RecordValues);
                        ExtractedDataFromCurrentPage.Add(outputLine);
                    }
                    RecordValues.Clear();
                    CurrentRow.Clear();
                }
            }
            return ExtractedDataFromCurrentPage;
        }

        private bool IsEmpty(List<string> buff) {
            foreach (string s in buff) {
                if (s.Trim().Length > 0) {
                    return false;
                }
            }
            return true;
        }
        private bool GroupTriggered() {
            // In the case of the GroupTrigger field spec, the "Field Name" is actually the text that identifies the start of the group header
            return (prof.GroupTrigger.CorrespondingValue(CurrentRow).Contains(prof.GroupTrigger.FieldName));
        }

        private void ExtractDataFromCurrentPage(List<string> ValueList, List<FieldSpec> FieldList, List<string> PageLines) {
            foreach (FieldSpec fspec in FieldList) {
                ValueList.Add(fspec.CorrespondingValue(PageLines));
            }
        }

        private void ExtractDataFromCurrentRow(List<string> ValueList, List<FieldSpec> FieldList) {
            foreach (FieldSpec fspec in FieldList) {
                if (fspec.LineNo <= CurrentRow.Count) {
                    ValueList.Add(fspec.CorrespondingValue(CurrentRow));
                } else {
                    ValueList.Add("");
                }
            }
        }


    }
}
