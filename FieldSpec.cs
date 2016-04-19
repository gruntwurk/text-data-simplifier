namespace GruntWurk {
    /// <summary>
    /// Specification for a field located within a text-based report.
    /// </summary>
    public class FieldSpec {
        public string FieldName;
        public int LineNo;
        public int ColNo;
        public int Width;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fieldSpecification">A string that describes the field (e.g. "Amount, Line 1, Col 31, Width 12")</param>
        public FieldSpec(string fieldSpecification) {
            Parse(fieldSpecification);
        }

        private void Parse(string fieldSpecification) {
            LineNo = 1;
            ColNo = 0;
            Width = 0;

            string[] parts = fieldSpecification.Split(StringUtils.JUST_COMMA);

            int partNo = 0;
            foreach (string part in parts) {
                partNo++;
                string partTrimmed = part.Trim();
                if (partNo == 1) {
                    FieldName = partTrimmed;
                    continue;
                }
                string[] subparts = partTrimmed.Split(StringUtils.JUST_SPACE, 2);
                switch (subparts[0].ToUpper()) {
                    case "LINE":
                        LineNo = int.Parse(subparts[1]);
                        break;
                    case "COL":
                        ColNo = int.Parse(subparts[1]);
                        break;
                    case "WIDTH":
                        Width = int.Parse(subparts[1]);
                        break;
                }
            }
        }

        public int AdjustedLineNo(int LineCount) {
            return (LineNo > 0) ? LineNo : LineCount + LineNo;
        }
    }
}
